using System;
using UnityEngine;

/* 낮, 밤 공통으로 사용되는 기능을 관리
 * 골드 추가 시 AddGold(int g)
 * 골드 소비 시 TrySpendGold(int g) -> true시 SpendGold(int g)
 */
public class GameManager : SingletonManager<GameManager>
{
    public enum GameState
    {
        DayPhase,
        NightPhase,
    }

    private bool isDataLoaded = false;
    public void isDataLoadedFalse() { isDataLoaded = false; }
    public event Action OnGoldChanged;

    public int currentWeek;
    public int totalGold;
    public bool unlockSwampLand;
    public bool unlockWinterLand;

    private int baseFee = 100;
    public int ManagementFee() => baseFee * (currentWeek / 4);

    public override void Awake()
    {
        base.Awake();
    }

    public void AddGold(int g)
    {
        totalGold += g;
        OnGoldChanged?.Invoke();
    }

    //골드 소비시 true 반환, 실패시 false 반환
    public bool TrySpendGold(int g)
    {
        if (totalGold >= g)
        {
            return true;
        }
        return false;
    }

    public void SpendGold(int g)
    {
        if (totalGold >= g)
        {
            totalGold -= g;
            OnGoldChanged?.Invoke();
        }
    }

    public void EndDayNightLoop()
    {
        Debug.Log("루프 끝! 맵 선택으로 넘어갑니다.");
        DayPhasePlayerManager.Instance.dayPlayer = null;
        currentWeek++;
        SaveManager.Instance.SaveGame();
    }

    public void PayManagementFee()
    {
        int pay = ManagementFee();
        if (TrySpendGold(pay))  // 관리비 납부 성공
        {
            SpendGold(pay);
            return;
        }
        else // 관리비 납부 실패
        {
            GameOver();
        }
    }

    public async void GameOver()
    {
        Debug.Log("게임 오버! 세이브 파일을 삭제합니다.");
        SaveManager.Instance.DeleteSaveData();

        bool result = await UI_ConfirmPopup.ShowAsync(
            info: "게임 오버!",
            left: "확인",
            right: "확인"
            );
        if (result || !result)
        {
            UIManager.Instance.CloseAllPopupUI();
            SceneLoader.Instance.LoadScene("MainLobbyScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    #region 게임 데이터 저장/로드 관리
    public void LoadDataOnSceneReady()
    {
        if (isDataLoaded) return;

        if (SceneLoader.Instance.CurrentLoadType == SceneLoader.LoadType.Continue)
        {
            SaveManager.Instance.LoadGame();
        }
        else
        {
            SaveManager.Instance.StartNewGame();
        }
        isDataLoaded = true;
    }

    private PlayerStatsManager GetPlayerStats()
    {
        return PlayerStatsManager.Instance;
    }

    public GameSaveData GetAllDataToSave()
    {
        GameSaveData data = new GameSaveData();
        PlayerStatsManager stats = GetPlayerStats();

        // 1. 플레이어 스탯
        if (stats != null)
        {
            data.maxHPLevel = stats.GetLevel(StatType.MaxHP);
            data.moveSpeedLevel = stats.GetLevel(StatType.MoveSpeed);
            data.attackLevel = stats.GetLevel(StatType.Attack);
            data.fishingLevel = stats.GetLevel(StatType.FishingRod);
            data.weightLevel = stats.GetLevel(StatType.BagWeight);
        }

        // 2. 게임 진행도
        data.currentWeek = this.currentWeek;
        data.currentGold = this.totalGold;
        data.unlockSwampLand = this.unlockSwampLand;
        data.unlockWinterLand = this.unlockWinterLand;

        // 3. 창고 / 인벤토리
        data.warehouseEntries = WarehouseManager.Instance.GetDataToSave();
        data.inventoryEntries = InventoryManager.Instance.GetDataToSave();

        return data;
    }

    public void ApplyAllSaveData(GameSaveData data)
    {
        PlayerStatsManager stats = GetPlayerStats();

        // 1. 플레이어 스탯
        if (stats != null)
        {
            stats.SetLevel(StatType.MaxHP, data.maxHPLevel);
            stats.SetLevel(StatType.MoveSpeed, data.moveSpeedLevel);
            stats.SetLevel(StatType.Attack, data.attackLevel);
            stats.SetLevel(StatType.FishingRod, data.fishingLevel);
            stats.SetLevel(StatType.BagWeight, data.weightLevel);
        }
        else
        {
            Debug.Log("PlayerStats 없음");
        }

        // 2. 게임 진행도
        this.currentWeek = data.currentWeek;
        this.totalGold = data.currentGold;
        this.unlockSwampLand = data.unlockSwampLand;
        this.unlockWinterLand = data.unlockWinterLand;

        // 3. 창고 / 인벤토리
        WarehouseManager.Instance.LoadData(data.warehouseEntries);
        InventoryManager.Instance.LoadData(data.inventoryEntries);

        OnGoldChanged?.Invoke();
    }

    public void StartNewGame()
    {
        Debug.Log("[GameManager] 모든 데이터를 초기화합니다. (새 게임)");

        // 1. 플레이어 스탯
        PlayerStatsManager stats = GetPlayerStats();
        if (stats != null)
        {
            stats.ResetLevels();
        }

        // 2. 게임 진행도
        this.currentWeek = 1;
        this.totalGold = 0;
        this.unlockSwampLand = false;
        this.unlockWinterLand = false;

        // 3. 창고/인벤 초기화 (null을 보내 초기화)
        WarehouseManager.Instance.LoadData(null);
        InventoryManager.Instance.LoadData(null);

        // 4. 새 게임 로드 완료 후, 이어하기 모드로
        SceneLoader.Instance.SetLoadType(SceneLoader.LoadType.Continue);
        OnGoldChanged?.Invoke();
    }
    #endregion
}
