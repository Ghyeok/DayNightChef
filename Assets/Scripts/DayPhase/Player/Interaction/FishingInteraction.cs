using UnityEngine;

public class FishingInteraction : MonoBehaviour, IInteract
{
    public DayPhaseManager.PlayerBehavior GetBehaviorType()
    {
        return DayPhaseManager.PlayerBehavior.Fishing;
    }

    public void Interact(GameObject interactor)
    {
        UIManager.Instance.ShowPopupUI<UI_Popup>("UI_FishingMiniGamePopup");
        Debug.Log("Fishing Mini Game Popup!");
    }
}
