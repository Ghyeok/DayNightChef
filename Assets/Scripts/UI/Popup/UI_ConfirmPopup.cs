using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Collections;
public class UI_ConfirmPopup : UI_Popup
{
    public enum Buttons
    {
        LeftBtn,
        RightBtn,
    }

    public enum Texts
    {
        InfoText,
        RightText,
        LeftText,
    }

    private TaskCompletionSource<bool> _tcs;
    private bool _clicked;
    private bool _bound;
    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        _bound = true;
        
    }
    // confirm팝업을 띄움 true = 예, false = 아니오
    public static Task<bool> ShowAsync(
        string info, string right, string left)
    {
        var popup = UIManager.Instance.ShowPopupUI<UI_ConfirmPopup>("UI_ConfirmPopup");
        return popup.Setup(info, right, left);
    }

    private Task<bool> Setup(string info, string right, string left)
    {
        // 이전 TCS 초기화
        _tcs = new TaskCompletionSource<bool>();
        _clicked = false;
        if (_bound)
        {
            ApplyTextsAndWire(info, right, left);
        }
        else
        {
            StartCoroutine(SetupAfterBindCo(info, right, left));
        }

        return _tcs.Task;
    }

    private IEnumerator SetupAfterBindCo(string info, string right, string left)
    {
        while (!_bound) yield return null;
        ApplyTextsAndWire(info, right, left);
    }

    private void ApplyTextsAndWire(string info, string right, string left)
    {
        var infoText = GetText((int)Texts.InfoText);
        var rightText = GetText((int)Texts.RightText);
        var leftText = GetText((int)Texts.LeftText);

        if (infoText) infoText.text = info ?? string.Empty;
        if (rightText) rightText.text = string.IsNullOrEmpty(right) ? "예" : right;
        if (leftText) leftText.text = string.IsNullOrEmpty(left) ? "아니오" : left;

        var leftBtn = GetButton((int)Buttons.LeftBtn);
        var rightBtn = GetButton((int)Buttons.RightBtn);

        if (leftBtn)
        {
            leftBtn.onClick.RemoveAllListeners();
            leftBtn.onClick.AddListener(() => OnClick(true));
        }

        if (rightBtn)
        {
            rightBtn.onClick.RemoveAllListeners();
            rightBtn.onClick.AddListener(() => OnClick(false));
        }
    }

    private void OnClick(bool yes)
    {
        // 더블클릭 방지
        if (_clicked) return;
        _clicked = true;
        var leftBtn = GetButton((int)Buttons.LeftBtn);
        var rightBtn = GetButton((int)Buttons.RightBtn);
        if (leftBtn) leftBtn.interactable = false;
        if (rightBtn) rightBtn.interactable = false;

        _tcs?.TrySetResult(yes);

        UIManager.Instance.ClosePopupUI(this);

    }
    private void OnDisable()
    {
        if(_tcs != null && !_tcs.Task.IsCompleted)
        {
            // 팝업이 닫힐 때 결과가 설정되지 않으면 false로 설정
            _tcs.TrySetResult(false);
        }

        var leftBtn = GetButton((int)Buttons.LeftBtn);
        var rightBtn = GetButton((int)Buttons.RightBtn);
        leftBtn?.onClick.RemoveAllListeners();
        rightBtn?.onClick.RemoveAllListeners();
    }
}
