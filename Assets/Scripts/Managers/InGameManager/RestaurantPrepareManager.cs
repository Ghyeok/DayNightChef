using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 레스토랑 운영 준비 매니저, 어떤 요리를 몇개 판매할건지 정하는 단계
/// </summary>
public class RestaurantPrepareManager : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIManager.Instance.ShowPopupUI<UI_RestaurantPreparePopup>("UI_RestaurantPreparePopup");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
