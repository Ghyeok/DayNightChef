using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class StoreSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Index")]
    public int Index; // 슬롯 인덱스

    [Header("UI")]
    [SerializeField] private Image StoreitemImage; // 아이템이미지
    [SerializeField] private TextMeshProUGUI StoreItemName; // 아이템이름
    [SerializeField] private TextMeshProUGUI StoreItemPrice; // 아이템가격

    private Canvas rootCanvas;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>(true);
        TryAutoWire();
    }
    private void TryAutoWire()
    {
        if(StoreitemImage == null)
            StoreitemImage = transform.Find("StoreItemImage")?.GetComponent<Image>();
        if(StoreItemName == null)
            StoreItemName = transform.Find("StoreItemName")?.GetComponent<TextMeshProUGUI>();
        if(StoreItemPrice == null)
            StoreItemPrice = transform.Find("StoreItemPrice")?.GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }
}
