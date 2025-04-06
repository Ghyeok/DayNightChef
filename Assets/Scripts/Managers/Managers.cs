using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/* 모든 매니저들을 자동으로 싱글톤 선언을 해주는 클래스
 * 
 */

public class Managers : MonoBehaviour
{
    private static Managers s_instance;
    public static Managers Instance { get { Init(); return s_instance; } }

    ResourceManager _resource = new ResourceManager();
    InputManager _input = new InputManager();
    UIManager _ui = new UIManager();
    DayPhaseManager _day = new DayPhaseManager();
    NightPhaseManager _night = new NightPhaseManager();

    public static InputManager Input { get { return Instance._input; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static DayPhaseManager DayPhase { get { return Instance._day; } }
    public static NightPhaseManager NightPhase { get { return Instance._night; } }

    void Awake()
    {
        Init();
    }

    void Update()
    {
        _input.OnUpdate();
    }

    static void Init()
    {
        if (Instance == null)
        {
            GameObject obj = GameObject.Find("@Managers");
            if(obj == null)
            {
                obj = new GameObject { name = "@Managers"};
                obj.AddComponent<Managers>();
            }

            DontDestroyOnLoad(obj);
            s_instance = obj.GetComponent<Managers>();
        }
    }
}
