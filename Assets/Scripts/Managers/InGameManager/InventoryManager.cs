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
    private void OnDestroy()
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
            _entries = new List<Entry>(slotCount);
            for (int i = 0; i < slotCount; i++)
            {
                _entries.Add(new Entry { item = null, count = 0 });
            }
        IsInitialized = true;
        OnInventoryChanged?.Invoke();

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

    public bool TryAdd(Item item, int count) // 아이템 추가 시도
    {
        if (!CanAdd(item, count)) return false;
        int remain = count;

        // 1. 이미 있는 아이템에 추가
        if(item.stackable)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].item == item && _entries[i].count < item.item_maxcount)
                {
                    int canPut = Math.Min(item.item_maxcount - _entries[i].count, remain);
                    _entries[i] = new Entry { item = item, count = _entries[i].count + canPut };
                    remain -= canPut;

                }
            }
        }

        // 2. 빈 슬롯에 추가
        for (int i = 0; i< _entries.Count && remain > 0; i++)
        {
            if (_entries[i].item == null)
            {
                int put = item.stackable ? Math.Min(item.item_maxcount, remain) : 1;
                _entries[i] = new Entry { item = item, count = put };
                remain -= put;
            }
        }
        OnInventoryChanged?.Invoke();
        return remain == 0;
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
