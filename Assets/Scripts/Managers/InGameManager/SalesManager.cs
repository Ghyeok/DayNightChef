using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
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
[DefaultExecutionOrder(-200)]
public class SalesManager : SingletonManager<SalesManager>
{
    /// <summary>
    /// 로그용
    /// </summary>
    private const string TAG = "[Sales]";
    private const bool VERBOSE = true;
    private static void Log(string msg)
    {
        if (VERBOSE) Debug.Log($"{TAG} {msg}");
    }
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
    [SerializeField] private int _totalCapacity; // 이번 영업 총 입장 한도 (Sum planned)
    [SerializeField] private int _spawnedTotal;  // 이번 영업 동안 실제 스폰된 손님 누계
    private readonly Queue<Order> _pendingQueue = new();
    private readonly Queue<Order> _readyQueue = new();
    private Order _cookingNow = null;
    private int _goldAccrued = 0;
    public int GoldAccrued() { return _goldAccrued; }
    private int _reputationAccrued = 0;
    public int ReputationAccrued() { return _reputationAccrued; }

    [Header("조리 시간")]
    [SerializeField] private float defaultCookSeconds = 4f;

    public event Action<Order> OnOrderQueued;
    public event Action<Order> OnOrderStarted;
    public event Action<Order> OnOrderReady;
    public event Action<Order> OnOrderRemoved;
    public event Action<Order> OnReadyDequeued; // 완료 큐에서 꺼낼때

    public bool IsServiceRunning => _isServiceRunning;
    public bool IsCooking => _cookingNow != null;
    public int ReadyCount => _readyQueue.Count;
    public Order CurrentCookingOrder => _cookingNow;

    public event Action<Recipe, Recipe, int, bool> OnSaleRecorded;

    private int _orderSeq = 0;

    [System.Serializable]
    public struct SeatSlot
    {
        public Transform point;
        public SeatSide side;
    }

    [Header("오늘의 영업 메뉴")]
    [SerializeField] private List<MenuPlan> menus = new();
    [Header("잘못 서빙 패널티")]
    [SerializeField] private int wrongServeReputationPenalty = -2;
    [SerializeField] private int correctServeReputationReward = +2;
    public List<MenuPlan> TodayMenus => menus;

    public void ResetForNewNightPhase()
    {
        menus.Clear();
        _isServiceRunning = false;
        customerCount = 0;
        _goldAccrued = 0;
        _reputationAccrued = 0;
    }

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
    public float CookSeconds => defaultCookSeconds;

    public void SetMenus(List<MenuPlan> plans)
    {
        menus = plans ?? new List<MenuPlan>();
    }

    public bool IsAllSoldOut() // 전체 예약 여부 확인
    {
        return menus.All(m => m.RemainingToOrder <= 0);

    }
    // 전체 판매 여부 확인
    public bool IsAllSalesCompleted()
    {
        return menus.All(m => m.sold >= m.planned);
    }
    private static string SafeRecipeName(Recipe r) => r != null ? r.recipe_name : "<NULL RECIPE>";
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
        Log($"주문 대기 큐 등록: #{od.orderId} {SafeRecipeName(recipe)}");
        OnOrderQueued?.Invoke(od);

        if (_cookingNow == null)
        {
            Log("조리 루프 시작");
            StartCoroutine(Co_CookLoop());
        }
    }

    // 조리 루프
    private IEnumerator Co_CookLoop()
    {
        while (_isServiceRunning)
        {
            if (_cookingNow == null && _pendingQueue.Count == 0)
            {
                Log("조리 루프 종료(대기 없음)");
                yield break;
            }

            if (_cookingNow == null && _pendingQueue.Count > 0)
            {
                _cookingNow = _pendingQueue.Dequeue();

                // 주문 자체가 깨졌는지(손님/레시피) 선제 점검
                if (_cookingNow == null)
                {
                    Log("경고: _cookingNow가 null입니다(비정상 주문). 다음 주문으로 넘어갑니다.");
                    continue;
                }

                // 손님 null이면 폐기
                if (_cookingNow.customer == null)
                {
                    _cookingNow.state = OrderState.Canceled;
                    Log($"주문 취소(고객 소멸): #{_cookingNow.orderId}, recipe={SafeRecipeName(_cookingNow.recipe)}");
                    OnOrderRemoved?.Invoke(_cookingNow);
                    _cookingNow = null;
                    continue;
                }

                // 레시피 null이면 폐기(여기서 NRE 방지)
                if (_cookingNow.recipe == null)
                {
                    _cookingNow.state = OrderState.Canceled;
                    Log($"주문 취소(recipe null): #{_cookingNow.orderId}");
                    OnOrderRemoved?.Invoke(_cookingNow);
                    _cookingNow = null;
                    continue;
                }

                _cookingNow.state = OrderState.Cooking;
                Log($"조리 시작: #{_cookingNow.orderId} {SafeRecipeName(_cookingNow.recipe)}");
                OnOrderStarted?.Invoke(_cookingNow);

                float cookSec = defaultCookSeconds;
                float cookStart = Time.realtimeSinceStartup;
                float cookEnd = cookStart + cookSec;

                while (Time.realtimeSinceStartup < cookEnd)
                {
                    if (!_isServiceRunning)
                        yield break;
                    yield return null;
                }

                // 조리 중 고객이 사라졌다면 폐기
                if (_cookingNow == null || _cookingNow.customer == null)
                {
                    if (_cookingNow != null)
                    {
                        _cookingNow.state = OrderState.Canceled;
                        Log($"조리 중 취소(고객 소멸): #{_cookingNow.orderId} {SafeRecipeName(_cookingNow.recipe)}");
                        OnOrderRemoved?.Invoke(_cookingNow);
                        _cookingNow = null;
                    }
                    continue;
                }

                // 조리 완료 직전에도 레시피 null 방어
                if (_cookingNow.recipe == null)
                {
                    _cookingNow.state = OrderState.Canceled;
                    Log($"조리 중 취소(recipe null): #{_cookingNow.orderId}");
                    OnOrderRemoved?.Invoke(_cookingNow);
                    _cookingNow = null;
                    continue;
                }

                _cookingNow.state = OrderState.Ready;
                _cookingNow.readyAt = Time.time;
                _readyQueue.Enqueue(_cookingNow);
                Log($"조리 완료: #{_cookingNow.orderId} {SafeRecipeName(_cookingNow.recipe)} (대기 완료큐)");
                OnOrderReady?.Invoke(_cookingNow);

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

            if (order == null || order.recipe == null)
            {
                order = null;
                return false;
            }
            order.state = OrderState.Served;
            Log($"완료 요리 수령: #{order.orderId} {order.recipe.recipe_name}");
            OnReadyDequeued?.Invoke(order); // UI에 제거하라고 알림
            return true;
        }
        order = null;
        return false;
    }

    // 영업 시작
    public void StartService(float serviceTime, int maxSeats, SeatSlot[] seatSlots)
    {
        if (_isServiceRunning) return;

        _timeLeft = Mathf.Max(1f, serviceTime);
        _maxSeat = Mathf.Max(1, maxSeats);
        _seatSlots = seatSlots ?? Array.Empty<SeatSlot>();

        seatOccupied = new bool[_seatSlots.Length];
        customerCount = 0;
        _spawnedTotal = 0;
        _totalCapacity = Mathf.Max(0, menus.Sum(m => Mathf.Max(0, m.planned)));
        _isServiceRunning = true;

        Log($"영업 시작: time={_timeLeft}s, seats={_maxSeat}/{_seatSlots.Length}, planned={menus.Sum(m => m.planned)}");
        StartCoroutine(Co_ServiceRoutine());
    }

    // 메인 루프
    private IEnumerator Co_ServiceRoutine()
    {
        Coroutine spawnLoop = StartCoroutine(Co_SpawnLoop());
        while (_timeLeft > 0f && !IsAllSalesCompleted())
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
        // 첫 스폰은 기존처럼 "즉시" 한 번 일어나도록, 현재 시간으로 초기화
        float nextSpawnTime = Time.realtimeSinceStartup;

        while (_timeLeft > 0f && _isServiceRunning)
        {
            // 오늘 올 손님 다 찼으면, 잠깐씩만 실시간으로 쉰 뒤 다시 검사
            if (_spawnedTotal >= _totalCapacity)
            {
                // 0.25초 후에 다시 체크 (실시간 기준)
                float checkUntil = Time.realtimeSinceStartup + 0.25f;
                while (Time.realtimeSinceStartup < checkUntil)
                    yield return null;

                // 여전히 서비스 중/시간 남았는지 확인 후 루프 계속
                continue;
            }


            float interval = spawnInterval;

            // 다음 스폰 시간이 지났다면 손님 스폰
            if (Time.realtimeSinceStartup >= nextSpawnTime)
            {
                TrySpawnCustomer();

                // 다음 스폰 예정 시간(실시간) 갱신
                nextSpawnTime = Time.realtimeSinceStartup + interval;
            }

            // 한 프레임 쉬면서 다시 검사
            yield return null;
        }
    }

    // 손님 생성

    private void TrySpawnCustomer()
    {
        if (_spawnedTotal >= _totalCapacity) return;
        if (customerCount >= _maxSeat) return;
            // 1) 빈 좌석 확인
        int seatIdx = FindFreeSeat();
        if (seatIdx < 0) { Log("스폰 취소: 빈 좌석 없음"); return; }

        // 2) 주문 가능한 메뉴 찾기 (recipe null 제거)
        var candidates = menus.Where(m => m.RemainingToOrder > 0 && m.recipe != null).ToList();
        if (candidates.Count == 0) { Log("스폰 취소: 주문 가능 메뉴 없음(품절 or recipe null)"); return; }

        // 3) 고객 프리팹 확인
        if (customerPrefabs == null || customerPrefabs.Length == 0)  return;

        // 4) 후보 선택 및 예약(allocated++)
        var chosen = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        chosen.allocated++;

        // 5) 좌석 슬롯/좌표 확인
        if (_seatSlots == null || (uint)seatIdx >= _seatSlots.Length)
        {
            Debug.LogError("[Sales] 스폰 취소: seat index가 유효하지 않습니다.");
            chosen.allocated = Mathf.Max(0, chosen.allocated - 1);
            return;
        }

        var slot = _seatSlots[seatIdx];
        if (slot.point == null)
        {
            chosen.allocated = Mathf.Max(0, chosen.allocated - 1);
            return;
        }

        // 6) 프리팹 인스턴스
        var prefab = customerPrefabs[UnityEngine.Random.Range(0, customerPrefabs.Length)];
        if (prefab == null)
        {
            chosen.allocated = Mathf.Max(0, chosen.allocated - 1);
            return;
        }

        var go = Instantiate(prefab);
        var customer = go.GetComponent<Customer>();
        if (customer == null)
        {
            Destroy(go);
            chosen.allocated = Mathf.Max(0, chosen.allocated - 1);
            return;
        }

        // 7) 스폰 위치 계산 및 투입
        Vector2 spawnPos = ComputeSpawnPos(slot);
        customer.Begin(this, seatIdx, chosen.recipe, slot.point, spawnPos);

        seatOccupied[seatIdx] = true;
        customerCount++;
        _spawnedTotal++;
        // 8) 주문 큐 등록
        PlaceOrder(customer, chosen.recipe);
    }
    // 빈 좌석 찾기
    private int FindFreeSeat()
    {
        int limit = _seatSlots.Length;
        var freeSeats = new List<int>();

        //  비어있는 좌석 인덱스를 모두 수집
        for (int i = 0; i < limit; i++)
        {
            if (!seatOccupied[i])
                freeSeats.Add(i);
        }

        //  비어있는 좌석이 없으면 -1 반환
        if (freeSeats.Count == 0)
            return -1;

        //  빈 좌석 중 하나를 랜덤 선택
        int randIndex = UnityEngine.Random.Range(0, freeSeats.Count);
        int chosenSeat = freeSeats[randIndex];

        return chosenSeat;
    }

    // 좌석 기준 스폰위치 확인

    private Vector2 ComputeSpawnPos(in SeatSlot slot)
    {
        if (slot.point == null) return Vector2.zero;
        Vector2 basePos = slot.point.position;

        return slot.side switch
        {
            SeatSide.BottomRow => basePos + Vector2.down * spawnOffset,
            SeatSide.LeftColumn => basePos + Vector2.left * spawnOffset,
            SeatSide.RightColumn => basePos + Vector2.right * spawnOffset,
            _ => basePos
        };
    }

    // 손님 퇴장 처리
    public void OnCustomerLeave(int seatIndex, bool success, Recipe wanted, Recipe served)
    {
        if (seatOccupied != null && (uint)seatIndex < seatOccupied.Length)
            seatOccupied[seatIndex] = false;

        if (served != null)
        {
            var soldPlan = menus.FirstOrDefault(m => m.recipe == served);
            if (soldPlan != null) soldPlan.sold++;

            // 돈/평판 반영
            RegisterSale(wanted, served);
        }

        customerCount = Mathf.Max(0, customerCount - 1);
        CheckEarlyClose();
    }
    // 영업 종료
    private void EndService()
    {
        if(!_isServiceRunning) return;

        _isServiceRunning = false;

        while (_pendingQueue.Count > 0)
        {
            var od = _pendingQueue.Dequeue();
            od.state = OrderState.Canceled;
            OnOrderRemoved?.Invoke(od);
        }

        _cookingNow = null;
        while (_readyQueue.Count > 0)
        {
            var od = _readyQueue.Dequeue();
            od.state = OrderState.Canceled;
            OnOrderRemoved?.Invoke(od);
        }

        int totalSold = menus.Sum(m => m.sold);
        int totalRevenue = menus.Sum(m => m.sold * (m.recipe != null ? m.recipe.recipe_price : 0));

        // 일괄 정산: 영업 종료 시 한 번에 반영
        if (_goldAccrued > 0)
            GameManager.Instance.AddGold(_goldAccrued);

        GameManager.Instance.restaurantReputation += _reputationAccrued;

        Log($"영업 종료: 총 판매 {totalSold}개, 매출 {totalRevenue}, " +
            $"정산 골드(누적) {_goldAccrued}, 정산 평판 {_reputationAccrued}");

        NightPhaseManager.Instance.EndService();
    }

    //강제 중단
    public void ResetSales()
    {
        StopAllCoroutines();
        _isServiceRunning = false;
        customerCount = 0;
    }
    private void RegisterSale(Recipe wanted, Recipe served)
    {
        if (served == null) return;

        bool matched = (wanted == served);
        int basePrice = served.recipe_price;
        int finalPrice = matched
            ? basePrice
            : 0;

        _goldAccrued += Mathf.Max(0, finalPrice);

        _reputationAccrued += matched ? correctServeReputationReward
                                      : wrongServeReputationPenalty;

        OnSaleRecorded?.Invoke(wanted, served, finalPrice, matched);
    }

    // 영업종료를 일찍하는 함수 ex) 서빙에 실패해도 올 손님들이 모두 왔다감
    private void CheckEarlyClose()
    {
        if (!_isServiceRunning) return;
        bool capacityReached = (_spawnedTotal >= _totalCapacity); // 오늘 올 사람은 다 왔음
        bool noCustomers = (customerCount <= 0);              // 현재 매장에 손님 없음

        if (capacityReached && noCustomers)
        {
            Log("조기 종료: 마지막 손님 퇴장 및 처리 완료");
            EndService();
        }
    }
}