using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : SingletonManager<UIManager>
{
    private GameObject _root;

    public float canvasWidth = 1920f;
    public float canvasHeight = 1080f;

    public void Init()
    {
        ShowSceneUI<UI_Scene>("UI_MainMenuScene");
    }

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
        foreach (var p in _popupStack)
        {
            if(p is T)
            {
                Debug.LogWarning($"{name} 팝업은 이미 열려있습니다.");
                return p as T;
            }
        }

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
        Destroy(popup.gameObject);
        popup = null;
        _order--;
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

    private Dictionary<PlayerBehavior, Sprite> interactionIcons;
    private void InitInteractionButton() // 상호작용 버튼 초기 세팅
    {
        if(interactionIcons != null)
            return;
        if(DayPhaseManager.Instance == null)
        {
            Debug.Log("DayPhaseManager Instance가 없습니다!");
            return;
        }
        interactionIcons = new Dictionary<PlayerBehavior, Sprite>
        {
            {PlayerBehavior.Hunting, ResourceManager.Instance.Load<Sprite>("Arts/UI/Hunting") },
            {PlayerBehavior.Fishing, ResourceManager.Instance.Load<Sprite>("Arts/UI/Fishing") },
            {PlayerBehavior.Gathering, ResourceManager.Instance.Load<Sprite>("Arts/UI/Gathering") },
        };
    }

    /// <summary>
    /// 상호작용에 알맞은 버튼 스프라이트 반환
    /// </summary>
    public Sprite SetInteractionButton(PlayerBehavior behavior)
    {
        if(DayPhaseManager.Instance == null)
        {
            Debug.Log("DayPhaseManager Instance가 없습니다!");
            return null;
        }
        InitInteractionButton();
        if (interactionIcons == null || !interactionIcons.ContainsKey(behavior))
        {
            Debug.Log("버튼 아이콘을 찾을 수 없습니다!");
            return null;
        }
        return interactionIcons[behavior];
    }
}
