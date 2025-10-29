using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RestaurantPreparePopup : UI_Popup
{
    private List<Recipe> recipeList;
    public GameObject recipeSlotPrefab;
    public Transform contentTransform;

    [Header("레시피 정보")]
    [SerializeField] private Image recipeImage;
    [SerializeField] private TextMeshProUGUI recipeName;
    [SerializeField] private TextMeshProUGUI recipePrice;
    [SerializeField] private TextMeshProUGUI recipeDesc;
    [SerializeField] private Transform itemRequirementTransform;
    [SerializeField] private GameObject requiredItemPrefab;

    private void Start()
    {
        InstantiateRecipeSlot();
    }

    private void InstantiateRecipeSlot()
    {
        recipeList = RecipeManager.Instance.GetAllRecipes();
        ShowRecipeInfo(recipeList[0]);

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
            else
            {
                Debug.LogError("버튼 컴포넌트가 없습니다!");
            }
        }
    }

    private void ShowRecipeInfo(Recipe recipe)
    {
        recipeName.text = recipe.recipe_name;
        recipeDesc.text = RecipeManager.Instance.GetRecipeDescription(recipe.recipe_number - 1);
        recipeImage.sprite = recipe.recipe_image;

        foreach(Transform child in itemRequirementTransform)
        {
            Destroy(child.gameObject);
        }

        foreach(var requirement in recipe.recipe_requireItems)
        {
            GameObject itemGO = Instantiate(requiredItemPrefab, itemRequirementTransform);
            UI_RecipeRequiredSlot slotUI = itemGO.GetComponent<UI_RecipeRequiredSlot>();
            if (slotUI != null)
            {
                int remain = WarehouseManager.Instance.GetCount(requirement.item);
                slotUI.SetData(requirement.item, requirement.count);
            }
        }
    }
}
