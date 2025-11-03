using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class DayPhaseManager : SingletonManager<DayPhaseManager>
{
    public enum PlayerState
    {
        Alive,
        Dead,
    }
    public enum AnimalStates
    {
        Patrol = 0,
        Attack,
        Chase,
        Die
    }
    public enum PlayerBehavior
    {
        Hunting,
        Fishing,
        Gathering,
        MaxCount,
    }
    public enum UpgradeType
    {
        Hp,
        MoveSpeed,
        Knife,
        Fishing,
        Bag,
        MaxCount,
    }

    //현재 존재하는 animals
    public List<Animal> animalList = new List<Animal>();

    public int hpLevel;
    public int moveSpeedLevel;
    public int knifeLevel;
    public int fishingLevel;
    public int bagLevel;

    public MapType curMapType;
    public MapType? currentLoadedMap = null;
    public AsyncOperation currentMapOp;
    public static event Action OnMapLoadComplete;
    private UI_DayPhaseScene UI_DayPhaseScene;

    public const string swampLandUnlocked = "swampLandUnlocked";
    public const string winterLandUnlocked = "winterLandUnlocked";

    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    private readonly string[] mapSceneNames = {
        "GrassLand",
        "SwampLand",
        "WinterLand"
    };

    public override void Awake()
    {
        base.Awake();

        SceneManager.sceneLoaded += OnSceneLoaded;
        UI_MapSelectPopup.OnMapSelected += LoadMap;
        OnMapLoadComplete += ShowDayPhaseSceneUI;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UI_MapSelectPopup.OnMapSelected -= LoadMap;
        OnMapLoadComplete -= ShowDayPhaseSceneUI;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = "DayPhaseScene";
        if(scene.name == sceneName)
        {
            UIManager.Instance.ShowPopupUI<UI_MapSelectPopup>("UI_MapSelectPopup");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(animalList == null)
            animalList = new List<Animal>();
    }

    // Update is called once per frame
    void Update()
    {
        if(animalList == null) return; 
        for(int i = 0; i < animalList.Count; ++i)
        {
            if(animalList[i] != null)
            {
                animalList[i].Updated();
            }
        }
    }

    public IEnumerator UnloadOldScene()
    {
        if (currentLoadedMap.HasValue) // currentLoadedMap이 null이 아니면
        {
            string oldScene = mapSceneNames[(int)currentLoadedMap.Value];
            yield return SceneManager.UnloadSceneAsync(oldScene);
        }
    }

    public void LoadMap(MapType mapType)
    {
        StartCoroutine(LoadMapRoutine(mapType));
    }

    private IEnumerator LoadMapRoutine(MapType mapType)
    {
        string newScene = mapSceneNames[(int)mapType];
        bool isComplete = false;
        Action sceneLoadedCallBack = () =>
        {
            curMapType = mapType;
            currentLoadedMap = mapType;

            Debug.Log($"[DayPhaseManager] Loaded {newScene}");
            OnMapLoadComplete?.Invoke();

            isComplete = true;
        };

        SceneLoader.Instance.LoadScene(newScene,LoadSceneMode.Additive, sceneLoadedCallBack);

        yield return new WaitUntil(() => isComplete);
    }

    private void ShowDayPhaseSceneUI()
    {
        if (UI_DayPhaseScene == null)
        {
            UI_DayPhaseScene = UIManager.Instance.ShowSceneUI<UI_DayPhaseScene>("UI_DayPhaseScene");
        }
        else
        {
            UI_DayPhaseScene.Init();
        }
    }

    public void UnlockSwampLand()
    {
        PlayerPrefs.SetInt(swampLandUnlocked, 1);
        PlayerPrefs.Save();
        Debug.Log("SwampLand 해금!");
    }
    public void UnlockWinterLand()
    {
        PlayerPrefs.SetInt(winterLandUnlocked, 1);
        PlayerPrefs.Save();
        Debug.Log("WinterLand 해금!");
    }
}
