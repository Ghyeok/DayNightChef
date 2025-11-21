using UnityEngine;
using UnityEngine.UI;
public class UI_NightPhaseScene : UI_Scene
{
    enum Buttons
    {
        SettingBtn,
    }

    void Start()
    {
        Init();
    }
    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        var settingBtn = GetButton((int)Buttons.SettingBtn).gameObject;
        AddUIEvent(settingBtn, _ => UI_SettingPopup.Show(), Define.UIEvent.Click);

    }
}
