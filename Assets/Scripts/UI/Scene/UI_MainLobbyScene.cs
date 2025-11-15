using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainLobbyScene : UI_Scene
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button howToButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;

    private string nextSceneName = "DayPhaseScene";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
        startButton.onClick.AddListener(OnClickedStartButton);
        loadButton.onClick.AddListener(OnClickedLoadButton);
        howToButton.onClick.AddListener(OnClickedHowToButton);
        settingButton.onClick.AddListener(OnClickedSettingButton);
        exitButton.onClick.AddListener(OnClickedExitButton);
    }

    private void OnClickedStartButton() // 메인 로비 -> 낮 페이즈 Base 씬
    {
        SceneLoader.Instance.SetLoadType(SceneLoader.LoadType.NewGame);
        SceneLoader.Instance.LoadScene(nextSceneName, LoadSceneMode.Single);
    }

    private void OnClickedLoadButton() // 메인 로비 -> 낮 페이즈 Base 씬
    {
        SceneLoader.Instance.SetLoadType(SceneLoader.LoadType.Continue);
        SceneLoader.Instance.LoadScene(nextSceneName, LoadSceneMode.Single);
    }

    private void OnClickedHowToButton() // 
    {
        UI_GameRulePopup.Show();
    }

    private void OnClickedSettingButton() // 
    {
        UI_SettingPopup.Show();
    }

    private void OnClickedExitButton()
    {
        UIManager.Instance.OnExitButton();
    }
}
