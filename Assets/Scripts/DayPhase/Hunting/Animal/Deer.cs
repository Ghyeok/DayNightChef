using UnityEngine;

public class Deer : Animals, IAttack, IDamagable
{

    private void Awake()
    {
        Init();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Init()
    {
        mapType = DayPhaseManager.MapType.Warm;
        // dropIngredient = ??

        maxHp = 10f;
        currentHp = maxHp;
        attack = 3f;
        attackRange = 1f;
        isDead = false;
    }

    public void Attack()
    {
        Debug.Log("Deer가 공격합니다!");
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;

        if(currentHp < 0)
        {
            Die();
        }
    }
}
