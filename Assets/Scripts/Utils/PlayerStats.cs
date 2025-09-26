using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class PlayerStats : MonoBehaviour
{
    public static event Action OnReady;
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
        if (db == null) return false;

        int cur = GetLevel(type);
        int max = db.GetMaxLevel(type);
        if (cur >= max) return false;
        int cost = db.GetGoldToNext(type, cur);
        if (_gm == null || !_gm.TrySpendGold(cost)) return false;
        levels[type] = cur + 1;
        return true;
    }
}
