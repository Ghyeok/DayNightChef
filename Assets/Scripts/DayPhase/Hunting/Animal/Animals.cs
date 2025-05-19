using UnityEngine;

public abstract class Animals : Organism , IDamagable

{
    // 주석
    public float maxHp;
    public float currentHp;
    public float attack;
    public float speed;
    public float attackRange;
    public bool isDead;
    public Vector3 spawnPoint;
    //가지고있는 모든 상태
    private State[] states;
    private State currentState;
    public BoxCollider attackcollider;

    // Animals가 가지는 모든 상태

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnPoint = transform.position;
        ChangeState(DayPhaseManager.AnimalStates.Patrol);
    }

    public override void Setup()
    {
        states = new State[4];
        states[(int)DayPhaseManager.AnimalStates.Patrol] = new AnimalOwnedStates.Patrol();
        states[(int)DayPhaseManager.AnimalStates.Attack] = new AnimalOwnedStates.Attack();
        states[(int)DayPhaseManager.AnimalStates.Die] = new AnimalOwnedStates.Die();
        states[(int)DayPhaseManager.AnimalStates.Chase] = new AnimalOwnedStates.Chase();

        //현재 상태를 Patrol 상태로 결정
        ChangeState(DayPhaseManager.AnimalStates.Patrol);

    }
    public override void Updated()
    {
        if (currentState != null)
        {
            currentState.Execute(this);
        }
    }

    public void ChangeState(DayPhaseManager.AnimalStates newState)
    {
        //새로 바꾸려는 상태가 비어있으면 상태를 바꾸지 않는다.
        if (states[(int)newState] == null) return;
        //현재 재생중인 상태가 있으면 Exit()메소드 호출
        if (currentState != null)
        {
            currentState.Exit(this);
        }

        //새로운 상태로 변경하고, 새로바뀐 상태의 Enter()메소드 호출
        currentState = states[(int)newState];
        currentState.Enter(this);

    }
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        if(currentHp <= 0) ChangeState(DayPhaseManager.AnimalStates.Die);
        else ChangeState(DayPhaseManager.AnimalStates.Chase);
    }
}
