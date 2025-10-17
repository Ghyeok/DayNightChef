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
        a.StartCoroutine(Testdmg(a));
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

    IEnumerator Testdmg(Animal a)
    {
        yield return new WaitForSeconds(5f);
        a.TakeDamage(1);
        
    }
}

public class Chase : IState<Animal>
{
    public void Enter(Animal a) { a.SetRunning(true);}

    public void Execute(Animal a, float dt)
    {
        if (!a.target) { a.ChangeState(new Return()); return; }

        // 사정거리 밖으로 나가면 추적 중지
        if (a.IsPlayerBeyondLeash()) { a.ChangeState(new Return()); return;}

        // 사거리 안이면 공격
        if (a.InAttackRange() && a.CanAttackNow()) { a.ChangeState(new Attack()); return;}

        // 추격
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
            a.ChangeState(new Chase());
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
        if (a.IsPlayerBeyondLeash()) { a.ChangeState(new Return()); return; }

        if(a.InAttackRange() && a.CanAttackNow()) a.ChangeState(new Attack());
        else a.ChangeState(new Chase());

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


