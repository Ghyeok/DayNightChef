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
/// <summary>
/// 외부에서 DayPlayer가 생성/활성화
/// 이 매니저에 BindPlayer()로 자신을 등록
/// 스텟 관리는 매니저가 단일로 담당
/// </summary>
public class DayPhasePlayerManager : SingletonManager<DayPhasePlayerManager>
{
    public float playerMaxHP { get; private set; }
    public float playerCurHP { get; private set; }

    public float playerAttack   { get; private set; }
    public float playerMoveSpeed { get; private set; }

    public float maxBagWeight => InventoryManager.Instance?.maxWeight ?? 0f;
    public float curBagWeight => InventoryManager.Instance?.CurrentWeight ?? 0f;

    public Transform spawnPoint;
    public GameObject dayPlayerPrefab;
    public GameObject dayPlayer;
    public IInteract currentInteract;

    public event Action<PlayerRuntimeSnapshot> OnSnapshotUpdated;
    public PlayerRuntimeSnapshot Snapshot { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Awake()
    {
        base.Awake();
        StartCoroutine(InitRoutine());
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

    public void BindPlayer(DayPlayer dp)
    {
        if (dp == null) return;
        dayPlayer = dp.gameObject;
        dp.Initialize(playerMaxHP, playerAttack, playerMoveSpeed);
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

    private IEnumerator InitRoutine()
    {
        PlayerStats ps = null;
        while ((ps = FindFirstObjectByType<PlayerStats>()) == null || !ps.IsReady)
                yield return null;
        playerMaxHP = ps.GetValue(StatType.MaxHP);
        playerCurHP = playerMaxHP;
        playerAttack = ps.GetValue(StatType.Attack);
        playerMoveSpeed = ps.GetValue(StatType.MoveSpeed);
        if (dayPlayer != null)
        {
            var dp = dayPlayer.GetComponent<DayPlayer>();
            if (dp != null)
                dp.Initialize(playerMaxHP, playerAttack, playerMoveSpeed);
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
        if (playerCurHP <= 0f)
        {
            // 죽음 처리 (ex :  밤페이즈로 전환)
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
