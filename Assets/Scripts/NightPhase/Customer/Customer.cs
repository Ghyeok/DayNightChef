using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Customer : MonoBehaviour
{
    /// <summary>
    /// 로그용
    /// </summary>
    private const string TAG = "[Customer]";
    private const bool VERBOSE = true;
    private static void Log(string msg)
    {
        if (VERBOSE) Debug.Log($"{TAG} {msg}");
    }
    [Header("이동/인내심")]
    [Min(0.5f)] public float moveSpeed = 2f;
    [Min(1f)] public float patienceSeconds = 25f;
    // 인내심 그래프 추가 필요

    [Header("좌석")]
    [SerializeField] private Transform seatTarget;
    [Header("머리 위 UI")]
    [SerializeField] private Canvas headCanvas;
    [SerializeField] private Image wantIcon;
    [SerializeField] private Image patienceFill; // 인내심 게이지
    [SerializeField] private GameObject bubbleGroup; // 머리 위 UI 오브젝트
    [Header("퇴장 연출")]
    [SerializeField, Min(0.1f)] private float leaveDistance = 2f;
    [SerializeField, Min(0.05f)] private float leaveSpeedMul = 2f;
    private Vector3 _spawnPosWS;

    private SalesManager _sales;
    private int _seatIndex = -1;
    private float _patience01 = 1f; // 0~1 사이 인내심 , 퍼센트에 따라 평판 변동
    private bool _isSeated = false;
    private bool _leaving = false;
    public Recipe Want { get; private set; }
    public bool IsServed { get; private set; }

    // SalesManager에서 생성 직후 호출

    public void Begin(SalesManager sales, int seatIndex, Recipe want, Transform seat, Vector2 spawnPos)
    {
        _sales = sales;
        _seatIndex = seatIndex;
        Want = want;
        seatTarget = seat;

        // 스폰 좌표 저장 (z는 현 z 유지)
        _spawnPosWS = new Vector3(spawnPos.x, spawnPos.y, transform.position.z);
        transform.position = _spawnPosWS;

        if (headCanvas != null) headCanvas.enabled = true;
        if (bubbleGroup != null) bubbleGroup.SetActive(false);
        UpdatePatienceUI();

        if (wantIcon != null)
        {
            wantIcon.sprite = (Want != null) ? Want.recipe_image : null;
            wantIcon.enabled = false; // 착석 후 표시
        }
        Log($"입장: seatIndex={_seatIndex}, want={(Want != null ? Want.recipe_name : "NULL")}, spawn={spawnPos}");
        StopAllCoroutines();
        StartCoroutine(Co_RunLife());
    }
    private IEnumerator Co_RunLife()
    {
        // 좌석으로 이동
        yield return StartCoroutine(Co_MoveTo(seatTarget));
        _isSeated = true;
        Log($"착석 완료: seatIndex={_seatIndex}");

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
                Log("인내심 0 → 퇴장(실패)");
                Leave(success: false, served: null);
                yield break;
            }
            yield return null;

        }
    }

    private IEnumerator Co_MoveTo(Transform target)
    {
        if (target == null) yield break;

        float zNow = transform.position.z;
        Vector3 endPos = new Vector3(target.position.x, target.position.y, zNow);

        float dist = Vector2.Distance(transform.position, endPos);
        float duration = Mathf.Max(dist / moveSpeed, 0.01f);

        // 이전 이동 트윈 정리
        transform.DOKill();

        Tweener tw = transform.DOMove(endPos, duration)
                              .SetEase(Ease.OutSine)
                              .SetLink(gameObject);
        Log($"이동 시작 → {endPos} (dur:{duration:F2}s)");
        yield return tw.WaitForCompletion();
        Log("이동 완료");
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

            Log($"서빙 성공: {served?.recipe_name}");
            Leave(success: true, served: served);
        }
        else
        {
            // 원하는 요리가 아닌 다른요리 서빙]
            Log($"오서빙(다른 요리): {served?.recipe_name} (원함:{Want?.recipe_name})");
            Leave(success: false, served: served);
        }
    }

    private void Leave(bool success, Recipe served)
    {
        if (_leaving) return;
        _leaving = true;

        Log($"퇴장: success={success}, served={served?.recipe_name}, rep={NightPhaseManager.Instance.Reputation}");

        // 좌석 비우기(게임 로직상 바로 해제)
        _sales?.OnCustomerLeave(_seatIndex, success, Want, served);

        // 머리 위 UI는 끄거나 페이드
        if (bubbleGroup != null) bubbleGroup.SetActive(false);

        // 퇴장 연출
        StopAllCoroutines();
        StartCoroutine(Co_LeaveRoutine());
    }
    // 퇴장 코루틴
    private IEnumerator Co_LeaveRoutine()
    {
        // 좌석 기준 → 스폰 방향 벡터
        Vector3 dir;
        if (seatTarget != null)
            dir = (_spawnPosWS - seatTarget.position).normalized;
        else
            dir = (_spawnPosWS - transform.position).normalized;

        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.down; // 안전 가드

        float zNow = transform.position.z;
        Vector3 endPos = transform.position + dir * leaveDistance;
        endPos.z = zNow;

        // 이동 시간: 평소 속도 기준으로 약간 빠르게
        float speed = Mathf.Max(0.1f, moveSpeed * leaveSpeedMul);
        float duration = Mathf.Max(leaveDistance / speed, 0.05f);

        // 기존 트윈 정리 후 새 트윈
        transform.DOKill();

        Tweener tw = transform.DOMove(endPos, duration)
                              .SetEase(Ease.InSine)
                              .SetLink(gameObject);

        Log($"퇴장 이동 시작 → {endPos} (dur:{duration:F2}s)");
        yield return tw.WaitForCompletion();
        Log("퇴장 이동 완료 → 오브젝트 파괴");

        Destroy(gameObject);
    }

}
