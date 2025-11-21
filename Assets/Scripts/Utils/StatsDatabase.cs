using UnityEngine;
using System.Collections.Generic;
// 모든 스탯 테이블을 관리하는 데이터베이스

[CreateAssetMenu(fileName = "StatsDatabase", menuName = "Stats Database")]
public class StatsDatabase : ScriptableObject
{
    [SerializeField] private List<StatTable> tables = new();
    private readonly Dictionary<StatType, StatTable> _cache = new();

    private void OnEnable()
    {
        _cache.Clear();
        if (tables == null) return;
        foreach (var t in tables)
        {
            if (t == null) continue;
            _cache[t.statType] = t;
        }
    }

    public bool TryGet(StatType type, out StatTable table) =>
        _cache.TryGetValue(type, out table);

    public int GetMaxLevel(StatType type) =>
        _cache.TryGetValue(type, out var t) ? t.MaxLevel : 0;

    public float GetValue(StatType type, int level) =>
        _cache.TryGetValue(type, out var t) ? t.GetValue(level) : 0f;

    public int GetGoldToNext(StatType type, int level) =>
        _cache.TryGetValue(type, out var t) ? t.GetGoldToNext(level) : 0;
    public int GetReputationToNext(StatType type, int level) =>
        _cache.TryGetValue(type, out var t) ? t.GetReputationToNext(level) : 0;
}
