using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TestButton : MonoBehaviour
{
    [Serializable]
    public struct Grant
    {
        public Item item;   // ScriptableObject Item (프로젝트의 SO 타입)
        public int count;   // 지급 수량
    }

    [Header("버튼 클릭 시 지급될 아이템들")]
    public List<Grant> grants = new List<Grant>();

    [Header("옵션")]
    public bool openInventoryAfter = true;  // 지급 후 인벤토리 팝업 열기
    public bool logToConsole = true;        // 콘솔 로그 남기기

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(Give);
    }

    private void Give()
    {
        // 인벤토리 초기화(안전)
        InventoryManager.Instance.Init();

        int success = 0, fail = 0;
        foreach (var g in grants)
        {
            if (g.item == null || g.count <= 0) continue;

            // InventoryManager가 ScriptableObject Item을 받도록 구현돼 있어야 합니다.
            bool ok = InventoryManager.Instance.TryAdd(g.item, g.count);
            if (ok) success++; else fail++;
        }

        if (logToConsole)
        {
            if (fail == 0) Debug.Log($"[DebugGrantButton] 지급 성공: {success} 항목");
            else Debug.LogWarning($"[DebugGrantButton] 일부 실패: 성공 {success}, 실패 {fail} (무게 초과 등)");
        }

        if (openInventoryAfter)
            UI_Inven.Show();
    }

}