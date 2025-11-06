using System.Collections;
using UnityEngine;

// 이 스크립트는 'DayPhaseScene'에만 존재하며, 싱글톤이 아닙니다.
// 씬이 로드될 때마다 새로 실행됩니다.
public class DayPhaseSceneInitializer : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(InitializationRoutine());
    }

    private IEnumerator InitializationRoutine()
    {
        // 1. 모든 싱글톤 매니저가 Awake()를 마칠 때까지 한 프레임 대기
        yield return null;

        // 2. DayPhaseManager 리셋
        if (DayPhaseManager.Instance != null)
        {
            DayPhaseManager.Instance.ResetForNewDayPhase();
        }
        else
        {
            Debug.LogError("DayPhaseManager가 없습니다!");
            yield break;
        }
        // 3. DayPhasePlayerManager 리셋
        if (DayPhasePlayerManager.Instance != null)
        {
            DayPhasePlayerManager.Instance.ResetForNewDayPhase();
        }
        else
        {
            Debug.LogError("DayPhasePlayerManager가 없습니다!");
            yield break;
        }

        // (다른 리셋해야 할 매니저가 있다면 여기서 호출)

        // 4. 모든 리셋이 완료된 후, 맵 선택 팝업을 표시
        Debug.Log("모든 매니저 리셋 완료. 맵 선택 팝업을 표시합니다.");
        UIManager.Instance.ShowPopupUI<UI_MapSelectPopup>("UI_MapSelectPopup");
    }
}