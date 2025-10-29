using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    [Header("이동/인내심")]
    [Min(0.5f)] public float moveSpeed = 2f;
    [Min(1f)] public float patienceSeconds = 25f;
    // 인내심 그래프 추가 필요

    [Header("좌석 & 입구")]
    [SerializeField] private Transform seatTarget;
    [SerializeField] private Transform entryPoint;

    [Header("머리 위 UI")]
    [SerializeField] private Canvas headCanvas;
    [SerializeField] private Image wantIcon;
    [SerializeField] private Image patienceFill; // 인내심 게이지
    [SerializeField] private GameObject bubbleGroup; // 머리 위 UI 오브젝트

    private SalesManager _sales;
    private int _seatIndex = -1;
    private float _patience01 = 1f; // 0~1 사이 인내심 , 퍼센트에 따라 평판 변동
    private bool _isSeated = false;
    private bool _leaving = false;
    public Recipe Want { get; private set; }
    public bool IsServed { get; private set; }

    // SalesManager에서 생성 직후 호출

    public void Begin(SalesManager sales, int seatIndex, Recipe want, Transform entry, Transform seat)
    {
        _sales = sales;
        _seatIndex = seatIndex;
        Want = want;
        entryPoint = entry;
        seatTarget = seat;

        // 시작 위치
        if (entryPoint != null) transform.position = entryPoint.position;

        // 머리 UI
        if (headCanvas != null) headCanvas.enabled = true;
        if (bubbleGroup != null) bubbleGroup.SetActive(false); // 앉은 뒤에 활성화 예정
        UpdatePatienceUI();

        if (wantIcon != null)
        {
            var icon = (Want != null) ? Want.recipe_image : null;
            wantIcon.sprite = icon;
            wantIcon.enabled = false; // 앉은 뒤에 활성화 예정
        }

        StopAllCoroutines();
        StartCoroutine(Co_RunLife());
    }

    private IEnumerator Co_RunLife()
    {
        // 좌석으로 이동
        yield return StartCoroutine(Co_MoveTo(seatTarget));
        _isSeated = true;

        // 앉은 뒤 UI 활성화
        if (bubbleGroup != null) bubbleGroup.SetActive(true);
        if (wantIcon != null) wantIcon.enabled = true;

        // 인내심 타이머 가동
        float t = 0f;
        while (!_leaving && !IsServed)
        {
            t += Time.deltaTime;
            _patience01 = Mathf.Clamp01(1f - t/patienceSeconds);
            UpdatePatienceUI();

            if (_patience01 <= 0f)
            {
                Leave(success: false, served: null);
                yield break;
            }
            yield return null;

        }
    }

    private IEnumerator Co_MoveTo(Transform target)
    {
        yield return null;
    }

    private void UpdatePatienceUI()
    {
        if (patienceFill != null) patienceFill.fillAmount = Mathf.Clamp01(_patience01);
    }

    // 서빙
    public void TryServe(Recipe served)
    {
        if (_leaving || IsServed) return;
        if (!_isSeated) return; // 앉기전 서빙 불가

        if (served == Want)
        {
            IsServed = true;
            if (served != null)
            {
                GameManager.Instance.AddGold(served.recipe_price);              
            }
            Leave(success: true, served: served);
        }
        else
        {
            // 원하는 요리가 아닌 다른요리 서빙]
            if (served != null)
            {
                GameManager.Instance.AddGold(served.recipe_price / 2); // 다른 요리 서빙 시 절반 금액 지급
            }
            Leave(success: false, served: served);
        }
    }

    private void Leave(bool success, Recipe served)
    {
        if (_leaving) return;
        _leaving = true;
        if (success)
        {
            // 평판 상승
            NightPhaseManager.Instance.Reputation += 2; // 원하는 요리 서빙 시 평판 상승
        }
        else NightPhaseManager.Instance.Reputation -= 1; //인내심 다 달거나 ,다른 요리 서빙 시 평판 하락
        Destroy(gameObject);
    }

}
