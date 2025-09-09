using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 1. (0도, 30도), (330도,360도), (150도, 210도) 가 성공 구간 -> 진행률 20퍼센트
/// 2. (10도, 350도) , (170도, 190도) 가 보너스 구간 -> 진행률 25퍼센트
/// 3. 나머지 구간은 실패 구간 -> 진행률 -5퍼센트
/// 원이 12시 방향을 기준으로 스페이스를 누른 순간 얼마나 돌아갔는지를 계산하여 성공 여부를 판단
/// </summary>
public class UI_FishingMiniGamePopup : UI_Popup
{
    [Header("참조")]
    [SerializeField] private RectTransform ring; // 외곽 링
    [SerializeField] private Image progressBar; // 진행률 바
    [SerializeField] private Button exitButton;
    
    [Header("회전")]
    public float rotateSpeed = 120f; // 도/초

    [Header("판정 & 진행률")]
    private static readonly (float min, float max)[] BonusRanges =
    {
    (0f,   10f),
    (170f, 190f),
    (350f, 359f),
    };

    private static readonly (float min, float max)[] SuccessRanges =
    {
    (0f,   30f),
    (150f, 210f),
    (330f, 359f),
    };

    [Range(0f, 100f)][SerializeField] private float progress = 0f;
    [Range(1f, 100f)][SerializeField] private float gainPerHit = 20f;
    [Range(1f, 100f)][SerializeField] private float gainBonusPerHit = 25f;
    [Range(1f, 100f)][SerializeField] private float missPenalty = 5f;

    public event Action OnSuccess; // 진행률이 100이 되면 Invoke
    public event Action OnHit; // 성공 범위면 Invoke
    public event Action OnMiss; // 성공 범위 밖이면 Invoke

    public enum Buttons
    {
        ExitButton,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();   
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));

        GameObject exit = GetButton((int)Buttons.ExitButton).gameObject;
        AddUIEvent(exit, ExitButtonOnClicked, Define.UIEvent.Click);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

        }
    }

    /// <summary>
    /// 임의의 각도를 [0, 360) 범위로 정규화 합니다.
    /// </summary>
    /// <param name="deg">정규화 할 각도(도 단위)</param>
    private static float NormalizeDegrees(float deg)
    {
        deg %= 360f;
        if (deg < 0) deg += 360f;
        return deg;
    }

    /// <summary>
    /// 12시=0°, 시계방향(+) 기준으로
    /// 시작각 start에서 arcLen(도)만큼 펼쳐진 부채꼴 안에 angle(도)이 포함되는지 검사합니다.
    /// 각도는 모두 [0,360)으로 정규화하며, 구간이 0도를 넘는 경우에도 올바르게 처리합니다.
    /// </summary>
    /// <param name="angle">판정할 각도</param>
    /// <param name="start">부채꼴 시작각</param>
    /// <param name="arcLen">부채꼴 호 길이(도)</param>
    /// <returns>angle이 부채꼴 안이면 true, 아니면 false 반환</returns>
    private static bool IsInArcRange(float angle, float start, float arcLen)
    {
        angle = NormalizeDegrees(angle);

        float a = NormalizeDegrees(start);
        float b = NormalizeDegrees(start + arcLen);

        if (arcLen <= 0f) return false;
        if (arcLen >= 360f) return true;

        return (a <= b) ? (angle >= a && angle <= b) : (angle >= a || angle <= b);
    }

    private void Judge(float degree)
    {
        
    }

    private void UpdateProgressUI()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = Mathf.Clamp01(progress / 100f);
        }
    }

    private void ExitButtonOnClicked(PointerEventData data)
    {
        Debug.Log("ExitButton Clicked!");
        UIManager.Instance.ClosePopupUI(this);
    }
}
