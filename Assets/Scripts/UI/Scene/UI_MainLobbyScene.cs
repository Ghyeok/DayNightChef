using System.IO;
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
    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();

        startButton.onClick.AddListener(OnClickedStartButton);
        loadButton.onClick.AddListener(OnClickedLoadButton);
        howToButton.onClick.AddListener(OnClickedHowToButton);
        settingButton.onClick.AddListener(OnClickedSettingButton);
        exitButton.onClick.AddListener(OnClickedExitButton);

        SoundManager.Instance.PlayAudioClip("MainLobbyBGM", SoundManager.SoundTypes.BGM);
        CheckSaveFile();
    }

    private void CheckSaveFile()
    {
        if (!File.Exists(SavePath))
        {
            loadButton.interactable = false; // 버튼 클릭 불가
        }
        else
        {
            loadButton.interactable = true;
        }
    }

    private async void OnClickedStartButton()
    {
        // 세이브 파일이 있다면 경고 팝업 띄우기
        if (File.Exists(SavePath))
        {
            bool confirm = await UI_ConfirmPopup.ShowAsync(
                info: "새 게임을 시작하면 기존 데이터가 삭제됩니다.\n진행하시겠습니까?",
                left: "예",
                right: "아니오"
            );

            if (!confirm) return; // 취소하면 함수 종료
        }

        // 확인했거나 파일이 없으면 진행
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
