using UnityEngine;

/* 낮, 밤 공통으로 사용되는 기능을 관리
 */

public class GameManager : SingletonManager<GameManager>
{
    public enum GameState
    {
        DayPhase,
        NightPhase,
    }

    public int currentWeek;
    public int totalGold;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
