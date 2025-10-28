using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RestaurantPreparePopup : UI_Popup
{
    public List<Recipe> recipeList;
    public GameObject recipeSlotPrefab;
    public Transform contentTransform;

    [Header("레시피 정보")]
    [SerializeField] private Image recipeImage;
    [SerializeField] private TextMeshProUGUI recipeName;
    [SerializeField] private TextMeshProUGUI recipePrice;
    [SerializeField] private TextMeshProUGUI recipeDesc;

    private void Start()
    {
        InstantiateRecipeSlot();
    }

    private void InstantiateRecipeSlot()
    {
        recipeList = RecipeManager.Instance.GetAllRecipes();

        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (Recipe recipe in recipeList)
        {
            GameObject slotGO = Instantiate(recipeSlotPrefab, contentTransform);
            UI_RecipeSlot slotUI = slotGO.GetComponent<UI_RecipeSlot>();
            if (slotUI != null)
            {
                slotUI.SetData(recipe);
            }
            Button slotBtn = slotGO.GetComponent<Button>();
            if(slotBtn != null)
            {
                Recipe curRecipe = slotUI.recipe;
                slotBtn.onClick.RemoveAllListeners();
                slotBtn.onClick.AddListener(() => ShowRecipeInfo(curRecipe));
            }
        }
    }

    private void ShowRecipeInfo(Recipe recipe)
    {
        recipeName.text = recipe.recipe_name;
        recipeDesc.text = RecipeManager.Instance.GetRecipeDescription(recipe.recipe_number);
        recipeImage.sprite = recipe.recipe_image;
    }
}
