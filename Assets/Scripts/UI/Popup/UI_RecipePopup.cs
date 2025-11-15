using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RecipePopup : UI_Popup
{
    [Header("Prefabs")]
    [Tooltip("레시피 목록에 표시될 슬롯 프리팹")]
    [SerializeField] private GameObject recipeSlotPrefab;
    [Tooltip("필요 재료를 표시할 슬롯 프리팹")]
    [SerializeField] private GameObject requiredItemPrefab;

    private List<Recipe> recipeList;
    private Recipe currentSelectedRecipe;

    public enum Buttons
    {
        ExitButton, // 닫기 버튼
    }

    public enum GameObjects
    {
        ContentTransform, // 레시피 슬롯이 생성될 스크롤뷰 Content
        ItemRequirementTransform, // 필요 재료 슬롯이 생성될 루트
    }

    public enum Images
    {
        RecipeImage, // 선택된 레시피 이미지
    }

    public enum Texts
    {
        RecipeNameText, // 선택된 레시피 이름
        RecipeDescText, // 선택된 레시피 설명
    }

    public override void Init()
    {
        base.Init();

        // 컴포넌트 자동 바인딩
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        // 닫기 버튼 이벤트 연결
        var exitBtn = GetButton((int)Buttons.ExitButton);
        if (exitBtn != null)
        {
            AddUIEvent(exitBtn.gameObject, _ => UIManager.Instance.ClosePopupUI(this));
        }
        else
        {
            Debug.LogError("[UI_RecipePopup] ExitButton을 찾지 못했습니다! 이름을 확인하세요.");
        }

        // 프리팹 연결 확인
        if (recipeSlotPrefab == null || requiredItemPrefab == null)
        {
            Debug.LogError("[UI_RecipePopup] Prefab이 인스펙터에 연결되지 않았습니다!");
        }

        // 레시피 목록 생성
        InstantiateRecipeSlot();
    }

    /// <summary>
    /// RecipeManager에서 모든 레시피를 가져와 왼쪽 스크롤 목록을 생성합니다.
    /// </summary>
    private void InstantiateRecipeSlot()
    {
        recipeList = RecipeManager.Instance.GetAllRecipes();
        if (recipeList == null || recipeList.Count == 0)
        {
            Debug.LogError("[UI_RecipePopup] Recipe list is empty!");
            return;
        }

        Transform contentTransform = Get<GameObject>((int)GameObjects.ContentTransform)?.transform;
        if (contentTransform == null)
        {
            Debug.LogError("[UI_RecipePopup] ContentTransform을 찾지 못했습니다! 이름을 확인하세요.");
            return;
        }

        // 기존 슬롯이 있다면 모두 삭제
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // 새 슬롯 생성 및 이벤트 연결
        foreach (Recipe recipe in recipeList)
        {
            if (recipeSlotPrefab == null) continue;

            GameObject slotGO = Instantiate(recipeSlotPrefab, contentTransform);
            UI_RecipeSlot slotUI = slotGO.GetComponent<UI_RecipeSlot>();
            if (slotUI != null)
            {
                slotUI.SetData(recipe);
            }

            Button slotBtn = slotGO.GetComponent<Button>();
            if (slotBtn != null)
            {
                Recipe curRecipe = slotUI.recipe; // 리스너를 위해 레시피 캡처
                slotBtn.onClick.RemoveAllListeners();
                slotBtn.onClick.AddListener(() => ShowRecipeInfo(curRecipe));
            }
        }

        // 기본으로 첫 번째 레시피 정보 표시
        ShowRecipeInfo(recipeList[0]);
    }

    /// <summary>
    /// 선택된 레시피의 상세 정보(이름, 설명, 이미지, 필요 재료)를 UI에 표시합니다.
    /// </summary>
    private void ShowRecipeInfo(Recipe recipe)
    {
        if (recipe == null) return;

        currentSelectedRecipe = recipe;

        // 1. 레시피 기본 정보(이름, 설명, 이미지) 설정
        GetText((int)Texts.RecipeNameText).text = recipe.recipe_name;
        GetText((int)Texts.RecipeDescText).text = RecipeManager.Instance.GetRecipeDescription(recipe.recipe_number - 1);
        GetImage((int)Images.RecipeImage).sprite = recipe.recipe_image;

        Transform itemReqRoot = Get<GameObject>((int)GameObjects.ItemRequirementTransform)?.transform;
        if (itemReqRoot == null)
        {
            Debug.LogError("[UI_RecipePopup] ItemRequirementTransform을 찾지 못했습니다! 이름을 확인하세요.");
            return;
        }


        // 2. 기존 필요 재료 슬롯 삭제
        foreach (Transform child in itemReqRoot)
        {
            Destroy(child.gameObject);
        }

        // 3. 새 필요 재료 슬롯 생성
        foreach (var requirement in recipe.recipe_requireItems)
        {
            if (requiredItemPrefab == null) continue;

            GameObject itemGO = Instantiate(requiredItemPrefab, itemReqRoot);
            UI_RecipeRequiredSlot slotUI = itemGO.GetComponent<UI_RecipeRequiredSlot>();
            if (slotUI != null)
            {
                // UI_RecipeRequiredSlot이 스스로 창고 재고를 확인합니다.
                slotUI.SetData(requirement.item, requirement.count);
            }
        }
    }

    public static UI_RecipePopup Show()
    {
        return UIManager.Instance.ShowPopupUI<UI_RecipePopup>("UI_RecipePopup");
    }
}