using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_WarehousePopup : UI_Popup
{
    public enum Buttons
    {
        ExitBtn,
    }

    public enum GameObjects
    {
        SlotRoot, // 인벤 슬롯 루트
    }

    public enum Texts
    {
        SlotCountText, // 무게 대신 슬롯 카운트
    }

    private WarehouseSlot[] _slots; // 슬롯 배열
    private TextMeshProUGUI _countText;

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));
        Bind<TextMeshProUGUI>(typeof(Texts));

        var exitBtn = GetButton((int)Buttons.ExitBtn);
        if (exitBtn != null)
            UI_Base.AddUIEvent(exitBtn.gameObject, _ => UIManager.Instance.ClosePopupUI(this));
        else
            Debug.LogError("[UI_WarehousePopup] ExitBtn 을 찾지 못했습니다. 하이어라키 이름을 확인하세요.");

        _countText = GetText((int)Texts.SlotCountText);
        if (_countText == null)
            Debug.LogError("[UI_WarehousePopup] SlotCountText 텍스트가 없습니다. 하이어라키 이름을 확인하세요.");

        // 슬롯 루트 찾기
        var rootGo = Get<GameObject>((int)GameObjects.SlotRoot);
        if (rootGo == null)
        {
            Debug.LogError("[UI_WarehousePopup] SlotRoot 오브젝트를 찾지 못했습니다. 하이어라키 이름을 확인하세요.");
            return;
        }
        var root = rootGo.transform;

        // 슬롯 수집
        _slots = root.GetComponentsInChildren<WarehouseSlot>(includeInactive: true);
        if (_slots == null || _slots.Length == 0)
        {
            Debug.LogWarning("[UI_WarehousePopup] SlotRoot 하위에 Slot이 없습니다. 슬롯 프리팹을 배치하세요.");
        }
        else
        {
            // 각 슬롯에 인덱스 부여 (Slot.cs가 이 index를 사용할 것으로 예상)
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].index = i;
                _slots[i].TryAutoWireChildren();
            }
        }

        // WarehouseManager의 이벤트에 구독
        WarehouseManager.Instance.OnWarehouseChanged -= RefreshAll;
        WarehouseManager.Instance.OnWarehouseChanged += RefreshAll;

        // UI 즉시 갱신
        RefreshAll();
    }

    private void OnDestroy()
    {
        // 팝업이 닫히거나 씬 전환 시 이벤트 구독 해제
        if (WarehouseManager.Instance != null)
            WarehouseManager.Instance.OnWarehouseChanged -= RefreshAll;
    }

    /// <summary>
    /// 창고 UI 전체를 새로고침
    /// </summary>
    private void RefreshAll()
    {
        var warehouseSlots = WarehouseManager.Instance.Slots;

        int N = Mathf.Min(_slots.Length, warehouseSlots.Count);

        for (int i = 0; i < N; i++)
        {
            _slots[i].RefreshUI();
        }

        // 무게 UI 대신 슬롯 카운트 UI 갱신
        RefreshSlotCountUI();
    }

    /// <summary>
    /// 하단 슬롯 카운트 텍스트 갱신
    /// </summary>
    private void RefreshSlotCountUI()
    {
        if (_countText == null) return;

        var warehouseSlots = WarehouseManager.Instance.Slots;

        // 사용 중인 슬롯 개수 계산
        int usedCount = 0;
        foreach (var entry in warehouseSlots)
        {
            if (entry.item != null)
                usedCount++;
        }

        int maxCount = warehouseSlots.Count;

        _countText.text = $"Slots {usedCount} / {maxCount}";

        // 인벤 무게처럼 비율에 따라 색상 변경
        float ratio = (maxCount > 0f) ? ((float)usedCount / maxCount) : 0f;

        if (ratio >= 1f)
            _countText.color = new Color(0.9f, 0.2f, 0.2f); // 빨강 (꽉 참)
        else if (ratio >= 0.8f)
            _countText.color = new Color(1f, 0.6f, 0.2f); // 주황 (80% 이상)
        else
            _countText.color = Color.white; // 기본 흰색
    }

    /// <summary>
    /// 창고 팝업을 띄우는 static 함수
    /// </summary>
    public static UI_WarehousePopup Show()
    {
        // 프리팹 이름이 "UI_WarehousePopup"이라고 가정
        return UIManager.Instance.ShowPopupUI<UI_WarehousePopup>("UI_WarehousePopup");
    }
}