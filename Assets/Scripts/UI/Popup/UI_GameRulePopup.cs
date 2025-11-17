using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class GameRule
{
    public Sprite RuleImage;
    public string RuleName;
    [HideInInspector] public string RuleText;
}

public class UI_GameRulePopup : UI_Popup
{
    // 🔹 코드에서 직접 관리할 룰 데이터 (이 부분만 수정하면 됨)
    private static readonly (string name, string text)[] RULE_DATA = new[]
    {
        ("게임 소개","Day&Night Chef는 낮 페이즈와 밤 페이즈가 반복되는 루프 기반의 어드벤처, 경영 시뮬레이션 게임입니다. " +
        "\r\n플레이어가 낮에는 직접 재료를 수급하고, 밤에는 그 재료로 식당을 운영합니다."),

        ("게임 소개","낮 페이즈에는 사냥, 낚시, 채집을 통해 세 가지 지역에서 다양한 재료를 모을 수 있습니다." +
        "\r\n밤 페이즈에서는 수급한 재료로 요리를 등록하고, 식당 운영을 통해 재화를 얻습니다."),

        ("낮 페이즈","게임에는 온대 → 열대 → 한대로 이어지는 세 가지 지역이 존재하며, 난이도가 점점 증가합니다." +
        "각 지역의 보스를 처치해야 다음 지역으로 갈 수 있습니다." +
        "\r\n지역마다 총 네 종류의 동물·생선·채집물이 등장하며, 낮은 확률로 희귀 재료도 획득할 수 있습니다."),

        ("사냥","플레이어는 공격 버튼을 통해 동물을 사냥할 수 있습니다." +
        "\r\n플레이어가 동물을 공격하면 공격 당한 동물은 플레이어를 따라다니면서 공격합니다." +
        "\r\n만약 플레이어의 HP가 0이 되면, 이번 낮 페이즈에서 모았던 재료들 중 한 가지를 제외한 모든 재료를 잃고 밤 페이즈로 넘어갑니다."),

        ("채집","플레이어가 채집류 근처로 가면 채집을 할 수 있습니다.\r\n한 구역당 최대 5개의 채집류가 스폰됩니다."),

        ("낚시","플레이어가 낚시 스팟 근처로 가면 낚시를 할 수 있습니다." +
        "\r\n낚싯대 레벨에 따라 플레이어가 낚을 수 있는 생선의 종류가 달라집니다." +
        "\r\n낚시 미니게임을 통해 생선을 획득하세요!"),

        ("낚시 미니게임","작살이 노란색 또는 주황색 칸을 가리킬 때 타이밍에 맞춰 버튼을 누르면 진행도가 올라갑니다." +
        "\r\n진행도를 100% 달성하면 성공입니다."),

        ("가방","각 재료마다 고유한 무게를 가지고 있습니다. 플레이어는 한 번의 낮 페이즈 마다 최대 제한된 무게만큼의 재료를 수급할 수 있습니다."),

        ("업그레이드","플레이어는 식당 영업을 통해 얻은 재화를 통해 최대 HP, 이동속도, 공격력, 가방 무게, 낚싯대, 식당 레벨을 업그레이드 할 수 있습니다." +
        "\r\n낚싯대를 업그레이드 하면 얻을 수 있는 생선의 종류가 늘어납니다." +
        "\r\n식당 레벨을 업그레이드 하면 밤 페이즈 때 판매할 수 있는 요리의 개수와 손님의 좌석 수가 늘어납니다." +
        "일정 평판을 넘겨야 식당 레벨을 업그레이드 할 수 있습니다."),

        ("창고","낮 페이즈에서 밤 페이즈로 넘어가게 되면 가방에 있던 재료들이 자동으로 창고에 저장됩니다."),

        ("식료품 상점","플레이어는 소금, 간장, 설탕의 세 가지 재료를 낮 페이즈에서 구매할 수 있습니다."),

        ("레시피 도감","식당에서 판매할 요리의 상세 설명과 필요 재료를 미리 확인할 수 있습니다."),

        ("밤 페이즈","밤 페이즈에서는 식당 운영 준비 단계와 식당 운영 단계로 나뉩니다."),

        ("식당 운영 준비 단계","식당 운영 준비 단계에서는 어떤 요리를 몇개 팔 것인가 미리 등록해놓습니다."),

        ("식당 운영 단계","식당 운영 단계에서는 등록한 요리들을 판매합니다." +
        "\r\n플레이어는 셰프 근처에서 서빙 버튼을 눌러 완성된 요리를 들고 알맞은 손님에게 서빙해야 합니다." +
        "\r\n서빙에 성공하면 평판이 오르고 재화를 획득합니다. 만약 알맞지 않은 요리를 서빙하거나, 일정 시간 내에 손님에게 서빙을 하지 못한다면 평판이 깎입니다."),

        ("정산", "모든 요리가 완성되었거나, 모든 손님에게 서빙을 완료한 경우 식당 영업이 종료되고 정산합니다."),

        ("관리비 납부","4번의 루프마다 식당 관리비를 납부해야 합니다. 게임이 진행될수록 관리비는 점점 증가합니다." +
        "\r\n관리비를 납부하지 못한다면 게임오버 됩니다.")
    };

    [Header("게임룰")]
    [SerializeField] private List<GameRule> gameRules = new();

    private int _index = 0;

    private Image _ruleImage;
    private TextMeshProUGUI _ruleText;
    private TextMeshProUGUI _ruleName;

    public enum Buttons
    {
        ExitBtn,
        NextBtn,
        PreBtn,
    }

    public enum Images
    {
        RuleImage,
    }

    public enum Texts
    {
        RuleText,
        RuleName,
    }

    public override void Init()
    {
        base.Init();

        ApplyRuleDataFromCode();

        // 바인딩
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        // 컴포넌트 가져오기
        _ruleImage = GetImage((int)Images.RuleImage);
        _ruleText = GetText((int)Texts.RuleText);
        _ruleName = GetText((int)Texts.RuleName);

        // 버튼 가져오기
        var exitBtn = GetButton((int)Buttons.ExitBtn);
        var nextBtn = GetButton((int)Buttons.NextBtn);
        var preBtn = GetButton((int)Buttons.PreBtn);

        // Exit 버튼
        if (exitBtn != null)
            UI_Base.AddUIEvent(exitBtn.gameObject, _ => UIManager.Instance.ClosePopupUI(this));

        // Next 버튼
        if (nextBtn != null)
        {
            UI_Base.AddUIEvent(nextBtn.gameObject, _ =>
            {
                if (gameRules == null || gameRules.Count == 0) return;

                _index++;
                if (_index >= gameRules.Count)
                    _index = gameRules.Count - 1;

                SetRulePage(_index);
            });
        }

        // Prev 버튼
        if (preBtn != null)
        {
            UI_Base.AddUIEvent(preBtn.gameObject, _ =>
            {
                if (gameRules == null || gameRules.Count == 0) return;

                _index--;
                if (_index < 0)
                    _index = 0;

                SetRulePage(_index);
            });
        }

        _index = 0;
        SetRulePage(_index);
    }

    private void ApplyRuleDataFromCode()
    {
        if (gameRules == null)
            gameRules = new List<GameRule>();

        for (int i = 0; i < RULE_DATA.Length; i++)
        {
            if (i >= gameRules.Count)
                gameRules.Add(new GameRule());

            gameRules[i].RuleName = RULE_DATA[i].name;
            gameRules[i].RuleText = RULE_DATA[i].text;
        }

        if (gameRules.Count > RULE_DATA.Length)
            gameRules.RemoveRange(RULE_DATA.Length, gameRules.Count - RULE_DATA.Length);
    }

    private void SetRulePage(int idx)
    {
        if (gameRules == null || gameRules.Count == 0)
        {
            if (_ruleText != null) _ruleText.text = "설정된 게임 룰이 없습니다.";
            if (_ruleName != null) _ruleName.text = "설정된 게임 룰이 없습니다.";
            if (_ruleImage != null) _ruleImage.sprite = null;
            return;
        }

        if (idx < 0) idx = 0;
        if (idx >= gameRules.Count) idx = gameRules.Count - 1;
        _index = idx;

        var rule = gameRules[_index];

        if (_ruleText != null)
            _ruleText.text = rule.RuleText;

        if (_ruleName != null)
            _ruleName.text = rule.RuleName;

        if (_ruleImage != null)
            _ruleImage.sprite = rule.RuleImage;
    }

    public static UI_GameRulePopup Show() =>
       UIManager.Instance.ShowPopupUI<UI_GameRulePopup>("UI_GameRulePopup");
}
