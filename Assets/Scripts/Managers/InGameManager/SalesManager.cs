using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;
using System;

[System.Serializable]
public class MenuPlan
{
    public Recipe recipe;
    [Min(0)] public int planned; // 목표 수량
    [HideInInspector] public int allocated; // 주문된 수량
    [HideInInspector] public int sold; // 판매된 수량
    // 주문 가능 잔여(주문 받을 수 있는 수량) = planned - allocated
    public int RemainingToOrder => Mathf.Max(0, planned - allocated);
    // 재고(목표) 소진 여부: 더 이상 주문 받을 수 없음
    public bool IsExhausted => RemainingToOrder <= 0;
}
public class SalesManager : SingletonManager<SalesManager>
{
    public enum OrderState { Queued, Cooking, Ready, Served, Canceled}

    [System.Serializable]
    public class Order
    {
        public int orderId;
        public Customer customer;
        public Recipe recipe;
        public OrderState state;
        public float queuedAt; // 시간
        public float readyAt; // 준비 완료 시간
    }

    private readonly Queue<Order> _pendingQueue = new();
    private readonly Queue<Order> _readyQueue = new();
    private Order _cookingNow = null;

    [Header("조리 시간")]
    [SerializeField] private float defaultCookSeconds = 4f;

    public event Action<Order> OnOrderQueued;
    public event Action<Order> OnOrderStarted;
    public event Action<Order> OnOrderReady;
    public event Action<Order> OnOrderRemoved;

    private int _orderSeq = 0;

    [System.Serializable]
    public struct SeatSlot
    {
        public Transform point;
        public NightPhaseManager.SeatSide side;
    }

    [Header("오늘의 영업 메뉴")]
    [SerializeField] private List<MenuPlan> menus = new();
    public List<MenuPlan> TodayMenus { get { return menus; } }

    [Header("손님")]
    [SerializeField] private GameObject[] customerPrefabs; // 손님 프리팹

    [Header("좌석/스폰 설정")]
    [SerializeField, Min(0.5f)] private float spawnOffset = 2f;
    private SeatSlot[] _seatSlots;   // NightPhaseManager에서 현재 레벨 기준으로 넘겨받음
    private bool[] seatOccupied;

    [Header("스폰 템포")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 5f;

    [Header("Runtime")]
    [SerializeField] private float _timeLeft;
    [SerializeField] private int _maxSeat;
    [SerializeField] private int customerCount; // 현재 손님 수
    private bool _isServiceRunning = false;

    public void SetMenus(List<MenuPlan> plans)
    {
        menus = plans ?? new List<MenuPlan>();
    }

    public bool IsAllSoldOut() // 전체 품절 여부 확인
    {
        return menus.All(m => m.RemainingToOrder <= 0);

    }

    // 손님 주문 시 호출
    public void PlaceOrder(Customer customer, Recipe recipe)
    {
        if (customer == null || recipe == null) return;

        var od = new Order
        {
            orderId = ++_orderSeq,
            customer = customer,
            recipe = recipe,
            state = OrderState.Queued,
            queuedAt = Time.time
        };
        _pendingQueue.Enqueue(od);
        OnOrderQueued?.Invoke(od);

        if (_cookingNow == null)
        {
            StartCoroutine(Co_CookLoop());
        }
    }
    // 조리 루프
    private IEnumerator Co_CookLoop()
    {
        while (_isServiceRunning)
        {
            // 더 조리할 것이 없고, 현재도 없으면 종료
            if (_cookingNow == null && _pendingQueue.Count == 0) yield break;

            // 조리 시작
            if (_cookingNow == null && _pendingQueue.Count > 0)
            {
                _cookingNow = _pendingQueue.Dequeue();
                if (_cookingNow.customer == null)
                {
                    // 손님이 사라지면 그 손님의 주문은 폐기
                    _cookingNow.state = OrderState.Canceled;
                    OnOrderRemoved?.Invoke(_cookingNow);
                    _cookingNow = null;
                    continue;
                }

                _cookingNow.state = OrderState.Cooking;
                OnOrderStarted?.Invoke(_cookingNow);
                float cookSec = defaultCookSeconds;

                yield return new WaitForSeconds(cookSec);

                if (_cookingNow.customer == null)
                {
                    _cookingNow.state = OrderState.Canceled;
                    OnOrderRemoved?.Invoke(_cookingNow);
                    _cookingNow = null;
                    continue;
                }

                _cookingNow.state = OrderState.Ready;
                _cookingNow.readyAt = Time.time;
                _readyQueue.Enqueue(_cookingNow);
                OnOrderQueued?.Invoke(_cookingNow);
                _cookingNow = null;
            }
            yield return null;
        }
    }

    // 요리 수령
    public bool TryPopReadyOrder(out Order order)
    {
        if (_readyQueue.Count > 0)
        {
            order = _readyQueue.Dequeue();
            return true;
        }
        order = null;
        return false;
    }

    private void RollbackAllocated(Recipe recipe)
    {
        var mp = menus.FirstOrDefault(m => m.recipe == recipe);
        if (mp != null) mp.allocated = Mathf.Max(0, mp.allocated - 1);
    }
    // 영업 시작
    public void StartService(float serviceTime, int maxSeats, SeatSlot[] seatSlots)
    {
        if (_isServiceRunning) return;

        _timeLeft = Mathf.Max(1f, serviceTime);
        _maxSeat = Mathf.Max(1, maxSeats);
        _seatSlots = seatSlots ?? System.Array.Empty<SeatSlot>();
        if (_seatSlots.Length == 0)
        {
            Debug.LogWarning("[SalesManager] 좌석이 설정되어 있지 않습니다.");
            return;
        }
        seatOccupied = new bool[_seatSlots.Length];
        customerCount = 0;
        _isServiceRunning = true;

        StartCoroutine(Co_ServiceRoutine());
    }
    // 메인 루프
    private IEnumerator Co_ServiceRoutine()
    {
        Coroutine spawnLoop = StartCoroutine(Co_SpawnLoop());
        while (_timeLeft > 0f && !IsAllSoldOut())
        {
            _timeLeft -= Time.deltaTime;
            NightPhaseManager.Instance.TickService(_timeLeft);
            yield return null;
        }

        if (spawnLoop != null) StopCoroutine(spawnLoop);
        EndService();
    }

    // 손님 생성 루프
    private IEnumerator Co_SpawnLoop()
    {
        while(_timeLeft > 0f && !IsAllSoldOut())
        {
            yield return new WaitForSeconds(spawnInterval);

            // 평판에 따른 스폰 간격 조정
            float rep = NightPhaseManager.Instance.Reputation;
            float interval = Mathf.Max(2f, spawnInterval - 0.04f * rep);

            TrySpawnCustomer();
            yield return new WaitForSeconds(interval);
        }
    }

    // 손님 생성
    private void TrySpawnCustomer()
    {
        // 좌석 체크
        int seatIdx = FindFreeSeat();
        if (seatIdx < 0) return;

        // 메뉴 설정
        var candidates = menus.Where(m => m.RemainingToOrder > 0).ToList();
        if (candidates.Count == 0) return;

        var chosen = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        chosen.allocated++;
        var prefab = customerPrefabs[UnityEngine.Random.Range(0, customerPrefabs.Length)];
        var go = Instantiate(prefab);

        var customer = go.GetComponent<Customer>();
        if (customer == null)
        {
            Debug.LogWarning("[SalesManager] Customer 컴포넌트가 없습니다. 프리팹을 확인하세요.");
            Destroy(go);
            return;
        }

        var slot = _seatSlots[seatIdx];
        Vector2 spawnPos = ComputeSpawnPos(slot);

        customer.Begin(this,
                       seatIdx,
                       chosen.recipe,
                       slot.point,       // 좌석 Transform
                       spawnPos);        // 방향별 계산된 스폰 좌표

        seatOccupied[seatIdx] = true;
        customerCount++;
        // 주문 큐 등록
        PlaceOrder(customer, chosen.recipe);
    }

    // 빈 좌석 찾기
    private int FindFreeSeat()
    {
        int limit = Mathf.Min(_maxSeat, _seatSlots.Length);
        for (int i = 0; i < limit; i++)
            if (!seatOccupied[i]) return i;
        return -1;
    }

    // 좌석 기준 스폰위치 확인

    private Vector2 ComputeSpawnPos(in SeatSlot slot)
    {
        if (slot.point == null) return Vector2.zero;
        Vector2 basePos = slot.point.position;

        return slot.side switch
        {
            NightPhaseManager.SeatSide.BottomRow => basePos + Vector2.down * spawnOffset,
            NightPhaseManager.SeatSide.LeftColumn => basePos + Vector2.left * spawnOffset,
            NightPhaseManager.SeatSide.RightColumn => basePos + Vector2.right * spawnOffset,
            _ => basePos
        };
    }

    // 손님 퇴장 처리
    public void OnCustomerLeave(int seatIndex, bool success,Recipe wanted, Recipe served)
    {
        if (seatOccupied != null && (uint)seatIndex < seatOccupied.Length)
            seatOccupied[seatIndex] = false;

        var mp = menus.FirstOrDefault(m => m.recipe == wanted);
        if (mp != null)
        {
            // 주문으로 잡아둔 수량은 무조건 해제
            mp.allocated = Mathf.Max(0, mp.allocated - 1);

            if (success)
            {
                // 정상 서빙 완료
                mp.sold++;
            }
            // 실패(인내심 0, 오서빙 등)는 sold 증가 없음
        }

        customerCount = Mathf.Max(0, customerCount - 1);
    }

    // 영업 종료
    private void EndService()
    {
        _isServiceRunning = false;

        // 모든 주문 취소 및 롤백
        while (_pendingQueue.Count > 0) { var od = _pendingQueue.Dequeue(); od.state = OrderState.Canceled; OnOrderRemoved?.Invoke(od); RollbackAllocated(od.recipe); }
        _cookingNow = null;
        while (_readyQueue.Count > 0) { var od = _readyQueue.Dequeue(); od.state = OrderState.Canceled; OnOrderRemoved?.Invoke(od); RollbackAllocated(od.recipe); }

        int totalSold = menus.Sum(m => m.sold);
        int totalRevenue = menus.Sum(m => m.sold * (m.recipe != null ? m.recipe.recipe_price : 0));

        NightPhaseManager.Instance.EndService();
    }

    //강제 중단
    public void ResetSales()
    {
        StopAllCoroutines();
        _isServiceRunning = false;
        customerCount = 0;
    }
}