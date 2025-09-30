using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.XR;
using AnimalOwnedStates;

public enum AnimalStateType { Patrol, Chase, Attack, Die }
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public abstract class Animals : Organism , IDamagable

{
    [Header("Stats")]
    [SerializeField] protected float maxHP = 10f;
    [SerializeField] protected float currentHP = 10f;
    [SerializeField] protected float attack = 3f;
    [SerializeField] protected float speed = 1f;
    [SerializeField] protected float attackRange = 3f;
    [SerializeField] protected bool isDead = false;

    [Header("Patrol/Chase Tunings")]
    [SerializeField] protected float patrolRadius = 20f;
    [SerializeField] protected float waitSeconds = 2f;
    [SerializeField] protected float moveDistance = 5f;
    [SerializeField] protected float chaseDIstance = 25f; // 스폰으로부터 최대 추격 거리
    [SerializeField] protected float targetUpdateMinDelta = 0.5f; // 타겟 위치 갱신 임계치

    [Header("References")]
    [SerializeField] protected Collider attackTrigger; // 하위 오브젝트의 Trigger Collider
    [SerializeField] protected Collider mainCollider; // 본체 콜라이더
    [SerializeField] protected Animator anim;
    [SerializeField] protected NavMeshAgent agent;

    [Header("Runtime")]
    public Vector3 spawnPoint;

    [Header("Combat")]
    [SerializeField] protected float damagedChaseDuration = 5f; // 피격 후 어그로 유지 시간
    public DayPlayer Player { get; set; }
    public Action<Animals> OnRequestRemove; // animalist에서 제거 등 처리
    public Transform AggroTarget { get; private set; }
    float _aggroExpireUntil = -1f;
    public bool HasAggro => Time.time <= _aggroExpireUntil;

    protected StateMachine<Animals> fsm;
    protected readonly Dictionary<AnimalStateType, State<Animals>> states = new Dictionary<AnimalStateType, State<Animals>>();

    // 외부에서 읽기용
    public float AttackPower => attack;
    public float Speed => speed;
    public float AttackRange => attackRange;
    public float PatrolRadius => patrolRadius;
    public float WaitSeconds => waitSeconds;
    public float ChaseDIstance => chaseDIstance;
    public float TargetUpdateMinDeltaSqr => targetUpdateMinDelta * targetUpdateMinDelta;
    public Collider AttackTrigger => attackTrigger;
    public bool IsFullHp => currentHP >= maxHP - 0.01f;

    public override void Init()
    {
        
    }

    public override void Setup()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (mainCollider == null) mainCollider = GetComponent<Collider>();

        if (spawnPoint == Vector3.zero) spawnPoint = transform.position;
        if (attackTrigger != null) attackTrigger.enabled = false;

        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        isDead = currentHP <= 0f;

        fsm = new StateMachine<Animals>();
        // 상태 인스턴스 등록
        states[AnimalStateType.Patrol] = new AnimalOwnedStates.Patrol();
        states[AnimalStateType.Attack] = new AnimalOwnedStates.Attack();
        states[AnimalStateType.Chase] = new AnimalOwnedStates.Chase();
        states[AnimalStateType.Die] = new AnimalOwnedStates.Die();

        ChangeState(AnimalStateType.Patrol);
    }

    protected virtual void Awake()
    {
        Init();
        Setup();
    }

    protected virtual void Update()
    {
        Updated();
    }

    public override void Updated()
    {
        fsm?.Update(this);
    }

    public void ChangeState(AnimalStateType next)
    {
        if (!states.TryGetValue(next, out var state)) return;
        fsm.ChangeState(state, this);
    }

    public virtual void TakeDamage(float damage)
    {
        TakeDamage(damage, null);
    }

    public virtual void TakeDamage(float damage, Transform attacker)
    {
        if (isDead) return;

        currentHP = Mathf.Clamp(currentHP - damage, 0f, maxHP);

        if (attacker != null)
        {
            AggroTarget = attacker;
            _aggroExpireUntil = Time.time + damagedChaseDuration;
            if (Player == null) Player = attacker.GetComponent<DayPlayer>();
        }

        if (currentHP <= 0f)
        {
            Die();
            return;
        }
        ChangeState(AnimalStateType.Chase);
    }

    protected virtual void Die()
    {
        isDead = true;
        ChangeState(AnimalStateType.Die);
    }
}
