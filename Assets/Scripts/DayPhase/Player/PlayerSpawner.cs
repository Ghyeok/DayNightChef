using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerPrefab;

    private void Start()
    {
        DayPhasePlayerManager dpm = DayPhasePlayerManager.Instance;
        GameObject go = GameObject.FindAnyObjectByType<PlayerController>().gameObject;
        if (spawnPoint == null)
        {
            spawnPoint = GameObject.Find("PlayerSpawner").transform;
        }
        if (go == null)
        {
            dpm.dayPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            dpm.dayPlayer = go;
            dpm.dayPlayer.transform.position = spawnPoint.position;
            dpm.dayPlayer.transform.rotation = spawnPoint.rotation;
        }
    }
}

