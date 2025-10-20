using UnityEngine;

public interface IInteract
{
    void Interact(GameObject interactor = null);
    public DayPhaseManager.PlayerBehavior GetBehaviorType();
}
