using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DayPhaseScene : UI_Scene
{
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
        SetWeightText();
        SetHPBarImage();
        SetInteractionIcon();
        SetGoldText();
    }

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        GameObject interact = GetButton((int)Buttons.InteractionButton).gameObject;
        AddUIEvent(interact, InteractionButtonOnclicked, Define.UIEvent.Click);

        var weightBtn = GetButton((int)Buttons.WeightButton).gameObject;
        AddUIEvent(weightBtn, _ => UI_Inven.Show(), Define.UIEvent.Click);

        SetJoyStickToPlayer();
    }

    public void SetJoyStickToPlayer()
    {
        PlayerController pc = DayPhasePlayerManager.Instance.dayPlayer.GetComponent<PlayerController>();
        var joystick = Get<GameObject>((int)GameObjects.Joystick);
        pc.joystick = joystick.GetComponent<VariableJoystick>();
    }

    public void SetWeightText()
    {
        GetText((int)Texts.WeightText).text = $"{DayPhasePlayerManager.Instance.curBagWeight}" + " / " + $"{DayPhasePlayerManager.Instance.maxBagWeight}";
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
