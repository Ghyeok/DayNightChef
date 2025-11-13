using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
public class UI_UpgradePopup : UI_Popup
{
    public enum Buttons
    {
        ExitBtn,
        AttackBtn,
        SpeedBtn,
        WeightBtn,
        HPBtn,
        FishingBtn,
        RestaurantBtn,
    }

    public enum Texts
    {
        CurGoldText,
        AttackLvText,
        AttackGoldText,
        SpeedLvText,
        SpeedGoldText,
        WeightLvText,
        WeightGoldText,
        HPLvText,
        HPGoldText,
        FishingLvText,
        FishingGoldText,
        RestaurantLvText,
        RestaurantGoldText,
    }

    private TextMeshProUGUI _curGold;
    private TextMeshProUGUI _attackLv, _attackGold;
    private TextMeshProUGUI _speedLv, _speedGold;
    private TextMeshProUGUI _weightLv, _weightGold;
    private TextMeshProUGUI _hpLv, _hpGold;
    private TextMeshProUGUI _fishingLv, _fishingGold;
    private TextMeshProUGUI _restaurantLv, _restaurantGold;

    private PlayerStatsManager _stats;
    private GameManager _gm;
    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        _stats = PlayerStatsManager.Instance;
        _gm = GameManager.Instance;

        var exitBtn = GetButton((int)Buttons.ExitBtn);
        if (exitBtn != null)
            UI_Base.AddUIEvent(exitBtn.gameObject, _ => UIManager.Instance.ClosePopupUI(this));

        _curGold = GetText((int)Texts.CurGoldText);
        _attackLv = GetText((int)Texts.AttackLvText);
        _attackGold = GetText((int)Texts.AttackGoldText);
        _speedLv = GetText((int)Texts.SpeedLvText);
        _speedGold = GetText((int)Texts.SpeedGoldText);
        _weightLv = GetText((int)Texts.WeightLvText);
        _weightGold = GetText((int)Texts.WeightGoldText);
        _hpLv = GetText((int)Texts.HPLvText);
        _hpGold = GetText((int)Texts.HPGoldText);
        _fishingLv = GetText((int)Texts.FishingLvText);
        _fishingGold = GetText((int)Texts.FishingGoldText);
        _restaurantLv = GetText((int)Texts.RestaurantLvText);
        _restaurantGold = GetText((int)Texts.RestaurantGoldText);

        WireUpgradeButton(Buttons.AttackBtn, StatType.Attack);
        WireUpgradeButton(Buttons.SpeedBtn, StatType.MoveSpeed);
        WireUpgradeButton(Buttons.WeightBtn, StatType.BagWeight);
        WireUpgradeButton(Buttons.HPBtn, StatType.MaxHP);
        WireUpgradeButton(Buttons.FishingBtn, StatType.FishingRod);
        WireUpgradeButton(Buttons.RestaurantBtn, StatType.Restaurant);

        if(_gm != null)
        {
            _gm.OnGoldChanged -= RefreshGold;
            _gm.OnGoldChanged += RefreshGold;
        }

        PlayerStatsManager.Instance.OnReady -= RefreshAll;
        PlayerStatsManager.Instance.OnReady += RefreshAll;

        PlayerStatsManager.Instance.OnStatChanged -= HandleStatChanged;
        PlayerStatsManager.Instance.OnStatChanged += HandleStatChanged;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGoldChanged -= RefreshAll;
        PlayerStatsManager.Instance.OnReady -= RefreshAll;
        PlayerStatsManager.Instance.OnStatChanged -= HandleStatChanged;
    }

    private void WireUpgradeButton(Buttons btnEnum, StatType type)
    {
        var btn = GetButton((int)btnEnum);
        if (btn == null) return;
        UI_Base.AddUIEvent(btn.gameObject, async _ =>
        {
            if (!btn.interactable) return;
            await TryUpgradeAsync(type);
        });
    }

    //UI 갱신 로직

    private void RefreshGold()
    {
        if (_curGold != null && _gm != null)
            _curGold.text = $"{_gm.totalGold}G";
    }

    private void RefreshAll()
    {
        if (_stats == null || _gm == null) return;

        RefreshGold();
        RefreshStat(_attackLv, _attackGold, StatType.Attack);
        RefreshStat(_speedLv, _speedGold, StatType.MoveSpeed);
        RefreshStat(_weightLv, _weightGold, StatType.BagWeight);
        RefreshStat(_hpLv, _hpGold, StatType.MaxHP);
        RefreshStat(_fishingLv, _fishingGold, StatType.FishingRod);
        RefreshStat(_restaurantLv, _restaurantGold, StatType.Restaurant);

    }

    private void RefreshStat(TextMeshProUGUI lvText, TextMeshProUGUI costText, StatType type)
    {
        if (_stats == null) return;

        int lv = _stats.GetLevel(type);
        int cost = _stats.GetNextCost(type);
        bool maxed = (cost <= 0);

        if (lvText) lvText.text = maxed ? $"현재 레벨 : {lv}" : $"현재 레벨 : {lv}";
        if (costText) costText.text = maxed ? "필요 골드\n-G" : $"필요 골드\n{cost}G";

        Button btn = null;
        switch (type)
        {
            case StatType.Attack: btn = GetButton((int)Buttons.AttackBtn); break;
            case StatType.MoveSpeed: btn = GetButton((int)Buttons.SpeedBtn); break;
            case StatType.BagWeight: btn = GetButton((int)Buttons.WeightBtn); break;
            case StatType.MaxHP: btn = GetButton((int)Buttons.HPBtn); break;
            case StatType.FishingRod: btn = GetButton((int)Buttons.FishingBtn); break;
            case StatType.Restaurant: btn = GetButton((int)Buttons.RestaurantBtn); break;
        }

        if (btn != null)
        {
            bool interactable = !(maxed || _gm.totalGold < cost);
            btn.interactable = interactable;

            var cg = btn.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = btn.gameObject.AddComponent<CanvasGroup>();
            }
            cg.alpha = interactable ? 1f : 0.7f;
        }
    }

    private void HandleStatChanged(StatType type, int oldLv, int newLv)
    {
        RefreshGold();

        switch(type)
        {
            case StatType.Attack:  RefreshStat(_attackLv, _attackGold, type); break;
            case StatType.MoveSpeed: RefreshStat(_speedLv, _speedGold, type); break;
            case StatType.BagWeight: RefreshStat(_weightLv, _weightGold, type); break;
            case StatType.MaxHP: RefreshStat(_hpLv, _hpGold, type); break;
            case StatType.FishingRod: RefreshStat(_fishingLv, _fishingGold, type); break;
            case StatType.Restaurant: RefreshStat(_restaurantLv, _restaurantGold, type); break;
        }
    }

    // 업그레이드 로직

    private string GetDisplayName(StatType type) => type switch
    {
        StatType.Attack => "공격력",
        StatType.MoveSpeed => "이동속도",
        StatType.BagWeight => "가방 무게",
        StatType.MaxHP => "최대 HP",
        StatType.FishingRod => "낚싯대",
        StatType.Restaurant => "레스토랑",
        _ => type.ToString()
    };

    private async Task TryUpgradeAsync(StatType type)
    {
        if (_stats == null || _gm == null) return;

        int curLv = _stats.GetLevel(type);
        int nextLv = curLv + 1;
        float curValue = _stats.GetValue(type);
        float nextValue = _stats.GetValueAtLevel(type, nextLv);
        int cost = _stats.GetNextCost(type);

        if (cost <= 0) // 최대 레벨
        {
            return;
        }

        string info =
            $"{curLv}Lv >> {nextLv}Lv\n" +
            $"{GetDisplayName(type)} + {nextValue - curValue}\n" +
            $"필요 골드 : {cost}G";
        bool ok = await UI_ConfirmPopup.ShowAsync(info, left: "예",right: "아니오");
        if (!ok) return;

        //Confirm 팝업이 닫힌 뒤, 이 팝업이 살아있는지 확인
        if (this == null || !isActiveAndEnabled) return;

        // 보유량 확인
        if (!_stats.CanUpgrade(type))
        {
            // TODO : 골드 부족 알림
            return;
        }

        // 실제 업그레이드
        if(_stats.TryUpgrade(type))
        {
            //TODO 성공시 이펙트 or 팝업 or 사운드
        }
        else
        {
            //TODO 실패시 팝업 or 사운드
        }

    }

    public static UI_UpgradePopup Show() =>
        UIManager.Instance.ShowPopupUI<UI_UpgradePopup>("UI_UpgradePopup");
}
