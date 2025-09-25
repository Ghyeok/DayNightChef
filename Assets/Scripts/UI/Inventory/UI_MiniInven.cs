using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
public class UI_MiniInven : UI_Base
{
    [SerializeField] private Transform slotRoot;
    [SerializeField] private MiniSlot slotPrefab;

    private readonly List<MiniSlot> _slots = new();
    private bool _isInit;
    private InventoryManager _inv;

    public override void Init()
    {
        if (_isInit) return;
        if (!slotRoot)
        {
            var t = transform.Find("MiniInvSlot");
            if (t) slotRoot = t;
            else
            {
                var gl = GetComponentInChildren<GridLayoutGroup>(true);
                if (gl) slotRoot = gl.transform;
            }
        }
        _isInit = true;
    }

    private void OnEnable()
    {
        Init();

        _inv = InventoryManager.Instance;
        if (_inv != null) _inv.OnInventoryChanged += Refresh;

        RebuildToEntryCount();
        Refresh();
    }

    private void OnDisable()
    {
        if (_inv != null) _inv.OnInventoryChanged -= Refresh;
        _inv = null;
    }

    private void RebuildToEntryCount()
    {
        if (_inv == null || _inv.Entries == null) return;

        int need = _inv.slotCount;

        for (int i = slotRoot.childCount - 1; i >= 0; i++)
        {
            var child = slotRoot.GetChild(i);
            if (child.GetComponent<MiniSlot>() != null)
                Destroy(child.gameObject);
        }
        _slots.Clear();

        for (int i = 0; i < need; i++)
        {
            var s = Instantiate(slotPrefab, slotRoot);
            s.Clear();
            _slots.Add(s);
        }
    }
    private void Refresh()
    {
        if (_inv == null || _inv.Entries == null) return;

        if (_slots.Count != _inv.slotCount)
            RebuildToEntryCount();

        int n = Mathf.Min(_slots.Count, _inv.Entries.Count);
        for (int i = 0; i < n; i++)
        {
            _slots[i].Set(_inv.Entries[i]);
        }

        for (int i = n; i < _slots.Count; i++)
        {
            _slots[i].Clear();
        }
}
}
