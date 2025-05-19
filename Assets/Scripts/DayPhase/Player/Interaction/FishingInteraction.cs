using UnityEngine;

public class FishingInteraction : MonoBehaviour, IInteract
{
    public void Interact(GameObject interactor)
    {
        Debug.Log("FishingInteraction!");
    }
}
