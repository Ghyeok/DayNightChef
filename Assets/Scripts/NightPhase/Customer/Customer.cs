using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    [Header("인내심")]
    [Min(1f)] public float patienceSeconds = 25f;
    // 인내심 그래프 추가 필요
    [Header("머리 위 이미지")]
    public Sprite wantImage;
    private SalesManager _sales;
    private int _seatIndex = -1;
    bool _issitted = false;

    public Recipe Want { get; private set; }
    public bool IsServed { get; private set; }

    public void Init(SalesManager sales, int seatIndex, Recipe want)
    {
        _sales = sales;
        _seatIndex = seatIndex;
        Want = want;
        wantImage = want.recipe_image;
    }

    private void MoveToSeat()
    {
        // 좌석으로 이동하는 로직 구현 필요
        _issitted = true;
    }

}
