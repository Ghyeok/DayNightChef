using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class UIManager
{
    int _order = 10; // 고정 ui : 값이 0으로 고정, 가장 먼저 그려져 밑에서 그려지게, 스태긍로 관리될 필요 x
    //팝업 ui : 고정 ui와 겹치지 않게 10부터 시작, 이후 11,12...
    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    UI_scene _sceneUI = null;

    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if(root == null)
                root = new GameObject { name = "@UI_Root"};
            return root;
        }
    }
    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true; // 캔버스 중첩의 경우 (부모캔버스가 어떤값을 가지던 나는 내 오더값을 가짐)

        if(sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else // 고정 ui
        {
            canvas.sortingOrder = 0;
        }
    }
    public T showSceneUI<T> (string name = null) where T : UI_scene
    {
        if(string.IsNullOrEmpty(name))
            name = typeof(T).Name;
        GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
        T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;
        go.transform.SetParent(Root.transform);
        return sceneUI;
    }
    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if(string.IsNullOrEmpty(name))
            name = typeof(T).Name;
        GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        go.transform.SetParent(Root.transform);
        return popup;

    }
    public void ClosePopupUI(UI_Popup popup)
    {
        if(_popupStack.Count == 0)
            return;
        if(_popupStack.Peek() != popup) // popup이 가장 위에것이 아니라면 삭제불가
        {
            Debug.Log("Close Popup Failed");
            return;
        }
    }
    public void ClosePopupUI()
    {
        if(_popupStack.Count == 0)
            return;
        UI_Popup popup = _popupStack.Pop();
        Managers.Resource.Destory(popup.gameObject);
        popup = null;
        _order--;
    }
    public void CloseAllPopupUI()
    {
        while(_popupStack.Count > 0)
            ClosePopupUI();
        
    }
}
