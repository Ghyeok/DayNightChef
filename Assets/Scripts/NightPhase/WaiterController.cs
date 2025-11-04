using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof(SpriteRenderer))]
public class WaiterController : MonoBehaviour
{
    [Header("Input")]
    public VariableJoystick joystick;   // 조이스틱만 사용
    [SerializeField] float deadZone = 0.15f;   // 드리프트 억제
    [SerializeField] bool useEightDirections = true; // 8방향 스냅(해제 시 4방향)
    [SerializeField] private float moveSpeed = 1f;

    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sr;

    // 애니메이터 파라미터
    private int hashIsWalk, hashSpeed, hashMoveX, hashMoveY;
    private bool hasIsWalk, hasSpeed, hasMoveX, hasMoveY;

    private Vector2 inputDir = Vector2.zero; // 정규화된 이동 입력
    private Vector2 lookDir = Vector2.up; // 마지막 바라봄(정지 시 유지)

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
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // 해쉬 값으로 가져와서 빠름
        hashIsWalk = Animator.StringToHash("isWalk");
        hashSpeed = Animator.StringToHash("Speed");
        hashMoveX = Animator.StringToHash("MoveX");
        hashMoveY = Animator.StringToHash("MoveY");

        // parameter로 존재하는지?
        hasIsWalk = HasParam(anim, hashIsWalk);
        hasSpeed = HasParam(anim, hashSpeed);
        hasMoveX = HasParam(anim, hashMoveX);
        hasMoveY = HasParam(anim, hashMoveY);
    }

    private void FixedUpdate()
    {
        Vector2 next = rb.position + inputDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }

    void Update()
    {
        ReadJoystick();
        UpdateFacing();
        UpdateAnimator();
    }

    private bool HasParam(Animator a, int hash)
    {
        foreach (var p in a.parameters)
            if (p.nameHash == hash) return true;
        return false;
    }

    private void UpdateAnimator()
    {
        float speedVal = inputDir.magnitude * moveSpeed;
        Vector2 animDir = (speedVal > 0.01f)
            ? (useEightDirections ? Quantize8(inputDir) : Quantize4(inputDir))
            : lookDir;

        if (animDir.x > 0.1f) // 오른쪽을 볼 때
        {
            sr.flipX = false;
        }
        else if (animDir.x < -0.1f) // 왼쪽을 볼 때
        {
            sr.flipX = true;
        }

        float animatorMoveX = Mathf.Abs(animDir.x);

        if (hasIsWalk) anim.SetBool(hashIsWalk, speedVal > 0.01f);
        if (hasSpeed) anim.SetFloat(hashSpeed, speedVal);
        if (hasMoveX) anim.SetFloat(hashMoveX, animatorMoveX);
        if (hasMoveY) anim.SetFloat(hashMoveY, animDir.y);
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
        int idx = Mathf.RoundToInt(ang / 45f) % 8;
        return Octant[idx];
    }

    private Vector2 Quantize4(Vector2 v)
    {
        if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(v.y));
    }
}
