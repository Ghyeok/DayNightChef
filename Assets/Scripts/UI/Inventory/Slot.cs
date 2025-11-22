using NUnit;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
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
    private RectTransform rootCanvasRect;     // 루트 캔버스의 RectTransform
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Vector2 originalAnchoredPos;      // 시작 anchoredPosition
    private GameObject placeholder;           // 레이아웃 유지용
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

        if (rectTransform != null)
            originalScale = rectTransform.localScale;

        TryAutoWireChildren();
    }
    // UI 자동화
    public void TryAutoWireChildren()
    {
        if(itemImage == null)
            itemImage = transform.Find("ItemImage")?.GetComponent<Image>();
        if(countText == null)
            countText = transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();
        if(itemWeight == null)
            itemWeight = transform.Find("ItemWeight")?.GetComponent<TextMeshProUGUI>();
        if (itemShowPanel == null)
            itemShowPanel = transform.Find("ItemShowPanel")?.gameObject;
        if (itemShowPanel != null && itemText == null)
            itemText = itemShowPanel.transform.Find("ItemText")?.GetComponent<TextMeshProUGUI>();
    }
    // UI 갱신
    public void RefreshUI()
    {
        var inv = InventoryManager.Instance;
        if (inv == null || inv.Entries == null)
        {
            if (itemImage) { itemImage.enabled = false; itemImage.sprite = null; }
            if (countText) countText.text = string.Empty;
            if (itemWeight) itemWeight.text = string.Empty;
            if (itemText) itemText.text = string.Empty;
            return;
        }
        if (index < 0 || index >= inv.Entries.Count)
        {
            if (itemImage) { itemImage.enabled = false; itemImage.sprite = null; }
            if (countText) countText.text = string.Empty;
            if (itemWeight) itemWeight.text = string.Empty;
            if (itemText) itemText.text = string.Empty;
            return;
        }
        var e = InventoryManager.Instance.Entries[index];
        bool has = (e.item != null && e.count > 0); // 매니저에서 슬롯에 아이템 유무를 확인

        if(itemImage)
        {
            itemImage.enabled = has;
            itemImage.sprite = has ? e.item.item_image : null;
        }
        if(countText)
        {
            countText.text = (has && e.item.stackable) ? e.count.ToString() : string.Empty;
        }
        if(itemWeight)
        {
            float w = has ? e.item.item_weight * e.count : 0f;
            itemWeight.text = has ? $"{w:0.##}kg" : string.Empty;
        }
        if(itemText)
        {
            itemText.text = has ? e.item.item_name : string.Empty;
        }
    }

    private bool HasItem()
    {
        var inv = InventoryManager.Instance;
        if (inv == null || inv.Entries == null) return false;
        if (index < 0 || index >= inv.Entries.Count) return false;

        var e = inv.Entries[index];
        return (e.item != null && e.count > 0);
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

        // TODO: 툴팁 닫기
        // UITooltip.Hide();
        itemShowPanel.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        itemShowPanel.SetActive(false);
        isDragging = false; // 기본값

        var inv = InventoryManager.Instance;
        if (inv == null || inv.Entries == null) return;
        if (index < 0 || index >= inv.Entries.Count) return;
        if (inv.Entries[index].item == null) return; // 빈 슬롯 X
        if (!rootCanvasRect) return;

        isDragging = true;

        originalParent = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;

        // 레이아웃 유지용 placeholder 생성
        placeholder = new GameObject("Placeholder", typeof(LayoutElement));
        var myLE = GetComponent<LayoutElement>();
        var le = placeholder.GetComponent<LayoutElement>();
        if(myLE != null)
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

        placeholder.transform.SetParent(originalParent, false); // placeholder를 원래 부모에 넣기
        placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());// placeholder를 원래 위치에 넣기

        // 부모를 먼저 캔버스로 옮기고 로컬 좌표계 유지
        transform.SetParent(rootCanvas.transform, false);
        transform.SetAsLastSibling();

        // anchor/pivot 백업 후 센터로 통일
        oldAnchorMin = rectTransform.anchorMin;
        oldAnchorMax = rectTransform.anchorMax;
        oldPivot = rectTransform.pivot;

        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvasRect, e.position, uiCamera, out var lp))
        {
            rectTransform.anchoredPosition = lp;
        }

        // Drop 대상이 이벤트를 받도록 설정
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

        Debug.Log($"[Drag] Mouse:{e.position} Local:{rectTransform.anchoredPosition}");
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
        if (placeholder)
            Destroy(placeholder);
    }
    public void OnDrop(PointerEventData e)
    {
        var dragObj = e.pointerDrag;
        if (!dragObj) return;
        var fromSlot = dragObj.GetComponent<Slot>();
        if(!fromSlot || fromSlot == this) return; // 자기 자신이거나 슬롯이 아니면 무시
        var inv = InventoryManager.Instance;
        if (fromSlot.index < 0 || fromSlot.index >= inv.Entries.Count) return;
        if (inv == null || inv.Entries == null) return;
        if (index < 0 || index >= inv.Entries.Count) return;
        InventoryManager.Instance.SwapOrMerge(fromSlot.index, this.index);
    }
    private void ReturnToOriginalParent()
    {
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalAnchoredPos;
        if(placeholder)
            transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
    }
}
