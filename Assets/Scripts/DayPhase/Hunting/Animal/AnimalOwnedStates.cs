using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;

namespace AnimalOwnedStates
{
    public class Patrol : State
    {
        Animator anim;
        Transform animalTransform;
        Vector3 targetPos;
        NavMeshAgent agent;
        bool isReturningToSpawn = true;
        bool isWaiting = false;
        Animals currentEntity;
        MonoBehaviour coroutineHost; // Coroutine을 실행할 호스트
        public override void Enter(Animals entity)
        {
            currentEntity = entity;
            animalTransform = entity.transform;
            agent = entity.GetComponent<NavMeshAgent>();
            anim = entity.GetComponent<Animator>();
            coroutineHost = entity;
            agent.speed = entity.speed;
            agent.stoppingDistance = 0.2f;
            isWaiting = false;
            anim.SetBool("IsWalking", false); // 처음엔 정지 상태
            agent.SetDestination(entity.spawnPoint); // spawnPoint로 복귀
            // spawnPoint와 너무 가까운 경우 바로 행동 결정
            if (Vector3.Distance(animalTransform.position, entity.spawnPoint) < 0.5f)
            {
                isReturningToSpawn = false;
                DecideNextAction(); // ✅ 바로 행동 결정
            }
            else
            {
                agent.SetDestination(entity.spawnPoint);
                isReturningToSpawn = true;
                agent.SetDestination(entity.spawnPoint);
            }
            coroutineHost.StartCoroutine(test1());
        }
//- - - - - - - - - - - - - - - -- - - - - - - - Test
        IEnumerator test1() 
        {
            yield return new WaitForSeconds(10f);
            test();
        }
        public void test()
        {
            currentEntity.TakeDamage(3f);
        }
//- - - - - - - - - - - - - - - -- - - - - - - - Test
        public override void Execute(Animals entity)
        {
            if (agent.pathPending) return;
            if (isReturningToSpawn)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    isReturningToSpawn = false;
                    DecideNextAction();
                }
            }
            else if (!isWaiting && agent.remainingDistance <= agent.stoppingDistance)
            {
                DecideNextAction();
            }
        }
        public override void Exit(Animals entity)
        {
            anim.SetBool("IsWalking", false);
            agent.ResetPath();
            if (coroutineHost != null)
                coroutineHost.StopAllCoroutines();
        }
        private void DecideNextAction()
        {
            int decision = Random.Range(0, 2);
            if (decision < 1)
            {
                anim.SetBool("IsWalking", true);
                isWaiting = false;
                SetNewRandomDestination(); // ✅ 이동 시도 실행
            }
            else
            {
                anim.SetBool("IsWalking", false);
                isWaiting = true;
                agent.ResetPath();
                coroutineHost.StartCoroutine(WaitAndDecide());
            }

        }
        private IEnumerator WaitAndDecide()
        {
            yield return new WaitForSeconds(2f);
            isWaiting = false;
            DecideNextAction();
        }

        private void SetNewRandomDestination()
        {
            int maxAttempts = 10;
            float moveDistance = 5f;
            float patrolRadius = 20f;
            for (int attempt = 0; attempt < maxAttempts; attempt++) // 10번까지 이동 시도후 안되면 정지
            {
                Vector3 offset = Vector3.zero;
                int dir = Random.Range(0, 4);
                switch (dir)
                {
                    case 0: offset = Vector3.forward * moveDistance; break;
                    case 1: offset = Vector3.back * moveDistance; break;
                    case 2: offset = Vector3.right * moveDistance; break;
                    case 3: offset = Vector3.left * moveDistance; break;
                }

                Vector3 candidate = animalTransform.position + offset;
                float distanceFromSpawn = Vector3.Distance(candidate, currentEntity.spawnPoint);
                if (distanceFromSpawn <= patrolRadius)
                {
                    targetPos = candidate;
                    agent.SetDestination(targetPos);
                    return;
                }
            }
            //실패했을 경우
            anim.SetBool("isWalking", false);
            isWaiting = true;
            agent.ResetPath();
            coroutineHost.StartCoroutine(WaitAndDecide());
        }
    }

    public class Attack : State
    {
        Animator anim;
        Animals currentAniaml;
        MonoBehaviour coroutineHost;
        BoxCollider attackrange;

        public override void Enter(Animals entity)
        {
            anim = entity.GetComponent<Animator>();
            currentAniaml = entity;
            coroutineHost = entity;
            attackrange = entity.GetComponentInChildren<BoxCollider>();
            coroutineHost.StartCoroutine(Hit());
        }
        public override void Execute(Animals entity)
        {

        }
        public override void Exit(Animals entity)
        {
            anim.SetBool("IsAttack", false);
        }
        IEnumerator Hit()
        {
            anim.SetBool("IsAttack", true);
            yield return new WaitForSeconds(0.2f);
            attackrange.enabled = true;
            yield return new WaitForSeconds(1f);
            attackrange.enabled = false;
            yield return new WaitForSeconds(1f);
            currentAniaml.ChangeState(DayPhaseManager.AnimalStates.Chase);

        }
    }

    public class Chase : State
    {
        NavMeshAgent agent;
        Animator anim;
        Animals currentEntity;
        Transform target;
        float attackRange;
        float maxChaseDistance = 25f; // spawnpoint와의 최대 거리
        bool isReturningToSpawn = false;
        public override void Enter(Animals entity)
        {
            currentEntity = entity;
            attackRange = entity.attackRange;
            agent = entity.GetComponent<NavMeshAgent>();
            anim = entity.GetComponent<Animator>();
            agent.speed = entity.speed * 3;
            anim.SetBool("IsChase", true);
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }

        }
        public override void Execute(Animals entity)
        {
            if (isReturningToSpawn == false)
            {
                if (target == null) return;
                //추적
                if (agent.destination != target.position)
                    agent.SetDestination(target.position);
                Targeting();
                //멀어진지 확인
                float distanceFromSpawn = Vector3.Distance(entity.spawnPoint, entity.transform.position);
                if (distanceFromSpawn > maxChaseDistance)
                {
                    agent.speed = entity.speed * 3;
                    agent.SetDestination(entity.spawnPoint);
                    entity.currentHp = entity.maxHp;
                    isReturningToSpawn = true;
                }
            }
            else
            {
                if (Vector3.Distance(entity.transform.position, entity.spawnPoint) < 3f)
                {
                    isReturningToSpawn = false;
                    entity.ChangeState(DayPhaseManager.AnimalStates.Patrol);
                }
            }


        }
        public override void Exit(Animals entity)
        {
            if (anim != null) anim.SetBool("IsChase", false);
            if (agent != null) agent.ResetPath();
        }

        void Targeting()
        {
            Collider[] rayHits = Physics.OverlapSphere(currentEntity.transform.position, attackRange, LayerMask.GetMask("Player"));
            if (rayHits.Length > 0) currentEntity.ChangeState(DayPhaseManager.AnimalStates.Attack);
        }
    }

    public class Die : State
    {
        public override void Enter(Animals entity)
        {

        }
        public override void Execute(Animals entity)
        {

        }
        public override void Exit(Animals entity)
        {

        }
    }
}