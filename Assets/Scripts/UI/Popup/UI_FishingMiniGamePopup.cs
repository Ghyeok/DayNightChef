using System;
using System.Collections;
using TMPro;
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
    [SerializeField] private bool useUnScaled = true;

    [Header("참조")]
    [SerializeField] private RectTransform ring; // 외곽 링
    [SerializeField] private Image progressBar; // 진행률 바
    [SerializeField] private Button exitButton;
    [SerializeField] private Image cooldownImage;
    
    [Header("회전")]
    public float rotateSpeed = 240f; // 도/초
    public float curRotation = 0f;

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
    [Range(1f, 100f)][SerializeField] private float missPenalty = 10f;

    [SerializeField] private float inputCooldown = 1f;
    private bool isPressCooldown = false;
    private float cooldownRemain = 0f;
    private Coroutine cooldownCo;
    public event Action OnPress; // 낚시 버튼이 눌리면 Invoke
    public event Action OnSuccess; // 진행률이 100이 되면 Invoke
    public event Action OnHit; // 성공 범위면 Invoke
    public event Action OnMiss; // 성공 범위 밖이면 Invoke

    public enum Buttons
    {
        FishingButton,
        ExitButton,
    }

    public enum Texts
    {
        ProgressText,
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
        Bind < TextMeshProUGUI>(typeof(Texts));

        GameObject exit = GetButton((int)Buttons.ExitButton).gameObject;
        AddUIEvent(exit, ExitButtonOnClicked, Define.UIEvent.Click);

        GameObject fish = GetButton((int)Buttons.FishingButton).gameObject;
        AddUIEvent(fish, FishingButtonOnClicked);

        curRotation = ring ? ring.localEulerAngles.z : 0f;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateProgressUI();

        if(cooldownRemain > 0f)
        {
            cooldownRemain -= Time.deltaTime;
            UpdateCooldownUI();
        }

        if (ring != null)
        {
            float dt = useUnScaled ? Time.unscaledDeltaTime : Time.deltaTime;
            curRotation = NormalizeDegrees(curRotation - rotateSpeed * dt);
            ring.localRotation = Quaternion.Euler(0f, 0f, curRotation);
        }
    }

    private void FishingButtonOnClicked(PointerEventData data)
    {
        OnPress?.Invoke();
    }

    private void HandlePress()
    {
        if (isPressCooldown) return;
        isPressCooldown = true;

        RandomRotationSpeed();
        Judge(curRotation);

        if(cooldownCo != null) StopCoroutine(cooldownCo);
        cooldownCo = StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        cooldownRemain =inputCooldown;

        if (useUnScaled) yield return new WaitForSecondsRealtime(inputCooldown);
        else yield return new WaitForSeconds(inputCooldown);
        isPressCooldown = false;
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
    /// 12시 = 0°, 시계방향(+) 기준으로
    /// 특정 각도가 범위 내에 있는지를 판단합니다.
    /// </summary>
    /// <param name="angle">판정할 각도</param>
    /// <param name="min">판정 최소 범위</param>
    /// <param name="max">판정 최대 범위</param>
    /// <returns>angle이 min과 max 사이에 있으면 true 반환</returns>
    private static bool IsInRange(float angle, float min, float max)
    {
        angle = NormalizeDegrees(angle);
        min = NormalizeDegrees(min);
        max = NormalizeDegrees(max);

        // min <= max: 일반 구간, min > max: 0°래핑 구간
        if (Mathf.Approximately(min, max)) return true; // 전체 원 의도 시
        return (min <= max) ? (angle >= min && angle <= max)
                            : (angle >= min || angle <= max);
    }

    private static bool IsInAnyRange(float angle, (float min, float max)[] ranges)
    {
        for (int i = 0; i < ranges.Length; i++)
            if (IsInRange(angle, ranges[i].min, ranges[i].max))
                return true;

        return false;
    }

    private void Judge(float degree)
    {
        Debug.Log($"현재 각도: {curRotation}");

        if (IsInAnyRange(degree, BonusRanges))
        {
            Debug.Log("보너스!");
            progress = Mathf.Min(100f, progress + gainBonusPerHit);
            OnHit?.Invoke();
        }
        else if (IsInAnyRange(degree, SuccessRanges))
        {
            Debug.Log("성공!");
            progress = Mathf.Min(100f, progress + gainPerHit);
            OnHit?.Invoke();
        }
        else
        {
            Debug.Log("실패!");
            progress = Mathf.Max(0f, progress - missPenalty);
            OnMiss?.Invoke();
        }

        if (progress >= 100f) OnSuccess?.Invoke();
    }

    private void UpdateProgressUI()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = Mathf.Clamp01(progress / 100f);
            GetText((int)Texts.ProgressText).text = $"진행률 {progress}%";
        }
    }

    private void UpdateCooldownUI()
    {
        if (cooldownImage != null)
        {
            float ratio = Mathf.Clamp01(cooldownRemain / inputCooldown);
            cooldownImage.fillAmount = ratio;
        }
    }

    private void RandomRotationSpeed()
    {
        rotateSpeed = UnityEngine.Random.Range(120f, 360f);
    }

    private void RandomRingRotation()
    {
        curRotation = UnityEngine.Random.Range(0f, 360f);
        if (ring != null)
        {
            ring.localRotation = Quaternion.Euler(0f, 0f, curRotation);
        }
    }

    private void SuccessFishing()
    {
        rotateSpeed = 0f;

        // 성공 문구와 함께 잡은 물고기 UI 표시 후 모든 팝업 닫음
        Item item = ItemManager.Instance.GetFishingItem(DayPhaseManager.Instance.curMapType, ItemType.Fish, PlayerStatsManager.Instance.GetLevel(StatType.FishingRod)); // 현재 낚싯대 레벨 가져오는 법?
        Debug.Log($"{item.item_name} 획득!");

        UIManager.Instance.ClosePopupUI(this);
    }

    private void ExitButtonOnClicked(PointerEventData data)
    {
        Debug.Log("ExitButton Clicked!");
        UIManager.Instance.ClosePopupUI(this);
    }

    private void OnEnable()
    {
        OnSuccess -= SuccessFishing;
        OnSuccess += SuccessFishing;

        OnHit -= RandomRingRotation;
        OnHit += RandomRingRotation;

        OnPress -= HandlePress;
        OnPress += HandlePress;
    }

    private void OnDisable()
    {
        OnSuccess -= SuccessFishing;
        OnHit -= RandomRingRotation;
        OnPress -= HandlePress;
    }
}
