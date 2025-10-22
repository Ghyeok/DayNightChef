using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : SingletonManager<ItemManager>
{
    [SerializeField]
    private List<Item> dropItems = new(); // 존재하는 모든 아이템을 갖고 있는 리스트

    private readonly int minItemCount = 1;
    private readonly int maxItemCount = 3;

    [SerializeField, Range(0f,1f)]
    private float specialItemProb = 0.01f;

    public override void Awake()
    {
        base.Awake();

        // Resources/Items에서 자동 로드
        if (dropItems == null || dropItems.Count == 0)
            dropItems = Resources.LoadAll<Item>("Items").ToList();
    }

    // 아이템의 타입, 맵 타입, 티어가 일치하는 아이템들의 리스트를 반환한다
    public Item[] GetItemList(ItemType itemType, MapType mapType, int tier)
    {
        List<Item> list = new List<Item>();
        foreach(Item item in dropItems)
        {
            if(item.item_Type == itemType &&
               item.item_MapType == mapType &&
               item.item_tier == tier)
            {
                list.Add(item);
            }
        }
        return list.ToArray();
    }

    private Item PickSpecialCandidate(Item[] candidates)
    {
        if (candidates == null || candidates.Length == 0) return null;

        var specials = candidates.Where(i => i != null && i.item_Grade == ItemGrade.Special).ToList();
        var normals = candidates.Where(i => i != null && i.item_Grade == ItemGrade.Normal).ToList();

        if (specials.Count > 0)
        {
            if (Random.value < specialItemProb) // 1% 스페셜 시도
                return specials[Random.Range(0, specials.Count)];

            // 실패 시 노말, 없으면 전체 폴백
            if (normals.Count > 0)
                return normals[Random.Range(0, normals.Count)];
            return candidates[Random.Range(0, candidates.Length)];
        }
        else
        {
            // 스페셜이 없으면 노말, 없으면 전체 폴백
            if (normals.Count > 0)
                return normals[Random.Range(0, normals.Count)];
            return candidates[Random.Range(0, candidates.Length)];
        }
    }

    private Item PickSpecialCandidate(MapType mapType, ItemType itemType, int tier)
    {
        var specials = dropItems.Where(i =>
            i.item_MapType == mapType &&
            i.item_Type == itemType &&
            i.item_tier == tier &&
            i.item_Grade == ItemGrade.Special).ToList();

        if(specials.Count == 0) return null;
        int idx = Random.Range(0, specials.Count);
        return specials[idx];
    }

    private void GetItem(Item item)
    {
        if (item == null) return;
        int count = Random.Range(minItemCount, maxItemCount);
        if (InventoryManager.Instance.CanAdd(item, count))
            InventoryManager.Instance.TryAdd(item, count);
    }

    /// <summary>
    /// 사냥에 성공하면 사냥한 동물이 드랍하는 아이템을 획득한다.
    /// </summary>
    public Item GetHuntingItem(MapType mapType, ItemType itemType, Item dropItem)
    {
        if (dropItem == null) return null;

        Item final = dropItem;
        // (0~1) 확률 비교
        if (dropItem.item_Grade == ItemGrade.Normal && Random.value < specialItemProb)
        {
            var special = PickSpecialCandidate(mapType, itemType, dropItem.item_tier);
            if (special != null) final = special;
        }

        GetItem(final);
        return final;
    }
    public Item GetHuntingItem(MapType mapType, ItemType itemType, Item[] dropCandidates)
    {
        var chosen = PickSpecialCandidate(dropCandidates);
        GetItem(chosen);
        return chosen;
    }

    /// <summary>
    /// 낚시에 성공하면 현재 낚싯대 레벨에 맞는 물고리를 랜덤으로 획득한다.
    /// </summary>
    public Item GetFishingItem(MapType mapType, ItemType itemType, int requireLevel = 1)
    {
        // 1) 후보군 구성: 같은 맵 + 타입(Fish) + 낚싯대 레벨 이하
        List<Item> fishCandidate = new();
        foreach (Item item in dropItems)
        {
            if (item.item_MapType == mapType &&
                item.item_Type == ItemType.Fish &&
                item.item_requireLevel <= requireLevel)
            {
                fishCandidate.Add(item);
            }
        }

        if (fishCandidate == null || fishCandidate.Count == 0)
        {
            Debug.LogWarning("조건에 맞는 물고기가 없습니다");
            return null;
        }

        // 2) 1% 스페셜 시도 → 없으면 노말(→ 전체)로 폴백
        bool wantSpecial = Random.value < specialItemProb; // specialItemProb = 0.01f
        Item chosen = null;

        if (wantSpecial)
        {
            // 스페셜 우선
            var specials = fishCandidate.Where(i => i != null && i.item_Grade == ItemGrade.Special).ToList();
            if (specials.Count > 0)
                chosen = specials[Random.Range(0, specials.Count)];
        }

        if (chosen == null)
        {
            // 노말 우선
            var normals = fishCandidate.Where(i => i != null && i.item_Grade == ItemGrade.Normal).ToList();
            if (normals.Count > 0)
                chosen = normals[Random.Range(0, normals.Count)];
            else
                chosen = fishCandidate[Random.Range(0, fishCandidate.Count)]; // 최종 폴백
        }

        // 3) 인벤토리에 지급
        GetItem(chosen);
        return chosen;
    }

    /// <summary>
    /// 채집에 성공하면 채집한 채집물이 드랍하는 아이템을 획득한다.
    /// </summary>
    public Item GetGatherItem(MapType mapType, ItemType itemType, Item dropItem)
    {
        if (dropItem == null) return null;

        Item final = dropItem;
        // (0~1) 확률 비교
        if (Random.value < specialItemProb)
        {
            var special = PickSpecialCandidate(mapType, itemType, dropItem.item_tier);
            if (special != null) final = special;
        }

        GetItem(final);
        return final;
    }
    public Item GetGatherItem(MapType mapType, ItemType itemType, Item[] dropCandidates)
    {
        var chosen = PickSpecialCandidate(dropCandidates);
        GetItem(chosen);
        return chosen;
    }
}
