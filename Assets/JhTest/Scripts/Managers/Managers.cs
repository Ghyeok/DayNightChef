using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Managers : MonoBehaviour
{
    static Managers s_instance;

    ResourceManager _resource = new ResourceManager();
    InputManager _input = new InputManager();
    UIManager _ui = new UIManager();

    public static InputManager Input {get {return Instance._input;}}
    public static ResourceManager Resource {get {return Instance._resource;}}
    public static UIManager UI { get {return Instance._ui;}}

    public static Managers Instance
    {
        get
        {
            Init();
            return s_instance;
        }
    }
    void Start()
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
