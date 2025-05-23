using System.Collections;
using UnityEngine;

public class GatheringInteraciton : MonoBehaviour, IInteract
{
    public DayPhaseManager.PlayerBehavior GetBehaviorType()
    {
        return DayPhaseManager.PlayerBehavior.Gathering;
    }

    public void Interact(GameObject interactor)
    {
        StartCoroutine(Gathering());
    }

    IEnumerator Gathering()
    {
        Animator anim = DayPhasePlayerManager.Instance.dayPlayer.GetComponent<Animator>();
        anim.SetTrigger("Gathering");

        yield return new WaitForSeconds(2f);

        GameObject gather = GetComponent<GatheringInteraciton>().gameObject;
        Destroy(gather);
        Debug.Log($"Gathering Success! name : {gather.gameObject.name}");
    }
}
