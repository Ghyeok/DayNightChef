using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_RestaurantPreparePopup : UI_Popup
{
    private List<Recipe> recipeList;
    public GameObject recipeSlotPrefab;
    public Transform contentTransform;

    [Header("레시피 정보")]
    [SerializeField] private Image recipeImage;
    [SerializeField] private TextMeshProUGUI recipeNameText;
    //[SerializeField] private TextMeshProUGUI recipePriceText;
    [SerializeField] private TextMeshProUGUI recipeDescText;
    [SerializeField] private Transform itemRequirementTransform;
    [SerializeField] private GameObject requiredItemPrefab;

    [Header("레시피 판매 결정")]
    [SerializeField] private TextMeshProUGUI totalRecipeCountText;
    [SerializeField] private Button plus;
    [SerializeField] private Button minus;
    [SerializeField] private Button confirm;
    [SerializeField] private TextMeshProUGUI curRecipeCountText; // 현재 선택한 레시피 수량
    [SerializeField] private Button OperationStartButton; // 식당 운영 시작 버튼

    private Recipe currentSelectedRecipe;
    private int currentRecipeCount = 0; // 현재 UI에서 조작 중인 수량, 이 값을 등록
    private int currentRecipeSavedCount = 0;
    private int currentRecipeMaxCookableCount = 0; // 현재 재고로 만들 수 있는 최대 수량


    private void Start()
    {
        InstantiateRecipeSlot();

        plus.onClick.AddListener(OnClickedPlusButton);
        minus.onClick.AddListener(OnClickedMinusButton);
        confirm.onClick.AddListener(OnClickedConfirmButton);
        OperationStartButton.onClick.AddListener(OnClickedOperationStartButton);

        RestaurantPrepareManager.Instance.OnMenuChanged += UpdateTotalCountUI;
        UpdateTotalCountUI();
    }

    private void OnDestroy()
    {
        if(RestaurantPrepareManager.Instance != null)
        {
            RestaurantPrepareManager.Instance.OnMenuChanged -= UpdateTotalCountUI;
        }
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
        currentSelectedRecipe = recipe;
        recipeNameText.text = recipe.recipe_name;
        recipeDescText.text = RecipeManager.Instance.GetRecipeDescription(recipe.recipe_number - 1);
        recipeImage.sprite = recipe.recipe_image;

        foreach (Transform child in itemRequirementTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (var requirement in recipe.recipe_requireItems)
        {
            GameObject itemGO = Instantiate(requiredItemPrefab, itemRequirementTransform);
            UI_RecipeRequiredSlot slotUI = itemGO.GetComponent<UI_RecipeRequiredSlot>();
            if (slotUI != null)
            {
                int remain = WarehouseManager.Instance.GetCount(requirement.item);
                slotUI.SetData(requirement.item, requirement.count);
            }
        }

        // 최대 제조 가능 수량 계산
        currentRecipeMaxCookableCount = WarehouseManager.Instance.GetMaxCookableCount(recipe);
        // 이 레시피의 이미 저장된 수량 가져오기
        currentRecipeSavedCount = RestaurantPrepareManager.Instance.GetPlannedCount(recipe);
        // U! 조작용 수량을 저장된 값으로 초기화
        currentRecipeCount = currentRecipeSavedCount;
        // UI 갱신
        UpdatePlanUI();
    }

    private void UpdateTotalCountUI()
    {
        RestaurantPrepareManager rpm = RestaurantPrepareManager.Instance;
        totalRecipeCountText.text = $"{rpm.CurTotalPlannedCount} / {rpm.MaxTotalItemCount}";
    }

    private void UpdatePlanUI()
    {
        curRecipeCountText.text = currentRecipeCount.ToString();

        minus.interactable = currentRecipeCount > 0;

        bool canAddMore = true;
        // 1. 재고가 있는가?
        if (currentRecipeCount >= currentRecipeMaxCookableCount)
        {
            canAddMore = false;
        }

        // 2. 메뉴판에 자리가 있는가?
        // '현재 총 개수' = (전체 저장된 개수) - (이 아이템의 '저장된' 개수) + (이 아이템의 'UI상' 개수)
        int totalSaved = RestaurantPrepareManager.Instance.CurTotalPlannedCount;
        int maxTotal = RestaurantPrepareManager.Instance.MaxTotalItemCount;

        // (현재 UI에 반영된 값을 기준으로) 예상되는 총 개수
        int projectedTotalCount = totalSaved - currentRecipeSavedCount + currentRecipeCount;

        // 예상 총 개수가 한도보다 크거나 같으면 더 못 더함
        if (projectedTotalCount >= maxTotal)
        {
            canAddMore = false;
        }

        plus.interactable = canAddMore;
    }
    // 필요 재료 UI를 새로고침하는 함수 (OnClickConfirmPlan에서 호출)
    private void UpdateRequiredItemsUI(Recipe recipe)
    {
        foreach (Transform child in itemRequirementTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (var requirement in recipe.recipe_requireItems)
        {
            GameObject itemGO = Instantiate(requiredItemPrefab, itemRequirementTransform);
            UI_RecipeRequiredSlot slotUI = itemGO.GetComponent<UI_RecipeRequiredSlot>();
            if (slotUI != null)
            {
                slotUI.SetData(requirement.item, requirement.count);
                ShowRecipeInfo(recipe);
            }
        }
    }

    private void OnClickedPlusButton()
    {
        currentRecipeCount++;
        UpdatePlanUI();
    }

    private void OnClickedMinusButton()
    {
        currentRecipeCount--;
        UpdatePlanUI();
    }

    /// <summary>
    /// "결정" 버튼: 현재 UI의 수량을 RestaurantPrepareManager에 저장 '시도'
    /// </summary>
    private void OnClickedConfirmButton()
    {
        if (currentSelectedRecipe == null) return;

        // 1. 매니저에게 (레시피, 희망 수량)으로 갱신 '요청'
        bool success = RestaurantPrepareManager.Instance.UpdateMenuPlan(currentSelectedRecipe, currentRecipeCount);

        if (success)
        {
            currentRecipeSavedCount = currentRecipeCount;
            Debug.Log("계획이 저장되었습니다.");
        }
        else
        {
            currentRecipeCount = currentRecipeSavedCount;
            Debug.LogWarning("재료가 부족하거나 한도를 초과하여 계획을 되돌립니다.");
        }
        // 4. UI 갱신
        UpdatePlanUI();
        UpdateRequiredItemsUI(currentSelectedRecipe);
    }

    private void OnClickedOperationStartButton()
    {
        var menuList = RestaurantPrepareManager.Instance.TodayMenu;

        if (RestaurantPrepareManager.Instance.CurTotalPlannedCount == 0)
        {
            Debug.LogWarning("메뉴를 하나 이상 등록해야 합니다!");
            return;
        }
        NightPhaseManager.Instance.state = NightPhaseManager.RestaurantState.Open;
        // 1. 데이터 전달
        Debug.Log("--- 영업 시작! ---");
        SalesManager.Instance.SetMenus(menuList);
        UIManager.Instance.ClosePopupUI(this);
    }
}
