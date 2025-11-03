using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ReadyQueueUI : MonoBehaviour
{
    [Header("슬롯들이 놓일 부모")]
    [SerializeField] private Transform contentRoot;

    [Header("ChefSlot 프리팹")]
    [SerializeField] private ChefSlot slotPrefab;

    private readonly Dictionary<int, ChefSlot> _map = new();

    private void OnEnable()
    {
        var sales = SalesManager.Instance;
        if (!sales) return;

        sales.OnOrderReady += HandleOrderReady;     // 완료 시 슬롯 추가
        sales.OnReadyDequeued += HandleReadyDequeued;  // 픽업 시 슬롯 제거
        sales.OnOrderRemoved += HandleOrderRemoved;   // 취소/롤백 시 슬롯 제거
    }

    private void OnDisable()
    {
        var sales = SalesManager.Instance;
        if (!sales) return;

        sales.OnOrderReady -= HandleOrderReady;
        sales.OnReadyDequeued -= HandleReadyDequeued;
        sales.OnOrderRemoved -= HandleOrderRemoved;
    }

    private void HandleOrderReady(SalesManager.Order od)
    {
        if (_map.ContainsKey(od.orderId)) return;
        var slot = Instantiate(slotPrefab, contentRoot);
        slot.gameObject.SetActive(true);
        slot.Bind(od);
        // 최신 항목을 맨위로
        slot.transform.SetAsFirstSibling();
        _map.Add(od.orderId, slot);
        var cg = slot.GetComponent<CanvasGroup>();
        if (cg)
        {
            cg.alpha = 0f;
            cg.DOFade(1f, 0.2f);
            slot.transform.localScale = Vector3.zero;
            slot.transform.DOScale(Vector3.one, 0.2f);
        }
    }

    private void HandleReadyDequeued(SalesManager.Order od)
    {
        RemoveSlot(od.orderId);
    }

    private void HandleOrderRemoved(SalesManager.Order od)
    {
        RemoveSlot(od.orderId);
    }

    private void RemoveSlot(int orderId)
    {
        if (!_map.TryGetValue(orderId, out var slot) || slot == null)
        {
            _map.Remove(orderId);
            return;
        }

        _map.Remove(orderId);

        // 페이드 아웃 후 파괴
        var cg = slot.GetComponent<CanvasGroup>();
        if (cg)
        {
            cg.DOFade(0f, 0.15f).OnComplete(() =>
            {
                if (slot) Destroy(slot.gameObject);
            });
        }
        else
        {
            Destroy(slot.gameObject);
        }

    }
}
