using UnityEngine;

/* 낮 페이즈에서 사용될 기능을 모아놓는 매니저
 * 
 */

public class DayPhaseManager : Managers<DayPhaseManager>
{
    public enum PlayerState
    {
        Alive,
        Dead,
    }

    public enum PlayerBehavior
    {
        Hunting,
        Fishing,
        Gathering,
        MaxCount,
    }

    public enum MapType // 온대, 열대, 한대 기후
    {
        Warm,
        Hot,
        Cold,
        MaxCount,
    }

    public enum UpgradeType
    {
        Hp,
        MoveSpeed,
        Knife,
        Fishing,
        Bag,
        MaxCount,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
