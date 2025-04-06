using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IDragHandler
{   
    // IPointerClickHandl 마우스 클릭 이벤트가 발생하면 자동 실행, OnClickHandler 액션에 등록된 함수들 모두 실행
    // 클릭 이벤트가 발생하면 OnDragHandler 액션에 등록된 함수들이 모두 실행

    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnDragHandler = null;

    public void OnPointerClick(PointerEventData eventData)// eventdata 에 이벤트와 관련 정보 담김 ex)마우스 좌표
    {
        if (OnClickHandler != null)
            OnClickHandler.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (OnDragHandler != null)
            OnDragHandler.Invoke(eventData);
    }
}