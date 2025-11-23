using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    // 1. 게임 내 업그레이드 수치 저장
    public int maxHPLevel;
    public int moveSpeedLevel;
    public int attackLevel;
    public int fishingLevel;
    public int weightLevel;
    public int restaurantLevel;

    // 2. 게임 진행도 저장
    public int currentWeek;
    public int currentGold;
    public int reputation;
    public bool isFeePayed;
    public bool unlockSwampLand;
    public bool unlockWinterLand;

    // 3. 창고 데이터 저장
    public List<WarehouseEntry> warehouseEntries;

    // 4. 인벤토리 저장
    public List<InventoryManager.Entry> inventoryEntries;
}

public class SaveManager : SingletonManager<SaveManager>
{
    // 1. 경로 프로퍼티 2개로 분리
    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");
    private string TempSavePath => Path.Combine(Application.persistentDataPath, "savegame_temp.json");

    public override void Awake()
    {
        base.Awake();
    }

    public void SaveGame()
    {
        Debug.Log("게임 저장 중...");

        // 1. 게임 데이터 불러오기
        GameSaveData dataToSave = GameManager.Instance.GetAllDataToSave();

        if (!IsValidGameData(dataToSave))
        {
            Debug.LogWarning("[SaveManager] 아직 유효한 게임 진행 데이터가 아니어서 저장을 건너뜁니다.");
            return;
        }

        // 2. JSON으로 직렬화 하기
        string json = JsonUtility.ToJson(dataToSave, true);

        // 3. 파일 쓰기
        try
        {
            File.WriteAllText(TempSavePath, json); // 임시 경로에 먼저 씀

            if (File.ReadAllText(TempSavePath).Length == 0)
            {
                throw new System.Exception("임시 저장 파일이 비어있습니다.");
            }

            if (File.Exists(SavePath)) // 기존 원본 파일 삭제
            {
                File.Delete(SavePath);
            }

            File.Move(TempSavePath, SavePath); // 임시 파일의 이름을 원본 파일로 변경

            Debug.Log($"저장 완료: {SavePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"저장 실패: {e.Message}");

            // 실패 시 생성된 임시 파일이 있다면 삭제
            if (File.Exists(TempSavePath))
            {
                try { File.Delete(TempSavePath); }
                catch { }
            }
        }
    }

    private bool IsValidGameData(GameSaveData data)
    {
        if (data == null) return false;

        if(data.maxHPLevel < 0) return false;
        if(data.moveSpeedLevel < 0) return false;
        if (data.attackLevel < 0) return false;
        if (data.weightLevel < 0) return false;
        if(data.fishingLevel < 0) return false;
        if(data.restaurantLevel < 0) return false;
        if(data.maxHPLevel < 0) return false;

        if (data.currentWeek <= 0) return false;

        return true;
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
            string json = File.ReadAllText(SavePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("세이브 파일이 비어있습니다. 새 게임을 시작합니다.");
                StartNewGame();
                return;
            }

            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

            if (!IsValidGameData(data))
            {
                Debug.LogWarning("[SaveManager] 유효하지 않은 세이브 데이터(currentWeek <= 0). 새 게임 시작.");
                StartNewGame();
                return;
            }

            // 저장 데이터 덮어쓰기
            GameManager.Instance.ApplyAllSaveData(data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로드 실패 (세이브 파일 손상 추정): {e.Message}");
            StartNewGame();
        }
    }

    /// <summary>
    /// 저장된 세이브 파일을 디스크에서 삭제합니다.
    /// </summary>
    public void DeleteSaveData()
    {
        string path = SavePath; // 기존 SavePath 프로퍼티 활용
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
                Debug.Log($"세이브 파일 삭제 완료: {path}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"세이브 파일 삭제 실패: {e.Message}");
            }
        }
        else
        {
            Debug.Log("삭제할 세이브 파일이 없습니다.");
        }
    }

    public void StartNewGame()
    {
        DeleteSaveData();

        Debug.Log("새 게임 시작");
        GameManager.Instance.StartNewGame();
    }

    /// <summary>
    /// 게임 종료 시 자동 저장
    /// </summary>
    private void OnApplicationQuit()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentWeek > 0)
        {
            SaveGame();
        }
    }
}
