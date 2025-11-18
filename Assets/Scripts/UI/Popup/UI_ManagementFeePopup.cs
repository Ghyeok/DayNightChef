using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 4, 8, 12 ... 4의 배수 주차
/// </summary>
public class UI_ManagementFeePopup : UI_Popup
{
    [SerializeField] private TextMeshProUGUI managementFeeText;
    [SerializeField] private TextMeshProUGUI currentPrice;
    [SerializeField] private TextMeshProUGUI totalPrice;
    [SerializeField] private Button confirmButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        SetManagementFeeText();
        confirmButton.onClick.AddListener(OnClickedConfirmButton);
    }

    private void SetManagementFeeText()
    {
        managementFeeText.text = $"현재 관리비 : {GameManager.Instance.ManagementFee()}G / 다음 관리비 : {GameManager.Instance.ManagementFee() + GameManager.Instance.baseFee}G";
        currentPrice.text = $"납부 전 골드 : {GameManager.Instance.totalGold}G";
        totalPrice.text = $"납부 후 골드 : {GameManager.Instance.totalGold - GameManager.Instance.ManagementFee()}G";
    }

    private void OnClickedConfirmButton()
    {
        GameManager.Instance.PayManagementFee();
        UIManager.Instance.ClosePopupUI(this);
    }
}
