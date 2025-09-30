using UnityEngine;


public class AttackRange : MonoBehaviour 
{
    Animals owner;

    private void Awake()
    {
        owner = GetComponentInParent<Animals>();
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<DayPlayer>(out var player))
        {
            float dmg = owner != null ? owner.AttackPower : 0f;
            player.TakeDamage(dmg);
        }
    }
}
    


