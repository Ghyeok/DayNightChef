using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SettlementPopup : UI_Popup
{
    [Header("판매 레시피")]
    [SerializeField] private SellSlot sellSlotPrefab;
    [SerializeField] private Transform sellSlotTransform;

    [Header("영업 결과")]
    [SerializeField] private TextMeshProUGUI currentPrice;
    [SerializeField] private TextMeshProUGUI totalPrice;
    [SerializeField] private TextMeshProUGUI reputation;
    [SerializeField] private Button confirmButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        SetMenusData();
        SetResultData();
        confirmButton.onClick.AddListener(OnClickedConfirmButton);
    }

    private void SetMenusData()
    {
        List<MenuPlan> menu = SalesManager.Instance.TodayMenus;

        foreach (MenuPlan menuPlan in menu)
        {
            if (menuPlan != null)
            {
                SellSlot sellSlot = Instantiate(sellSlotPrefab, sellSlotTransform);
                sellSlot.SetData(menuPlan);
            }
            else
            {
                Debug.Log("MenuPlan이 없습니다.");
            }
        }
    }

    private void SetResultData()
    {
        currentPrice.text = $"영업 전 골드 : {GameManager.Instance.totalGold - SalesManager.Instance.GoldAccrued()}G";
        totalPrice.text = $"영업 후 골드 : {GameManager.Instance.totalGold}G";
        reputation.text = $"현재 평판 : {GameManager.Instance.restaurantReputation}";
    }

    private void OnClickedConfirmButton()
    {
        UIManager.Instance.ClosePopupUI(this);
        SceneLoader.Instance.LoadScene("DayPhaseScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
