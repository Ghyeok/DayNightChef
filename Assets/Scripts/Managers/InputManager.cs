using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InputManager
{
    public Action KeyAction = null;

    public void OnUpdate()
    {
        if (Input.anyKey == false)
            return;
        if (KeyAction != null)
            KeyAction.Invoke(); // KeyAction에 등록된 함수가 있다면 실행시킴
    }
}
