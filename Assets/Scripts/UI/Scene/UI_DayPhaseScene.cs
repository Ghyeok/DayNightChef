using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_DayPhaseScene : UI_Scene
{
    private bool _bound = false; // 중복 바인딩 방지
    private Coroutine _waitCo; // 대기 코루틴

    [Header("Get Item Popup")]
    [SerializeField] private ShowGetItem showGetItem;
    private Coroutine _itemPopupCo;
    public enum GameObjects
    {
        Joystick,
    }

    public enum Texts
    {
        WeightText,
        GoldText,
        WeekText,
        FeeText,
    }

    public enum Images
    {
        HeartImage,
        HPBarImage,
        HPBackBarImage,
        GoldImage,
    }

    public enum Buttons
    {
        InteractionButton,
        UpgradeButton,
        WeightButton,
        NightPhaseButton,
        PauseButton,
        StoreButton,
        WarehouseButton,
        RecipeButton,
        StatButton,
        SettingButton,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        SetInteractionIcon();
    }

    public override void Init()
    {
        if (_bound)
            return; // 중복 바인딩 방지

        Bind<GameObject>(typeof(GameObjects));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        GameObject interact = GetButton((int)Buttons.InteractionButton).gameObject;
        AddUIEvent(interact, InteractionButtonOnclicked, Define.UIEvent.Click);

        var weightBtn = GetButton((int)Buttons.WeightButton).gameObject;
        AddUIEvent(weightBtn, _ => UI_Inven.Show(), Define.UIEvent.Click);

        var StoreBtn = GetButton((int)Buttons.StoreButton).gameObject;
        AddUIEvent(StoreBtn, _ => UI_GroceryStorePopup.Show(), Define.UIEvent.Click);

        var UpgradeBtn = GetButton((int)Buttons.UpgradeButton).gameObject;
        AddUIEvent(UpgradeBtn, _ => UI_UpgradePopup.Show(), Define.UIEvent.Click);

        var NightBtn = GetButton((int)Buttons.NightPhaseButton).gameObject;
        AddUIEvent(NightBtn, NightPhaseButtonOnclicked, Define.UIEvent.Click);

        var warehouseBtn = GetButton((int)Buttons.WarehouseButton).gameObject;
        AddUIEvent(warehouseBtn, _ => UI_WarehousePopup.Show(), Define.UIEvent.Click);

        var recipeBtn = GetButton((int)Buttons.RecipeButton).gameObject;
        AddUIEvent(recipeBtn, _ => UI_RecipePopup.Show(), Define.UIEvent.Click);

        var statBtn = GetButton((int)Buttons.StatButton).gameObject;
        AddUIEvent(statBtn, _ => UI_StatPopup.Show(), Define.UIEvent.Click);

        var settingBtn = GetButton((int)Buttons.SettingButton).gameObject;
        AddUIEvent(settingBtn, _ => UI_SettingPopup.Show(), Define.UIEvent.Click);
        // AddUIEvent

        _bound = true;
    }

    private void OnEnable()
    {
        if (_waitCo == null)
        {
            _waitCo = StartCoroutine(WaitAndBind());
        }
    }

    private void OnDisable()
    {
        if (_waitCo != null)
        {
            StopCoroutine(_waitCo);
            _waitCo = null;
        }
        if (GameManager.Instance != null)
            GameManager.Instance.OnGoldChanged -= RefreshGoldText;

        if (DayPhasePlayerManager.Instance != null)
        {
            DayPhasePlayerManager.Instance.OnSnapshotUpdated -= RefreshHPBar;
            DayPhasePlayerManager.Instance.OnSnapshotUpdated -= RefreshWeightText;
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryGetted -= HandleInventoryGetted;

        if (_itemPopupCo != null)
        {
            StopCoroutine(_itemPopupCo);
            _itemPopupCo = null;
        }
    }

    private static float SafeRatio(float cur, float max) // 분모 0 방지
    {
        if (max <= 0f || float.IsNaN(max) || float.IsInfinity(max)) return 0f;
        float r = cur / max;
        if (!float.IsFinite(r)) return 0f;
        return Mathf.Clamp01(r);
    }

    private IEnumerator WaitAndBind()
    {
        while (!_bound) yield return null; // 씬 바인드까지 대기

        // 인벤토리 매니저 Init까지 대기
        while (InventoryManager.Instance == null || !InventoryManager.Instance.IsInitialized) yield return null;

        // 게임매니저 Init까지 대기
        while (GameManager.Instance == null) yield return null;

        // HP 준비 대기 : 플레이어 스텟이 준비될때까지 대기
        while (DayPhasePlayerManager.Instance == null || DayPhasePlayerManager.Instance.playerMaxHP <= 0f)
            yield return null;

        // 이벤트 구독

        GameManager.Instance.OnGoldChanged -= RefreshGoldText;
        GameManager.Instance.OnGoldChanged += RefreshGoldText;

        DayPhaseManager.OnMapLoadComplete -= SetJoyStickToPlayer;
        DayPhaseManager.OnMapLoadComplete += SetJoyStickToPlayer;

        DayPhasePlayerManager.Instance.OnSnapshotUpdated -= RefreshHPBar;
        DayPhasePlayerManager.Instance.OnSnapshotUpdated += RefreshHPBar;

        DayPhasePlayerManager.Instance.OnSnapshotUpdated -= RefreshWeightText;
        DayPhasePlayerManager.Instance.OnSnapshotUpdated += RefreshWeightText;

        // 인벤토리 획득 이벤트 구독
        InventoryManager.Instance.OnInventoryGetted -= HandleInventoryGetted;
        InventoryManager.Instance.OnInventoryGetted += HandleInventoryGetted;

        // UI 초기 갱신
        RefreshGoldText();
        RefreshWeightText(DayPhasePlayerManager.Instance.Snapshot);
        RefreshHPBar(DayPhasePlayerManager.Instance.Snapshot);

        SetWeekText();
        SetJoyStickToPlayer();

        _waitCo = null;
    }

    public void SetJoyStickToPlayer()
    {
        // 1. 플레이어 매니저와 플레이어 준비 상태 확인
        if (DayPhasePlayerManager.Instance == null || !DayPhasePlayerManager.Instance.IsPlayerReady)
        {
            Debug.LogError("SetJoyStickToPlayer: PlayerManager가 준비되지 않았습니다!");
            return;
        }

        // 2. dayPlayer 객체가 null이거나 파괴되었는지 확인
        GameObject player = DayPhasePlayerManager.Instance.dayPlayer;
        if (player == null) // C# null이거나 Unity의 'dead reference'인지 확인
        {
            Debug.LogError("SetJoyStickToPlayer: dayPlayer가 null이거나 파괴되었습니다!");
            return;
        }

        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            var joystick = Get<GameObject>((int)GameObjects.Joystick);
            if (joystick != null)
            {
                pc.joystick = joystick.GetComponent<VariableJoystick>();
                pc.joystick.Init();
            }
        }
        else
        {
            Debug.LogError("플레이어에서 조이스틱 타겟 컴포넌트를 찾지 못했습니다!");
        }
    }

    public void RefreshWeightText(PlayerRuntimeSnapshot snapshot)
    {
        float cur = snapshot.CurBagWeight;
        float max = snapshot.MaxBagWeight;
        if (max <= 0f) max = 1f;

        cur = Mathf.Clamp(cur, 0f, max);
        GetText((int)Texts.WeightText).text = $"{cur:0.#}/{max:0.#}";
    }

    public void RefreshGoldText()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            GetText((int)Texts.GoldText).text = "0G";
            return;
        }

        GetText((int)Texts.GoldText).text = $"{gm.totalGold}G";
    }

    private void RefreshHPBar(PlayerRuntimeSnapshot snapshot)
    {
        float fill = SafeRatio(snapshot.CurHP, snapshot.MaxHP);
        var img = GetImage((int)Images.HPBarImage);
        if (img != null)
            img.fillAmount = fill;
    }

    private void SetInteractionIcon()
    {
        Button button = GetButton((int)Buttons.InteractionButton);
        var interact = DayPhasePlayerManager.Instance.currentInteract;
        if (interact == null)
        {
            button.image.sprite = UIManager.Instance.SetInteractionButton(PlayerBehavior.Hunting);
            button.image.enabled = true;
            return;
        }
        button.image.enabled = true;
        button.image.sprite = UIManager.Instance.SetInteractionButton(interact.GetBehaviorType());
    }

    public void InteractionButtonOnclicked(PointerEventData data)
    {
        var dpm = DayPhasePlayerManager.Instance;
        if (dpm.dayPlayer == null) return;
        if (dpm.currentInteract == null)
        {
            var pc = dpm.dayPlayer.GetComponent<PlayerController>();
            if (pc != null) pc.TryHunt();
        }
        else
        {
            dpm.currentInteract.Interact(dpm.dayPlayer);
        }
    }

    public async void NightPhaseButtonOnclicked(PointerEventData data)
    {
        bool result = await UI_ConfirmPopup.ShowAsync(
            info: "밤 페이즈로 이동하시겠습니까?",
            left: "예",
            right: "아니오"
            );

        if (result)
        {
            SaveManager.Instance.SaveGame();
            SceneLoader.Instance.LoadScene("NightPhaseScene", LoadSceneMode.Single);
        }
        else
        {
            UIManager.Instance.ClosePopupUI();
        }
    }

    public void SetWeekText()
    {
        GetText((int)Texts.FeeText).gameObject.SetActive(false);
        GetText((int)Texts.WeekText).text = $"{GameManager.Instance.currentWeek}주차";
        if ((GameManager.Instance.currentWeek % 4 - 3) == 0)
        {
            GetText((int)Texts.FeeText).gameObject.SetActive(true);
        }
    }

    private void HandleInventoryGetted(Item item, int count)
    {
        if (showGetItem == null || item == null || count <= 0)
            return;

        // 이전 코루틴 돌고 있으면 정지
        if (_itemPopupCo != null)
        {
            StopCoroutine(_itemPopupCo);
            _itemPopupCo = null;
        }

        // 텍스트 갱신 + 활성화
        showGetItem.gameObject.SetActive(true);
        showGetItem.ShowItem(item, count);

        // 일정 시간 뒤 자동으로 숨기기
        _itemPopupCo = StartCoroutine(HideGetItemPopup());
    }

    private IEnumerator HideGetItemPopup()
    {
        yield return new WaitForSeconds(1.5f);
        if (showGetItem != null)
            showGetItem.gameObject.SetActive(false);

        _itemPopupCo = null;
    }
}
