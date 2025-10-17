using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    public VariableJoystick joystick;   // 조이스틱만 사용
    [SerializeField] float deadZone = 0.15f;   // 드리프트 억제
    [SerializeField] bool useEightDirections = true; // 8방향 스냅(해제 시 4방향)

    private Rigidbody2D rb;
    private Animator anim;

    private Vector2 inputDir = Vector2.zero; // 정규화된 이동 입력
    private Vector2 lookDir = Vector2.up; // 마지막 바라봄(정지 시 유지)

    // 애니메이터 파라미터
    private int hashIsWalk, hashIsAttack, hashSpeed, hashMoveX, hashMoveY;
    private bool hasIsWalk, hasIsAttack, hasSpeed, hasMoveX, hasMoveY;

    private bool isAttacking = false;

    private static readonly Vector2[] Octant = new Vector2[]
    {
        new Vector2( 1f,  0f),    // 동 (0)
        new Vector2( 0.7071f,  0.7071f), // 북동 (1)
        new Vector2( 0f,  1f),    // 북 (2)
        new Vector2(-0.7071f,  0.7071f), // 북서 (3)
        new Vector2(-1f,  0f),    // 서 (4)
        new Vector2(-0.7071f, -0.7071f), // 남서 (5)
        new Vector2( 0f, -1f),    // 남 (6)
        new Vector2( 0.7071f, -0.7071f), // 남동 (7)
    };

    private void Awake()
    {
        Physics2D.IgnoreCollision(GetComponent<BoxCollider2D>(), GetComponentsInChildren<BoxCollider2D>()[1]); // 충돌 방지
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 해쉬 값으로 가져와서 빠름
        hashIsWalk = Animator.StringToHash("isWalk");
        hashIsAttack = Animator.StringToHash("isAttack");
        hashSpeed = Animator.StringToHash("Speed");
        hashMoveX = Animator.StringToHash("MoveX");
        hashMoveY = Animator.StringToHash("MoveY");

        // parameter로 존재하는지?
        hasIsWalk = HasParam(anim, hashIsWalk);
        hasIsAttack = HasParam(anim, hashIsAttack);
        hasSpeed = HasParam(anim, hashSpeed);
        hasMoveX = HasParam(anim, hashMoveX);
        hasMoveY = HasParam(anim, hashMoveY);
    }

    private void Update()
    {
        ReadJoystick();
        UpdateFacing();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        float speed = GetMoveSpeed();
        Vector2 next = rb.position + inputDir * speed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }

    private void ReadJoystick()
    {
        if (joystick == null) { inputDir = Vector2.zero; return; }

        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        Vector2 raw = new Vector2(h, v);
        // 데드존 처리, 일정 범위 이하의 입력은 0으로 간주하여 미세한 움직임을 방지한다
        if (raw.sqrMagnitude < deadZone * deadZone) { inputDir = Vector2.zero; return; }

        inputDir = raw.normalized;
    }

    private float GetMoveSpeed()
    {
        var mgr = DayPhasePlayerManager.Instance;
        return (mgr != null) ? mgr.playerMoveSpeed : 3.5f; // 안전 폴백
    }

    private void UpdateFacing()
    {
        if (inputDir.sqrMagnitude > 1e-6f) // 앱실론 보정
        {
            lookDir = useEightDirections ? Quantize8(inputDir) : Quantize4(inputDir);
        }
        // 정지 시: 기존 lookDir 유지
    }

    private Vector2 Quantize8(Vector2 v)
    {
        float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        if (ang < 0f) ang += 360f;
        int idx = Mathf.RoundToInt(ang / 45f) % 7;
        return Octant[idx];
    }

    private Vector2 Quantize4(Vector2 v)
    {
        if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(v.y));
    }

    private void UpdateAnimator()
    {
        float speedVal = inputDir.magnitude * GetMoveSpeed();
        Vector2 animDir = (speedVal > 0.01f)
            ? (useEightDirections ? Quantize8(inputDir) : Quantize4(inputDir))
            : lookDir;

        if (hasIsWalk) anim.SetBool(hashIsWalk, speedVal > 0.01f);
        if (hasSpeed) anim.SetFloat(hashSpeed, speedVal);
        if (hasMoveX) anim.SetFloat(hashMoveX, animDir.x);
        if (hasMoveY) anim.SetFloat(hashMoveY, animDir.y);
        if (hasIsAttack) anim.SetBool(hashIsAttack, isAttacking);
    }

    // UI 버튼(누름/뗌)에서 호출
    public void BeginAttack() { isAttacking = true; }
    public void EndAttack() { isAttacking = false; }

    private bool HasParam(Animator a, int hash)
    {
        foreach (var p in a.parameters)
            if (p.nameHash == hash) return true;
        return false;
    }
}
