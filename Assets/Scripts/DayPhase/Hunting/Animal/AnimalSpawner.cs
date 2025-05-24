using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [SerializeField] private GameObject animalPrefab;
    public int maxCount;
    public float respawntime;
    private bool isReSpawning = false;
    List<Animals> animallist = new List<Animals>();

    public void SpawnAnimal(Vector3 spawnPosition)
    {
        GameObject animalObj = Instantiate(animalPrefab, GetRandomPoint(spawnPosition, 5f), Quaternion.identity);
        Animals animal = animalObj.GetComponent<Animals>();

        if (animal != null)
        {
            DayPhaseManager.Instance.animalList.Add(animal);
            animallist.Add(animal);
        }
        else
        {
            Debug.LogWarning("스폰된 오브젝트에 Animals 스크립트가 없습니다.");
        }
    }

    private void Awake()
    {
    }
    private void Start()
    {
        for (int i = 0; i < maxCount; i++)
        {
            SpawnAnimal(gameObject.transform.position);
        }
    }

    private void Update()
    {
        for (int i = animallist.Count - 1; i >= 0; i--)
        {
            if (animallist[i] == null)
            {
                animallist.RemoveAt(i);
            }
        }
        while (animallist.Count < maxCount && !isReSpawning)
        {
            StartCoroutine(ReSpawn());
        }
    }

    Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        Vector2 randomPos = Random.insideUnitCircle * radius;
        return new Vector3(center.x + randomPos.x, center.y, center.z + randomPos.y);
    }

    IEnumerator ReSpawn()
    {
        isReSpawning = true;
        yield return new WaitForSeconds(respawntime);
        SpawnAnimal(transform.position);
        isReSpawning = false;
    }
}