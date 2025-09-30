using UnityEngine;

public class GroceryInteraction : MonoBehaviour, IInteract
{
    public DayPhaseManager.PlayerBehavior GetBehaviorType()
    {
        return DayPhaseManager.PlayerBehavior.GroceryStore;
    }

    public void Interact(GameObject interactor)
    {
        UIManager.Instance.ShowPopupUI<UI_GroceryStorePopup>("UI_GroceryStorePopup");
    }
}
