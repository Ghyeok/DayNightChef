using UnityEngine;
using System.Collections;
using SmallScaleInc.TopDownPixelCharactersPack1;
using DG.Tweening;

/// <summary>
/// 선딜 후 일정 시간 타깃을 향해 돌진하면서 공격
/// 사용 동물 : 늑대
/// </summary>

public class ChargeAttack : AttackBehavior
{
    [Header("Timings")]
    [SerializeField] float preDelay = 0.23f; // 선딜 시간
    [SerializeField] float attackDuration = 0.43f; // 돌진 시간
    [SerializeField] float postDelay = 0f; // 후딜 시간

    [Header("Motion")]
    [SerializeField] float chargeSpeed = 20.0f; // 돌진 속도
    [SerializeField] private bool stopOnHit = true; // 타격 시 돌진 멈춤 여부

    [Header("Hit Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float hitRadius = 0.5f; // 타격 범위
    [SerializeField] private float hitForwardOffset = 0.45f; // 타격범위의 몸 앞 오프셋
    [SerializeField] private LayerMask playerMask; // 플레이어 레이어

    private Coroutine co;
    private bool hitApplied;
    private Vector2 startPos;
    private Vector2 targetPos;
    private Tween moveTw;

    public override void OnEnter()
    {
        if (_busy) return;
        _busy = true;
        if (co != null) owner.StopCoroutine(co);
        co = owner.StartCoroutine(CoCharge());
    }

    public override bool OnUpdate(float dt) => co != null;

    public override void OnExit()
    {
        if (moveTw != null && moveTw.IsActive()) moveTw.Kill();
        moveTw = null;
        if (co != null) owner.StopCoroutine(co);
        co = null;
        owner.StopMove();
        owner.SetRunning(false);
        _busy = false;
    }

    private IEnumerator CoCharge()
    {
        Vector2 aim = owner.target ? (Vector2)owner.target.position - owner.rb.position : Vector2.right;

        owner.FaceTo(aim);
        owner.StopMove();
        owner.SetRunning(false);

        startPos = owner.rb.position;
        targetPos = owner.target ? (Vector2)owner.target.position : startPos + aim.normalized;

        // 선딜
        var animCtrl = owner.GetComponent<AnimatorController>();
        animCtrl.TriggerAttack();
        yield return new WaitForSeconds(preDelay);
        hitApplied = false;
        moveTw = owner.rb.DOMove(targetPos, attackDuration).SetEase(Ease.InQuart).SetUpdate(UpdateType.Fixed).OnComplete(() =>
        {
            TryHit(startPos, targetPos, animCtrl);
        });
        // 후딜
        yield return new WaitForSeconds(postDelay);
        moveTw = null;
        owner.StopMove();
        owner.SetRunning(false);
        owner.SetAttackCooldown();
        co = null;
    }

    private void TryHit(Vector2 startPos, Vector2 targetPos, AnimatorController animCtrl)
    {
        if (hitApplied) return;
        Vector2 dir = (targetPos - startPos).sqrMagnitude > 1e-4f
            ? (targetPos - startPos).normalized
            : animCtrl.LastDir.normalized;

        Vector2 hitPos = targetPos - dir * (hitForwardOffset * 0.2f); // 약간 뒤로 보정
        var hit = Physics2D.OverlapCircle(hitPos, hitRadius, playerMask);
        if (hit && hit.CompareTag("Player"))
        {
            var h = hit.GetComponent<DayPlayer>();
            if (h != null) h.TakeDamage(damage);
        }
        hitApplied = true;
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (owner == null) return;
        var animCtrl = owner.GetComponent<AnimatorController>();
        Vector2 dir = animCtrl ? animCtrl.LastDir.normalized : Vector2.right;

        Vector2 hitPos = owner.rb
            ? (Vector2)owner.rb.position + dir * hitForwardOffset
            : (Vector2)owner.transform.position + dir * hitForwardOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPos, hitRadius);
    }
#endif
}
