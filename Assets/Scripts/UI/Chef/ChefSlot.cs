using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChefSlot : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;

    public int OrderId { get; private set; }

    public void Bind(SalesManager.Order order)
    {
        OrderId = order.orderId;

        if (order.recipe != null)
        {
            if (icon) icon.sprite = order.recipe.recipe_image;
            if (nameText) nameText.text = order.recipe.recipe_name + " 준비됨";
        }
    }

}