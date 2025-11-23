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

    [Header("Chef Progress (조리 진행바)")]
    [SerializeField] private CanvasGroup chefProgressGroup;
    [SerializeField] private UnityEngine.UI.Image chefProgressFill;

    private Tween _cookTween;
    private SalesManager.Order _currentCookingOrder;

    private float GetCookSeconds()
    {
        var sales = SalesManager.Instance;
        if (sales == null) return 3f;
        try { return sales.CookSeconds; } catch { return 3f; }
    }

    private void ShowChefProgress(bool on, float alpha = 1f)
    {
        if (!chefProgressGroup) return;
        var go = chefProgressGroup.gameObject;
        if (go.activeSelf != on)
            go.SetActive(on);
        chefProgressGroup.alpha = on ? alpha : 0f;
        chefProgressGroup.interactable = on; 
        chefProgressGroup.blocksRaycasts = on;
        if (on)
            chefProgressGroup.transform.SetAsLastSibling();
    }

    private void SetChefFill(float v)
    {
        if (chefProgressFill) chefProgressFill.fillAmount = Mathf.Clamp01(v);
    }

    private void OnEnable()
    {
        var sales = SalesManager.Instance;
        if (!sales) return;

        // Ready 큐 UI
        sales.OnOrderReady += HandleOrderReady;      // 완료 시 슬롯 추가
        sales.OnReadyDequeued += HandleReadyDequeued;   // 픽업 시 슬롯 제거
        sales.OnOrderRemoved += HandleOrderRemoved;    // 취소/롤백 시 슬롯 제거

        // Chef 진행바 UI
        sales.OnOrderStarted += HandleOrderStarted;    // 조리 시작 → 진행바 시작
        sales.OnOrderReady += HandleOrderReadyBar;   // 조리 완료 → 진행바 마무리
        sales.OnOrderRemoved += HandleOrderRemovedBar; // 취소/고객 소멸 → 진행바 정리

        // 초기 상태 정리
        KillCookTween();
        SetChefFill(0f);
        ShowChefProgress(false);
    }

    private void OnDisable()
    {
        var sales = SalesManager.Instance;
        if (!sales) return;

        sales.OnOrderReady -= HandleOrderReady;
        sales.OnReadyDequeued -= HandleReadyDequeued;
        sales.OnOrderRemoved -= HandleOrderRemoved;

        sales.OnOrderStarted -= HandleOrderStarted;
        sales.OnOrderReady -= HandleOrderReadyBar;
        sales.OnOrderRemoved -= HandleOrderRemovedBar;

        KillCookTween();
    }

    private void HandleOrderReady(SalesManager.Order od)
    {
        if (_map.ContainsKey(od.orderId)) return;
        var slot = Instantiate(slotPrefab, contentRoot);
        slot.gameObject.SetActive(true);
        slot.Bind(od);
        // 최신 항목을 맨 위로
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

    private void HandleOrderStarted(SalesManager.Order od)
    {
        _currentCookingOrder = od;
        KillCookTween();

        float duration = Mathf.Max(0.05f, GetCookSeconds());
        SetChefFill(0f);

        ShowChefProgress(true, 1f);
        DebugCanvasGroups();
        _cookTween = DG.Tweening.DOTween.To(
            () => chefProgressFill ? chefProgressFill.fillAmount : 0f,
            v => SetChefFill(v),
            1f,
            duration
        ).SetEase(DG.Tweening.Ease.Linear);
    }

    private void HandleOrderReadyBar(SalesManager.Order od)
    {
        if (_currentCookingOrder == od) _currentCookingOrder = null;
        SetChefFill(1f);
        KillCookTween();

        if (chefProgressGroup)
        {
            var seq = DG.Tweening.DOTween.Sequence();
            seq.Append(chefProgressGroup.DOFade(1f, 0.05f));
            seq.Append(chefProgressGroup.DOFade(0.6f, 0.08f));
            seq.Append(chefProgressGroup.DOFade(1f, 0.08f));
            seq.AppendInterval(0.12f);
            seq.Append(chefProgressGroup.DOFade(0f, 0.2f))
               .OnComplete(() =>
               {
                   SetChefFill(0f);
                   ShowChefProgress(false); // 진행바 그룹만 숨김
               });
        }
        else
        {
            ShowChefProgress(false);
            SetChefFill(0f);
        }
    }

    private void HandleOrderRemovedBar(SalesManager.Order od)
    {
        if(_currentCookingOrder != od)
            return;

        KillCookTween();
        _currentCookingOrder = null;
        SetChefFill(0f);
        ShowChefProgress(false);
    }

    private void KillCookTween()
    {
        if (_cookTween != null && _cookTween.IsActive())
        {
            _cookTween.Kill();
            _cookTween = null;
        }
    }

    private void DebugCanvasGroups()
    {
        if (!chefProgressGroup) return;

        var groups = chefProgressGroup.GetComponentsInParent<CanvasGroup>(true);
        foreach (var g in groups)
        {
            Debug.Log($"[CG] {g.name} alpha={g.alpha}, active={g.gameObject.activeInHierarchy}");
        }
    }
}
