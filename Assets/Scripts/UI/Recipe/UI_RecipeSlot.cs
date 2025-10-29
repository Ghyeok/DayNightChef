using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RecipeSlot : MonoBehaviour
{
    [SerializeField] private Image recipeImage;
    [SerializeField] private TextMeshProUGUI recipePrice;
    public Recipe recipe;

    public void SetData(Recipe recipe)
    {
        this.recipe = recipe;
        recipeImage.sprite = recipe.recipe_image;
        recipePrice.text = recipe.recipe_price.ToString() + "G";
    }
}
