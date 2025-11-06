using UnityEngine;

public interface IInteract
{
    void Interact(GameObject interactor = null);
    public PlayerBehavior GetBehaviorType();
}
