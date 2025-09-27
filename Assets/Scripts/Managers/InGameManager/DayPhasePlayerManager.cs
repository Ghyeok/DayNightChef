using System.Collections;
using UnityEngine;

public class DayPhasePlayerManager : SingletonManager<DayPhasePlayerManager>
{
    public float playerMaxHP { get; private set; }
    public float playerCurHP { get; private set; }

    public float playerAttack   { get; private set; }
    public float playerMoveSpeed { get; private set; }

    public float maxBagWeight => InventoryManager.Instance?.maxWeight ?? 0f;
    public float curBagWeight => InventoryManager.Instance?.CurrentWeight ?? 0f;

    public Transform spawnPoint;
    public GameObject dayPlayerPrefab;
    public GameObject dayPlayer;
    public IInteract currentInteract;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Awake()
    {
        base.Awake();
        StartCoroutine(InitRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator InitRoutine()
    {
        spawnPoint = GameObject.Find("PlayerSpawner").transform;
        GameObject go = GameObject.FindAnyObjectByType<PlayerController>().gameObject;
        if (go == null)
        {
            dayPlayer = Instantiate(dayPlayerPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            dayPlayer = go;
            dayPlayer.transform.position = spawnPoint.position;
            dayPlayer.transform.rotation = spawnPoint.rotation;
        }

        PlayerStats ps = null;
        while ((ps = FindFirstObjectByType<PlayerStats>()) == null || !ps.IsReady)
                yield return null;
        playerMaxHP = ps.GetValue(StatType.MaxHP);
        playerCurHP = playerMaxHP;
        playerAttack = ps.GetValue(StatType.Attack);
        playerMoveSpeed = ps.GetValue(StatType.MoveSpeed);
    }
    
}
