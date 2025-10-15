using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    public VariableJoystick joystick; // 없으면 키보드로 폴백

    private Rigidbody2D rb;
    private Animator anim;

    // 캐시된 입력 벡터(정규화)
    private Vector2 inputDir = Vector2.zero;

    // 애니메이터 파라미터 유무 캐시
    private int hashIsWalk, hashIsAttack, hashSpeed, hashMoveX, hashMoveY;
    private bool hasIsWalk, hasIsAttack, hasSpeed, hasMoveX, hasMoveY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // 2D 탑다운 기본 세팅
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 파라미터 해시 & 존재 여부 캐시
        hashIsWalk = Animator.StringToHash("isWalk");
        hashIsAttack = Animator.StringToHash("isAttack");
        hashSpeed = Animator.StringToHash("Speed");
        hashMoveX = Animator.StringToHash("MoveX");
        hashMoveY = Animator.StringToHash("MoveY");

        hasIsWalk = HasParam(anim, hashIsWalk);
        hasIsAttack = HasParam(anim, hashIsAttack);
        hasSpeed = HasParam(anim, hashSpeed);
        hasMoveX = HasParam(anim, hashMoveX);
        hasMoveY = HasParam(anim, hashMoveY);
    }

    private void Update()
    {
        ReadInput();
        UpdateAnimator();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        float speed = GetMoveSpeed();
        Vector2 next = rb.position + inputDir * speed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }

    private void ReadInput()
    {
        if (joystick != null)
        {
            float h = joystick.Horizontal;
            float v = joystick.Vertical;
            inputDir = new Vector2(h, v).normalized;
        }
        else // 조이스틱 없으면 키보드로
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            inputDir = new Vector2(h, v).normalized;
        }
    }

    private float GetMoveSpeed()
    {
        // DayPhasePlayerManager를 우선 사용, 없거나 아직 초기화 전이면 fallback
        var mgr = DayPhasePlayerManager.Instance;
        return mgr.playerMoveSpeed;
    }

    private void UpdateAnimator()
    {
        float speedVal = inputDir.magnitude * GetMoveSpeed();

        if (hasIsWalk) anim.SetBool(hashIsWalk, speedVal > 0.01f);
        if (hasSpeed) anim.SetFloat(hashSpeed, speedVal);
        if (hasMoveX) anim.SetFloat(hashMoveX, inputDir.x);
        if (hasMoveY) anim.SetFloat(hashMoveY, inputDir.y);
    }

    private void HandleAttack()
    {
        bool attacking = Input.GetKey(KeyCode.E);
        if (hasIsAttack) anim.SetBool(hashIsAttack, attacking);
    }

    private bool HasParam(Animator a, int hash)
    {
        foreach (var p in a.parameters)
            if (p.nameHash == hash) return true;
        return false;
    }
}
