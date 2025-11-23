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
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sr;

    [Header("Interaction")]
    [SerializeField, Min(0.2f)] float chefPickupRadius = 1.2f; // 셰프 반경
    [SerializeField, Min(0.2f)] float serveRayDIstance = 2.0f; // 손님 정면 레이 거리
    [SerializeField] LayerMask chefLayer;
    [SerializeField] LayerMask customerLayer;

    [Header("비쥬얼")]
    [SerializeField] SpriteRenderer carryIcon;
    [SerializeField] Vector3 carryIconOffset = new Vector3(0, 0.8f, 0);

    private Recipe carriedRecipe = null; // 들고있는 요리
    [SerializeField] private float interactCooldown = 0.1f;
    private float _lastInteractTime = -999f;

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

        UpdateCarryIcon();
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
    // 요리 관련
    public void OnInteract()
    {
        if (Time.time - _lastInteractTime < interactCooldown)
            return;
        _lastInteractTime = Time.time;
        // 1. 들고있는 요리가 없으면 셰프에서 수령
        if (carriedRecipe == null)
        {
            if (TryTakeFromChef()) return;
        }

        // 2. 들고 있는 요리가 있으면 정면 손님에게 서빙 시도
        if (carriedRecipe != null)
        {
            if (TryServeToFrontCustomer()) return;
        }
    }

    private bool TryTakeFromChef()
    {
        if (carriedRecipe != null)
            return false;
        Debug.Log(" 셰프 감지 시작");
        // 주변 원형 감지로 셰프 확인
        var hits = Physics2D.OverlapCircleAll(transform.position, chefPickupRadius, chefLayer);
        if (hits == null || hits.Length == 0) return false;
        Debug.Log("셰프 감지 성공");

        // 레디큐에서 1개 수령
        var sales = SalesManager.Instance;
        if (sales == null) return false;

        if (sales.TryPopReadyOrder(out var order))
        {
            if (order == null || order.recipe == null)
                return false;

            if (carriedRecipe != null)
                return false;

            carriedRecipe = order.recipe;
            UpdateCarryIcon();
            //TODO 이펙트 or 사운드
            return true;
        }
        return false;
    }

    private bool TryServeToFrontCustomer()
    {
        Vector2 origin = transform.position;
        Vector2 dir = lookDir.sqrMagnitude > 1e-6f? lookDir.normalized : Vector2.zero;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, serveRayDIstance, customerLayer);

        if (hit.collider == null) return false;
        var customer = hit.collider.GetComponent<Customer>();
        if(customer == null) return false;

        // 손님에게 서빙 시도
        customer.TryServe(carriedRecipe);

        // 서빙 후 빈 손으로 전환
        carriedRecipe = null;
        UpdateCarryIcon();
        return true;
    }

    private void UpdateCarryIcon()
    {
        if (carryIcon == null) return;
        if (carriedRecipe == null)
        {
            carryIcon.enabled = false;
            carryIcon.sprite = null;
        }
        else
        {
            carryIcon.enabled = true;
            carryIcon.transform.localPosition = carryIconOffset;
            carryIcon.sprite = carriedRecipe.recipe_image;
        }
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
