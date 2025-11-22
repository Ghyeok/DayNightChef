using System.Buffers;
using System.Collections;
using UnityEngine;

public enum AnimalStates
{
    Patrol = 0,
    Attack,
    Chase,
    Die
}

public class Patrol : IState<Animal>
{
    enum SubState { Idle, Move }

    SubState sub;
    float subUntil;        // 현재 서브 상태가 끝나는 시각
    Vector2 moveDir;       // Move 상태에서 유지할 이동 방향

    // 이동/대기 시간 범위 (원하면 Animal에 넣어도 됨)
    const float MoveMin = 1.0f;
    const float MoveMax = 3.0f;

    public void Enter(Animal a)
    {
        SwitchToIdle(a);
    }

    void SwitchToIdle(Animal a)
    {
        sub = SubState.Idle;
        subUntil = Time.time + Random.Range(a.IdleMin, a.IdleMax);

        a.SetRunning(false);
        a.StopMove();
    }

    void SwitchToMove(Animal a)
    {
        sub = SubState.Move;
        subUntil = Time.time + Random.Range(MoveMin, MoveMax);

        // 0벡터 방지
        Vector2 dir = Random.insideUnitCircle;
        if (dir.sqrMagnitude < 1e-4f)
            dir = Vector2.right;

        moveDir = dir.normalized;
        a.SetRunning(false); // 걷기라면 false 유지, 뛰게 하고 싶으면 true
    }

    public void Execute(Animal a, float dt)
    {
        switch (sub)
        {
            case SubState.Idle:
                if (Time.time >= subUntil)
                {
                    // Idle 끝 → Move 시작
                    SwitchToMove(a);
                }
                else
                {
                    a.StopMove();
                }
                break;

            case SubState.Move:
                {
                    // PatrolRadius 밖으로 너무 나가면 방향을 안쪽으로 틀어주기
                    Vector2 pos = a.rb.position;
                    Vector2 toCenter = a.SpawnPoint - pos;
                    float distFromCenter = toCenter.magnitude;

                    // 만약 중심에서 너무 멀어졌으면, 안쪽으로 방향 전환
                    if (distFromCenter > a.PatrolRadius)
                    {
                        if (toCenter.sqrMagnitude > 1e-4f)
                            moveDir = toCenter.normalized;
                    }

                    // moveDir 방향으로 이동
                    a.SetRunning(false);
                    a.MoveToWards(a.rb.position + moveDir, a.WalkSpeed);

                    // Move 시간 끝났으면 다시 Idle
                    if (Time.time >= subUntil)
                    {
                        SwitchToIdle(a);
                    }
                }
                break;
        }
    }

    public void Exit(Animal a)
    {
        a.StopMove();
        a.SetRunning(false);
    }
}

public class Chase : IState<Animal>
{
    float enterTime;
    const float MinChaseDuration = 0.5f;
    public void Enter(Animal a)
    {
        a.SetRunning(true);
        enterTime = Time.time;
        Debug.Log($"[{a.name}] Entered Chase | target={(a.target ? a.target.name : "null")} | leash={a.leashDIstance}");
    }

    public void Execute(Animal a, float dt)
    {
        if (!a.target)
        {
            Debug.Log($"[{a.name}] target null → Return");
            a.ChangeState(new Return());
            return;
        }

        if (a.IsPlayerBeyondLeash())
        {
            Debug.Log($"[{a.name}] 리쉬 초과 (거리={Vector2.Distance(a.target.position, a.SpawnPoint):F2}) → Return");
            a.ChangeState(new Return());
            return;
        }

        if (Time.time - enterTime >= MinChaseDuration && a.InAttackRange())
        {
            Debug.Log($"[{a.name}] 공격 범위 진입 → HoldAndStrike");
            a.ChangeState(new HoldAndStrike());
            return;
        }

        a.SetRunning(true);
        a.MoveToWards(a.target.position, a.RunSpeed);
    }

    public void Exit(Animal a) { a.SetRunning(false); }
}

public class  Attack : IState<Animal>
{
    public void Enter(Animal a)
    {
        if (!a.CanAttackNow())
        {
            a.ChangeState(new HoldAndStrike());
            return;
        }
        a.SetMovementLock(true);
        a.StopMove();
        a.SetRunning(false);
        a.attackBehavior?.OnEnter();
    }

    public void Execute(Animal a, float dt)
    {
        if(!a || !a.target) { a.ChangeState(new Return()); return; }

        bool cont = a.attackBehavior != null && a.attackBehavior.OnUpdate(dt);
        if (cont) return;

        // 공격 중에도 플레이어가 리쉬 초과하면 귀환
        if (a.IsPlayerBeyondLeash()) 
        {
            Debug.Log("Return due to leash"); 
            a.ChangeState(new Return());
            return; 
        }

        if(a.InAttackRange() && a.CanAttackNow()) a.ChangeState(new Attack());
        else a.ChangeState(new HoldAndStrike());

    }

    public void Exit(Animal a) {
        a.attackBehavior?.OnExit();
        a.EnsureAttackCooldown(a.AttackCooldown);
        a.SetMovementLock(false);
    }
}

public class Return : IState<Animal>
{
    public void Enter(Animal a) { a.SetRunning(true); }

    public void Execute(Animal a, float dt)
    {
        a.Regen(dt);

        float dist = Vector2.Distance(a.rb.position, a.SpawnPoint);
        float speed = (dist > a.PatrolRadius) ? a.RunSpeed : a.WalkSpeed;
        a.SetRunning(dist > a.PatrolRadius);
        a.MoveToWards(a.SpawnPoint, speed);

        if(dist <= a.ReturnArriveRadius)
        {
            a.ChangeState(new Patrol());
            return;
        }
    }

    public void Exit(Animal a) { a.SetRunning(false); }
}

public class  HoldAndStrike : IState<Animal>
{
    public void Enter(Animal a)
    {
        a.StopMove();
        a.SetRunning(false);
    }

    public void Execute(Animal a, float dt)
    {
        if (!a.target) { a.ChangeState(new Return()); return; }
        if (a.IsPlayerBeyondLeash()) {
            a.ChangeState(new Return());
            return;
        }

        if (!a.InAttackRange())
        {
            a.ChangeState(new Chase());
            return;
        }

        a.FaceTo(a.target.position);

        if (a.CanAttackNow())
        {
            a.ChangeState(new Attack());
            return;
        }

        a.StopMove();
    }

    public void Exit(Animal a) { }
}

