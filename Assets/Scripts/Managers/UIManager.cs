using System;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class UIManager : SingletonManager<UIManager>
{
    private GameObject _root;

    public static GameObject Root
    {
        get
        {
            if (Instance._root == null)
            {
                GameObject root = GameObject.Find("UI_Root");
                if (root == null)
                {
                    root = new GameObject { name = "UI_Root" };
                }

                Instance._root = root;
                return Instance._root;
            }
            else
            {
                return Instance._root;
            }
        }
    }

    // 나중에 ExitButton 있는 UI 클래스에서 호출하세요
    // UIManager.Instance.OnExitButton(); 이런식으로
    public void OnExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // UI 코드가 한곳에 집중됩니다. 유지보수 편리하고 재사용성도 좋습니다
    // UIManager.LoadUI(Define.UIType.PauseUI) 이런식으로 쓰시면 됩니다
    public static GameObject LoadUI(Define.UIType newUIType)
    {
        GameObject ui;
        try
        {
            ui = Instantiate(Resources.Load<GameObject>($"Prefabs/UIs/{newUIType}"));
            ui.transform.SetParent(Root.transform, false);
            return ui;
        }
        catch
        {
            Debug.LogError($"[Error] UI Load 실패 : Prefabs/UIs/{newUIType} 확인 바람");
            return null;
        }
    }

    // 이건 String으로 호출하는거
    public static GameObject LoadUI(string newUIType)
    {
        GameObject ui;
        try
        {
            ui = Instantiate(Resources.Load<GameObject>($"Prefabs/UIs/{newUIType}"));
            ui.transform.SetParent(Root.transform, false);
            return ui;
        }
        catch
        {
            Debug.LogError($"[Error] UI Load 실패 : Prefabs/UIs/{newUIType} 확인 바람");
            return null;
        }
    }

    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;


        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }

    // UI 팝업 구현
    int _order = 10;
    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        GameObject go = LoadUI($"Popup/{name}"); // Resources/Prefabs/UIs/Popup/
        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        return popup;
    }

    // 엉뚱한 UI를 삭제하는 것을 방지
    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupStack.Count == 0)
        {
            return;
        }

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!");
            return;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
        {
            return;
        }

        UI_Popup popup = _popupStack.Pop();
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
        {
            ClosePopupUI();
        }
    }

    // 팝업이 아닌 Scene에 구현되어 있는 UI
    UI_Scene _sceneUI = null;

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = LoadUI($"Scene/{name}");
        T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;

        return sceneUI;
    }
}
