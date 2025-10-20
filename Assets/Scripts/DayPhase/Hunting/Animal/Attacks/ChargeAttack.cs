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
    [SerializeField] private bool stopOnHit = false; // 타격 시 돌진 멈춤 여부

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
    private bool _chargeArmed;

    private RigidbodyConstraints2D _prevConstraints;

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
        if (owner && owner.rb) owner.rb.constraints = _prevConstraints;
        _busy = false;
        _chargeArmed = false;
    }

    private IEnumerator CoCharge()
    {
        if (moveTw != null && moveTw.IsActive()) moveTw.Kill();
        moveTw = null;
        DOTween.Kill(owner.rb, complete: false);
        DOTween.Kill(owner.transform, complete: false);

        hitApplied = false;

        owner.SetMovementLock(true);
        owner.StopMove();
        owner.SetRunning(false);
        owner.rb.linearVelocity = Vector2.zero;
        owner.rb.angularVelocity = 0f;

        _prevConstraints = owner.rb.constraints;
        owner.rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        
        var animCtrl = owner.GetComponent<AnimatorController>();
        Vector2 aim = owner.target ? (Vector2)owner.target.position - owner.rb.position : animCtrl.LastDir;
        if (aim.sqrMagnitude <= 1e-6f) aim = animCtrl.LastDir;

        owner.FaceTo(aim);
        _chargeArmed = true;
        animCtrl.TriggerAttack();
        animCtrl.LockFacingFor(preDelay + attackDuration + postDelay + 0.05f, aim.normalized);

        float timeout = Mathf.Max(0.05f, preDelay + 0.35f);
        float t0 = Time.time;
        while (_chargeArmed && (Time.time - t0) < timeout) yield return null;

        if (_chargeArmed)
        {
            _chargeArmed = false;
            StartChargeTween();
        }

        yield return new WaitForSeconds(attackDuration);
        if (postDelay > 0f)
            yield return new WaitForSeconds(postDelay);

        owner.StopMove();
        owner.SetMovementLock(false);
        owner.SetRunning(false);
        owner.rb.constraints = _prevConstraints;
        owner.SetAttackCooldown();
        moveTw = null;
        co = null;
    }

    public void AnimEvent_ChargeStart()
    {
        if (!_chargeArmed) return;
        _chargeArmed = false;
        StartChargeTween();
    }
    private void StartChargeTween()
    {
        owner.rb.constraints = _prevConstraints;
        owner.rb.linearVelocity = Vector2.zero;
        owner.rb.angularVelocity = 0f;
        owner.StartCoroutine(CoStartChargeTweenAfterFixed());

    }

    private IEnumerator CoStartChargeTweenAfterFixed()
    {
        yield return new WaitForFixedUpdate();
        var animCtrl = owner.GetComponent<AnimatorController>();
        startPos = owner.rb.position;

        Vector2 curAim = owner.target
            ? (Vector2)owner.target.position - startPos
            : animCtrl.LastDir;
        if (curAim.sqrMagnitude <= 1e-6f) curAim = animCtrl.LastDir;

        targetPos = owner.target
            ? (Vector2)owner.target.position
            : startPos + curAim.normalized;


        moveTw = owner.rb.DOMove(targetPos, attackDuration)
           .SetEase(Ease.InQuart)
           .SetUpdate(UpdateType.Fixed)
           .OnUpdate(() =>
           {
               if (!hitApplied)
               {
                   Vector2 dir = (targetPos - startPos).sqrMagnitude > 1e-4f
                       ? (targetPos - startPos).normalized
                       : animCtrl.LastDir.normalized;

                   Vector2 hitPos = owner.rb.position + dir * hitForwardOffset;
                   var hit = Physics2D.OverlapCircle(hitPos, hitRadius, playerMask);
                   var dp = hit ? hit.GetComponentInParent<DayPlayer>() : null;
                   if (dp != null)
                   {
                       dp.TakeDamage(damage);
                       hitApplied = true;

                       if (stopOnHit && moveTw != null && moveTw.IsActive())
                           moveTw.Kill(); // 옵션: 히트 즉시 돌진 중단
                   }
               }
           })
           .OnComplete(() =>
           {
               if (!hitApplied) TryHit(startPos, targetPos, animCtrl);
           });
    }
    private void TryHit(Vector2 startPos, Vector2 targetPos, AnimatorController animCtrl)
    {
        if (hitApplied) return;
        Vector2 dir = (targetPos - startPos).sqrMagnitude > 1e-4f
            ? (targetPos - startPos).normalized
            : animCtrl.LastDir.normalized;

        Vector2 hitPos = targetPos - dir * (hitForwardOffset * 0.2f); // 약간 뒤로 보정
        var hit = Physics2D.OverlapCircle(hitPos, hitRadius, playerMask);
        var dp = hit ? hit.GetComponentInParent<DayPlayer>() : null;
        if (dp != null) dp.TakeDamage(damage);
        hitApplied = true;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
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
