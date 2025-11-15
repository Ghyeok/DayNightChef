using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class GameRule
{
    public Sprite RuleImage;
    [TextArea]
    public string RuleText;
}

public class UI_GameRulePopup : UI_Popup
{
    [Header("게임룰")]
    [SerializeField] private List<GameRule> gameRules = new();

    private int _index = 0;

    private Image _ruleImage;
    private TextMeshProUGUI _ruleText;

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
    }

    public override void Init()
    {
        base.Init();

        // 바인딩
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        // 컴포넌트 가져오기
        _ruleImage = GetImage((int)Images.RuleImage);
        _ruleText = GetText((int)Texts.RuleText);

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
                    _index = 0; // 처음에서 막기

                SetRulePage(_index);
            });
        }

        // 처음 페이지 세팅
        _index = 0;
        SetRulePage(_index);
    }

    private void SetRulePage(int idx)
    {
        if (gameRules == null || gameRules.Count == 0)
        {
            if (_ruleText != null) _ruleText.text = "설정된 게임 룰이 없습니다.";
            if (_ruleImage != null) _ruleImage.sprite = null;
            return;
        }

        // 인덱스 보정
        if (idx < 0) idx = 0;
        if (idx >= gameRules.Count) idx = gameRules.Count - 1;
        _index = idx;

        var rule = gameRules[_index];

        if (_ruleText != null)
            _ruleText.text = rule.RuleText;

        if (_ruleImage != null)
            _ruleImage.sprite = rule.RuleImage;
    }

    public static UI_GameRulePopup Show() =>
       UIManager.Instance.ShowPopupUI<UI_GameRulePopup>("UI_GameRulePopup");
}