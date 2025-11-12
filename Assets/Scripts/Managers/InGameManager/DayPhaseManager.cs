using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 1. 맵 로드
/// 2. 플레이어 스폰
/// 3. UI 표시
/// </summary>
public class DayPhaseManager : SingletonManager<DayPhaseManager>
{
    [Header("맵 정보")]
    public MapType curMapType;
    public MapType? currentLoadedMap = null;

    public AsyncOperation currentMapOp;
    public static event Action OnMapLoadComplete;

    [Header("UI 참조")]
    [SerializeField] private UI_DayPhaseScene _uiDayPhaseScene;
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    public const string swampLandUnlocked = "swampLandUnlocked";
    public const string winterLandUnlocked = "winterLandUnlocked";

    public List<Animal> animalList = new List<Animal>();

    private readonly string[] mapSceneNames = {
        "GrassLand",
        "SwampLand",
        "WinterLand"
    };

    public override void Awake()
    {
        base.Awake();

        UI_MapSelectPopup.OnMapSelected += LoadMap;
        OnMapLoadComplete += ShowDayPhaseSceneUI;
    }

    private void OnDestroy()
    {
        UI_MapSelectPopup.OnMapSelected -= LoadMap;
        OnMapLoadComplete -= ShowDayPhaseSceneUI;
    }

    // DayPhaseSceneInitializer가 호출할 리셋 함수
    public void ResetForNewDayPhase()
    {
        Debug.Log("[DayPhaseManager] 씬 참조를 리셋합니다.");
        _uiDayPhaseScene = null;
        currentLoadedMap = null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (animalList == null)
            animalList = new List<Animal>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animalList == null) return;
        for (int i = 0; i < animalList.Count; ++i)
        {
            if (animalList[i] != null)
            {
                animalList[i].Updated();
            }
        }
    }

    public IEnumerator UnloadOldScene()
    {
        if (currentLoadedMap.HasValue)
        {
            string oldSceneName = mapSceneNames[(int)currentLoadedMap.Value];

            Scene oldScene = SceneManager.GetSceneByName(oldSceneName);
            if (oldScene.IsValid() && oldScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(oldSceneName);
            }
        }
        currentLoadedMap = null;
    }

    public void LoadMap(MapType mapType)
    {
        StartCoroutine(LoadMapRoutine(mapType));
    }

    private IEnumerator LoadMapRoutine(MapType mapType)
    {
        string newScene = mapSceneNames[(int)mapType];
        yield return StartCoroutine(UnloadOldScene());

        Action sceneLoadedCallBack = () => // SceneLoader.Instance.LoadScene 후 콜백할 함수 등록
        {
            StartCoroutine(PostMapLoadSequence(mapType));
        };

        SceneLoader.Instance.LoadScene(newScene,LoadSceneMode.Additive, sceneLoadedCallBack);
    }

    // 맵 로드 후에 순차적으로 실행할 작업들
    private IEnumerator PostMapLoadSequence(MapType mapType)
    {
        // 1. 맵 상태 설정
        curMapType = mapType;
        currentLoadedMap = mapType;
        Debug.Log($"[DayPhaseManager] Loaded {mapSceneNames[(int)mapType]}");

        // 2. 플레이어 스폰 (완료될 때까지 대기)
        if (DayPhasePlayerManager.Instance != null)
        {
            yield return StartCoroutine(DayPhasePlayerManager.Instance.SpawnPlayerRoutine());
        }
        else
        {
            Debug.LogError("DayPhasePlayerManager가 없습니다!");
            yield break;
        }

        // 3. 데이터 로드

        // 4. UI 업데이트
        OnMapLoadComplete?.Invoke(); 
    }

    private void ShowDayPhaseSceneUI()
    {
        if (_uiDayPhaseScene == null)
        {
            _uiDayPhaseScene = UIManager.Instance.ShowSceneUI<UI_DayPhaseScene>("UI_DayPhaseScene");
        }
        else
        {
            _uiDayPhaseScene.Init();
        }
    }

    public void UnlockSwampLand()
    {
        GameManager.Instance.unlockSwampLand = true;
        Debug.Log("SwampLand 해금!");
    }
    public void UnlockWinterLand()
    {
        GameManager.Instance.unlockSwampLand = false;
        Debug.Log("WinterLand 해금!");
    }
}
