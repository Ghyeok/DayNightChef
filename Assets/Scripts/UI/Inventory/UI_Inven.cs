using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_Inven : UI_Popup
{
    public enum Buttons
    {
        ExitBtn,
    }

    public enum GameObjects
    {
        InvenSlot,
    }

    public enum Texts
    {
        InvenWeight,
    }

    private Slot[] _slots; // 슬롯 배열
    private TextMeshProUGUI _weightText;

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
            Debug.LogError("[UI_Inven] ExitBtn 을 찾지 못했습니다. 하이어라키 이름을 확인하세요.");

        _weightText = GetText((int)Texts.InvenWeight);
        if (_weightText == null)
            Debug.LogError("[UI_Inven] InvenWeight 텍스트가 없습니다. 하이어라키 이름을 확인하세요.");

        InventoryManager.Instance.Init();

        // 슬롯 루트 찾기
        var rootGo = Get<GameObject>((int)GameObjects.InvenSlot);
        if (rootGo == null)
        {
            Debug.LogError("[UI_Inven] InvenSlot 오브젝트를 찾지 못했습니다. 하이어라키 이름을 확인하세요.");
            return; // 더 진행하면 NRE
        }
        var root = rootGo.transform;

        // 슬롯 수집
        _slots = root.GetComponentsInChildren<Slot>(includeInactive: true);
        if (_slots == null || _slots.Length == 0)
        {
            Debug.LogWarning("[UI_Inven] InvenSlot 하위에 Slot이 없습니다. 슬롯 프리팹을 배치하세요.");
        }
        else
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].index = i;
                _slots[i].TryAutoWireChildren();
            }
        }

        InventoryManager.Instance.OnInventoryChanged -= RefreshAll;
        InventoryManager.Instance.OnInventoryChanged += RefreshAll;
        RefreshAll();
    }

    private void OnDestroy()
    {
        // 팝업이 닫히거나 씬 전환 시 이벤트 구독 해제
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshAll;
    }

    private void RefreshAll()
    {
        var entries = InventoryManager.Instance.Entries;
        int N = Mathf.Min(_slots.Length, entries.Count);

        for (int i = 0; i < N; i++)
        {
            _slots[i].RefreshUI();
        }
        RefreshWeightUI();
    }

    private void RefreshWeightUI()
    {
        if (_weightText == null) return;
        float cur = InventoryManager.Instance.CurrentWeight;
        float max = InventoryManager.Instance.maxWeight;
        string curStr = $"{cur:0.##}";
        string maxStr = $"{max:0.##}";
        _weightText.text = $"Weight {curStr} / {maxStr} kg";

        float ratio = (max > 0f) ? (cur / max) : 0f;
        if (ratio >= 1f)
            _weightText.color = new Color(0.9f, 0.2f, 0.2f); // 빨강
        else if (ratio >= 0.8f)
            _weightText.color = new Color(1f, 0.6f, 0.2f); // 주황
        else
            _weightText.color = Color.white; // 기본 흰색
    }

    public static UI_Inven Show() => UIManager.Instance.ShowPopupUI<UI_Inven>("UI_Inven");
}
