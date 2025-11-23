using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MapSelectPopup : UI_Popup
{
    [SerializeField] private Button grassLandSelect;
    [SerializeField] private Button swampLandSelect;
    [SerializeField] private Button winterLandSelect;
    [SerializeField] private Button mainLobbyButton;

    [SerializeField] private GameObject swampPanel;
    [SerializeField] private GameObject winterPanel;

    public static event Action<MapType> OnMapSelected;

    private bool unlockSwampLand;
    private bool unlockWinterLand;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        grassLandSelect.onClick.AddListener(OnClickedGrassLandSelect);
        swampLandSelect.onClick.AddListener(OnClickedSwampLandSelect);
        winterLandSelect.onClick.AddListener(OnClickedWinterLandSelect);
        mainLobbyButton.onClick.AddListener(OnClickedMainLobbyButton);
    }

    private void OnEnable()
    {
        unlockSwampLand = GameManager.Instance.unlockSwampLand;
        swampLandSelect.interactable = unlockSwampLand;
        swampPanel.SetActive(!unlockSwampLand);

        unlockWinterLand = GameManager.Instance.unlockWinterLand;
        winterLandSelect.interactable = unlockWinterLand;
        winterPanel.SetActive(!unlockWinterLand);
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
    private void OnClickedMainLobbyButton()
    {
        GameManager.Instance.isDataLoadedFalse();

        UIManager.Instance.ClosePopupUI(this);
        SceneLoader.Instance.SetLoadType(SceneLoader.LoadType.None);
        SceneLoader.Instance.LoadScene("MainLobbyScene",LoadSceneMode.Single);
    }
}
