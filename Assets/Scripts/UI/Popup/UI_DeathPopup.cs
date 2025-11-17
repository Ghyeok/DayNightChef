using UnityEngine;
using UnityEngine.UI;

public class UI_DeathPopup : UI_Popup
{
    public enum Buttons
    {
        ConfirmButton,
    }

    public enum GameObjects
    {
        InvenSlot,
    }

    private Slot[] _slots;

    // 선택된 슬롯 인덱스 (-1이면 선택 없음)
    private int _selectedIndex = -1;

    // 슬롯 기본 색 / 선택 색
    private Color _slotDefaultColor = Color.white;
    private Color _slotSelectedColor = new Color(1f, 0.6f, 0.5f); // 살짝 노란색

    public override void Init()
    {
        base.Init();

        _selectedIndex = -1;

        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));

        var confirmBtn = GetButton((int)Buttons.ConfirmButton);
        if (confirmBtn != null)
        {
            AddUIEvent(confirmBtn.gameObject, _ =>
            {
                var inv = InventoryManager.Instance;

                if (inv != null && inv.Entries != null)
                { 
                    if (HasAnyItem()) // 아이템이 하나라도 있으면
                    {
                        if (HasValidSelection())
                        {
                            KeepOnlySelectedItem();
                        }
                        else
                        {
                            ClearAllItems();
                        }
                    }
                }

                // 팝업 닫고 밤 페이즈로 이동
                UIManager.Instance.ClosePopupUI(this);
                SceneLoader.Instance.LoadScene(
                    "NightPhaseScene",
                    UnityEngine.SceneManagement.LoadSceneMode.Single);
            });
        }
        else
        {
            Debug.LogError("[UI_DeathPopup] ConfirmButton 을 찾지 못했습니다. 하이어라키 이름을 확인하세요.");
        }

        var rootGo = Get<GameObject>((int)GameObjects.InvenSlot);
        if (rootGo == null)
        {
            Debug.LogError("[UI_DeathPopup] InvenSlot 오브젝트를 찾지 못했습니다. 하이어라키 이름을 확인하세요.");
            return;
        }

        var root = rootGo.transform;
        _slots = root.GetComponentsInChildren<Slot>(includeInactive: true);
        if (_slots == null || _slots.Length == 0)
        {
            Debug.LogWarning("[UI_DeathPopup] InvenSlot 하위에 Slot이 없습니다. 슬롯 프리팹을 배치하세요.");
        }
        else
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].index = i;
                _slots[i].TryAutoWireChildren();

                // 슬롯 클릭 시 선택 처리
                int capture = i;
                AddUIEvent(_slots[i].gameObject, _ => OnClickSlot(capture));
            }

            // 첫 슬롯의 Image 색을 기본 색으로 사용
            var firstImg = _slots[0].GetComponent<Image>();
            if (firstImg != null)
                _slotDefaultColor = firstImg.color;
        }

        // 인벤토리 변경 시 슬롯 UI 갱신
        var inv = InventoryManager.Instance;
        if (inv != null)
        {
            inv.OnInventoryChanged -= RefreshAll;
            inv.OnInventoryChanged += RefreshAll;
        }

        RefreshAll();
        ShowAnnouncement();
    }

    private async void ShowAnnouncement()
    {
        await UI_ConfirmPopup.ShowAsync(
            info: "사망했습니다!\n밤 페이즈로 가져갈 재료를 한 가지 선택하세요.",
            left: "확인",
            right: "확인");
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshAll;
    }

    private void RefreshAll()
    {
        var inv = InventoryManager.Instance;
        if (inv == null || inv.Entries == null || _slots == null) return;

        int entryCount = inv.Entries.Count;
        int slotCount = _slots.Length;

        for (int i = 0; i < slotCount; i++)
        {
            _slots[i].index = i;

            if (i < entryCount)
            {
                _slots[i].RefreshUI();
            }
            else
            {
                _slots[i].RefreshUI();
            }

            // 선택 색상 반영
            var img = _slots[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = (i == _selectedIndex) ? _slotSelectedColor : _slotDefaultColor;
            }
        }
    }

    public static UI_DeathPopup Show()
        => UIManager.Instance.ShowPopupUI<UI_DeathPopup>("UI_DeathPopup");

    /// <summary>
    /// 슬롯을 클릭했을 때 호출
    /// </summary>
    private void OnClickSlot(int slotIndex)
    {
        var inv = InventoryManager.Instance;
        if (inv == null || inv.Entries == null) return;
        if (slotIndex < 0 || slotIndex >= inv.Entries.Count) return;

        var entry = inv.Entries[slotIndex];

        // 빈 슬롯이면 선택하지 않음
        if (entry.item == null || entry.count <= 0)
            return;

        // 현재 선택 해제
        if (_selectedIndex == slotIndex)
        {
            var curImg = _slots[slotIndex].GetComponent<Image>();
            if (curImg != null)
                curImg.color = _slotDefaultColor;

            _selectedIndex = -1;
            Debug.Log("[UI_DeathPopup] 슬롯 선택 해제");
            return;
        }

        // 이전 선택 해제
        if (_selectedIndex >= 0 && _selectedIndex < _slots.Length)
        {
            var prevImg = _slots[_selectedIndex].GetComponent<Image>();
            if (prevImg != null)
                prevImg.color = _slotDefaultColor;
        }

        // 새 슬롯 선택
        _selectedIndex = slotIndex;

        // 새 선택 하이라이트
        if (_selectedIndex >= 0 && _selectedIndex < _slots.Length)
        {
            var img = _slots[_selectedIndex].GetComponent<Image>();
            if (img != null)
                img.color = _slotSelectedColor;
        }

        Debug.Log($"[UI_DeathPopup] 슬롯 {_selectedIndex} 선택");
    }

    /// <summary>
    /// 현재 선택된 인덱스가 유효하고 아이템이 존재하는지 검사
    /// </summary>
    private bool HasValidSelection()
    {
        var inv = InventoryManager.Instance;
        if (inv == null || inv.Entries == null) return false;
        if (_selectedIndex < 0 || _selectedIndex >= inv.Entries.Count) return false;

        var e = inv.Entries[_selectedIndex];
        return (e.item != null && e.count > 0);
    }

    /// <summary>
    /// 인벤토리에 아이템이 하나라도 있는지 확인
    /// </summary>
    private bool HasAnyItem()
    {
        var inv = InventoryManager.Instance;
        if (inv == null || inv.Entries == null) return false;

        foreach (var e in inv.Entries)
        {
            if (e.item != null && e.count > 0)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 선택된 슬롯 하나만 남기고 나머지 인벤토리 슬롯 비우기
    /// </summary>
    private void KeepOnlySelectedItem()
    {
        var inv = InventoryManager.Instance;
        if (inv == null || inv.Entries == null) return;
        if (!HasValidSelection()) return;

        int count = inv.Entries.Count;

        if (!HasValidSelection())
        {
            for (int i = 0; i < count; i++)
            {
                inv.SetSlot(i, null, 0);
            }
            Debug.Log("[UI_DeathPopup] 선택한 재료가 없어, 모든 인벤토리를 비우고 넘어갑니다.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            if (i == _selectedIndex) continue;

            inv.SetSlot(i, null, 0);
        }
    }

    /// <summary>
    /// 인벤토리 전체 아이템 삭제
    /// </summary>
    private void ClearAllItems()
    {
        var inv = InventoryManager.Instance;
        if (inv == null || inv.Entries == null) return;

        int count = inv.Entries.Count;
        for (int i = 0; i < count; i++)
        {
            inv.SetSlot(i, null, 0);
        }

        Debug.Log("[UI_DeathPopup] 선택된 슬롯이 없어, 인벤토리 전체를 비우고 넘어갑니다.");
    }
}
