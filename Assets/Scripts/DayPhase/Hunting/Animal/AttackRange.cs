using UnityEngine;


public class AttackRange : MonoBehaviour 
{
    Animals parentAnimal;
    float damage;
    DayPlayer player;

    private void Awake()
    {
        parentAnimal = GetComponentInParent<Animals>();
        damage = GetComponentInParent<Animals>().attack;
    }

    private void OnTriggerEnter(Collider other)
    {
        player = other.gameObject.GetComponent<DayPlayer>();
        if(other.CompareTag("Player"))
        {
            player.TakeDamage(damage);
        }    
    }
}
    


