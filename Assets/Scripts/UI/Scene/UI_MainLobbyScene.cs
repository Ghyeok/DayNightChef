using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainLobbyScene : UI_Scene
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button howToButton;
    [SerializeField] private Button settingButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startButton.onClick.AddListener(OnClickedStartButton);
    }

    void OnClickedStartButton()
    {
        string sceneName = "TestDayPhase";
        SceneManager.LoadScene(sceneName);
    }
}
