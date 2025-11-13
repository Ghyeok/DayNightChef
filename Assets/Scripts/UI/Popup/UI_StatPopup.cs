using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
public class UI_StatPopup : UI_Popup
{
    public enum Buttons
    {
        ExitBtn,
    }
    public enum Texts
    {
        AttackLvText,
        SpeedLvText,
        WeightLvText,
        HPLvText,
        FishingLvText,
        RestaurantLvText,
    }
    private TextMeshProUGUI _attackLv;
    private TextMeshProUGUI _speedLv;
    private TextMeshProUGUI _weightLv;
    private TextMeshProUGUI _hpLv;
    private TextMeshProUGUI _fishingLv;
    private TextMeshProUGUI _restaurantLv;
    private PlayerStatsManager _stats;

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        _stats = PlayerStatsManager.Instance;

        var exitBtn = GetButton((int)Buttons.ExitBtn);
        if (exitBtn != null)
            UI_Base.AddUIEvent(exitBtn.gameObject, _ => UIManager.Instance.ClosePopupUI(this));

        _attackLv = GetText((int)Texts.AttackLvText);
        _speedLv = GetText((int)Texts.SpeedLvText);
        _weightLv = GetText((int)Texts.WeightLvText);
        _hpLv = GetText((int)Texts.HPLvText);
        _fishingLv = GetText((int)Texts.FishingLvText);
        _restaurantLv = GetText((int)Texts.RestaurantLvText);

        PlayerStatsManager.Instance.OnStatChanged -= HandleStatChanged;
        PlayerStatsManager.Instance.OnStatChanged += HandleStatChanged;

        SetStat(_attackLv, StatType.Attack);
        SetStat(_speedLv, StatType.MoveSpeed);
        SetStat(_weightLv, StatType.BagWeight);
        SetStat(_hpLv, StatType.MaxHP);
        SetStat(_fishingLv, StatType.FishingRod);
        SetStat(_restaurantLv,StatType.Restaurant);
    }

    private void SetStat(TextMeshProUGUI lvText, StatType type)
    {
        if (_stats == null) return;

        int lv = _stats.GetLevel(type);
        float stat = _stats.GetValueAtLevel(type, lv);

        if (lvText)
        {
            if ((type == StatType.FishingRod) || (type == StatType.Restaurant))
            {
                lvText.text = $"Lv{lv}";
            }
            else lvText.text = $"Lv{lv}\n{stat}";
        }
    }

    private void HandleStatChanged(StatType type, int oldLv, int newLv)
    {
        switch (type)
        {
            case StatType.Attack: SetStat(_attackLv, type); break;
            case StatType.MoveSpeed: SetStat(_speedLv, type); break;
            case StatType.BagWeight: SetStat(_weightLv, type); break;
            case StatType.MaxHP: SetStat(_hpLv, type); break;
            case StatType.FishingRod: SetStat(_fishingLv, type); break;
            case StatType.Restaurant: SetStat(_restaurantLv, type);break;
        }
    }

    private void OnDisable()
    {
        PlayerStatsManager.Instance.OnStatChanged -= HandleStatChanged;
    }

    public static UI_StatPopup Show() =>
        UIManager.Instance.ShowPopupUI<UI_StatPopup>("UI_StatPopup");
}
