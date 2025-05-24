using UnityEngine;

public class HuntingInteraction : MonoBehaviour, IInteract
{
    public DayPhaseManager.PlayerBehavior GetBehaviorType()
    {
        return DayPhaseManager.PlayerBehavior.Hunting;
    }

    public void Interact(GameObject interactor)
    {
        Animals animal = GetComponent<HuntingInteraction>().gameObject.GetComponent<Animals>();

        animal.TakeDamage(DayPhasePlayerManager.Instance.playerAttack);
        Debug.Log($"Attack Success! Remain HP: {animal.currentHp}");
    }
}
