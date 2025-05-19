using UnityEngine;

public class GatheringInteraciton : MonoBehaviour, IInteract
{
    public void Interact(GameObject interactor)
    {
        Debug.Log("GatheringInteraction!");
    }
}
