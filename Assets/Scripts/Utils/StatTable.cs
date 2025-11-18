using UnityEngine;
using System.Collections.Generic;
using System;
public enum  StatType
{
    Attack,
    BagWeight,
    MaxHP,
    MoveSpeed,
    FishingRod,
    Restaurant,
}

[System.Serializable]
public struct LevelRecord
{
    [Min(1)] public int level;
    public float value;
    [Min(0)] public int goldToNext;
}

[CreateAssetMenu(fileName = "StatTable", menuName = "Stat Table")]
public class StatTable : ScriptableObject
{
    public StatType statType;

    //이 스텟이 value를 사용하는지 ex) 낚싯대 = false
    public bool useValue = true;
    public List<LevelRecord> records = new();

    public int MaxLevel => records?.Count ?? 0;

    // 레벨의 유효 범위 자동 보정
    private LevelRecord GetClamped(int level)
    {
        if (records == null || records.Count == 0)
            return default;
        if (level < 1) level = 1;
        if (level > records.Count) level = records.Count;
        return records[level - 1];
    }
    //레벨에 따른 value 반환
    public float GetValue(int level)
    {
        if (!useValue) return 0f;
        return (float)Math.Round(GetClamped(level).value, 2);
    }
    //레벨에 따른 다음 레벨업에 필요한 골드 반환
    public int GetGoldToNext(int level)
    {
        var rec = GetClamped(level);
        return rec.goldToNext;
    }

    // 에디터에서 검증
#if UNITY_EDITOR
    private void OnValidate()
    {
        // 레벨마다 검증
        for (int i = 0; i < records.Count; i++)
        {
            records[i] = new LevelRecord
            {
                level = Mathf.Max(1, i + 1),
                value = records[i].value,
                goldToNext = Mathf.Max(0, records[i].goldToNext)
            };
        }
    }
#endif
}
