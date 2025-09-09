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
    [Header("Drag Visual")]
    [SerializeField] private float draggingAlpha = 0.7f;
    private Canvas canvas; // 최상위 캔버스
    private RectTransform rectTransform; // 슬롯 위치
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalAnchoredPos;
    private GameObject placeholder; // 드래그시 레이아웃 유지를 위한 빈 오브젝트
    private bool isDragging;
    void Awake()
    {
        canvas = GetComponentInParent<Canvas>(true);
        if (canvas == null)
        {
            Debug.LogError("[Slot] 상위에서 Canvas를 찾지 못했습니다. 이 Slot은 반드시 Canvas 하위에 있어야 합니다.", this);
        }
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
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
            return;
        }
        if (index < 0 || index >= inv.Entries.Count)
        {
            Debug.LogWarning($"[Slot] 인덱스 범위 초과 index={index}, entries={inv.Entries.Count}", this);
            if (itemImage) { itemImage.enabled = false; itemImage.sprite = null; }
            if (countText) countText.text = string.Empty;
            if (itemWeight) itemWeight.text = string.Empty;
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
    }
    //포인터/드래그(터치+마우스)
    public void OnPointerEnter(PointerEventData e)
    {
        // 아이템 터치 했을때 표시 (툴팁 & 하이라이트)
    }

    public void OnPointerExit(PointerEventData e)
    {
        // 아이템 터치 땠을때 표시 (툴팁 & 하이라이트 해체)
    }

    public void OnBeginDrag(PointerEventData e)
    {
        isDragging = false; // 기본값

        var inv = InventoryManager.Instance;
        if (inv == null || inv.Entries == null) return;
        if (index < 0 || index >= inv.Entries.Count) return;

        var entry = inv.Entries[index];
        if (entry.item == null) return; // 빈 슬롯이면 드래그 시작 안 함

        if (canvas == null)
        {
            Debug.LogError("[Slot] Canvas가 없어 드래그를 시작할 수 없습니다.", this);
            return;
        }
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
        {
            // LayoutElement가 없다면 Rect 크기로 대강 맞춰줌
            var rt = GetComponent<RectTransform>();
            if (rt)
            {
                le.preferredWidth = rt.rect.width;
                le.preferredHeight = rt.rect.height;
            }
        }
        placeholder.transform.SetParent(originalParent, false); // placeholder를 원래 부모에 넣기
        placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());// placeholder를 원래 위치에 넣기
        // 캔버스 최상단으로 올려 자유 이동
        transform.SetParent(canvas.transform, false);
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
        if (canvas == null || rectTransform == null) return;
        rectTransform.anchoredPosition += e.delta / canvas.scaleFactor; // 캔버스 스케일 보정
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!isDragging) return;
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
