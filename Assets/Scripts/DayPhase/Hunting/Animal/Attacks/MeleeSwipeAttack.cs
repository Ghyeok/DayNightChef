using System.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 근접 단발 공격
/// </summary>
public class MeleeSwipeAttack : AttackBehavior
{
    [SerializeField] float preDelay = 0.12f;
    [SerializeField] float postDelay = 0.35f;
    [SerializeField] int damage;
    [SerializeField] float hitRadius = 0.6f;
    [SerializeField] LayerMask playerMask;
    Coroutine co;

    public override void OnEnter()
    {
        if (_busy) return;
        damage = owner.AttackPower;
        _busy = true;
        if (co != null) owner.StopCoroutine(co);
        co = owner.StartCoroutine(CoAttack());
    }

    public override bool OnUpdate(float dt) => co != null;

    public override void OnExit()
    {
        if (co != null) owner.StopCoroutine(co);
        co = null;
        _busy = false;
    }

    IEnumerator CoAttack()
    {
        var anim = owner.GetComponent<AnimatorController>();

        // 0) 이동/러닝 비활성
        owner.StopMove();
        owner.SetRunning(false);

        // 1) 초기 조준(0벡터 안전 처리)
        Vector2 initAim = (owner && owner.target)
            ? (Vector2)owner.target.position - owner.rb.position
            : anim.LastDir;
        if (initAim.sqrMagnitude <= 1e-6f) initAim = anim.LastDir;
        Vector2 face = initAim.normalized;

        // 2) 선딜~후딜 내내 방향 락 + 즉시 바라보기
        anim.LockFacingFor(preDelay + postDelay + 0.1f, face);
        owner.FaceTo(face);

        // 3) 공격 트리거
        anim.TriggerAttack();

        // 4) 선딜: 타깃을 계속 따라보게 갱신
        float t = 0f;
        while (t < preDelay)
        {
            if (owner && owner.target)
            {
                Vector2 aim = (Vector2)owner.target.position - owner.rb.position;
                if (aim.sqrMagnitude > 1e-6f)
                    owner.FaceDir(aim); // 락 중에는 _lockedDir도 갱신됨
            }
            t += Time.deltaTime;
            yield return null;
        }

        // 5) 히트박스 (락이 있으므로 LastDir이 곧 현재 바라보는 방향)
        Vector2 hitPos = owner.rb.position + anim.LastDir * 0.5f;
        var hit = Physics2D.OverlapCircle(hitPos, hitRadius, playerMask);
        var dp = hit ? hit.GetComponentInParent<DayPlayer>() : null;
        if (dp != null) dp.TakeDamage(damage);

        // 6) 후딜
        yield return new WaitForSeconds(postDelay);

        // 7) 쿨타임 시작
        owner.SetAttackCooldown();

        co = null;
        _busy = false;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!owner) return;
        var animCtrl = owner.GetComponent<AnimatorController>();
        Vector2 dir = animCtrl ? animCtrl.LastDir.normalized : Vector2.right;
        Vector2 hitPos = owner.rb
            ? (Vector2)owner.rb.position + dir * 0.5f
            : (Vector2)owner.transform.position + dir * 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPos, hitRadius);

#if UNITY_EDITOR
        Handles.color = Color.red;
        Handles.Label(hitPos + Vector2.up * (hitRadius + 0.1f), $"hitRadius: {hitRadius:F2}");
#endif
    }
#endif
}


