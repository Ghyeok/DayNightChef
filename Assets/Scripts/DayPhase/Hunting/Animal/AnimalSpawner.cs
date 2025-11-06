using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Animal animalPrefab;

    [Header("스폰 개체 수")]
    public int count = 3;

    [Tooltip("스포너 중심으로 랜덤 배치할 반경")]
    public float radius = 5f;

    [Header("Target")]
    [Tooltip("모든 스폰 개체가 추적할 대상(플레이어)")]
    public Transform target;

    [Tooltip("리스폰 지연(초)")]
    public float respawnDelay = 8f;

    readonly List<Animal> _spawned = new List<Animal>();

    private void Start()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }

        SpawnAll();
    }

    public void SpawnAll()
    {
        if (!animalPrefab) return;
        for (int i = 0; i < count; i++)
            SpawnOne();
    }
    private Animal SpawnOne()
    {
        Vector2 offset = Random.insideUnitCircle * radius;
        Vector3 pos = transform.position + new Vector3(offset.x, offset.y, 0f);

        Animal a = Instantiate(animalPrefab, pos, Quaternion.identity, transform);
        if (target) a.target = target;

        // 파괴(사망) 알림 연결
        var notifier = a.GetComponent<DeathNotifier>() ?? a.gameObject.AddComponent<DeathNotifier>();
        notifier.animal = a;
        notifier.spawner = this;

        _spawned.Add(a);
        return a;
    }
    public void HandleAnimalDestroyed(Animal a)
    {
        if (!this.enabled || !gameObject.activeInHierarchy)
        {
            return;
        }

        _spawned.Remove(a);

        // 목표 수 유지: 부족할 때만 보충
        if (_spawned.Count < count)
            StartCoroutine(Co_RespawnAfterDelay());
    }

    private IEnumerator Co_RespawnAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, respawnDelay));
        if (!this || !enabled) yield break;      // 씬/오브젝트 파괴 방지
        if (_spawned.Count < count)
            SpawnOne();
    }
}
