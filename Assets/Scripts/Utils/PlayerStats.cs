using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class PlayerStats : MonoBehaviour
{
    public int hpLevel;
    public int moveSpeedLevel;
    public int knifeLevel;
    public int fishingLevel;
    public int bagLevel;

    public static event Action OnReady;
    public static event Action<StatType, int, int> OnStatChanged;

    //각 스텟의 현재 레벨을 저장하는 딕셔너리
    [SerializeField]
    private Dictionary<StatType, int> levels = new()
    {
        { StatType.Attack, 1 },
        { StatType.BagWeight, 1 },
        { StatType.MaxHP, 1 },
        { StatType.MoveSpeed, 1 },
        { StatType.FishingRod, 1 },
    };

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
    }

    public int GetLevel(StatType type) =>
        levels.TryGetValue(type, out var lv) ? lv : 0;

    public float GetValue(StatType type) =>
        db != null ? db.GetValue(type, GetLevel(type)) : 0f;
    public float GetValueAtLevel(StatType type, int level)
    {
        return db != null ? db.GetValue(type, level) : 0f;
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
        return true;
    }

    public void SetLevel(StatType type, int level)
    {
        if (levels.ContainsKey(type))
        {
            levels[type] = Mathf.Max(level, 1);
        }
    }

    public void ResetLevels()
    {
        Debug.Log("[PlayerStats] 모든 레벨을 1로 초기화합니다.");

        // 딕셔너리의 모든 키를 가져와서 값을 1로 설정
        var keys = new List<StatType>(levels.Keys);
        foreach (var key in keys)
        {
            levels[key] = 1;
        }
        OnReady?.Invoke(); // 스탯이 준비되었다고 알림
    }
}
