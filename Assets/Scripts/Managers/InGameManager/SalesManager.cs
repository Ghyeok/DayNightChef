using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;

[System.Serializable]
public class MenuPlan
{
    public Recipe recipe;
    [Min(0)] public int planned; // 목표 수량
    [HideInInspector] public int sold; // 판매된 수량
    public int Remaining => Mathf.Max(0, planned - sold); // 남은 수량
}
public class SalesManager : SingletonManager<SalesManager>
{
    [Header("오늘의 영업 메뉴")]
    [SerializeField] private List<MenuPlan> menus = new();
    public List<MenuPlan> TodayMenus { get { return menus; } }

    [Header("손님")]
    [SerializeField] private GameObject[] customerPrefabs; // 손님 프리팹
    [SerializeField] private Transform[] _seatPoints; // 좌석 위치들
    [SerializeField] private Transform entryPoint; // 입구 위치
    private bool[] seatOccupied; // 좌석 점유 상태
    private float spawnInterval = 5f; // 손님 생성 간격

    [Header("Runtime")]
    [SerializeField] private float _timeLeft;
    [SerializeField] private int _maxSeat;
    [SerializeField] private int _remainingSeats;
    [SerializeField] private int customerCount; // 현재 손님 수
    private bool _isServiceRunning = false;

    public void SetMenus(List<MenuPlan> plans)
    {
        menus = plans ?? new List<MenuPlan>();
    }

    public bool IsAllSoldOut() // 전체 품절 여부 확인
    {
        return menus.All(m => m.Remaining <= 0);

    }
    // 영업 시작
    public void StartService(float serviceTime, int maxSeats, Transform[] seatPoints)
    {
        if (_isServiceRunning) return;

        _seatPoints = seatPoints;
        _isServiceRunning = true;
        _timeLeft = serviceTime;
        _maxSeat = maxSeats;
        _remainingSeats = maxSeats;
        seatOccupied = new bool[_seatPoints.Length];

        StartCoroutine(Co_ServiceRoutine());
    }
    // 메인 루프
    private IEnumerator Co_ServiceRoutine()
    {
        Coroutine spawnLoop = StartCoroutine(Co_SpawnLoop());
        while (_timeLeft > 0f && !IsAllSoldOut())
        {
            _timeLeft -= Time.deltaTime;
            //NightPhaseManager.Instance.TickService(_timeLeft);
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

        // 판매 가능한 메뉴 선택
        var candidates = menus.Where(m => m.Remaining > 0).ToList();
        if (candidates.Count == 0) return;

        MenuPlan chosen = candidates[Random.Range(0, candidates.Count)];
        GameObject prefab = customerPrefabs[Random.Range(0, customerPrefabs.Length)];
        GameObject go = Instantiate(prefab);

        var c = go.GetComponent<Customer>();
        if (c == null) return;
        c.Begin(this, seatIdx, chosen.recipe, entryPoint, _seatPoints[seatIdx]);
        seatOccupied[seatIdx] = true;
        customerCount++;
    }

    // 빈 좌석 찾기
    private int FindFreeSeat()
    {
        for(int i = 0; i < Mathf.Min(_maxSeat, _seatPoints.Length); i++)
        {
            if (!seatOccupied[i]) return i;
        }
        return -1;
    }

    // 손님 퇴장 처리
    public void OnCustomerLeave(int seatIndex, bool success, Recipe served)
    {
        if (seatIndex >= 0 && seatIndex < seatOccupied.Length)
            seatOccupied[seatIndex] = false;
        if (success && served != null)
        {
            var mp = menus.FirstOrDefault(m => m.recipe == served);
            if (mp != null) mp.sold++;
        }

        customerCount = Mathf.Max(0, customerCount - 1);
    }

    // 영업 종료
    private void EndService()
    {
        _isServiceRunning = false;
        int totalSold = menus.Sum(m => m.sold);
        int totalRevenue = menus.Sum(m => m.sold * (m.recipe != null ? m.recipe.recipe_price : 0));

        //NightPhaseManager.Instance.EndService();
    }

    //강제 중단
    public void ResetSales()
    {
        StopAllCoroutines();
        _isServiceRunning = false;
        customerCount = 0;
    }
}