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

    private async void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOpened || !collision.CompareTag("Player"))
        {
            return;
        }
        isOpened = true;

        bool result = await UI_ConfirmPopup.ShowAsync(
        info: "맵 선택 창으로 이동하시겠습니까?",
        left: "예",
        right: "아니오"
        );

        if (result)
        {
            UIManager.Instance.ClosePopupUI();
            UIManager.Instance.ShowPopupUI<UI_MapSelectPopup>("UI_MapSelectPopup");
            SaveManager.Instance.SaveGame();
        }
        else
        {
            UIManager.Instance.ClosePopupUI();
            isOpened = false;
        }
    }
}
