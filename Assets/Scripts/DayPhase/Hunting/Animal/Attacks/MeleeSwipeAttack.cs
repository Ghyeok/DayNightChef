using UnityEngine;
using System.Collections;

/// <summary>
/// 근접 단발 공격
/// 사용 동물 : 사슴
/// </summary>
public class MeleeSwipeAttack : AttackBehavior
{
    [SerializeField] float preDelay = 0.12f;
    [SerializeField] float postDelay = 0.35f;
    [SerializeField] int damage = 8;
    [SerializeField] float hitRadius = 0.6f;
    [SerializeField] LayerMask playerMask;

    Coroutine co;

    public override void OnEnter()
    {
        if (co != null) owner.StopCoroutine(co);
        co = owner.StartCoroutine(CoAttack());
    }

    public override bool OnUpdate(float dt) => co != null;

    public override void OnExit()
    {
        if (co != null) owner.StopCoroutine(co);
        co = null;
    }

    IEnumerator CoAttack()
    {
        // 조준(정지 공격 시 방향 고정)
        Vector2 aim = owner.target ? (Vector2)owner.target.position - owner.rb.position : Vector2.right;
        owner.FaceTo(aim);

        // 애니메이션 트리거
        owner.GetComponent<AnimatorController>().TriggerAttack();

        // 선딜
        yield return new WaitForSeconds(preDelay);

        // 히트박스
        Vector2 hitPos = owner.rb.position + owner.GetComponent<AnimatorController>().LastDir * 0.5f;
        var hit = Physics2D.OverlapCircle(hitPos, hitRadius, playerMask);
        if (hit && hit.CompareTag("Player"))
        {
            // 데미지 입히기
            var h = hit.GetComponent<DayPlayer>();
            if (h != null) h.TakeDamage(damage);
        }

        // 후딜
        yield return new WaitForSeconds(postDelay);
        co = null;
    }
}

