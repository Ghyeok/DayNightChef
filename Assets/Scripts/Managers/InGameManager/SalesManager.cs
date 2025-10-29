using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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
    [SerializeField] private List<MenuPlan> menus = new(); // TODO 준비단계에서 채움
    [SerializeField] private List<bool> soldOutMenus = new(); // 품절된 메뉴들

    [Header("손님")]
    [SerializeField] private GameObject[] customerPrefabs; // 손님 프리팹
    [SerializeField] private Transform[] seatPoints; // 좌석 위치들
    private bool[] seatOccupied; // 좌석 점유 상태
    private float spawnInterval = 5f; // 손님 생성 간격

    [Header("Runtime")]
    [SerializeField] private float _timeLeft;
    [SerializeField] private int _maxSeat;
    [SerializeField] private int _remainingSeats;
    [SerializeField] private int customerCount; // 현재 손님 수

    public void SetMenus(List<MenuPlan> plans)
    {
        menus = plans ?? new List<MenuPlan>();
    }

    public bool IsAllSoldOut() // 전체 품절 여부 확인
    {
        return menus.All(m => m.Remaining <= 0);

    }
}