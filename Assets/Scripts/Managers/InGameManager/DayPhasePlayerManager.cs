using UnityEngine;

public class DayPhasePlayerManager : SingletonManager<DayPhasePlayerManager>
{
    public float playerMaxHP;
    public float playerCurHP;

    public float playerAttack;
    public float playerMoveSpeed;

    public float maxBagWeight;
    public float curBagWeight;

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
        playerMaxHP = 100f;
        playerCurHP = playerMaxHP;

        playerAttack = 10f;
        playerMoveSpeed = 3f;

        maxBagWeight = 10f;
        curBagWeight = maxBagWeight;
    }
}
