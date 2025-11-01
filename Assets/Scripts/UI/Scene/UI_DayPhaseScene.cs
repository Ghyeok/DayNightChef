using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DayPhaseScene : UI_Scene
{
    private bool _bound = false; // 중복 바인딩 방지
    private Coroutine _waitCo; // 대기 코루틴
    public enum GameObjects
    {
        Joystick,
    }

    public enum Texts
    {
        WeightText,
        GoldText,
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
        PauseButton,
        UpgradeButton,
        WeightButton,
        //테스트용 상점 버튼
        TestStoreButton,
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        //SetWeightText(); Update가 아닌 이벤트를 구독하여 필요시에만 호출하도록함
        SetHPBarImage();
        SetInteractionIcon();
        //SetGoldText(); 위와 동일
    }

    public override void Init()
    {
        if(_bound)
            return; // 중복 바인딩 방지
        Bind<GameObject>(typeof(GameObjects));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        GameObject interact = GetButton((int)Buttons.InteractionButton).gameObject;
        AddUIEvent(interact, InteractionButtonOnclicked, Define.UIEvent.Click);

        var weightBtn = GetButton((int)Buttons.WeightButton).gameObject;
        AddUIEvent(weightBtn, _ => UI_Inven.Show(), Define.UIEvent.Click);

        var TestStoreBtn = GetButton((int)Buttons.TestStoreButton).gameObject;
        AddUIEvent(TestStoreBtn, _ => UI_GroceryStorePopup.Show(), Define.UIEvent.Click);

        var UpgradeBtn = GetButton((int)Buttons.UpgradeButton).gameObject;
        AddUIEvent(UpgradeBtn, _ => UI_UpgradePopup.Show(), Define.UIEvent.Click);

        SetJoyStickToPlayer();

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
        if(_waitCo != null)
        {
            StopCoroutine(_waitCo);
            _waitCo = null;
        }
        if(InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshWeightText;
        if(GameManager.Instance != null)
            GameManager.Instance.OnGoldChanged -= RefreshGoldText;
    }

    private static float SafeRatio (float cur, float max) // 분모 0 방지
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
        InventoryManager.Instance.OnInventoryChanged -= RefreshWeightText;
        GameManager.Instance.OnGoldChanged -= RefreshGoldText;
        DayPhaseManager.OnMapLoadComplete -= SetJoyStickToPlayer;
        InventoryManager.Instance.OnInventoryChanged += RefreshWeightText;
        GameManager.Instance.OnGoldChanged += RefreshGoldText;
        DayPhaseManager.OnMapLoadComplete += SetJoyStickToPlayer;

        // UI 초기 갱신
        RefreshWeightText();
        RefreshGoldText();

        _waitCo = null;
    }

    public void SetJoyStickToPlayer()
    {
        PlayerController pc = DayPhasePlayerManager.Instance.dayPlayer.GetComponent<PlayerController>();
        var joystick = Get<GameObject>((int)GameObjects.Joystick);
        pc.joystick = joystick.GetComponent<VariableJoystick>();
        pc.joystick.Init();
    }

    public void RefreshWeightText()
    {
        var inv = InventoryManager.Instance;
        if (inv == null)
        {
            GetText((int)Texts.WeightText).text = "0/0";
            return;
        }

        float cur = inv.CurrentWeight;
        float max = inv.maxWeight;
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

    public void SetHPBarImage()
    {
        var dpm = DayPhasePlayerManager.Instance;
        if (dpm == null)
            return;

        float fill = SafeRatio(dpm.playerCurHP, dpm.playerMaxHP);
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
            button.image.sprite = UIManager.Instance.SetInteractionButton(DayPhaseManager.PlayerBehavior.Hunting);
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
}
