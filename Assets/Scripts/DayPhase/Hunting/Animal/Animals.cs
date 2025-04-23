using UnityEngine;

public abstract class Animals : Organism

{
    public float maxHp;
    public float currentHp;
    public float attack;
    public float attackRange;
    public bool isDead;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Die();
    }

    public virtual void Die()
    {
        if(!isDead && currentHp <= 0)
        {
            Debug.Log($"{gameObject.name}가 죽었습니다!");
            isDead = true;
            Destroy(gameObject);
        }
    }
}
