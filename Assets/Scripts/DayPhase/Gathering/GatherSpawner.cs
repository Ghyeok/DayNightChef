using System;
using System.Collections;
using UnityEngine;

public class GatherSpawner : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 5f;
    [SerializeField] private int curTier; // 어떤 티어의 스포너인지
    [SerializeField] Item[] spawnItems;
    [SerializeField] private Transform[] spawnPoints = new Transform[5];
    [SerializeField] private GatheringInteraciton gatherPrefab;
    [SerializeField] private Transform rootSpawnPoint;
    [SerializeField] private float spawnRange = 2.5f;

    private readonly int[] dx = {0,1,-1,1,-1};
    private readonly int[] dy = {0,1,1,-1,-1};

    private Slot[] slots = new Slot[5];

    private GameObject gathers;

    private struct Slot
    {
        public Transform point;
        public GatheringInteraciton current;
        public Coroutine co;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gathers = new GameObject("Gathers");

        spawnItems = ItemManager.Instance.GetItemList(ItemType.Gather, DayPhaseManager.Instance.curMapType, curTier);
        if(spawnItems == null || spawnItems.Length == 0)
        {
            Debug.LogWarning("스폰할 아이템이 없습니다!");
            return;
        }
        for (int i = 0; i < 5; i++)
        {
            if (spawnPoints[i] == null)
            {
                var go = new GameObject($"SpawnPoint_{i}");
                spawnPoints[i] = go.transform;
                spawnPoints[i].SetParent(transform, false);
            }
            Vector2 v = new Vector2(spawnRange * dx[i], spawnRange * dy[i]);
            spawnPoints[i].position = (Vector2)rootSpawnPoint.position + v;
            slots[i].point = spawnPoints[i];
        }
        for (int i = 0; i < 5; i++)
        {
            SpawnAt(i);
        }
    }

    private void SpawnAt(int index)
    {
        if (gatherPrefab == null || slots[index].point == null) return;

        var node = Instantiate(gatherPrefab, slots[index].point.position, Quaternion.identity, gathers.transform);
        node.name = $"GatherNode_{index}";

        // 아이템을 정하지 않고, 컨텍스트만 전달
        node.SetContext(ItemType.Gather, curTier);

        node.OnCollected -= HandleCollected;
        node.OnCollected += HandleCollected;

        if (slots[index].co != null) { StopCoroutine(slots[index].co); slots[index].co = null; }
        slots[index].current = node;
    }

    /// <summary>
    /// 채집물이 수거되면 호출되는 콜백.
    /// 그 포인트에 대해 respawnDelay 후 재스폰 코루틴 시작.
    /// </summary>
    private void HandleCollected(GatheringInteraciton g)
    {
        // 어떤 슬롯인지 식별
        int idx = FindSlotIndexByGatherable(g);
        if (idx < 0) return;

        // 슬롯 상태 리셋
        slots[idx].current = null;

        // 해당 포인트만 독립적으로 리스폰 타이머 시작
        if (slots[idx].co != null) StopCoroutine(slots[idx].co);
        slots[idx].co = StartCoroutine(CoRespawn(idx));
    }

    private IEnumerator CoRespawn(int index)
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnAt(index);
        slots[index].co = null;
    }

    private int FindSlotIndexByGatherable(GatheringInteraciton g)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].current == g) return i;
            // 파괴 직전이라 current가 null일 수도 있으니 위치로도 보조 판정
            if (slots[i].point != null && g != null)
            {
                if (Vector2.SqrMagnitude((Vector2)slots[i].point.position - (Vector2)g.transform.position) < 0.01f)
                    return i;
            }
        }
        return -1;
    }
}
