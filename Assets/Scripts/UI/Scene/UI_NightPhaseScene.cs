using UnityEngine;
using UnityEngine.UI;
public class UI_NightPhaseScene : UI_Scene
{
    [SerializeField] private VariableJoystick joystick;
    private WaiterController waiterController;

    enum Buttons
    {
        SettingBtn,
        Serving,
    }

    void Start()
    {
        Init();
    }
    public override void Init()
    {
        base.Init();
        waiterController = FindAnyObjectByType<WaiterController>();
        Bind<Button>(typeof(Buttons));

        var settingBtn = GetButton((int)Buttons.SettingBtn).gameObject;
        AddUIEvent(settingBtn, _ => UI_SettingPopup.Show(), Define.UIEvent.Click);

        var servingBtn = GetButton((int)Buttons.Serving).gameObject;
        AddUIEvent(servingBtn, _ => waiterController.OnInteract(), Define.UIEvent.Click);

        waiterController.joystick = joystick;
    }
}
