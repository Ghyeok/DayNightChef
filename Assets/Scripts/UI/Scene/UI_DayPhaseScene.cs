using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DayPhaseScene : UI_Scene
{
    public enum Texts
    {
        WeightText,

    }

    public enum Images
    {
        WeightImage,
        HeartImage,
        HPBarImage,
        HPBackBarImage,
    }

    public enum Buttons
    {
        JoyStickButton,
        InteractionButton,
        PauseButton,
        UpgradeButton,
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
    }

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        GameObject interact = GetButton((int)Buttons.InteractionButton).gameObject;
        AddUIEvent(interact, InteractionButtonOnclicked, Define.UIEvent.Click);
    }

    public void SetWeightText()
    {
        GetText((int)Texts.WeightText).text = $"{DayPhasePlayerManager.Instance.curBagWeight}" + " / " + $"{DayPhasePlayerManager.Instance.maxBagWeight}";
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
