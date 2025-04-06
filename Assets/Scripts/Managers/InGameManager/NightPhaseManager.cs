using UnityEngine;

/* 밤 페이즈에서 사용될 기능을 모아놓는 매니저
 * 
 */

public class NightPhaseManager : MonoBehaviour
{
    public enum RestaurantType
    {
        Ready,
        Open,
        Close,
    }

    public enum CustomerType
    {
        Normal,
        Special
    }

    public enum UpgradeType
    {
        Restaurant,
    }

    public int managementFee;
    public int reputation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
