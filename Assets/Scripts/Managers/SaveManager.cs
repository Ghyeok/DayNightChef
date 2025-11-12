using System.Collections.Generic;
using System.IO;
using UnityEngine;

[SerializeField]
public class GameSaveData
{
    // 1. 게임 내 업그레이드 수치 저장
    public int maxHPLevel;
    public int moveSpeedLevel;
    public int attackLevel;
    public int fishingLevel;
    public int weightLevel;

    // 2. 게임 진행도 저장
    public int currentWeek;
    public int currentGold;
    public bool unlockSwampLand;
    public bool unlockWinterLand;

    // 3. 창고 데이터 저장
    public List<WarehouseEntry> warehouseEntries;

    // 4. 인벤토리 저장
    public List<InventoryManager.Entry> inventoryEntries;
}

public class SaveManager : SingletonManager<SaveManager>
{
    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    public override void Awake()
    {
        
    }

    public void SaveGame()
    {
        Debug.Log("게임 저장 중...");

        // 1. 게임 데이터 불러오기
        GameSaveData dataToSave = GameManager.Instance.GetAllDataToSave();

        // 2. JSON으로 직렬화 하기
        string json = JsonUtility.ToJson(dataToSave, true);

        // 3. 파일 쓰기
        try
        {
            File.WriteAllText(SavePath, json);
            Debug.Log($"저장 완료: {SavePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("저장 파일 없음. 새 게임 시작");
            StartNewGame();
            return;
        }

        Debug.Log("게임 로드 중....");
        try
        {
            // 1. 파일 읽기
            string json = File.ReadAllText(SavePath);

            // 2. Json에서 데이터 복원
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            GameManager.Instance.ApplyAllSaveData(data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로드 실패: {e.Message}");
        }
    }

    public void StartNewGame()
    {
        Debug.Log("새 게임 시작");
        GameManager.Instance.StartNewGame();
    }

    /// <summary>
    /// 게임 종료 시 자동 저장
    /// </summary>
    public void OnApplicationQuit()
    {
        SaveGame();
    }
}
