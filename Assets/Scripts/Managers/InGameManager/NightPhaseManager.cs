using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* 밤 페이즈에서 사용될 기능을 모아놓는 매니저
 * 
 */

[DefaultExecutionOrder(-100)]
public class NightPhaseManager : SingletonManager<NightPhaseManager>
{
    private const string TAG = "[Night]";
    private const bool VERBOSE = true;
    private static void Log(string msg)
    {
        if (VERBOSE) Debug.Log($"{TAG} {msg}");
    }
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

    public void ResetForNewNightPhase()
    {
        state = RestaurantState.Ready;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
        var unlocked = all.Where(s => IsSideUnlocked(RestaurantLevel, s.side)).ToArray();
        Log($"좌석 해제: L{RestaurantLevel} → {unlocked.Length}좌석 사용 가능");
        return unlocked;
    }

    public void TickService(float timeLeft)
    {
        OnServiceTick?.Invoke(Mathf.Max(0f, timeLeft));
    }

    public void EndService()
    {
        if (state != RestaurantState.Open) return;
        Log("EndService() 호출 → 상태 Close");
        OnServiceEnded?.Invoke();
        state = RestaurantState.Close;
        EndNightPhase();
    }
    // 영업준비 매니저 에서 오늘의 메뉴 받아오기 -> SalesManager에 세팅 -> 영업 시작
    public void StartService()
    {
        if (state == RestaurantState.Open)
        {
            Log("StartService 호출 무시: 이미 Open 상태");
            return;
        }
        state = RestaurantState.Open;
        Log("StartService → 상태 Open, OnServiceStarted 이벤트");
        OnServiceStarted?.Invoke();

        var unlocked = GetUnlockedSeatSlots();
        int allowedMaxSeat = Mathf.Min(_maxSeat, unlocked.Length);
        SalesManager.Instance.SetMenus(RestaurantPrepareManager.Instance.TodayMenu);

        Log($"영업 시작 위임: serviceTime={_serviceTime}s, maxSeat={allowedMaxSeat}");
        SalesManager.Instance.StartService(_serviceTime, allowedMaxSeat, unlocked);
    }

    /// <summary>
    /// 밤 페이즈의 끝
    /// 1. 관리비 납부 주인지 확인, 만약 관리비 납부 주라면 관리비 납부 팝업이 뜬다.
    /// 2. 
    /// </summary>
    public void EndNightPhase()
    {
        GameManager.Instance.EndDayNightLoop(); // currentWeek 증가, 목표 금액 달성했는가? 
        // TODO -> 정산 팝업 띄우기
        SceneLoader.Instance.LoadScene("DayPhaseScene",UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
