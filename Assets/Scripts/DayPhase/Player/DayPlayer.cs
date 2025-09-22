using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DayPlayer :  MonoBehaviour , IDamagable
{
    [Header("플레이어 스탯")]
    [SerializeField]
    private float playerMaxHP;
    [SerializeField]
    private float playerCurHP;

    private float playerAttack;
    private float playerMoveSpeed;

    public float CurBagWeight => InventoryManager.Instance?.CurrentWeight ?? 0f;
    public float MaxBagWeight => InventoryManager.Instance?.maxWeight ?? 0f;

    [Header("주변 오브젝트 탐지")]
    public float radius;
    public Collider[] colliders;
    int layerMask = 1 << 10;

    void Awake()
    {
        radius = 1f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, radius);
    }

    void Update()
    {
        playerMaxHP = DayPhasePlayerManager.Instance.playerMaxHP;
        playerCurHP = DayPhasePlayerManager.Instance.playerCurHP;

        playerAttack = DayPhasePlayerManager.Instance.playerAttack;
        playerMoveSpeed = DayPhasePlayerManager.Instance.playerMoveSpeed;

        DetectGameObject(layerMask);
    }

    public void TakeDamage(float damage)
    {
        //애니메이션 추가
        playerCurHP -= damage;

        if (playerCurHP < 0)
        {

        }
    }

    public void Dead()
    {

    }

    private void DetectGameObject(LayerMask layer)
    {
        colliders = Physics.OverlapSphere(transform.position, radius, layer);

        float closestDistance = float.MaxValue;
        IInteract closestInteract = null;

        foreach (Collider collider in colliders)
        {   
            IInteract interact = collider.gameObject.GetComponentInParent<IInteract>();
            if (interact != null)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteract = interact;
                }
            }
            
        }

        if (colliders.Length == 0)
        {
            DayPhasePlayerManager.Instance.currentInteract = null;
        }

        if (closestInteract != null)
        {
            DayPhasePlayerManager.Instance.currentInteract = closestInteract;
        }
    }
}