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

    [Header("Debug")]
    [SerializeField] bool debugLogs = false;

    Vector2 _lastDir = Vector2.down; // 초기값 남쪽
    static readonly Vector2[] DIR8 = {
        new Vector2( 1, 0), new Vector2( 0.7071068f,  0.7071068f),
        new Vector2( 0, 1), new Vector2(-0.7071068f,  0.7071068f),
        new Vector2(-1, 0), new Vector2(-0.7071068f, -0.7071068f),
        new Vector2( 0,-1), new Vector2( 0.7071068f, -0.7071068f)
    };
    Vector2 Snap8(Vector2 v)
    {
        if (v.sqrMagnitude <= 1e-6f) return _lastDir;
        Vector2 n = v.normalized;
        int best = 0; float bestDot = -999f;
        for (int i = 0; i < DIR8.Length; i++)
        {
            float d = Vector2.Dot(n, DIR8[i]);
            if (d > bestDot) { bestDot = d; best = i; }
        }
        return DIR8[best];
    }

    //파라미터 이름 미리 해쉬
    static readonly int HashSpeed = Animator.StringToHash("Speed");
    static readonly int HashMoveX = Animator.StringToHash("MoveX");
    static readonly int HashMoveY = Animator.StringToHash("MoveY");
    static readonly int HashIsRunning = Animator.StringToHash("IsRunning");
    static readonly int HashAttack = Animator.StringToHash("Attack");
    static readonly int HashHit = Animator.StringToHash("Hit");
    static readonly int HashDie = Animator.StringToHash("Die");

    bool _lockFacing;
    float _lockUntil;
    Vector2 _lockedDir;

    public void Setup(Animator anim) { animator = anim; }
    public void LockFacingFor(float seconds, Vector2 dir)
    {
        _lockFacing = true;
        _lockUntil = Time.time + Mathf.Max(0f, seconds);
        _lockedDir = (dir.sqrMagnitude > 1e-6f) ? dir.normalized :
                     (_lockedDir.sqrMagnitude > 1e-6f ? _lockedDir : Vector2.right);
        _lastDir = _lockedDir;
        if (debugLogs) LogParams("[LockFacingFor]");
    }


    // 이동/방향 상태 갱신
    public void ApplyMovement(Vector2 velocity, bool isRunning)
    {
        float speed = velocity.magnitude;
        animator.SetFloat(HashSpeed, speed);
        animator.SetBool(HashIsRunning, isRunning);

        Vector2 dir;
        if (_lockFacing && Time.time < _lockUntil)
        {
            dir = _lockedDir; // 락 유지
        }
        else
        {
            if (_lockFacing && Time.time >= _lockUntil) _lockFacing = false;

            if (speed <= idleStickEps)
                dir = _lastDir;           // 정지 시 마지막 방향 유지
            else
            {
                dir = velocity.normalized; // 이동 중에만 갱신
                _lastDir = dir;
            }
        }

        if (clampUnit && dir.sqrMagnitude > 1e-6f) dir = dir.normalized;
        animator.SetFloat(HashMoveX, dir.x);
        animator.SetFloat(HashMoveY, dir.y);

        if (debugLogs) LogParams("[ApplyMovement]");
    }

    // 방향만 바꾸고 싶을 때 (공격 등)
    public void FaceTo(Vector2 aimOrDir)
    {
        if (aimOrDir.sqrMagnitude <= 1e-6f) return;
        var d = aimOrDir.normalized;
        if (_lockFacing) d = Snap8(d);    // 락 중엔 8방향 스냅
        _lastDir = d;
        if (_lockFacing) _lockedDir = d;

        animator.SetFloat(HashMoveX, d.x);
        animator.SetFloat(HashMoveY, d.y);

        if (debugLogs) LogParams("[FaceTo]");
    }

    // 단발 액션 트리거
    public void TriggerAttack()
    {
        animator.ResetTrigger(HashAttack);
        animator.SetTrigger(HashAttack);
        if (debugLogs) LogParams("[TriggerAttack]");
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
    public Vector2 LastDir => _lockFacing ? _lockedDir : _lastDir;
    void LogParams(string tag)
    {
        float sx = animator.GetFloat(HashSpeed);
        float mx = animator.GetFloat(HashMoveX);
        float my = animator.GetFloat(HashMoveY);
        bool run = animator.GetBool(HashIsRunning);
        Debug.Log($"{tag} t={Time.time:F3} | Speed={sx:F3} Move=({mx:F2},{my:F2}) Run={run} Lock={_lockFacing} Last=({_lastDir.x:F2},{_lastDir.y:F2})");
    }
}
