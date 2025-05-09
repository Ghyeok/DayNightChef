using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [SerializeField] private GameObject deerPrefab;

    public void SpawnDeer(Vector3 spawnPosition)
    {
        GameObject deerObj = Instantiate(deerPrefab, spawnPosition, Quaternion.identity);
        Animals deer = deerObj.GetComponent<Animals>();

        if (deer != null)
        {
            DayPhaseManager.Instance.animalList.Add(deer);
        }
        else
        {
            Debug.LogWarning("스폰된 오브젝트에 Animals 스크립트가 없습니다.");
        }
    }

    // 테스트용 자동 스폰
    private void Start()
    {
        SpawnDeer(transform.position);
    }
}