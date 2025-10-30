using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

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

    private readonly string[] mapSceneNames = {
        "GrassLand",
        "SwampLand",
        "WinterLand"
    };

    public override void Awake()
    {
        base.Awake();

        SceneManager.sceneLoaded += OnSceneLoaded;
        UI_MapSelectPopup.OnMapSelected += HandleMapSelection;
        OnMapLoadComplete += ShowDayPhaseSceneUI;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UI_MapSelectPopup.OnMapSelected -= HandleMapSelection;
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

    private void HandleMapSelection(MapType type) { StartCoroutine(LoadMap(type)); }
    public IEnumerator LoadMap(MapType mapType)
    {
        if (currentLoadedMap.HasValue) // currentLoadedMap이 null이 아니면
        {
            string oldScene = mapSceneNames[(int)currentLoadedMap.Value];
            yield return SceneManager.UnloadSceneAsync(oldScene);
        }

        string newScene = mapSceneNames[(int)mapType];
        currentMapOp = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);

        yield return currentMapOp;
        currentLoadedMap = mapType;

        Debug.Log($"[DayPhaseManager] Loaded {newScene}");
        OnMapLoadComplete?.Invoke();
    }

    public void OnMapChangeButton(MapType nextMap) // 항구 트리거로 사용
    {
        StartCoroutine(DayPhaseManager.Instance.LoadMap(nextMap));
    }

    private void ShowDayPhaseSceneUI()
    {
        UIManager.Instance.ShowSceneUI<UI_DayPhaseScene>("UI_DayPhaseScene");
    }
}
