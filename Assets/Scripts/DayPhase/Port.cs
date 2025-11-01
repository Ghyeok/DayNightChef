using UnityEngine;

public class Port : MonoBehaviour
{
    private Collider2D col;
    bool isOpened;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        isOpened = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isOpened)
        {
            UIManager.Instance.ShowPopupUI<UI_MapSelectPopup>("UI_MapSelectPopup");
            isOpened = true;
        }
    }
}
