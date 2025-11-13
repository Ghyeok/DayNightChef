using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellSlot : MonoBehaviour
{
    [SerializeField] private Image recipeImage;
    [SerializeField] private TextMeshProUGUI recipeName;
    [SerializeField] private TextMeshProUGUI recipeCount;
    [SerializeField] private TextMeshProUGUI sellPrice;

    public void SetData(MenuPlan plan)
    {
        this.recipeImage.sprite = plan.recipe.recipe_image;
        this.recipeName.text = $"{plan.recipe.recipe_name}";
        this.recipeCount.text = $"{plan.sold} / {plan.planned}";
        this.sellPrice.text = $"{plan.sold * plan.recipe.recipe_price}G";
    }
}
