using UnityEngine;

public class ItemManager : SingletonManager<ItemManager>
{
    public override void Awake()
    {
        base.Awake();
    }

    public void GetItem(Item item) // 아이템을 인벤토리에 추가하는 함수
    {
        int testCount = 1;

        if (InventoryManager.Instance.CanAdd(item, testCount))
        {
            InventoryManager.Instance.TryAdd(item, testCount);
        }
    }


}
