using UnityEngine;

public interface IInteract
{
    void Interact(GameObject interactor);
    public DayPhaseManager.PlayerBehavior GetBehaviorType();
}
