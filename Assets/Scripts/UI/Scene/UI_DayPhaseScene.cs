using TMPro;
using UnityEngine;
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

    }

    // Update is called once per frame
    void Update()
    {
        SetWeightText();
        SetHPBarImage();
    }

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        Button _upgrade = GetButton((int)Buttons.UpgradeButton);
    }

    public void SetWeightText()
    {
        GetText((int)Texts.WeightText).text = $"{DayPhasePlayerManager.Instance.curBagWeight}" + " / " + $"{DayPhasePlayerManager.Instance.maxBagWeight}";
    }

    public void SetHPBarImage()
    {
        GetImage((int)Images.HPBarImage).fillAmount = DayPhasePlayerManager.Instance.playerCurHP / DayPhasePlayerManager.Instance.playerMaxHP;
    }
}
