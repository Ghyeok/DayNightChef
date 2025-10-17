using System.Collections;
using UnityEngine;
/// <summary>
/// 지속 애니메이션 : Speed/MoveX/MoveY/IsRunning (Blend Tree)
/// 단발 애니메이션 " Attack/Hit/DIe (Trigger)
/// FSM : Patrol/Chase/Attack/Return
/// HP/전투/이동은 Animal이 자체 관리
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer), typeof(AnimatorController))]
public class Animal : Organism
{
    [Header("Stats")]
    [SerializeField] float maxHP = 50f;
    [SerializeField] int attackPower = 8;
    [SerializeField] float walkSpeed = 1.8f;
    [SerializeField] float runSpeed = 3.2f;
    [SerializeField] float leashDIstance = 6f; // 스폰기준 리쉬
    [SerializeField] float returnArriveRadius = 9f;//스폰포인트 근처 구역, 복귀완료로 판단할 거리
    [SerializeField] float regenPerSec = 8f; // 복귀시 초당 체력 회복
    [SerializeField] float attackRange = 1.2f; //공격 범위

    [Header("Patrol")]
    [SerializeField] float patrolRadius = 3.5f;
    [SerializeField] float idleMin = 1.0f; // Idle 시간의 최소값
    [SerializeField] float idleMax = 2.0f;

    [Header("Reference")]
    public Transform target;
    public AttackBehavior attackBehavior; // 공격 패턴
    [SerializeField] bool debugLogs = false;

    [Header("Attack Cooldown")]
    [SerializeField] public float attackCooldown = 1f; // 쿨타임
    float nextAttackAllowedAt = 0f;

    [Header("경직 시간")]
    [SerializeField] float hitStunDuration = 0.35f;
    float hitStunUntil = 0f;

    public float HP { get; private set; }
    public bool IsDead { get; private set; }
    public Vector2 SpawnPoint { get; private set; }

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Animator animator;
    bool _movementLocked;

    AnimatorController animCtrl;
    StateMachine<Animal> fsm;

    Vector2 _desiredVelocity; // 현재 animal의 속도
    bool _isRunning;

    public float AttackCooldown => attackCooldown;
    public bool CanAttackNow() => Time.time >= nextAttackAllowedAt;
    public void SetAttackCooldown(float seconds) => nextAttackAllowedAt = Time.time + seconds;
    public void SetAttackCooldown() => nextAttackAllowedAt = Time.time + attackCooldown;

    // Organism
    public override void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animCtrl = GetComponent<AnimatorController>();
        animCtrl.Setup(animator);

        SpawnPoint = rb.position;
        HP = maxHP;
        IsDead = false;

        fsm = new StateMachine<Animal>(this);
        fsm.ChangeState(new Patrol());
    }

    public override void Setup()
    {
        attackBehavior?.Setup(this);
    }

    public override void Updated()
    {
        if (IsDead) return;
        fsm.Update(Time.deltaTime);
        if (_movementLocked)
        {
            _desiredVelocity = Vector2.zero;
            _isRunning = false;
        }
        animCtrl.ApplyMovement(_desiredVelocity, _isRunning);
    }

    public void SetMovementLock(bool v)
    {
        _movementLocked = v;
        if (v)
        {
            _desiredVelocity = Vector2.zero;
            _isRunning = false;
            if (rb) rb.linearVelocity = Vector2.zero;        //  물리 속도도 즉시 0
            if (debugLogs) Debug.Log($"[Animal] lock move ON t={Time.time:F3}");
        }
        else
        {
            if (debugLogs) Debug.Log($"[Animal] lock move OFF t={Time.time:F3}");
        }
    }

    public void EnsureAttackCooldown(float seconds)
    {
        float target = Time.time + seconds;
        // nextAttackAllowedAt은 private이므로 SetAttackCooldown(float)로 덮어쓰기
        // 더 긴 쿨이 이미 잡혀있을 수도 있으니, "더 늦은 시각"으로만 연장
        if (!CanAttackNow()) return; // 이미 쿨다운 중이면 그대로 두고 종료
        SetAttackCooldown(seconds);
        if (debugLogs) Debug.Log($"[Animal] cooldown set {seconds:F2}s until t={target:F2}");
    }

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        Setup();
    }

    // FSM 연동
    public void ChangeState(IState<Animal> next) => fsm.ChangeState(next);

    public void MoveToWards(Vector2 dest, float speed)
    {
        if (_movementLocked)
        {
            _desiredVelocity = Vector2.zero;
            return;
        }
        Vector2 dir = dest - rb.position;
        _desiredVelocity = (dir.sqrMagnitude > 0.0001f) ? dir.normalized * speed : Vector2.zero;
        rb.MovePosition(rb.position + _desiredVelocity * Time.deltaTime);
    }
    public void StopMove() => _desiredVelocity = Vector2.zero;
    public void SetRunning(bool v) => _isRunning = v;
    public void FaceTo(Vector2 worldPos) => animCtrl.FaceTo(worldPos - rb.position);
    public void FaceDir(Vector2 dir) => animCtrl.FaceTo(dir);

    // 사거리/ 리쉬 판단
    public bool InAttackRange()
    {
        if (!target) return false;
        return Vector2.Distance(rb.position, target.position) <= attackRange;
    }

    // 플레이어가 스폰포인트에서 멀어졌는지 판단
    public bool IsPlayerBeyondLeash()
    {
        if (!target) return false;
        return Vector2.Distance((Vector2)target.position, SpawnPoint) > leashDIstance;
    }

    // 전투/HP
    public void TakeDamage(float dmg)
    {
        if (IsDead) return;

        HP = Mathf.Max(0f, HP - dmg);
        animCtrl.TriggerHit();

        hitStunUntil = Time.time + hitStunDuration;
        SetMovementLock(true);
        
        if (HP <= 0f)
        {
            IsDead = true;
            StopMove();
            animCtrl.TriggerDie();
            Invoke(nameof(DestroySelf), 2f);
            return;
        }
        StopCoroutine(CoStun());
        StartCoroutine(CoStun());
    }

    IEnumerator CoStun()
    {
        while (Time.time < hitStunUntil) yield return null;
        SetMovementLock(false);
        ChangeState(new Chase());
    }

    public int AttackPower => attackPower;

    public void Regen(float dt)
    {
        if (HP < maxHP) HP = Mathf.Min(maxHP, HP + regenPerSec * dt);
    }

    void DestroySelf() => Destroy(gameObject);

    public float PatrolRadius => patrolRadius;
    public float IdleMin => idleMin;
    public float IdleMax => idleMax;
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float ReturnArriveRadius => returnArriveRadius;
}
