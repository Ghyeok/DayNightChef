using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatio : MonoBehaviour
{
    // 목표하는 가로 세로 비율 (1920 / 1080 = 1.777...)
    public float targetAspect = 16.0f / 9.0f;

    void Start()
    {
        SetViewport();
    }

    void SetViewport()
    {
        Camera camera = GetComponent<Camera>();
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = camera.rect;

        if (scaleHeight < 1.0f) // 현재 화면이 목표보다 세로로 길다 (레터박스 필요)
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else // 현재 화면이 목표보다 가로로 길다 (필러박스 필요)
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        camera.rect = rect;
    }

    void OnPreCull() => GL.Clear(true, true, Color.black);
}