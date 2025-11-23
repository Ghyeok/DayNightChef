using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeManager : SingletonManager<RecipeManager>
{
    [SerializeField]
    private List<Recipe> recipes = new();

    private string[] recipeDescriptions =
    {
    "은은한 허브향이 어우러진 부드러운 스테이크",
    "깊은 뼈 육수 맛의 고급 수프",
    "달콤한 산딸기 소스로 향을 더한 강한 풍미",
    "향긋한 소스로 구운 정통 바비큐",
    "매운 향신료로 끓인 사나운 스튜",
    "호랑이 가죽 육포",
    "달콤하면서 매운 향의 열대 카레",
    "쫄깃한 식감이 특징인 고급 육류 요리",
    "야생의 향을 살린 매운 구이 요리",
    "체력을 올려주는 스페셜 수프",
    "묵직한 국물 맛의 대형 요리",
    "이국적인 식감과 향이 나는 특제 구이",
    "뼈 속까지 달콤짭짤한 맛이 배어든 부드러운 찜 요리",
    "바다 내음이 느껴지는 해산물풍 고기요리",
    "상쾌한 향이 도는 희귀 스테이크",
    "시원한 냉 스튜, 극지방 한정 요리",
    "냉기의 정수가 깃든 전설의 스테이크",
    "체력 회복 효과가 뛰어난 특급 정식",
    "가벼운 식전 샐러드",
    "피로 회복용 향긋한 차",
    "상큼한 디저트 파이",
    "고급 손님용 한정 디저트",
    "열대와 냉기의 만남, 시원한 디저트",
    "폭발적 향신료 향의 스페셜 스프",
    "부드럽고 달콤한 디저트 음료",
    "강력한 해독 효과를 가진 위험한 술",
    "차갑지만 바삭한 특제 튀김",
    "상쾌한 청량감의 주스",
    "차가운 디저트 젤리",
    "회복과 저항력을 높여주는 수프",
    "담백한 생선 구이",
    "짭조름한 밥도둑 요리",
    "매운 장어 요리",
    "감전 주의! 톡 쏘는 튀김",
    "최고급 식재료로 만든 정식",
    "강한 풍미의 자극적 요리",
    "상큼한 열대 해산물 덮밥",
    "단단한 껍질 속 부드러운 살코기 요리",
    "생명을 건 고급 스튜",
    "강한 향의 특선 요리",
    "부드러운 구운 연어와 크리미한 코코넛 소스가 조화로운 고급 요리",
    "시원한 서리열매 소스를 곁들인 차가운 생선 구이 형태의 겨울 별미.",
    "허브와 고사리를 넣어 향긋하고 따뜻한 국물이 특징인 송어 스튜"
};

    public override void Awake()
    {
        base.Awake();

        if (recipes != null || recipes.Count != 0)
            recipes = Resources.LoadAll<Recipe>("Recipes").ToList();
    }

    public List<Recipe> GetAllRecipes() { return recipes; }
    public string GetRecipeDescription(int index) { return recipeDescriptions[index]; }

    public Recipe GetRecipeByName(string name)
    {
        // 모든 레시피 중에서 이름이 같은 것을 찾아서 반환
        return recipes.Find(r => r.recipe_name == name);
    }
}
