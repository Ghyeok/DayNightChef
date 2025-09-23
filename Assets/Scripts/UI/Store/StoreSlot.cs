using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class StoreSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Index")]
    public int Index; // 슬롯 인덱스

    [Header("UI")]
    [SerializeField] private Image StoreItemImage; // 아이템이미지
    [SerializeField] private TextMeshProUGUI StoreItemName; // 아이템이름
    [SerializeField] private TextMeshProUGUI StoreItemPrice; // 아이템가격

    private Item _item;
    private bool _interactable = true; // 중복 클릭 방지용

    private void Awake()
    {
        TryAutoWire();
    }
    private void TryAutoWire()
    {
        if(StoreItemImage == null)
            StoreItemImage = transform.Find("StoreItemImage")?.GetComponent<Image>();
        if(StoreItemName == null)
            StoreItemName = transform.Find("StoreItemName")?.GetComponent<TextMeshProUGUI>();
        if(StoreItemPrice == null)
            StoreItemPrice = transform.Find("StoreItemPrice")?.GetComponent<TextMeshProUGUI>();
        if(GetComponent<CanvasGroup>() == null) gameObject.AddComponent<CanvasGroup>();
    }

    public void SetData(Item item, int index)
    {
        _item = item;
        Index = index;

        if (StoreItemImage) StoreItemImage.sprite = item ? item.item_image : null;
        if (StoreItemName) StoreItemName.text = item ? item.item_name : null;
        if (StoreItemPrice) StoreItemPrice.text = item ? item.item_price.ToString() : null;

        SetInteractable(item != null);
    }

    public void SetInteractable(bool v)
    {
        _interactable = v;
        var cg = GetComponent<CanvasGroup>();
        if (cg)
        {
            cg.alpha = v ? 1f : 0.5f;
            cg.interactable = v;
            cg.blocksRaycasts = v;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_interactable || _item == null) return;

        SetInteractable(false); // 중복 클릭 방지
        bool ok = StoreManager.Instance.TryPurchase(Index);
        if (!ok) SetInteractable(true); // 구매 실패시 다시 활성화

        StartCoroutine(ReEnableAfter(0.5f)); // 0.5초 후 다시 활성화
    }

    private IEnumerator ReEnableAfter(float s)
    {
        yield return new WaitForSecondsRealtime(s);

        if (this != null && gameObject != null) SetInteractable(true);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
