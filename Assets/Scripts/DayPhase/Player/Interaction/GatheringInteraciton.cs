using System;
using System.Collections;
using UnityEngine;

public class GatheringInteraciton : MonoBehaviour, IInteract
{
    [SerializeField] private Item dropItem;
    public event Action<GatheringInteraciton> OnCollected;

    public void SetItem(Item item) => dropItem = item;

    public DayPhaseManager.PlayerBehavior GetBehaviorType()
    {
        return DayPhaseManager.PlayerBehavior.Gathering;
    }

    public void Interact(GameObject interactor)
    {
        if (dropItem == null) return;

        var map = DayPhaseManager.Instance.curMapType;
        ItemManager.Instance.GetGatherItem(map, ItemType.Gather, dropItem);

        OnCollected?.Invoke(this);
        Destroy(gameObject);
        Debug.Log($"{dropItem.item_name} 획득!");
    }
}
