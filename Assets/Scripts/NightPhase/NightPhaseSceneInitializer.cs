using System.Collections;
using UnityEngine;
using static NightPhaseManager;
public enum SeatSide { LeftColumn, BottomRow, RightColumn }
[System.Serializable]
public class SeatGroup
{
    public Transform[] leftSeats;
    public Transform[] bottomSeats;
    public Transform[] rightSeats;

    public SalesManager.SeatSlot[] BuildSlots()
    {
        var list = new System.Collections.Generic.List<SalesManager.SeatSlot>();
        if (bottomSeats != null)
            foreach (var t in bottomSeats)
                if (t) list.Add(new SalesManager.SeatSlot { point = t, side = SeatSide.BottomRow });

        if (rightSeats != null)
            foreach (var t in rightSeats)
                if (t) list.Add(new SalesManager.SeatSlot { point = t, side = SeatSide.RightColumn });

        if (leftSeats != null)
            foreach (var t in leftSeats)
                if (t) list.Add(new SalesManager.SeatSlot { point = t, side = SeatSide.LeftColumn });

        return list.ToArray();
    }
}
public class NightPhaseSceneInitializer : MonoBehaviour
{
    [Header("좌석들")]
    [SerializeField] private SeatGroup SeatGroup = new SeatGroup();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(InitializationRoutine());
    }

    private IEnumerator InitializationRoutine()
    {
        // 1. 모든 싱글톤 매니저가 Awake()를 마칠 때까지 한 프레임 대기
        yield return null;

        // 2. NightPhaseManager 리셋
        if (NightPhaseManager.Instance != null)
        {
            NightPhaseManager.Instance.ResetForNewNightPhase();
            NightPhaseManager.Instance.SetSeatGroup(SeatGroup);
        }
        else
        {
            Debug.LogError("NightPhaseManager가 없습니다!");
            yield break;
        }
        // 3. SalesManager 리셋
        if (SalesManager.Instance != null)
        {
            SalesManager.Instance.ResetForNewNightPhase();
        }
        else
        {
            Debug.LogError("SalesManager가 없습니다!");
            yield break;
        }

        // (다른 리셋해야 할 매니저가 있다면 여기서 호출)

        UIManager.Instance.ShowPopupUI<UI_RestaurantPreparePopup>("UI_RestaurantPreparePopup");
        WarehouseManager.Instance.OnEnterNightPhase_TransferAll();
    }
}
