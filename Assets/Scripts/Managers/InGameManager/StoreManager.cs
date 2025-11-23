
using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;

public class StoreManager : SingletonManager<StoreManager>
{
    [Header("판매 아이템 목록")]
    public List<Item> items = new();

    public event Action<Item> OnPurchased;

    public bool TryPurchase(int index)
    {
        if (index < 0 || index >= items.Count) return false;
        var item = items[index];
        if (item == null) return false;
        var inv = InventoryManager.Instance;
        if (inv == null) return false;

        if (!GameManager.Instance.TrySpendGold(item.item_price)) return false;
        if (!inv.CanAdd(item, 1)) return false;
        else
        {
            inv.TryAdd(item, 1);
            GameManager.Instance.SpendGold(item.item_price);
            OnPurchased?.Invoke(item);
            return true;
        }
    }
}
