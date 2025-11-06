using System.Collections;
using UnityEngine;
using System;

public struct PlayerRuntimeSnapshot
{   
    public float MaxHP;
    public float CurHP;
    public float Attack;
    public float MoveSpeed;
    public float MaxBagWeight;
    public float CurBagWeight;
}

public class DayPhasePlayerManager : SingletonManager<DayPhasePlayerManager>
{
    public bool IsPlayerReady {  get; private set; }

    public float playerMaxHP { get; private set; }
    public float playerCurHP { get; private set; }
    public float playerAttack   { get; private set; }
    public float playerMoveSpeed { get; private set; }
    public float maxBagWeight => InventoryManager.Instance?.maxWeight ?? 0f;
    public float curBagWeight => InventoryManager.Instance?.CurrentWeight ?? 0f;

    public event Action OnPlayerDamaged;

    public GameObject dayPlayerPrefab;
    public GameObject dayPlayer;
    public IInteract currentInteract;

    public event Action<PlayerRuntimeSnapshot> OnSnapshotUpdated;
    public PlayerRuntimeSnapshot Snapshot { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        PlayerStats.OnStatChanged -= HandleStatChanged;
        PlayerStats.OnStatChanged += HandleStatChanged;

        if (InventoryManager.Instance != null) InventoryManager.Instance.OnInventoryChanged += PushSnapshot;
    }

    private void OnDisable()
    {
        PlayerStats.OnStatChanged -= HandleStatChanged;
        if (InventoryManager.Instance != null) InventoryManager.Instance.OnInventoryChanged -= PushSnapshot;
    }

    public void ResetForNewDayPhase()
    {
        dayPlayer = null;
        IsPlayerReady = false;
    }

    public IEnumerator SpawnPlayerRoutine()
    {
        IsPlayerReady = false;
        dayPlayer = null; // 이전 참조를 확실히 제거

        PlayerStats ps = null;
        while ((ps = FindFirstObjectByType<PlayerStats>()) == null || !ps.IsReady)
            yield return null;

        // 스탯 초기화
        playerMaxHP = ps.GetValue(StatType.MaxHP);
        playerCurHP = playerMaxHP;
        playerAttack = ps.GetValue(StatType.Attack);
        playerMoveSpeed = ps.GetValue(StatType.MoveSpeed);

        // 플레이어 스폰 함수 호출
        SpawnPlayer();

        if (dayPlayer == null)
        {
            Debug.LogError("플레이어 스폰에 실패했습니다!");
            yield break; // 스폰 실패 시 중단
        }

        IsPlayerReady = true;
        PushSnapshot();
    }

    private void SpawnPlayer()
    {
        if (dayPlayerPrefab == null)
        {
            Debug.LogError("DayPlayer Prefab이 할당되지 않았습니다!");
            return;
        }
        PlayerSpawner ps = FindFirstObjectByType<PlayerSpawner>();
        dayPlayer = Instantiate(dayPlayerPrefab, ps.transform.position, ps.transform.rotation);

        DayPlayer dp = dayPlayer.GetComponent<DayPlayer>();
        if (dp != null)
        {
            dp.Initialize(playerMaxHP, playerAttack, playerMoveSpeed);
        }
    }

    private void HandleStatChanged(StatType type, int oldLv, int newLv)
    {
        PlayerStats ps = FindFirstObjectByType<PlayerStats>();
        if (ps == null || !ps.IsReady) return;
        switch (type)
        {
            case StatType.MaxHP:
                playerMaxHP = ps.GetValue(StatType.MaxHP);
                playerCurHP = Mathf.Min(playerCurHP, playerMaxHP);
                break;
            case StatType.Attack:
                playerAttack = ps.GetValue(StatType.Attack);
                break;
            case StatType.MoveSpeed:
                playerMoveSpeed = ps.GetValue(StatType.MoveSpeed);
                break;
        }

        PushSnapshot();
    }

    private void PushSnapshot()
    {
        Snapshot = new PlayerRuntimeSnapshot
        {
            MaxHP = playerMaxHP,
            CurHP = playerCurHP,
            Attack = playerAttack,
            MoveSpeed = playerMoveSpeed,
            MaxBagWeight = maxBagWeight,
            CurBagWeight = curBagWeight
        };

        OnSnapshotUpdated?.Invoke(Snapshot);
    }

    public void ApplyDamage(float damage)
    {
        if (damage <= 0f) return;
        playerCurHP = Mathf.Max(0f, playerCurHP - damage);
        PushSnapshot();
        OnPlayerDamaged?.Invoke();
        if (playerCurHP <= 0f)
        {
            // 죽음 처리
            // TODO -> 아이템 한 가지만 선택해서 창고에 넣고 나머지는 잃음, 밤 페이즈로 넘어감
        }
    }

    public void Heal(float value)
    {
        if (value <= 0f) return;
        playerCurHP = Mathf.Min(playerMaxHP, playerCurHP + value);
        PushSnapshot();
    }

    public void SetHP(float newHP, bool clamp = true)
    {
        playerCurHP = clamp ? Mathf.Clamp(newHP, 0f, playerMaxHP) : newHP;
        PushSnapshot();
    }

}
