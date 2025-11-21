using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatsManager : SingletonManager<PlayerStatsManager>
{
    // --- 1단계: 인스펙터 표시용 내부 클래스 추가 ---
    [System.Serializable]
    private class StatLevelDebugView
    {
        public StatType type;
        public int level;

        public StatLevelDebugView(StatType t, int l)
        {
            type = t;
            level = l;
        }
    }
    // ------------------------------------------

    public event Action OnReady;
    public event Action<StatType, int, int> OnStatChanged;

    //각 스텟의 현재 레벨을 저장하는 딕셔너리
    [SerializeField]
    private Dictionary<StatType, int> levels = new()
    {
        { StatType.Attack, 1 },
        { StatType.BagWeight, 1 },
        { StatType.MaxHP, 1 },
        { StatType.MoveSpeed, 1 },
        { StatType.FishingRod, 1 },
        { StatType.Restaurant, 1 },
    };

    // --- 2단계: 디버그용 리스트 필드 추가 ---
    [Header("Debug View (Read-Only)")]
    [SerializeField]
    private List<StatLevelDebugView> currentLevelsForInspector = new List<StatLevelDebugView>();
    // -------------------------------------

    private StatsDatabase db;
    private GameManager _gm;

    public bool IsReady { get; private set; }

    private IEnumerator Start()
    {
        // db 로드
        db = ResourceManager.Instance.Load<StatsDatabase>("Data/Stat/StatsDatabase");
        // GameManager 준비 대기
        while (GameManager.Instance == null)
            yield return null;
        _gm = GameManager.Instance;
        IsReady = true;
        OnReady?.Invoke();

#if UNITY_EDITOR
        UpdateInspectorDebugView(); // 레벨 변경 시 업데이트
#endif
    }

    public int GetLevel(StatType type) =>
        levels.TryGetValue(type, out var lv) ? lv : 0;

    public float GetValue(StatType type) =>
        db != null ? db.GetValue(type, GetLevel(type)) : 0f;

    public float GetValueAtLevel(StatType type, int level)
    {
        return db != null ? db.GetValue(type, level) : 0f;
    }
    public int GetNextReputation(StatType type)
    {
        if (db == null) return 0;
        int cur = GetLevel(type);
        if (cur >= db.GetMaxLevel(type)) return 0;
        return db.GetReputationToNext(type, cur);
    }

    public int GetNextCost(StatType type)
    {
        if (db == null) return 0;
        int cur = GetLevel(type);
        if (cur >= db.GetMaxLevel(type)) return 0;
        return db.GetGoldToNext(type, cur);
    }

    public bool CanUpgrade(StatType type)
    {
        if (db == null) return false;
        int cur = GetLevel(type);
        int max = db.GetMaxLevel(type);
        if (cur >= max) return false;

        int cost = db.GetGoldToNext(type, cur);
        if (type == StatType.Restaurant)
        {
            if (db.GetReputationToNext(type, cur) > _gm.restaurantReputation)
                return false;
        }
        return _gm != null && _gm.TrySpendGold(cost);
    }

    public bool TryUpgrade(StatType type)
    {
        if (db == null || _gm == null) return false;

        int oldLv = GetLevel(type);
        int max = db.GetMaxLevel(type);
        if (oldLv >= max) return false;

        int cost = db.GetGoldToNext(type, oldLv);
        if (_gm == null || !_gm.TrySpendGold(cost)) return false;
        _gm.SpendGold(cost);
        int newLv = oldLv + 1;
        levels[type] = newLv;

        OnStatChanged?.Invoke(type, oldLv, newLv);
        SaveManager.Instance.SaveGame();

#if UNITY_EDITOR
        UpdateInspectorDebugView(); // 레벨 변경 시 업데이트
#endif

        return true;
    }

    public void SetLevel(StatType type, int level)
    {
        if (!levels.ContainsKey(type)) return;

        int oldLv = levels[type];
        int newLv = Mathf.Max(1, level);

        if (oldLv == newLv) return;

        levels[type] = newLv;
        OnStatChanged?.Invoke(type, oldLv, newLv);

#if UNITY_EDITOR
        UpdateInspectorDebugView(); // 레벨 변경 시 업데이트
#endif
    }

    public void ResetLevels()
    {
        Debug.Log("[PlayerStats] 모든 레벨을 1로 초기화합니다.");

        var keys = new List<StatType>(levels.Keys);
        foreach (var key in keys)
        {
            int oldLv = levels[key];
            if (oldLv != 1)
            {
                levels[key] = 1;
                OnStatChanged?.Invoke(key, oldLv, 1);
            }
        }
#if UNITY_EDITOR
        UpdateInspectorDebugView(); // 레벨 변경 시 업데이트
#endif
    }

    // --- 3단계: 디버그 리스트 업데이트 함수 추가 ---
#if UNITY_EDITOR
    private void UpdateInspectorDebugView()
    {
        // (게임이 실행 중이 아닐 때는 실행 방지)
        if (!Application.isPlaying || levels == null) return;

        currentLevelsForInspector.Clear();

        // 딕셔너리의 모든 키를 가져와 스탯 타입 순서대로 정렬 (선택 사항이지만 깔끔함)
        var sortedKeys = levels.Keys.OrderBy(key => key.ToString());

        foreach (var key in sortedKeys)
        {
            currentLevelsForInspector.Add(new StatLevelDebugView(key, levels[key]));
        }
    }
#endif
    // ------------------------------------------
}
