using TMPro;
using UnityEngine;

public class ShowGetItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI PopupText;

    public void ShowItem(Item item, int count)
    {
        PopupText.text = $"{item.item_name} {count}개 획득!";
    }
}
