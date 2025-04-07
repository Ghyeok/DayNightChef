using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class UI_scene : UI_Base
{
    public override void Init()
    {
        UIManager.Instance.SetCanvas(gameObject, false);
    }
}
