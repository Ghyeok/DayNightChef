using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public struct WarehouseEntry { public Item item; public int count; }

public class WarehouseManager : SingletonManager<WarehouseManager>
{
    [Header("창고 용량(슬롯)")]
    [SerializeField] private int slotCount = 200;

    public event Action OnWarehouseChanged;

    private List<WarehouseEntry> _slots;
    public List<WarehouseEntry> slots {  get { return _slots; } }

    private string SavePath => Path.Combine(Application.persistentDataPath, "warehouse.json");

    public IReadOnlyList<WarehouseEntry> Slots => _slots;

    public override void Awake()
    {
        base.Awake();

        if (_slots == null)
        {
            _slots = new List<WarehouseEntry>(slotCount);
            for (int i = 0; i < slotCount; i++)
                _slots.Add(new WarehouseEntry { item = null, count = 0 });
        }

        Load();
        OnWarehouseChanged?.Invoke();
    }

    #region 기본 유틸
    public int GetCount(Item item)
    {
        if (item == null) return 0;
        int tot = 0;
        foreach (var s in _slots) if (s.item == item) tot += s.count;
        return tot;
    }

    public bool CanAdd(Item item, int count) => item != null && count > 0;

    /// <summary>
    /// 창고에 아이템을 추가합니다. (가능한 한 많이 채움)
    /// 남는 수량이 있으면 false를 반환합니다.
    /// </summary>
    public bool TryAdd(Item item, int count)
    {
        if (!CanAdd(item, count)) return false;
        int remain = count;

        // 1) 같은 아이템 스택 채우기
        if (item.stackable)
        {
            for (int i = 0; i < _slots.Count && remain > 0; i++)
            {
                var e = _slots[i];
                if (e.item == item && e.count < item.item_maxcount)
                {
                    int canPut = Math.Min(item.item_maxcount - e.count, remain);
                    e.count += canPut;
                    _slots[i] = e;
                    remain -= canPut;
                }
            }
        }

        // 2) 빈 칸 채우기
        for (int i = 0; i < _slots.Count && remain > 0; i++)
        {
            var e = _slots[i];
            if (e.item == null)
            {
                int put = item.stackable ? Math.Min(item.item_maxcount, remain) : 1;
                _slots[i] = new WarehouseEntry { item = item, count = put };
                remain -= put;
            }
        }

        if (count != remain) OnWarehouseChanged?.Invoke();
        return remain == 0;
    }

    /// <summary>
    /// 창고에서 아이템을 제거합니다. (여러 슬롯에 분산된 스택을 돌며 차감)
    /// </summary>
    public bool TryRemove(Item item, int count)
    {
        if (item == null || count <= 0) return false;
        if (GetCount(item) < count) return false;

        int remain = count;
        for (int i = 0; i < _slots.Count && remain > 0; i++)
        {
            var e = _slots[i];
            if (e.item != item || e.count == 0) continue;

            int take = Mathf.Min(e.count, remain);
            e.count -= take;
            if (e.count == 0) e.item = null;
            _slots[i] = e;
            remain -= take;
        }
        OnWarehouseChanged?.Invoke();
        return true;
    }
    #endregion

    #region 가방 -> 창고 아이템 이동
    /// <summary>
    /// 가방의 모든 아이템을 1개 단위로라도 창고에 최대한 이동.
    /// 밤 페이즈 진입 시 호출하세요.
    /// </summary>
    public void DepositAllGreedy()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return;

        for (int i = 0; i < inv.Entries.Count; i++)
        {
            var e = inv.Entries[i];
            if (e.item == null || e.count <= 0) continue;

            int want = e.count;
            int moved = 0;

            // 먼저 전량 시도
            if (TryAdd(e.item, want))
            {
                moved = want;
            }
            else
            {
                // 전량이 안 되면 1개씩 최대한
                while (want > 0 && TryAdd(e.item, 1))
                {
                    want--;
                    moved++;
                }
            }

            if (moved > 0)
            {
                int remain = e.count - moved;
                if (remain <= 0) inv.SetSlot(i, null, 0);
                else inv.SetSlot(i, e.item, remain);
            }
        }
    }
    #endregion

    #region 밤 페이즈 훅/소비(창고 전용)
    /// <summary>
    /// 밤 페이즈 진입 시 호출: 가방의 재료를 가능한 한 모두 창고로 옮깁니다.
    /// </summary>
    public void OnEnterNightPhase_TransferAll()
    {
        DepositAllGreedy();
    }

    /// <summary>
    /// 레시피 소비는 오직 창고에서만 검증/차감합니다.
    /// </summary>
    public bool TryConsumeForRecipe(Recipe recipe, int times = 1)
    {
        if (recipe == null || times <= 0) return false;

        // 1) 재고 확인(창고만 확인)
        foreach (var need in recipe.recipe_requireItems)
        {
            int totalNeed = need.count * times;
            if (GetCount(need.item) < totalNeed)
                return false;
        }

        // 2) 실제 차감(창고만 차감)
        foreach (var need in recipe.recipe_requireItems)
        {
            int totalNeed = need.count * times;
            if (!TryRemove(need.item, totalNeed))
                return false; // 방어적
        }

        return true;
    }

    public int GetMaxCookableCount(Recipe recipe)
    {
        if (recipe == null || recipe.recipe_requireItems.Count == 0) return 0;

        int maxCookable = int.MaxValue;

        foreach (var need in recipe.recipe_requireItems)
        {
            int currentStock = GetCount(need.item); // 현재 재고
            int needPerUnit = need.count; // 필요 개수

            if (needPerUnit <= 0) continue;

            int possibleCount = currentStock / needPerUnit;
            if (possibleCount < maxCookable) // 가장 적게 만들 수 있는 개수가 만들 수 있는 최대치
            {
                maxCookable = possibleCount;
            }
            if (maxCookable == 0) return 0; // 하나라도 재료가 부족하면 0을 반환
        }
        return maxCookable;
    }
    #endregion

    #region 저장/로드(프로토타입)
    [Serializable] private class SaveData { public List<WarehouseEntry> slots; }

    public void Save()
    {
        try
        {
            var data = new SaveData { slots = _slots };
            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }
        catch (Exception e) { Debug.LogWarning($"Warehouse Save Failed: {e.Message}"); }
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(SavePath)) return;
            var json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data?.slots != null && data.slots.Count == slotCount)
                _slots = data.slots;
        }
        catch (Exception e) { Debug.LogWarning($"Warehouse Load Failed: {e.Message}"); }
    }

    private void OnApplicationQuit() => Save();
    #endregion

    // WarehouseManager.cs 클래스 내부에 이 함수를 추가하세요.

    /// <summary>
    /// 창고 내의 두 슬롯을 교환하거나 합칩니다.
    /// </summary>
    public void SwapOrMergeSlots(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _slots.Count || toIndex < 0 || toIndex >= _slots.Count)
        {
            Debug.LogError($"[WarehouseManager] SwapOrMergeSlots: 인덱스 범위 오류. from:{fromIndex}, to:{toIndex}");
            return;
        }

        if (fromIndex == toIndex)
            return; // 같은 슬롯

        var fromEntry = _slots[fromIndex];
        var toEntry = _slots[toIndex];

        // Case 1: Merge (두 아이템이 같고, to 슬롯이 가득 차지 않았을 때)
        if (fromEntry.item != null &&
            fromEntry.item == toEntry.item &&
            toEntry.item.stackable &&
            toEntry.count < toEntry.item.item_maxcount)
        {
            // to 슬롯에 더 담을 수 있는 양
            int canMove = toEntry.item.item_maxcount - toEntry.count;
            // from 슬롯에서 실제 옮길 양
            int toMove = Mathf.Min(fromEntry.count, canMove);

            if (toMove > 0)
            {
                toEntry.count += toMove;
                fromEntry.count -= toMove;

                // from 슬롯이 비었으면 아이템 제거
                if (fromEntry.count <= 0)
                {
                    fromEntry.item = null;
                    fromEntry.count = 0;
                }

                // 변경된 데이터를 리스트에 반영
                _slots[fromIndex] = fromEntry;
                _slots[toIndex] = toEntry;

                OnWarehouseChanged?.Invoke();
            }
        }
        // Case 2: Swap (아이템이 다르거나, 합칠 수 없는 상태일 때)
        else
        {
            _slots[fromIndex] = toEntry;
            _slots[toIndex] = fromEntry;

            OnWarehouseChanged?.Invoke();
        }
    }
}
