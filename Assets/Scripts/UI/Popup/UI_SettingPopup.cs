using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SettingPopup : UI_Popup
{
    public enum Buttons
    {
        ExitBtn,
        SFXBtn,
        BGMBtn,
        SFXMuteBtn,
        BGMMuteBtn,
        SaveBtn,
        EndBtn,
        LobbyBtn,
    }
    [Header("슬라이더")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("아이콘 이미지")]
    [SerializeField] private Sprite noteIcon;
    [SerializeField] private Sprite muteIcon;

    // 마지막 세팅 기억용
    private float lastBgmValue;
    private float lastSfxValue;

    public override void Init()
    {
        base.Init();
        GameManager.Instance.SetPause(true);

        Bind<Button>(typeof(Buttons));

        // Exit 버튼
        var exitBtn = GetButton((int)Buttons.ExitBtn);
        if (exitBtn != null)
            UI_Base.AddUIEvent(exitBtn.gameObject, _ => { UIManager.Instance.ClosePopupUI(this);
                GameManager.Instance.SetPause(false);
            });

        // BGM/SFX 슬라이더 초기값 로드
        var sm = SoundManager.Instance;
        if (sm != null)
        {
            _bgmSlider.value = sm.BGMVolume;
            _sfxSlider.value = sm.SFXVolume;

            lastBgmValue = (_bgmSlider.value > 0f) ? _bgmSlider.value : 1f;
            lastSfxValue = (_sfxSlider.value > 0f) ? _sfxSlider.value : 1f;

            sm.SetBGMVolume(lastBgmValue);
            sm.SetSFXVolume(lastSfxValue);
        }

        // 슬라이더 리스너 등록 및 초기화
        _bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        _sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        OnBgmSliderChanged(_bgmSlider.value);
        OnSfxSliderChanged(_sfxSlider.value);

        // 음소거 버튼 등록
        var bgmMuteBtn = GetButton((int)Buttons.BGMMuteBtn);
        if (bgmMuteBtn != null)
            UI_Base.AddUIEvent(bgmMuteBtn.gameObject, _ => ToggleBgmMute(bgmMuteBtn));

        var sfxMuteBtn = GetButton((int)Buttons.SFXMuteBtn);
        if (sfxMuteBtn != null)
            UI_Base.AddUIEvent(sfxMuteBtn.gameObject, _ => ToggleSfxMute(sfxMuteBtn));

        // Save
        var saveBtn = GetButton((int)Buttons.SaveBtn);
        if (saveBtn != null)
            UI_Base.AddUIEvent(saveBtn.gameObject, _ => OnClickSave());

        // Lobby
        var lobbyBtn = GetButton((int)Buttons.LobbyBtn);
        if (lobbyBtn != null)
            UI_Base.AddUIEvent(lobbyBtn.gameObject, _ => OnClickLobby());

        // Quit
        var endBtn = GetButton((int)Buttons.EndBtn);
        if (endBtn != null)
            UI_Base.AddUIEvent(endBtn.gameObject, _ => OnClickQuit());

        // 아이콘 초기화
        RefreshIcons();
    }

    // 슬라이더 관련
    private void OnBgmSliderChanged(float value)
    {
        var sm = SoundManager.Instance;
        if (sm != null) sm.SetBGMVolume(value);
        if (value > 0f) lastBgmValue = value;
        RefreshIcons();
    }

    private void OnSfxSliderChanged(float value)
    {
        var sm = SoundManager.Instance;
        if (sm != null) sm.SetSFXVolume(value);
        if (value > 0f) lastSfxValue = value;
        RefreshIcons();
    }

    // 음소거 버튼
    private void ToggleBgmMute(Button btn)
    {
        if (_bgmSlider.value > 0f) _bgmSlider.value = 0f;
        else _bgmSlider.value = lastBgmValue;
        RefreshIcons();
    }

    private void ToggleSfxMute(Button btn)
    {
        if (_sfxSlider.value > 0f) _sfxSlider.value = 0f;
        else _sfxSlider.value = lastSfxValue;
        RefreshIcons();
    }

    private void RefreshIcons()
    {
        var bgmMuteBtn = GetButton((int)Buttons.BGMMuteBtn);
        var sfxMuteBtn = GetButton((int)Buttons.SFXMuteBtn);

        if (bgmMuteBtn != null) bgmMuteBtn.image.sprite = (_bgmSlider.value <= 0f) ? muteIcon : noteIcon;
        if (sfxMuteBtn != null) sfxMuteBtn.image.sprite = (_sfxSlider.value <= 0f) ? muteIcon : noteIcon;
    }

    // 버튼들
    private void OnClickSave()
    {
        SaveManager.Instance?.SaveGame();
        PlayerPrefs.Save();
    }

    private void OnClickLobby()
    {
        SaveManager.Instance?.SaveGame();
        UIManager.Instance.CloseAllPopupUI();
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.SetLoadType(SceneLoader.LoadType.Continue);
            SceneLoader.Instance.LoadScene("MainLobbyScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("SceneLoader Instance가 없습니다!");
        }
    }

    private void OnClickQuit()
    {
        SaveManager.Instance?.SaveGame();
        UIManager.Instance.CloseAllPopupUI();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            GameManager.Instance.SetPause(false);
    }
    public static UI_SettingPopup Show() =>
       UIManager.Instance.ShowPopupUI<UI_SettingPopup>("UI_SettingPopup");
}
