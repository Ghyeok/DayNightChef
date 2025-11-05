using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[DisallowMultipleComponent]
public class ChefCookingUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image fillImage;
    [SerializeField] private Vector3 worldOffset = new Vector3(-0.35f, 1f, 0f);

    [Header("옵션")]
    [SerializeField] private bool hideWhenIdle = true; // 조리 중이 아닐 때 자동 숨김

    private Transform _target; 
    private Coroutine _fillRoutine;

    private void Reset()
    {
        canvas = GetComponentInChildren<Canvas>();
        fillImage = GetComponentInChildren<Image>();
    }

    private void Awake()
    {
        _target = transform;
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            // 캔버스를 셰프 머리 위로 오프셋
            canvas.transform.localPosition = worldOffset;
        }
        SetVisible(false);
        SetFill(0f);
    }

    private void OnEnable()
    {
        if (SalesManager.Instance != null)
        {
            SalesManager.Instance.OnOrderStarted += HandleOrderStarted;
            SalesManager.Instance.OnOrderReady += HandleOrderReady;
            SalesManager.Instance.OnOrderRemoved += HandleOrderRemoved; // 취소 케이스 대비
        }
    }

    private void OnDisable()
    {
        if (SalesManager.Instance != null)
        {
            SalesManager.Instance.OnOrderStarted -= HandleOrderStarted;
            SalesManager.Instance.OnOrderReady -= HandleOrderReady;
            SalesManager.Instance.OnOrderRemoved -= HandleOrderRemoved;
        }
    }

    private void LateUpdate()
    {
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            canvas.transform.position = _target.position + worldOffset;
            canvas.transform.rotation = Quaternion.identity; 
        }
    }

    private void HandleOrderStarted(SalesManager.Order od)
    {
        float duration = SalesManager.Instance != null ? SalesManager.Instance.CookSeconds : 3f;

        if (_fillRoutine != null) StopCoroutine(_fillRoutine);
        _fillRoutine = StartCoroutine(FillRoutine(duration));
    }

    private void HandleOrderReady(SalesManager.Order od)
    {
        if (_fillRoutine != null) StopCoroutine(_fillRoutine);
        StartCoroutine(ReadyFlashRoutine());
    }
    private IEnumerator ReadyFlashRoutine()
    {
        SetFill(1f);
        SetVisible(true);
        // 0.25초 동안 1→0.7→1 플래시
        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.deltaTime;
            float s = 1f + 0.1f * Mathf.Sin(t * 30f * Mathf.Deg2Rad); // 아주 가벼운 깜빡
            if (fillImage) fillImage.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        if (fillImage) fillImage.transform.localScale = Vector3.one;
        if (hideWhenIdle) SetVisible(false);
    }

    private void HandleOrderRemoved(SalesManager.Order od)
    {
        // 조리 중 취소 등 → 진행 UI 초기화
        if (_fillRoutine != null) StopCoroutine(_fillRoutine);
        SetFill(0f);
        if (hideWhenIdle) SetVisible(false);
    }

    private IEnumerator FillRoutine(float duration)
    {
        if (duration <= 0.01f) duration = 0.01f;

        SetFill(0f);
        SetVisible(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetFill(Mathf.Clamp01(t / duration));
            yield return null;
        }

        // 완료 직전엔 OnOrderReady가 올 것이므로 여기서는 유지
        // 혹시 이벤트 순서 타이밍 문제로 Ready가 늦게 오면 꽉 찬 상태 유지
        SetFill(1f);
    }

    private void SetFill(float v)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(v);
        }
    }

    private void SetVisible(bool on)
    {
        if (canvas != null)
            canvas.enabled = on;
        if (fillImage != null)
            fillImage.enabled = on || !hideWhenIdle;
    }
}