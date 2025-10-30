using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    private Vector3 offset = new Vector3(0f, 0f, -10f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = offset + target.position;
        }
    }

    void SetTarget()
    {
        CameraController cc = FindFirstObjectByType<Camera>().GetComponent<CameraController>();
        if (DayPhasePlayerManager.Instance.dayPlayer != null)
        {
            cc.target = DayPhasePlayerManager.Instance.dayPlayer.transform;
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
