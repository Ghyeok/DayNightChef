using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class WarehouseSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Index")]
    public int index; // 슬롯 인덱스

    [Header("UI")]
    [SerializeField] private Image itemImage; // 아이템이미지
    [SerializeField] private TextMeshProUGUI countText; // 아이템갯수
    [SerializeField] private TextMeshProUGUI itemWeight; // 아이템무게
    [SerializeField] private GameObject itemShowPanel; // 아이템 하이라이트 패널
    [SerializeField] private TextMeshProUGUI itemText; // 아이템 이름 텍스트

    [Header("Drag Visual")]
    [SerializeField] private float draggingAlpha = 0.7f;

    private Canvas rootCanvas;
    private RectTransform rootCanvasRect;      // 루트 캔버스의 RectTransform
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Vector2 originalAnchoredPos;       // 시작 anchoredPosition
    private GameObject placeholder;             // 레이아웃 유지용
    private bool isDragging;

    private Vector2 dragOffset;
    private Camera uiCamera;

    // anchor/pivot 백업용
    private Vector2 oldAnchorMin, oldAnchorMax, oldPivot;
    // 하이라이트용 원래 스케일
    private Vector3 originalScale = Vector3.one;

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>(true);
        if (rootCanvas != null)
        {
            rootCanvasRect = rootCanvas.transform as RectTransform;
            uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        }
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        TryAutoWireChildren();
    }

    // UI 자동화
    public void TryAutoWireChildren()
    {
        if (itemImage == null)
            itemImage = transform.Find("ItemImage")?.GetComponent<Image>();
        if (countText == null)
            countText = transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();
        if (itemWeight == null)
            itemWeight = transform.Find("ItemWeight")?.GetComponent<TextMeshProUGUI>();
        if (itemShowPanel == null)
            itemShowPanel = transform.Find("ItemShowPanel")?.gameObject;
        if (itemShowPanel != null && itemText == null)
            itemText = itemShowPanel.transform.Find("ItemText")?.GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// UI 갱신 (WarehouseManager 기준)
    /// </summary>
    public void RefreshUI()
    {
        var wh = WarehouseManager.Instance;
        if (wh == null || wh.Slots == null)
        {
            if (itemImage) { itemImage.enabled = false; itemImage.sprite = null; }
            if (countText) countText.text = string.Empty;
            if (itemWeight) itemWeight.text = string.Empty;
            return;
        }

        // 인덱스가 창고 슬롯 범위를 벗어나는지 확인
        if (index < 0 || index >= wh.Slots.Count)
        {
            if (itemImage) { itemImage.enabled = false; itemImage.sprite = null; }
            if (countText) countText.text = string.Empty;
            if (itemWeight) itemWeight.text = string.Empty;
            return;
        }

        // WarehouseManager의 데이터로 UI 갱신
        var e = wh.Slots[index];
        bool has = (e.item != null && e.count > 0);

        if (itemImage)
        {
            itemImage.enabled = has;
            itemImage.sprite = has ? e.item.item_image : null;
        }
        if (countText)
        {
            countText.text = (has && e.item.stackable) ? e.count.ToString() : string.Empty;
        }
        if (itemWeight)
        {
            float w = has ? e.item.item_weight * e.count : 0f;
            itemWeight.text = has ? $"{w:0.##}kg" : string.Empty;
        }
        if (itemText)
        {
            itemText.text = has ? e.item.item_name : string.Empty;
        }
    }
    //포인터/드래그(터치+마우스)
    public void OnPointerEnter(PointerEventData e)
    {
        if (!HasItem() || isDragging) return;

        if (rectTransform != null)
            rectTransform.localScale = originalScale * 1.05f;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (itemImage != null)
        {
            var c = itemImage.color;
            itemImage.color = new Color(c.r, c.g, c.b, 1f);
        }

        itemShowPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (isDragging) return;

        if (rectTransform != null)
            rectTransform.localScale = originalScale;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        itemShowPanel.SetActive(false);
    }

    private bool HasItem()
    {
        var wh = WarehouseManager.Instance;
        if (wh == null || wh.Slots == null) return false;
        if (index < 0 || index >= wh.Slots.Count) return false;

        var e = wh.Slots[index];
        return (e.item != null && e.count > 0);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        isDragging = false; // 기본값

        var wh = WarehouseManager.Instance;
        if (wh == null || wh.Slots == null) return;
        if (index < 0 || index >= wh.Slots.Count) return;
        if (wh.Slots[index].item == null) return; // 빈 슬롯 드래그 불가
        if (!rootCanvasRect) return;

        isDragging = true;

        originalParent = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;

        placeholder = new GameObject("Placeholder", typeof(LayoutElement));
        var myLE = GetComponent<LayoutElement>();
        var le = placeholder.GetComponent<LayoutElement>();
        if (myLE != null)
        {
            le.minWidth = myLE.minWidth; le.minHeight = myLE.minHeight;
            le.preferredWidth = myLE.preferredWidth; le.preferredHeight = myLE.preferredHeight;
            le.flexibleWidth = myLE.flexibleWidth; le.flexibleHeight = myLE.flexibleHeight;
        }
        else
        {
            var rt = rectTransform;
            le.preferredHeight = rt.rect.height;
            le.preferredWidth = rt.rect.width;
        }

        placeholder.transform.SetParent(originalParent, false);
        placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());

        transform.SetParent(rootCanvas.transform, false);
        transform.SetAsLastSibling();

        oldAnchorMin = rectTransform.anchorMin;
        oldAnchorMax = rectTransform.anchorMax;
        oldPivot = rectTransform.pivot;

        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvasRect, e.position, uiCamera, out var lp))
        {
            rectTransform.anchoredPosition = lp;
        }

        if (canvasGroup)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = draggingAlpha;
        }
    }

    public void OnDrag(PointerEventData e)
    {
        if (!isDragging) return;
        if (rootCanvas == null || rectTransform == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvasRect, e.position, uiCamera, out var lp))
        {
            rectTransform.anchoredPosition = lp;
        }
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!isDragging) return;

        // anchor/pivot 원상복귀
        rectTransform.anchorMin = oldAnchorMin;
        rectTransform.anchorMax = oldAnchorMax;
        rectTransform.pivot = oldPivot;

        // 드롭 실패시 원래 위치로 복귀
        ReturnToOriginalParent();
        if (canvasGroup)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        if (placeholder)
        {
            Destroy(placeholder);
            placeholder = null;
        }

        isDragging = false;
    }

    /// <summary>
    /// WarehouseSlot <-> WarehouseSlot 간의 드롭 처리
    /// </summary>
    public void OnDrop(PointerEventData e)
    {
        var dragObj = e.pointerDrag;
        if (!dragObj) return;

        // 드래그한 객체가 WarehouseSlot인지 확인
        var fromSlot = dragObj.GetComponent<WarehouseSlot>();
        if (fromSlot == null || fromSlot == this) return; // WarehouseSlot이 아니거나, 자기 자신이면 무시

        var wh = WarehouseManager.Instance;
        if (wh == null || wh.Slots == null) return;

        // 인덱스 유효성 검사
        if (fromSlot.index < 0 || fromSlot.index >= wh.Slots.Count) return;
        if (this.index < 0 || this.index >= wh.Slots.Count) return;

        // WarehouseManager에 슬롯 교환/합치기 요청
        wh.SwapOrMergeSlots(fromSlot.index, this.index);
    }

    private void ReturnToOriginalParent()
    {
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalAnchoredPos;
        if (placeholder)
            transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
    }
}