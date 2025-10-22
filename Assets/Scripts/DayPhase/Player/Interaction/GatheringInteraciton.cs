using System;
using UnityEngine;

public class GatheringInteraciton : MonoBehaviour, IInteract
{
    [Header("Drop Context")]
    [SerializeField] private ItemType itemType = ItemType.Gather;
    [SerializeField] private int tier = 1;

    public event Action<GatheringInteraciton> OnCollected;

    // (선택) 스포너가 런타임으로 세팅할 때 사용
    public void SetContext(ItemType type, int t) { itemType = type; tier = t; }

    public DayPhaseManager.PlayerBehavior GetBehaviorType()
        => DayPhaseManager.PlayerBehavior.Gathering;

    public void Interact(GameObject interactor)
    {
        var map = DayPhaseManager.Instance.curMapType;
        Item[] candidates = ItemManager.Instance.GetItemList(itemType, map, tier);

        Item result = null;
        if (candidates != null && candidates.Length > 0)
            result = ItemManager.Instance.GetGatherItem(map, itemType, candidates);
        else
            Debug.LogWarning($"[Gathering] 후보가 없습니다. (map:{map}, type:{itemType}, tier:{tier})");

        OnCollected?.Invoke(this);
        Destroy(gameObject);

        if (result != null) Debug.Log($"{result.item_name} 획득!");
    }
}