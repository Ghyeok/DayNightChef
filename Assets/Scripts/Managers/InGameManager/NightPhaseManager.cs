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
}
