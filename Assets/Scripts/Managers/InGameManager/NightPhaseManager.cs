using System;
using System.Collections.Generic;
using System.Linq;
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
    public enum SeatSide { LeftColumn, BottomRow, RightColumn }

    [Serializable]
    public class SeatGroup
    {
        public Transform[] leftSeats;
        public Transform[] bottomSeats;
        public Transform[] rightSeats;

        public SalesManager.SeatSlot[] BuildSlots()
        {
            var list = new System.Collections.Generic.List<SalesManager.SeatSlot>();
            if (leftSeats != null)
                foreach (var t in leftSeats)
                    if (t) list.Add(new SalesManager.SeatSlot{point = t, side = SeatSide.LeftColumn});
            if (bottomSeats != null)
                foreach (var t in bottomSeats)
                    if (t) list.Add(new SalesManager.SeatSlot{point = t, side = SeatSide.BottomRow});
            if (rightSeats != null)
                foreach (var t in rightSeats)
                    if (t) list.Add(new SalesManager.SeatSlot{point = t, side = SeatSide.RightColumn});
            return list.ToArray();
        }
    }

    [Header("세 개의 테이블 좌석 (좌/하/우) 한 번만 세팅")]
    public SeatGroup seatGroup;

    const int MaxRestaurantLevel = 3;


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
    private static bool IsSideUnlocked(int level, SeatSide side)
    {
        // Lv1: Bottom만, Lv2: Bottom+Right, Lv3: Bottom+Right+Left
        return side switch
        {
            SeatSide.BottomRow => level >= 1,
            SeatSide.RightColumn => level >= 2,
            SeatSide.LeftColumn => level >= 3,
            _ => false
        };
    }

    private SalesManager.SeatSlot[] GetUnlockedSeatSlots()
    {
        if (seatGroup == null) return Array.Empty<SalesManager.SeatSlot>();
        var all = seatGroup.BuildSlots();
        return all.Where(s => IsSideUnlocked(RestaurantLevel, s.side)).ToArray();
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

        var unlocked = GetUnlockedSeatSlots();
        List<MenuPlan> menus = SalesManager.Instance.TodayMenus;
        //SalesManager.Instance.StartService(_serviceTime, _maxSeat, SeatPointsByLevel[RestaurantLevel - 1]);
        int allowedMaxSeat = Mathf.Min(_maxSeat, unlocked.Length);

        SalesManager.Instance.StartService(_serviceTime, allowedMaxSeat, unlocked);
    }
}
