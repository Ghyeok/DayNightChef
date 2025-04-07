using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UI_Popup : UI_Base
{
    public override void Init()
    {
        UIManager.Instance.SetCanvas(gameObject, true);
    }

    public virtual void ClosedPopupUI()
    {
        UIManager.Instance.ClosePopupUI(this);
    }
}
