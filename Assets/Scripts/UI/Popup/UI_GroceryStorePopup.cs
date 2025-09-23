using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using NUnit.Framework.Constraints;
public class UI_GroceryStorePopup : UI_Popup
{
    public enum Buttons
    {
        ExitBtn,
    }

    public enum Texts
    {
        GoldText,
        WeightText,
    }

    public enum GameObjects
    {
        StoreSlots,
    }
    [SerializeField] private GameObject storeSlotPrefab;

    private List<StoreSlot> _storeSlots = new();
    private InventoryManager _inv;
    private GameManager _gm;

    private TextMeshProUGUI _goldText;
    private TextMeshProUGUI _weightText;
    private Transform _slotRoot;

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        var exitBtn = GetButton((int)Buttons.ExitBtn);
        if (exitBtn != null)
            UI_Base.AddUIEvent(exitBtn.gameObject, _ => UIManager.Instance.ClosePopupUI(this));
        else
            Debug.LogError("[UI_Inven] ExitBtn 을 찾지 못했습니다. 하이어라키 이름을 확인하세요.");
        _goldText = GetText((int)Texts.GoldText);
        _weightText = GetText((int)Texts.WeightText);

        // 슬롯 루트 찾기
        var rootGo = Get<GameObject>((int)GameObjects.StoreSlots);
        if (rootGo == null)
        {
            Debug.LogError("[UI_Inven] InvenSlot 오브젝트를 찾지 못했습니다. 하이어라키 이름을 확인하세요.");
            return;
        }
        _slotRoot = rootGo.transform;
        
        _inv = InventoryManager.Instance;
        _gm = GameManager.Instance;

        CollectOrBuildSlots();

        InjectStoreData();

        RefreshGold();
        RefreshWeight();

        if (_gm != null) _gm.OnGoldChanged += RefreshGold;
        if (_inv != null) _inv.OnInventoryChanged += RefreshWeight;
    }
    // 상점 슬롯에 상점매니저에서 받아온 아이템 주입
    private void InjectStoreData()
    {
        var items = StoreManager.Instance.items;
        for (int i = 0; i < _storeSlots.Count; i++)
        {
            bool active = i < items.Count && items[i] != null;
            _storeSlots[i].gameObject.SetActive(active);
            if (active) _storeSlots[i].SetData(items[i], i);
        }
    }
    // 상점 슬롯 할당
    private void CollectOrBuildSlots()
    {
        _storeSlots.Clear();
        _slotRoot.GetComponentsInChildren(true, _storeSlots);
        int need = StoreManager.Instance?.items?.Count?? 0;
        while (_storeSlots.Count < need)
        {
            if(storeSlotPrefab == null) break;

            var go = Instantiate(storeSlotPrefab, _slotRoot);
            var slot = go.GetComponent<StoreSlot>();
            if (slot == null) break;
            _storeSlots.Add(slot);

        }
    }

    private void RefreshGold()
    {
        if (_goldText != null && _gm != null)
            _goldText.text = $"{(_gm?.totalGold ?? 0)}G";
    }

    private void RefreshWeight()
    {
        if(_weightText != null && _inv != null)
        {
            _weightText.text = $"{_inv.CurrentWeight:0.#}/{_inv.maxWeight:0.#}g";
        }
    }

    private void OnDisable()
    {
        if (_gm != null) _gm.OnGoldChanged -= RefreshGold;
        if (_inv != null) _inv.OnInventoryChanged -= RefreshWeight;
    }

    public static UI_GroceryStorePopup Show() => UIManager.Instance.ShowPopupUI<UI_GroceryStorePopup>("UI_GroceryStorePopup");
}
