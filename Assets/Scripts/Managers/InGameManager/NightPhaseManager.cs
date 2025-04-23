using UnityEngine;

/* 밤 페이즈에서 사용될 기능을 모아놓는 매니저
 * 
 */

public class NightPhaseManager : SingletonManager<NightPhaseManager>
{
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

    public int reputation;
    public int customerNum;

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
}
