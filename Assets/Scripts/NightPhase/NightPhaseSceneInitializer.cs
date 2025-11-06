using System.Collections;
using UnityEngine;

public class NightPhaseSceneInitializer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(InitializationRoutine());
    }

    private IEnumerator InitializationRoutine()
    {
        // 1. 모든 싱글톤 매니저가 Awake()를 마칠 때까지 한 프레임 대기
        yield return null;

        // 2. NightPhaseManager 리셋
        if (NightPhaseManager.Instance != null)
        {
            NightPhaseManager.Instance.ResetForNewNightPhase();
        }
        else
        {
            Debug.LogError("NightPhaseManager가 없습니다!");
            yield break;
        }
        // 3. SalesManager 리셋
        if (SalesManager.Instance != null)
        {
            SalesManager.Instance.ResetForNewNightPhase();
        }
        else
        {
            Debug.LogError("SalesManager가 없습니다!");
            yield break;
        }

        // (다른 리셋해야 할 매니저가 있다면 여기서 호출)

        UIManager.Instance.ShowPopupUI<UI_RestaurantPreparePopup>("UI_RestaurantPreparePopup");
        WarehouseManager.Instance.OnEnterNightPhase_TransferAll();
    }
}
