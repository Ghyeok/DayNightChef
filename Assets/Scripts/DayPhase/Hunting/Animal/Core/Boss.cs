using UnityEngine;

public class Boss : MonoBehaviour
{
    public void UnlockedMap(MapType mapType)
    {
        if (mapType == MapType.Warm) DayPhaseManager.Instance.UnlockSwampLand();
        if (mapType == MapType.Hot) DayPhaseManager.Instance.UnlockWinterLand();
    }
}
