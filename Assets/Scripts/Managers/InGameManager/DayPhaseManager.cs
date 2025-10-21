using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using NUnit.Framework;

public class DayPhaseManager : SingletonManager<DayPhaseManager>
{
    //현재 존재하는 animals
    public List<Animal> animalList = new List<Animal>();

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

    public int hpLevel;
    public int moveSpeedLevel;
    public int knifeLevel;
    public int fishingLevel;
    public int bagLevel;

    public MapType curMapType;
    private AsyncOperation currentMapOp;

    private readonly string[] mapSceneNames = {
        "GrassLand",
        "SwampLand",
        "SnowLand"
    };

    public override void Awake()
    {
        base.Awake();
        StartCoroutine(LoadMap(curMapType));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "TestDayPhase")
            InitGame();

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

    private void InitGame()
    {
        UI_DayPhaseScene _day = UIManager.Instance.ShowSceneUI<UI_DayPhaseScene>("DayPhaseScene");
    }

    public IEnumerator LoadMap(MapType mapType)
    {
        // 기존 맵 언로드
        if (currentMapOp != null)
        {
            string curScene = mapSceneNames[(int)curMapType];
            yield return SceneManager.UnloadSceneAsync(curScene);
        }

        // 새로운 맵 로드
        string newScene = mapSceneNames[(int)mapType];
        currentMapOp = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
        curMapType = mapType;

        yield return currentMapOp;
        Debug.Log($"[DayPhaseManager] Loaded {newScene}");
    }

    public void OnMapChangeButton(MapType nextMap) // 항구 트리거로 사용
    {
        StartCoroutine(DayPhaseManager.Instance.LoadMap(nextMap));
    }
}
