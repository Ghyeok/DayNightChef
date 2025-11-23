using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonManager<InventoryManager>
{
    [Serializable]
    public struct Entry
    {
        public Item item; // 아이템
        public int count; // 아이템 개수
    }

    [Header("최대 무게(기본값, DB 로드전 임시 무게")]
    [SerializeField] private float defaultMaxWeight = 30f;
    public int slotCount = 40; // 가방 슬롯 개수

    public event Action OnInventoryChanged; // 인벤토리 변경 이벤트
    public event Action<Item, int> OnInventoryGetted;

    private List<Entry> _entries; // 인벤토리 항목 리스트

    public bool IsInitialized { get; private set; }
    public IReadOnlyList<Entry> Entries => _entries; // 인벤토리 항목 읽기 전용 리스트
    private PlayerStatsManager _ps;
    private PlayerStatsManager PS
    {
        get
        {
            if (_ps == null)
                _ps = PlayerStatsManager.Instance;
            return _ps;
        }
    }

    private float _cacheMaxWeight = -1f; 
    public float maxWeight
    {
        get
        {
            if (PS != null && PS.IsReady)
            {
                return PS.GetValue(StatType.BagWeight);
            }
            return defaultMaxWeight;
        }
    }
    private void OnEnable()
    {
        PlayerStatsManager.Instance.OnReady -= HandlePlayerStatsReady;
        PlayerStatsManager.Instance.OnReady += HandlePlayerStatsReady;

        PlayerStatsManager.Instance.OnStatChanged -= HandlePlayerStatChanged;
        PlayerStatsManager.Instance.OnStatChanged += HandlePlayerStatChanged;

        SyncCapacity();
    }

    private void OnDisable()
    {
        PlayerStatsManager.Instance.OnReady -= HandlePlayerStatsReady;
        PlayerStatsManager.Instance.OnStatChanged -= HandlePlayerStatChanged;
    }

    private void HandlePlayerStatChanged(StatType type, int oldlv, int newlv)
    {
        if (type != StatType.BagWeight) return;
        SyncCapacity();
    }
    private void SyncCapacity()
    {
        float newMax = maxWeight;
        if (!Mathf.Approximately(_cacheMaxWeight, newMax))
        {
            _cacheMaxWeight = newMax;
            OnInventoryChanged?.Invoke();
        }
    }

    private void HandlePlayerStatsReady()
    {
        SyncCapacity();
    }

    public void Init() // 인벤토리 초기화
    {
        if (_entries == null)
        {
            _entries = new List<Entry>(slotCount);
            for (int i = 0; i < slotCount; i++)
            {
                _entries.Add(new Entry { item = null, count = 0 });
            }
            IsInitialized = true;
            OnInventoryChanged?.Invoke();
        }

        PlayerStatsManager.Instance.OnReady -= HandlePlayerStatsReady;
        PlayerStatsManager.Instance.OnReady += HandlePlayerStatsReady;
    }

    public override void Awake() // 싱글톤 초기화
    {
        base.Awake();
        Init();
    }

    public float CurrentWeight // 현재 무게 계산
    {
        get
        {
            float w = 0f;
            foreach (var e in _entries)
            {
                if (e.item != null) w += e.item.item_weight * e.count;
            }
            return w;
        }
    }

    public bool CanAdd(Item item, int count) // 아이템 추가 가능 여부 확인
    {
        if (item == null || count <= 0) return false;
        float after = CurrentWeight + item.item_weight * count;
        return after <= maxWeight;
    }

    /// <summary>
    /// 아이템 획득을 시도합니다.
    /// 무게와 슬롯이 허용하는 만큼 최대한 넣고, 남은 개수를 반환합니다.
    /// </summary>
    /// <returns>인벤토리에 들어가지 못하고 남은 개수 (0이면 모두 획득 성공)</returns>
    public int TryAdd(Item item, int count)
    {
        if (item == null || count <= 0) return count;

        // 1. 무게 체크: 현재 여유 무게로 몇 개까지 더 넣을 수 있는지 계산
        float weightPerItem = item.item_weight;
        float remainingWeight = maxWeight - CurrentWeight;

        // 무게가 0 이하거나 매우 가벼운 아이템 처리
        int maxAddableByWeight = int.MaxValue;
        if (weightPerItem > 0.0001f)
        {
            maxAddableByWeight = (int)(remainingWeight / weightPerItem);
        }

        // 실제로 시도할 개수 (요청 개수 vs 무게 한계 중 작은 값)
        int actualToAdd = Mathf.Min(count, maxAddableByWeight);

        if (actualToAdd <= 0)
        {
            // 무게 초과로 1개도 넣을 수 없음
            Debug.Log("[Inventory] 무게 한계로 아이템 획득 실패");
            return count; // 요청 개수 전량 반환
        }

        // 2. 슬롯에 넣기 (기존 스택 -> 빈 슬롯 순서)
        int remainToAdd = actualToAdd;

        // A. 기존 스택에 합치기
        if (item.stackable)
        {
            for (int i = 0; i < _entries.Count && remainToAdd > 0; i++)
            {
                if (_entries[i].item == item && _entries[i].count < item.item_maxcount)
                {
                    int space = item.item_maxcount - _entries[i].count;
                    int toPut = Mathf.Min(space, remainToAdd);

                    // 구조체 수정 후 재할당
                    var entry = _entries[i];
                    entry.count += toPut;
                    _entries[i] = entry;

                    remainToAdd -= toPut;
                }
            }
        }

        // B. 빈 슬롯에 채우기
        for (int i = 0; i < _entries.Count && remainToAdd > 0; i++)
        {
            if (_entries[i].item == null)
            {
                int maxStack = item.stackable ? item.item_maxcount : 1;
                int toPut = Mathf.Min(maxStack, remainToAdd);

                _entries[i] = new Entry { item = item, count = toPut };
                remainToAdd -= toPut;
            }
        }

        // 3. 결과 정산
        // 원래 넣으려고 했던 양(actualToAdd) 중에서 슬롯 부족으로 못 넣은 양(remainToAdd)을 뺌
        int successCount = actualToAdd - remainToAdd;

        // 최종적으로 못 넣은 양 = (무게 때문에 잘린 것) + (슬롯 없어서 못 넣은 것)
        int totalLeftover = (count - actualToAdd) + remainToAdd;

        if (successCount > 0)
        {
            OnInventoryChanged?.Invoke();
            OnInventoryGetted?.Invoke(item, successCount); // 획득한 개수만 이벤트 알림
        }

        return totalLeftover;
    }

    // 슬롯 간 이동
    public void SwapOrMerge(int from, int to)
    {
        if (from == to) return;
        if ((uint)from >= _entries.Count || (uint)to >= _entries.Count) return; // 최대 슬롯보다 큰 곳 터치시 반환
        var A = _entries[from];
        var B = _entries[to];

        if (A.item == null)
        {
            return; // A가 빈칸이면 아무것도 안함
        }

        //같은 아이템이면 합치기
        if(B.item != null && B.item == A.item && A.item.stackable)
        {
            int canMove = Mathf.Min(A.count, B.item.item_maxcount - B.count);
            if(canMove > 0)
            {
                B.count += canMove;
                A.count -= canMove;
                if (A.count == 0) A.item = null;
                _entries[from] = A;
                _entries[to] = B;
                OnInventoryChanged?.Invoke();
            }
            return;
        }

        // 아니면 그냥 스왑
        _entries[from] = B;
        _entries[to] = A;
        OnInventoryChanged?.Invoke();
    }

    public void SetSlot(int index, Item item, int count) // 슬롯 직접 설정
    {
        if ((uint)index >= _entries.Count) return;
        _entries[index] = new Entry { item = item, count = count };
        OnInventoryChanged?.Invoke();
    }

    #region 저장/로드(프로토타입)
    public List<Entry> GetDataToSave()
    {
        return _entries;
    }

    /// <summary>
    /// 로드된 슬롯 데이터로 현재 창고 상태를 덮어씁니다.
    /// </summary>
    public void LoadData(List<Entry> loadedSlots)
    {
        if (loadedSlots != null && loadedSlots.Count == slotCount)
        {
            _entries = loadedSlots;
            Debug.Log($"[InventoryManager] 인벤토리 데이터 로드 완료. ({_entries.Count}개 슬롯)");
        }
        else
        {
            _entries = new List<Entry>(slotCount);
            for (int i = 0; i < slotCount; i++)
            {
                _entries.Add(new Entry { item = null, count = 0 });
            }
        }
        OnInventoryChanged?.Invoke();
    }
    #endregion
}
