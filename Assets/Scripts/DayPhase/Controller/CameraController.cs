using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    private Vector3 offset = new Vector3(0f, 7f, -6f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}
