using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatio : MonoBehaviour
{
    public float targetAspect = 16.0f / 9.0f;

    private Camera _cam;
    private float _lastWidth;
    private float _lastHeight;

    void Start()
    {
        _cam = GetComponent<Camera>();
        UpdateViewport();
    }

    void Update()
    {
        // 화면 크기가 변했는지 매 프레임 체크 (모바일 회전 대응)
        if (Screen.width != _lastWidth || Screen.height != _lastHeight)
        {
            UpdateViewport();
        }
    }

    void UpdateViewport()
    {
        // 현재 해상도 저장 (다음 프레임 비교용)
        _lastWidth = Screen.width;
        _lastHeight = Screen.height;

        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = _cam.rect;

        if (scaleHeight < 1.0f) // 레터박스 (위아래)
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else // 필러박스 (좌우)
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        _cam.rect = rect;
    }

    void OnPreCull() => GL.Clear(true, true, Color.black);
}