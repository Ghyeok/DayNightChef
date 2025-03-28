using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
public abstract class UI_Base : MonoBehaviour
{
    //_objects = 씬 상에 존재하는 오브젝트들을 로드하여 이 곳에 바인딩하여 보관하는 dictionary
    //objects = _objects에 Value로 담기 위한 배열
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();
    public abstract void Init();
    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects); // _objects 안에 추가
        // T 에 속하는 오브젝트들을 Dictionary의 Value인 objects 배열의 원소들에 하나하나 추가

        for(int i=0; i<names.Length; i++)
        {
            if(typeof(T) == typeof(GameObject))
                objects[i] = Util.FindChild(gameObject, names[i], true);
            else
                objects[i] = Util.FindChild<T>(gameObject,names[i], true);
            if (objects == null)
                Debug.Log($"Failed to bind({names[i]})");
        }
        
    }
    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if(_objects.TryGetValue(typeof(T), out objects) == false)
            return null;
        return objects[idx] as T;
    }
    protected GameObject GetObject(int idx) { return Get<GameObject>(idx);}
    protected TextMeshProUGUI GetText(int idx) { return Get<TextMeshProUGUI>(idx);}
    protected Button GetButton(int idx) { return Get<Button>(idx);}
    protected Image GetImage(int idx) { return Get<Image>(idx);}

    public static void BindEvent(GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {//go 에 이벤트 핸들러를 붙여 이벤트 콜백을 받을 수 있게 함
    //핸들러에 정의되어 있는 이벤트 들이 발생하면 action에 등록된 것들이 실행되도록 함
    //Define에 정의된 이벤트 종류별 Enum도 같이 넘겨서 어떤 액션에 등록할 것인지를 받는다.
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        switch(type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler -= action;
                evt.OnDragHandler += action;
                break;
        }
    }
}
