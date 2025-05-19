using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static UnityEditor.Experimental.GraphView.GraphView;

public class DayPlayer :  MonoBehaviour , IDamagable
{
    [Header("플레이어 스탯")]
    [SerializeField]
    private float playerMaxHP;
    [SerializeField]
    private float playerCurHP;

    private float playerAttack;
    private float playerMoveSpeed;

    private float maxBagWeight;
    private float curBagWeight;

    [Header("주변 오브젝트 탐지")]
    public float radius;
    public Collider[] colliders;
    int layerMask = 1 << 10;

    void Awake()
    {
        radius = 3f;
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

        maxBagWeight = DayPhasePlayerManager.Instance.maxBagWeight;
        curBagWeight = DayPhasePlayerManager.Instance.curBagWeight;

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
            IInteract interact = collider.gameObject.GetComponent<IInteract>();
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

        if (closestInteract != null)
        {
            Interact(closestInteract);
        }
    }

    public void Interact(IInteract interact)
    {
        interact.Interact(this.gameObject); // 플레이어를 인자로 넘긴다.
    }
}