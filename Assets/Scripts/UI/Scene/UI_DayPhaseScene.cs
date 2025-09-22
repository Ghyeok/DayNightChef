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
        SetGoldText();
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

        SetJoyStickToPlayer();

        _bound = true;
    }

    private void OnEnable()
    {
        _waitCo = StartCoroutine(InvWaitAndBind());
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
    }

    private IEnumerator InvWaitAndBind()
    {
        while (!_bound) yield return null; // 씬 바인드까지 대기
        while (InventoryManager.Instance == null || !InventoryManager.Instance.IsInitialized) yield return null;
        // 인벤토리 매니저 Init까지 대기
        InventoryManager.Instance.OnInventoryChanged += RefreshWeightText;
        RefreshWeightText();
    }

    public void SetJoyStickToPlayer()
    {
        PlayerController pc = DayPhasePlayerManager.Instance.dayPlayer.GetComponent<PlayerController>();
        var joystick = Get<GameObject>((int)GameObjects.Joystick);
        pc.joystick = joystick.GetComponent<VariableJoystick>();
    }

    public void RefreshWeightText()
    {
        float cur = InventoryManager.Instance?.CurrentWeight ?? 0f;
        float max = InventoryManager.Instance?.maxWeight ?? 0f;
        GetText((int)Texts.WeightText).text = $"{cur:0.#}/{max:0.#}";
    }

    public void SetGoldText()
    {
        GetText((int)Texts.GoldText).text = $"{GameManager.Instance.totalGold}" + "G";
    }

    public void SetHPBarImage()
    {
        GetImage((int)Images.HPBarImage).fillAmount = DayPhasePlayerManager.Instance.playerCurHP / DayPhasePlayerManager.Instance.playerMaxHP;
    }

    private void SetInteractionIcon()
    {
        Button button = GetButton((int)Buttons.InteractionButton);
        var interact = DayPhasePlayerManager.Instance.currentInteract;

        if(interact == null)
        {
            button.image.sprite = null;
            button.image.enabled = false;
            return;
        }

        button.image.enabled = true;
        button.image.sprite = UIManager.Instance.SetInteractionButton(interact.GetBehaviorType());
    }

    public void InteractionButtonOnclicked(PointerEventData data)
    {
        DayPhasePlayerManager.Instance.currentInteract.Interact(DayPhasePlayerManager.Instance.dayPlayer);
    }
}
