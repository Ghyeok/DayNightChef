using UnityEngine;

public class Deer : Animals//IAttack, IDamagable
{
    BoxCollider boxcollider;
    private void Awake()
    {
        Init();
        Setup();
    }

    public override void Init()
    {
        mapType = MapType.Warm;
        // dropIngredient = ??
        boxcollider=GetComponent<BoxCollider>();
        maxHp = 10f;
        currentHp = maxHp;
        attack = 3f;
        speed = 1f;
        attackRange = 3f;
        isDead = false;
    }

    public void Attack()
    {
        Debug.Log("Deer�� �����մϴ�!");
    }

    /*public void TakeDamage(float damage)
    {
        currentHp -= damage;

        if(currentHp < 0)
        {
            Die();
        }
    }*/
}
