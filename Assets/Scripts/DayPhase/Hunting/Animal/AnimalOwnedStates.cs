using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using Unity.VisualScripting;

namespace AnimalOwnedStates
{
    public class Patrol : State<Animals>
    {
        Animator anim;
        Transform tr;
        NavMeshAgent agent;

        bool isReturningToSpawn;
        bool isWaiting;
        MonoBehaviour host; // 코루틴 호출할 호스트
        Coroutine waitCo;
        public override void Enter(Animals entity)
        {
            tr = entity.transform;
            agent = entity.GetComponent<NavMeshAgent>();
            anim = agent.GetComponent<Animator>();
            host = entity;

            isWaiting = false;
            anim.SetBool(AniHash.IsWalking, false);

            agent.speed = entity.Speed;
            agent.stoppingDistance = 0.2f;

            if (Vector3.Distance(tr.position, entity.spawnPoint) < 0.5f)
            {
                isReturningToSpawn = false;
                DecideNextAction(entity);
            }
            else
            {
                isReturningToSpawn = true;
                agent.SetDestination(entity.spawnPoint);
            }
        }

        public override void Execute(Animals entity)
        {
            if (agent.pathPending) return;

            if (isReturningToSpawn)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    isReturningToSpawn=false;
                    DecideNextAction(entity);
                }
            }
            else if (!isWaiting && agent.remainingDistance <= agent.stoppingDistance)
            {
                DecideNextAction(entity);
            }
        }

        public override void Exit(Animals entity)
        {
            anim.SetBool(AniHash.IsWalking, false);
            agent.ResetPath();

            if (waitCo != null)
            {
                host.StopCoroutine(waitCo);
                waitCo = null;
            }
        }

        void DecideNextAction(Animals entity)
        {
            int decision = Random.Range(0, 2);
            if (decision == 0)
            {
                anim.SetBool(AniHash.IsWalking, true);
                isWaiting = false;
                SetNewRandomDestination(entity);
            }
            else
            {
                anim.SetBool(AniHash.IsWalking, false);
                isWaiting = true;
                agent.ResetPath();
                StartWait(entity);
            }
        }

        void StartWait(Animals entity)
        {
            if (waitCo != null) host.StopCoroutine(waitCo);
            waitCo = host.StartCoroutine(WaitAndDecide(entity));
        }

        IEnumerator WaitAndDecide(Animals entity)
        {
            yield return new WaitForSeconds(entity.WaitSeconds);
            isWaiting = false;
            DecideNextAction(entity);
        }

        void SetNewRandomDestination(Animals entity)
        {
            const int maxAttempts = 10;
            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 rnd = Random.insideUnitCircle * entity.PatrolRadius;
                Vector3 candidate = entity.spawnPoint + new Vector3(rnd.x, 0f, rnd.y);

                if (NavMesh.SamplePosition(candidate, out var hit, 2f, NavMesh.AllAreas))
                {
                    if (Vector3.Distance(hit.position, entity.spawnPoint) <= entity.PatrolRadius)
                    {
                        agent.SetDestination(hit.position);
                        return;
                    }
                }
            }

            // 실패 : 대기 전환
            anim.SetBool(AniHash.IsWalking, false);
            isWaiting = true;
            agent.ResetPath();
            StartWait(entity);
        }
    }

    public class Attack : State<Animals>
    {
        Animator anim;
        MonoBehaviour host;
        Collider attackTrigger;
        Coroutine attackCo;

        public override void Enter(Animals entity)
        {
            anim = entity.GetComponent<Animator>();
            host = entity;
            attackTrigger = entity.AttackTrigger;

            if(attackTrigger == null)
            {
                Debug.Log("Attack Trigger가 지정 안됨");
                entity.ChangeState(AnimalStateType.Chase);
                return;
            }

            if (attackCo != null) host.StopCoroutine(attackCo);
            attackCo = host.StartCoroutine(Hit(entity));
        }
        public override void Execute(Animals entity)
        {

        }
        public override void Exit(Animals entity)
        {
            anim.SetBool(AniHash.IsAttack, false);
            if (attackTrigger != null) attackTrigger.enabled = false;

            if (attackCo != null)
            {
                host.StopCoroutine(attackCo);
                attackCo = null;
            }
        }
        IEnumerator Hit(Animals entity)
        {
            anim.SetBool(AniHash.IsAttack, true);
            yield return new WaitForSeconds(0.2f); // 선딜
            attackTrigger.enabled = true;
            yield return new WaitForSeconds(1f); // 활성화 구간
            attackTrigger.enabled = false;
            yield return new WaitForSeconds(1f); // 후딜
            entity.ChangeState(AnimalStateType.Chase);

        }
    }

    public class Chase : State<Animals>
    {
        NavMeshAgent agent;
        Animator anim;
        Animals owner;

        Transform target;
        Vector3 lastTargetPos;
        bool isReturningToSpawn;

        static readonly Collider[] hits = new Collider[8];
        public override void Enter(Animals entity)
        {
            owner = entity;
            agent = entity.GetComponent<NavMeshAgent>();
            anim = entity.GetComponent<Animator>();

            if (!agent.enabled) agent.enabled = true;
            agent.isStopped = false;
            agent.speed = entity.Speed * 3f;
            anim.SetBool(AniHash.IsChase, true);

            // 플레이어 참조, 없으면 Find
            if (entity.AggroTarget != null) target = entity.AggroTarget.transform;
            else if (entity.Player != null) target = entity.Player.transform;
            else
            {
                var PlayerGo = GameObject.FindGameObjectWithTag("Player");
                if (PlayerGo != null) target = PlayerGo.transform;
            }

            if (target != null) lastTargetPos = target.position;
            isReturningToSpawn = false;

        }
        public override void Execute(Animals entity)
        {
            if (isReturningToSpawn == false)
            {
                if (target == null)
                {
                    if (!entity.HasAggro)
                    {
                        agent.SetDestination(entity.spawnPoint);
                        isReturningToSpawn= true;
                    }
                    return;
                }

                //목적지 갱신 : 일정거리 이상 움직일때마다
                Vector3 tp = target.position;
                if ((tp - lastTargetPos).sqrMagnitude > entity.TargetUpdateMinDeltaSqr)
                {
                    if (NavMesh.SamplePosition(tp, out var hit, 2f, NavMesh.AllAreas))
                        agent.SetDestination(hit.position);
                    else agent.SetDestination(tp);
                    lastTargetPos = tp;
                }

                //추적 거리 검사
                if ((entity.transform.position - entity.spawnPoint).sqrMagnitude > entity.ChaseDIstance * entity.ChaseDIstance)
                {
                    agent.SetDestination(entity.spawnPoint);
                    isReturningToSpawn = true;
                    return;
                }

                // 타겟팅 (공격 사거리 감지)
                int count = Physics.OverlapSphereNonAlloc(
                    entity.transform.position,
                    entity.AttackRange,
                    hits,
                    LayerMask.GetMask("Player"));
                if (count > 0)
                {
                    entity.ChangeState(AnimalStateType.Attack);
                }
            }
            else
            {
                // 스폰 복귀 중: 스폰 근처에서 체력 회복 후 다시 Patrol
                if (Vector3.Distance(entity.transform.position, entity.spawnPoint) < 3f)
                {
                    entity.TakeDamage(-Time.deltaTime * 5f); // 자연 회복
                    if (entity.IsFullHp)
                    {
                        isReturningToSpawn = false;
                        entity.ChangeState(AnimalStateType.Patrol);
                    }
                }
            }
        }
        public override void Exit(Animals entity)
        {
            anim.SetBool(AniHash.IsChase, false);
            agent.ResetPath();
        }
    }

    public class Die : State<Animals>
    {
        Animator anim;
        Collider mainCol;
        MonoBehaviour host;
        Coroutine dieCo;
        public override void Enter(Animals entity)
        {
            anim = entity.GetComponent<Animator>();
            host = entity;

            // 본체 콜라이더 비활성
            mainCol = entity.GetComponent<Collider>();
            if (mainCol != null) mainCol.enabled = false;

            // 공격 트리거 비활성
            if (entity.AttackTrigger != null) entity.AttackTrigger.enabled = false;

            anim.SetTrigger(AniHash.DoDie);

            if (dieCo != null) host.StopCoroutine(dieCo);
            dieCo = host.StartCoroutine(WaitDie(entity));
        }
        public override void Execute(Animals entity)
        {

        }
        public override void Exit(Animals entity)
        {

        }

        IEnumerator WaitDie(Animals entity)
        {
            float wait = 2f; // 죽는 애니메이션 길이
            var info = anim.GetCurrentAnimatorStateInfo(0);
            if (info.length > 0.1f) wait = info.length;

            yield return new WaitForSeconds(wait);
            if (InventoryManager.Instance != null) InventoryManager.Instance.TryAdd(entity.DropItem, 1);
            entity.OnRequestRemove?.Invoke(entity);
            Object.Destroy(entity.gameObject);
        }
    }
}