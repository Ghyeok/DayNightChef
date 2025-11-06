using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    private Vector3 offset = new Vector3(0f, 0f, -10f);

    // Update is called once per frame
    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = offset + target.position;
        }
    }

    public void SetTarget()
    {
        if (DayPhasePlayerManager.Instance == null)
        {
            Debug.LogError("[CameraController] PlayerManager가 없습니다!");
            return;
        }

        if (DayPhasePlayerManager.Instance.dayPlayer != null)
        {
            this.target = DayPhasePlayerManager.Instance.dayPlayer.transform;
            Debug.Log($"[CameraController] 카메라 타겟 설정 완료: {target.name}");
        }
        else
        {
            // OnMapLoadComplete 이벤트가 발생했는데도 player가 null인 경우
            Debug.LogError("[CameraController] SetTarget이 호출되었으나 target이 null입니다!");
        }
    }

    private void OnEnable()
    {
        DayPhaseManager.OnMapLoadComplete += SetTarget;
    }
    private void OnDisable()
    {
        DayPhaseManager.OnMapLoadComplete -= SetTarget;
    }
}
