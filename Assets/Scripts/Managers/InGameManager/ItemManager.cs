using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : SingletonManager<ItemManager>
{
    [SerializeField]
    private List<Item> dropItems = new(); // 존재하는 모든 아이템을 갖고 있는 리스트

    private readonly int minItemCount = 1;
    private readonly int maxItemCount = 3;

    public override void Awake()
    {
        base.Awake();

        // Resources/Items에서 자동 로드
        if (dropItems != null || dropItems.Count != 0)
            dropItems = Resources.LoadAll<Item>("Items").ToList();
    }

    private void GetItem(Item item)
    {
        int count = Random.Range(minItemCount, maxItemCount); // 1 ~ 3개 랜덤 드랍
        if(InventoryManager.Instance.CanAdd(item, count))
        {
            InventoryManager.Instance.TryAdd(item, count);
        }
    }

    /// <summary>
    /// 사냥에 성공하면 사냥한 동물이 드랍하는 아이템을 획득한다.
    /// </summary>
    public Item GetHuntingItem(MapType mapType, ItemType itemType, Item dropItem) 
    {
        GetItem(dropItem);
        return dropItem;
    }

    /// <summary>
    /// 낚시에 성공하면 현재 낚싯대 레벨에 맞는 물고리를 랜덤으로 획득한다.
    /// </summary>
    public Item GetFishingItem(MapType mapType, ItemType itemType, int requireLevel = 1)
    {
        List<Item> fishCandidate = new(); // 낚시 성공 시 얻을 수 있는 물고리 후보 리스트, 이 중에 랜덤으로 얻음

        foreach (Item item in dropItems)
        {
            if (item.item_MapType == mapType &&
               item.item_Type == ItemType.Fish &&
               item.item_requireLevel <= requireLevel)
            {
                fishCandidate.Add(item);
            }
        }

        Item randomFish = ChooseRandomFish(fishCandidate);
        GetItem(randomFish);
        return randomFish;
    }

    private Item ChooseRandomFish(List<Item> candidates)
    {
        if(candidates == null || candidates.Count == 0)
        {
            Debug.LogWarning("조건에 맞는 물고기가 없습니다");
            return null;
        }
        int idx = Random.Range(0, candidates.Count);
        return candidates[idx];
    }

    /// <summary>
    /// 채집에 성공하면 채집한 채집물이 드랍하는 아이템을 획득한다.
    /// </summary>
    public Item GetGatherItem(MapType mapType, ItemType itemType, Item dropItem)
    {
        GetItem(dropItem);
        return dropItem;
    }
}
