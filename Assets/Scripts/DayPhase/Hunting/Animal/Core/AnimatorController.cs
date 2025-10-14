using UnityEngine;
/// <summary>
/// animal 애니메이터 드라이버
/// - Idle/Walk/Run : Speed, MoveX, MoveY, IsRunning
/// - Attack/Hit/Die : Trigger
/// 8방향은 MoveX/MoveY로 Blend Tree 사용
/// </summary>
[DisallowMultipleComponent]
public class AnimatorController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator animator;

    [Header("조절값")]
    [SerializeField] float idleStickEps = 1e-4f; // 정지 판정 임계값
    [SerializeField] bool clampUnit = true; // MoveX/Y를 단위벡터로 고정할지

    Vector2 _lastDir = Vector2.down; // 초기값 남쪽

    //파라미터 이름 미리 해쉬
    static readonly int HashSpeed = Animator.StringToHash("Speed");
    static readonly int HashMoveX = Animator.StringToHash("MoveX");
    static readonly int HashMoveY = Animator.StringToHash("MoveY");
    static readonly int HashIsRunning = Animator.StringToHash("IsRunning");

    static readonly int HashAttack = Animator.StringToHash("Attack");
    static readonly int HashHit = Animator.StringToHash("Hit");
    static readonly int HashDie = Animator.StringToHash("DIe");

    public void Setup(Animator anim) { animator = anim; }

    // 이동/방향 상태 갱신
    public void ApplyMovement(Vector2 velocity, bool isRunning)
    {
        float speed = velocity.magnitude;
        animator.SetFloat(HashSpeed, speed);
        animator.SetBool(HashIsRunning, isRunning);

        Vector2 dir;
        if (speed <= idleStickEps)
        {
            dir = _lastDir;
        }
        else
        {
            dir = velocity.normalized;
            _lastDir = dir;
        }

        if (clampUnit && dir.sqrMagnitude > 1e-4f)
        {
            dir = dir.normalized;
        }

        animator.SetFloat(HashMoveX, dir.x);
        animator.SetFloat(HashMoveY, dir.y);
    }

    // 방향만 바꾸고 싶을 때 (공격 등)
    public void FaceTo(Vector2 aim)
    {
        if (aim.sqrMagnitude <= 1e-4f) return;
        Vector2 dir = aim.normalized;
        _lastDir = dir;
        animator.SetFloat(HashMoveX, dir.x);
        animator.SetFloat(HashDie, dir.y);
    }

    // 단발 액션 트리거
    public void TriggerAttack()
    {
        animator.ResetTrigger(HashAttack);
        animator.SetTrigger(HashAttack);
    }

    public void TriggerHit()
    {
        animator.ResetTrigger(HashHit);
        animator.SetTrigger(HashHit);
    }

    public void TriggerDie()
    {
        animator.ResetTrigger(HashDie);
        animator.SetTrigger(HashDie);
    }

    // 외부에서 마지막 방향 호출용
    public Vector2 LastDir => _lastDir;
}
