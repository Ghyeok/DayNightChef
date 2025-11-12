using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainLobbyScene : UI_Scene
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button howToButton;
    [SerializeField] private Button settingButton;

    private string nextSceneName = "DayPhaseScene";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
        startButton.onClick.AddListener(OnClickedStartButton);
        loadButton.onClick.AddListener(OnClickedLoadButton);
    }

    private void OnClickedStartButton() // 메인 로비 -> 낮 페이즈 Base 씬
    {
        SaveManager.Instance.StartNewGame();
        SceneLoader.Instance.LoadScene(nextSceneName, LoadSceneMode.Single);
    }

    private void OnClickedLoadButton() // 메인 로비 -> 낮 페이즈 Base 씬
    {
        SaveManager.Instance.LoadGame();
        SceneLoader.Instance.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}
