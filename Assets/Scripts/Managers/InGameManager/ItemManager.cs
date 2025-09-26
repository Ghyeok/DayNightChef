using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : SingletonManager<ItemManager>
{
    private List<Item> dropItems = new();

    public override void Awake()
    {
        base.Awake();

        // Resources/Items에서 자동 로드
        if (dropItems == null || dropItems.Count == 0)
            dropItems = Resources.LoadAll<Item>("Items").ToList();

        Debug.Log(dropItems[0]);
    }

    public void GetItem(DayPhaseManager.PlayerBehavior playerBehavior)
    {

    }

    //private Item GetHuntingItem()
    //{

    //}

    //private Item GetFishingItem(int level)
    //{

    //}

    //private Item GetGatherItem()
    //{

    //}

}
