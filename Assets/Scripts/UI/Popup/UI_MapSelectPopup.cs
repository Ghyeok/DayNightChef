using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_MapSelectPopup : UI_Popup
{
    [SerializeField] private Button grassLandSelect;
    [SerializeField] private Button swampLandSelect;
    [SerializeField] private Button winterLandSelect;

    public static event Action<MapType> OnMapSelected;

    private int unlockSwampLand;
    private int unlockWinterLand;

    private void Start()
    {
        grassLandSelect.onClick.AddListener(OnClickedGrassLandSelect);
        swampLandSelect.onClick.AddListener(OnClickedSwampLandSelect);
        winterLandSelect.onClick.AddListener(OnClickedWinterLandSelect);
    }

    private void OnEnable()
    {
        unlockSwampLand = PlayerPrefs.GetInt(DayPhaseManager.swampLandUnlocked, 0);
        unlockWinterLand = PlayerPrefs.GetInt(DayPhaseManager.winterLandUnlocked, 0);

        swampLandSelect.interactable = unlockSwampLand > 0;
        winterLandSelect.interactable = unlockWinterLand > 0;
    }

    private void OnClickedGrassLandSelect()
    {
        UIManager.Instance.ClosePopupUI(this);
        OnMapSelected?.Invoke(MapType.Warm);
    }
    private void OnClickedSwampLandSelect()
    {
        UIManager.Instance.ClosePopupUI(this);
        OnMapSelected?.Invoke(MapType.Hot);
    }
    private void OnClickedWinterLandSelect()
    {
        UIManager.Instance.ClosePopupUI(this);
        OnMapSelected?.Invoke(MapType.Cold);
    }
}
