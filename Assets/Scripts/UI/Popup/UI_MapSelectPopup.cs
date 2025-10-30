using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_MapSelectPopup : UI_Popup
{
    [SerializeField] private Button grassLandSelect;
    [SerializeField] private Button swampLandSelect;
    [SerializeField] private Button winterLandSelect;

    public static event Action<MapType> OnMapSelected;

    private void Start()
    {
        grassLandSelect.onClick.AddListener(OnClickedGrassLandSelect);
        swampLandSelect.onClick.AddListener(OnClickedSwampLandSelect);
        winterLandSelect.onClick.AddListener(OnClickedWinterLandSelect);
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
