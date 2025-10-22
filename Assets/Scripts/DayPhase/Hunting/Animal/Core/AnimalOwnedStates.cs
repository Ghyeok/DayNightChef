using System.Buffers;
using System.Collections;
using UnityEngine;

public class Patrol : IState<Animal>
{
    Vector2 target;
    bool waiting;
    float waitUntil;

    public void Enter(Animal a)
    {
        waiting = true;
        waitUntil = Time.time + Random.Range(a.IdleMin, a.IdleMax);
        a.SetRunning(false);
        a.StopMove();
    }

    public void Execute(Animal a, float dt)
    {
        if (waiting)
        {
            if (Time.time >= waitUntil)
            {
                waiting = false;
                var rnd = Random.insideUnitCircle * a.PatrolRadius;
                target = a.SpawnPoint + rnd;
            }
            else { a.StopMove(); return; }
        }

        a.SetRunning(false);
        a.MoveToWards(target, a.WalkSpeed);

        if(Vector2.Distance(a.rb.position, target) < 0.2f)
        {
            waiting = true;
            waitUntil = Time.time + Random.Range(a.IdleMin, a.IdleMax);
            a.StopMove();
        }
    }

    public void Exit(Animal a) { }
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

