using UnityEngine;

public class DayPhasePlayerManager : SingletonManager<DayPhasePlayerManager>
{
    public float playerMaxHP;
    public float playerCurHP;

    public float playerAttack;
    public float playerMoveSpeed;

    public float maxBagWeight;
    public float curBagWeight;

    public Transform spawnPoint;
    public GameObject dayPlayerPrefab;
    public GameObject dayPlayer;
    public IInteract currentInteract;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Awake()
    {
        base.Awake();
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Init()
    {
        GameObject go = GameObject.FindAnyObjectByType<PlayerController>().gameObject;
        if(go == null)
        {
            dayPlayer = Instantiate(dayPlayerPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            dayPlayer = go;
        }

        playerMaxHP = 100f;
        playerCurHP = playerMaxHP;

        playerAttack = 1f;
        playerMoveSpeed = 3f;

        maxBagWeight = 10f;
        curBagWeight = maxBagWeight;
    }
}
