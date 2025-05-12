using UnityEngine;

public class UI_Popup : UI_Base
{
    public override void Init()
    {
        UIManager.Instance.SetCanvas(gameObject);
    }
}
