using UnityEngine;

public class FishingInteraction : MonoBehaviour, IInteract
{
    private bool isFishing = false;
    public PlayerBehavior GetBehaviorType()
    {
        return PlayerBehavior.Fishing;
    }

    public void Interact(GameObject interactor)
    {
        if(isFishing) return;
        isFishing = true;

        UIManager.Instance.ShowPopupUI<UI_Popup>("UI_FishingMiniGamePopup");
        Debug.Log("Fishing Mini Game Popup!");
    }
}
