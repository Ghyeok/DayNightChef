using System.Collections;
using UnityEditor;
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
    [SerializeField] public float leashDIstance = 6f; // 스폰기준 리쉬
    [SerializeField] float returnArriveRadius = 9f;//스폰포인트 근처 구역, 복귀완료로 판단할 거리
    [SerializeField] float regenPerSec = 8f; // 복귀시 초당 체력 회복
    [SerializeField] float attackRange = 1.2f; //공격 범위
    private Boss boss;

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
    [SerializeField] float hitStunDuration = 1.25f;
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
        boss = GetComponent<Boss>();
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
        fsm.FixedUpdate(Time.fixedDeltaTime);
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
            if (rb)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }//  물리 속도도 즉시 0
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
        Physics2D.IgnoreCollision(GetComponent<BoxCollider2D>(), GetComponentsInChildren<BoxCollider2D>()[1]);// 충돌 방지
        Init();
    }

    private void Start()
    {
        Setup();
    }

    // FSM 연동
    public void ChangeState(IState<Animal> next)
    {
        string prev = fsm.current?.GetType().Name ?? "None";
        string nextName = next?.GetType().Name ?? "Null";
        Debug.Log($"[FSM] {name} : {prev} → {nextName} (t={Time.time:F2})");

        fsm.ChangeState(next);
    }

    public void MoveToWards(Vector2 dest, float speed)
    {
        if (_movementLocked)
        {
            _desiredVelocity = Vector2.zero;
            return;
        }
        Vector2 dir = dest - rb.position;
        _desiredVelocity = (dir.sqrMagnitude > 0.0001f) ? dir.normalized * speed : Vector2.zero;
        rb.MovePosition(rb.position + _desiredVelocity * Time.fixedDeltaTime);
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

        if (HP <= 0f)
        {
            IsDead = true;
            StopMove();
            animCtrl.TriggerDie();
            ItemManager.Instance.GetHuntingItem(base.MapType, ItemType.Animal, DropItem);
            Invoke(nameof(DestroySelf), 1.5f);
            if (boss != null) boss.UnlockedMap(MapType);
            return;
        }

        animCtrl.TriggerHit();

        hitStunUntil = Time.time + hitStunDuration;
        SetMovementLock(true);

        StopAllCoroutines();
        StartCoroutine(CoStun());
    }
    IEnumerator CoStun()
    {
        while (Time.time < hitStunUntil) yield return null;
        SetMovementLock(false);

        if (InAttackRange())
        {
            Debug.Log($"[{name}] 스턴 해제 → HoldAndStrike 전이 (거리={Vector2.Distance(rb.position, target.position):F2})");
            ChangeState(new HoldAndStrike());
        }
        else
        {
            Debug.Log($"[{name}] 스턴 해제 → Chase 전이 (거리={Vector2.Distance(rb.position, target.position):F2})");
            ChangeState(new Chase());
        }
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
#if UNITY_EDITOR
    [Header("Gizmos")]
    [SerializeField] bool drawGizmos = true;
    [SerializeField] Color attackRangeColor = new Color(1f, 0.92f, 0.016f, 0.8f); // 노랑
    [SerializeField] Color leashColor = new Color(0f, 0.75f, 1f, 0.5f);      // 하늘
    [SerializeField] Color patrolColor = new Color(0.5f, 1f, 0.5f, 0.35f);    // 연두
    [SerializeField] Color returnColor = new Color(1f, 1f, 1f, 0.35f);        // 흰색

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // 기준 위치
        Vector2 pos = rb ? rb.position : (Vector2)transform.position;

        // 1) attackRange (현재 위치 기준)
        Gizmos.color = attackRangeColor;
        Gizmos.DrawWireSphere(pos, attackRange);
#if UNITY_EDITOR
        Handles.color = attackRangeColor;
        Handles.Label(pos + Vector2.up * (attackRange + 0.15f), $"attackRange: {attackRange:F2}");
#endif

        // 2) SpawnPoint가 초기화 전이면 현재 위치 대체
        Vector2 spawn = (SpawnPoint.sqrMagnitude > 0.0001f) ? SpawnPoint : pos;

        // (선택) Leash, Patrol, Return 범위도 함께 확인하고 싶으면 유지
        // Leash Distance
        Gizmos.color = leashColor;
        Gizmos.DrawWireSphere(spawn, leashDIstance);
#if UNITY_EDITOR
        Handles.color = leashColor;
        Handles.Label(spawn + Vector2.right * (leashDIstance + 0.15f), $"leash: {leashDIstance:F2}");
#endif

        // Patrol Radius
        Gizmos.color = patrolColor;
        Gizmos.DrawWireSphere(spawn, patrolRadius);

        // Return Arrive Radius
        Gizmos.color = returnColor;
        Gizmos.DrawWireSphere(spawn, returnArriveRadius);
    }
#endif
}
