using System;
using UnityEngine;

/* 낮, 밤 공통으로 사용되는 기능을 관리
 * 골드 추가 시 AddGold(int g)
 * 골드 소비 시 TrySpendGold(int g) -> true시 SpendGold(int g)
 */

public class GameManager : SingletonManager<GameManager>
{
    public enum GameState
    {
        DayPhase,
        NightPhase,
    }

    public event Action OnGoldChanged;

    public int currentWeek;
    public int totalGold;
    public int[] managementFees;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddGold(int g)
    {
        totalGold += g;
        OnGoldChanged?.Invoke();
    }
    //골드 소비시 true 반환, 실패시 false 반환
    public bool TrySpendGold(int g)
    {
        if (totalGold >= g)
        {
            return true;
        }
        return false;
    }
    public void SpendGold(int g)
    {
        if (totalGold >= g)
        {
            totalGold -= g;
            OnGoldChanged?.Invoke();
        }
    }
    //테스트용
    public void GiveGold()
    {
        int g = 100;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(g);
        }
    }

    public void TestSpendGold()
    {
        int g = 1;
        if (GameManager.Instance != null)
        {
            if(GameManager.Instance.TrySpendGold(g))
            {
                GameManager.Instance.SpendGold(g);
            }
        }
    }

    public void EndDayNightLoop()
    {
        Debug.Log("루프 끝! 맵 선택으로 넘어갑니다.");
        currentWeek++;
    }
}
