using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/* 밤 페이즈에서 사용될 기능을 모아놓는 매니저
 * 
 */

public class NightPhaseManager : SingletonManager<NightPhaseManager>
{
    [Header("레스토랑 정보")]
    public int RestaurantLevel = 1; // 현재 레스토랑 레벨
    public int Reputation = 0; // 현재 레스토랑 평판
    [Header("레스토랑 스텟")]
    [SerializeField] private int _maxSeat = 5; // 최대 좌석 수
    [SerializeField] private float _serviceTime = 120f; // 현재 레스토랑 레벨에 따른 영업 시간
    [Header("레벨 별 좌석 위치들 저장용")]
    public Transform[][] SeatPointsByLevel; // 2차원 배열로 레벨별 좌석 위치들 저장

    public int MaxSeat => _maxSeat;
    public float ServiceTime => _serviceTime;

    public event Action OnServiceStarted;
    public event Action<float> OnServiceTick; // 남은 시간 전달
    public event Action OnServiceEnded;

    public enum RestaurantState
    {
        Ready,
        Open,
        Close,
    }

    public enum CustomerType
    {
        Normal,
        Special,
    }

    public RestaurantState state;
    public int reputation;
    public int customerNum;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state = RestaurantState.Ready;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpgradeRestaurant()
    {

    }

    public void TickService(float timeLeft)
    {
        OnServiceTick?.Invoke(Mathf.Max(0f, timeLeft));
    }

    public void EndService()
    {
        if (state != RestaurantState.Open) return;

        OnServiceEnded?.Invoke();
        state = RestaurantState.Close;
        // TODO 결과 팝업 , 낮페이지 돌입
    }
    // 영업준비 매니저 에서 오늘의 메뉴 받아오기 -> SalesManager에 세팅 -> 영업 시작
    public void StartService()
    {
        if (state != RestaurantState.Open) return;
        OnServiceStarted?.Invoke();
        List<MenuPlan> menus = SalesManager.Instance.TodayMenus;
        SalesManager.Instance.StartService(_serviceTime, _maxSeat, SeatPointsByLevel[RestaurantLevel - 1]);
    }
}
