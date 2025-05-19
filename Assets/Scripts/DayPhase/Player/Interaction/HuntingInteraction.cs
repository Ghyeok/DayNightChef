using UnityEngine;

public class HuntingInteraction : MonoBehaviour, IInteract
{
    public void Interact(GameObject interactor)
    {
        Debug.Log("Hunting Interaction!");
    }
}
