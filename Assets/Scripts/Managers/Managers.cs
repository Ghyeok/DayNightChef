using System;
using UnityEngine;

public class Managers<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance { get { Init(); return _instance; } }

    static void Init()
    {
        if (_instance == null)
        {
            _instance = (T)FindAnyObjectByType(typeof(T));

            if (_instance == null)
            {
                CreateInstance();
            }
        }
    }

    static void CreateInstance()
    {
        _instance = (T)FindAnyObjectByType(typeof(T));

        if (_instance == null)
        {
            GameObject go = new GameObject();
            go.name = typeof(T).Name;
            _instance = go.AddComponent<T>();
            DontDestroyOnLoad(go);
        }
    }

    public virtual void Awake()
    {
        RemoveDuplicates();
    }

    private void RemoveDuplicates()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
