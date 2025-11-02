using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoadingCanvas : MonoBehaviour
{
    [SerializeField]
    private Slider progressBar;

    [SerializeField]
    private TextMeshProUGUI progressText;

    // SceneLoader가 이 함수를 호출하여 진행률을 업데이트합니다.
    public void SetProgress(float value)
    {
        if (progressBar != null)
        {
            progressBar.value = value;
        }
        if (progressText != null)
        {
            // 텍스트 포맷은 여기서 관리
            progressText.text = $"{(value * 100f):F0}%";
        }
    }
}
