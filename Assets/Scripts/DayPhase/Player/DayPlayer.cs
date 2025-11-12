using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum PlayerState
{
    Alive,
    Dead,
}

public enum PlayerBehavior
{
    Hunting,
    Fishing,
    Gathering,
    MaxCount,
}

public class DayPlayer :  MonoBehaviour , IDamagable
{
    [Header("플레이어 스탯")]
    [SerializeField] private float playerMaxHP;
    [SerializeField] private float playerCurHP;
    [SerializeField] private float playerAttack;
    [SerializeField] private float playerMoveSpeed;

    public float CurBagWeight => InventoryManager.Instance?.CurrentWeight ?? 0f;
    public float MaxBagWeight => InventoryManager.Instance?.maxWeight ?? 0f;

    [Header("주변 오브젝트 탐지")]
    public float radius;
    public Collider2D[] colliders;
    int layerMask = 1 << 10;

    private IInteract lastInteract;
    private float revertGrace = 0.2f; // 상호작용이 사라진 뒤 Hunting으로 돌아가는 유예 시간
    private float revertTimer = 0f;

    private void OnEnable()
    {
        var pm = DayPhasePlayerManager.Instance;
        if (pm != null)
        {
            pm.OnSnapshotUpdated -= SyncFromManager;
            pm.OnSnapshotUpdated += SyncFromManager;

            SyncFromManager(pm.Snapshot);
        }
    }

    private void OnDisable()
    {
        var pm = DayPhasePlayerManager.Instance;
        if (pm != null)
            pm.OnSnapshotUpdated -= SyncFromManager;
    }

    /// <summary>
    /// 매니저가 브로드캐스트하는 스텟 스냅샷으로 동기화
    /// </summary>
    public void SyncFromManager(PlayerRuntimeSnapshot s)
    {
        playerMaxHP = s.MaxHP;
        playerCurHP = s.CurHP;
        playerAttack = s.Attack;
        playerMoveSpeed = s.MoveSpeed;
    }
    void Awake()
    {
        radius = 1f;
    }
    public void Initialize(float maxHP, float attack, float moveSpeed)
    {
        playerMaxHP = maxHP;
        playerCurHP = maxHP;   // 초기 한 번만 풀피
        playerAttack = attack;
        playerMoveSpeed = moveSpeed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    void Update()
    {
        DetectGameObject(layerMask);
    }

    public void TakeDamage(float damage)
    {
        //애니메이션 추가
        DayPhasePlayerManager.Instance?.ApplyDamage(damage);
    }

    public void Dead()
    {
        // 연출만 담당
    }

    private void DetectGameObject(LayerMask layer)
    {
        colliders = Physics2D.OverlapCircleAll(transform.position, radius, layerMask);

        float closestDistance = float.PositiveInfinity;
        IInteract closestInteract = null;

        foreach (Collider2D collider in colliders)
        {   
            IInteract interact = collider.gameObject.GetComponentInParent<IInteract>();
            if (interact != null)
            {
                float distance = (collider.transform.position - transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteract = interact;
                }
            }
            
        }
        if (closestInteract != null) // 채집, 사냥을 감지
        {
            DayPhasePlayerManager.Instance.currentInteract = closestInteract;
            lastInteract = closestInteract;
            revertTimer = 0f;
        }
        else // 아무것도 감지 못함
        {
            revertTimer += Time.deltaTime;
            if (revertTimer > revertGrace)
            {
                revertTimer = 0f;
                lastInteract = null;
                DayPhasePlayerManager.Instance.currentInteract = null;
            }
        }
    }
}