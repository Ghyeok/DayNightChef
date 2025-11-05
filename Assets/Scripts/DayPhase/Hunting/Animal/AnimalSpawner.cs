using UnityEngine;
using System.Collections.Generic;

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
        if (animalPrefab == null)
        {
            return;
        }

        for (int i=0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0f);

            Animal a = Instantiate(animalPrefab, spawnPos, Quaternion.identity, transform);
            if (target != null) a.target = target;

            if (DayPhaseManager.Instance != null)
            {
                DayPhaseManager.Instance.animalList.Add(a);
            }

            _spawned.Add(a);
        }
    }
}
