using System;
using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour, ISaveable
{
    public static RelicManager Instance { get; private set; }

    [SerializeField] private RelicDatabaseSO _database;
    public RelicDatabaseSO Database => _database;

    [Header("Deck Config")]
    [SerializeField] private int _maxEquippedSlots = 3;

    [Header("Debug Testing Tool")]
    [Tooltip("Isi dengan relicId yang ingin diuji coba, lalu klik kanan script ini di Inspector")]
    [SerializeField] private string _testRelicId = "relic_fire_01";

    private readonly Dictionary<string, RelicProgressEntry> _progressMap = new Dictionary<string, RelicProgressEntry>();

    [Header("Deck Config (3 Slots)")]
    private readonly string[] _equippedRelicIds = new string[3];
    public IReadOnlyList<string> EquippedRelicIds => _equippedRelicIds;
    public event Action OnRelicDataUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetFragments(string relicId)
    {
        return _progressMap.TryGetValue(relicId, out var entry) ? entry.currentFragments : 0;
    }

    public bool IsRelicUnlocked(string relicId)
    {
        return _progressMap.TryGetValue(relicId, out var entry) && entry.isUnlocked;
    }

    public void AddFragments(string relicId, int amount)
    {
        if (_database == null || amount <= 0) return;
        var relic = _database.GetRelicById(relicId);
        if (relic == null) return;

        var entry = GetOrCreateEntry(relicId);
        entry.currentFragments += amount;

        if (!entry.isUnlocked && entry.currentFragments >= relic.requiredFragments)
        {
            entry.isUnlocked = true;
            AlertManager.Instance?.Show($"Relic '{relic.relicName}' Terbuka!");
        }

        OnRelicDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
    }

    private RelicProgressEntry GetOrCreateEntry(string relicId)
    {
        if (!_progressMap.TryGetValue(relicId, out var entry))
        {
            entry = new RelicProgressEntry { relicId = relicId, currentFragments = 0, isUnlocked = false };
            _progressMap[relicId] = entry;
        }
        return entry;
    }

    public string GetEquippedRelicAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _equippedRelicIds.Length) return string.Empty;
        return _equippedRelicIds[slotIndex];
    }

    public void SetEquippedRelic(int slotIndex, string relicId)
    {
        if (slotIndex < 0 || slotIndex >= _equippedRelicIds.Length) return;

        // Cegah duplikasi jika relik yang sama sudah terpasang di slot lain
        for (int i = 0; i < _equippedRelicIds.Length; i++)
        {
            if (_equippedRelicIds[i] == relicId)
                _equippedRelicIds[i] = string.Empty;
        }

        _equippedRelicIds[slotIndex] = relicId;
        OnRelicDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
    }

    public void SwapEquippedRelics(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _equippedRelicIds.Length ||
            toIndex < 0 || toIndex >= _equippedRelicIds.Length) return;

        string temp = _equippedRelicIds[toIndex];
        _equippedRelicIds[toIndex] = _equippedRelicIds[fromIndex];
        _equippedRelicIds[fromIndex] = temp;

        OnRelicDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
    }

    public void UnequipRelicAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _equippedRelicIds.Length) return;
        _equippedRelicIds[slotIndex] = string.Empty;
        OnRelicDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
    }

    public void UnequipRelic(string relicId)
    {
        bool changed = false;
        for (int i = 0; i < _equippedRelicIds.Length; i++)
        {
            if (_equippedRelicIds[i] == relicId)
            {
                _equippedRelicIds[i] = string.Empty;
                changed = true;
            }
        }
        if (changed)
        {
            OnRelicDataUpdated?.Invoke();
            SaveManager.Instance?.SaveLocal();
        }
    }
    // =========================================================================
    // TESTING TOOLS (Eksekusi via Klik Kanan Inspector saat Play Mode)
    // =========================================================================

    [ContextMenu("Debug: Toggle Test Relic Lock Status")]
    public void DebugToggleRelicStatus()
    {
        if (string.IsNullOrEmpty(_testRelicId)) return;

        var entry = GetOrCreateEntry(_testRelicId);
        var relic = _database != null ? _database.GetRelicById(_testRelicId) : null;

        entry.isUnlocked = !entry.isUnlocked;

        if (entry.isUnlocked)
        {
            int target = relic != null ? relic.requiredFragments : 50;
            entry.currentFragments = Mathf.Max(entry.currentFragments, target);
            Debug.Log($"<color=green>[Relic DEBUG]</color> {_testRelicId} diubah menjadi UNLOCKED!");
        }
        else
        {
            entry.currentFragments = 0;
            // Jika relik sedang terpasang di deck, copot otomatis agar state tetap valid
            UnequipRelic(_testRelicId);
            Debug.Log($"<color=yellow>[Relic DEBUG]</color> {_testRelicId} diubah menjadi LOCKED!");
        }

        OnRelicDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
    }

    [ContextMenu("Debug: Add 10 Fragments to Test Relic")]
    public void DebugAddFragmentsToTest()
    {
        if (string.IsNullOrEmpty(_testRelicId)) return;
        AddFragments(_testRelicId, 10);
        Debug.Log($"<color=cyan>[Relic DEBUG]</color> +10 Fragmen ditambahkan ke {_testRelicId}. Total: {GetFragments(_testRelicId)}");
    }

    [ContextMenu("Debug: Unlock All Relics")]
    public void DebugUnlockAll()
    {
        if (_database == null) return;

        foreach (var relic in _database.AllRelics)
        {
            if (relic == null) continue;
            var entry = GetOrCreateEntry(relic.relicId);
            entry.isUnlocked = true;
            entry.currentFragments = relic.requiredFragments;
        }

        OnRelicDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
        Debug.Log("<color=green>[Relic DEBUG]</color> Seluruh relik berhasil di-UNLOCK!");
    }

    [ContextMenu("Debug: Lock All Relics")]
    public void DebugLockAll()
    {
        if (_database == null) return;

        //_equippedRelicIds.Clear(); // Kosongkan seluruh slot terpasang

        foreach (var relic in _database.AllRelics)
        {
            if (relic == null) continue;
            var entry = GetOrCreateEntry(relic.relicId);
            entry.isUnlocked = false;
            entry.currentFragments = 0;
        }

        OnRelicDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
        Debug.Log("<color=red>[Relic DEBUG]</color> Seluruh relik berhasil di-LOCK!");
    }

    // =========================================================================
    // ISAVEABLE IMPLEMENTATION
    // =========================================================================

    public void PopulateSaveData(GameSaveData saveData)
    {
        if (saveData?.relicData == null) return;

        saveData.relicData.equippedRelicIds = new List<string>(_equippedRelicIds);
        saveData.relicData.relicProgressList.Clear();
        foreach (var pair in _progressMap.Values)
        {
            saveData.relicData.relicProgressList.Add(pair);
        }
    }

    public void LoadFromSaveData(GameSaveData saveData)
    {
        if (saveData?.relicData == null) return;

        _progressMap.Clear();
        foreach (var entry in saveData.relicData.relicProgressList)
        {
            _progressMap[entry.relicId] = entry;
        }

        for (int i = 0; i < _equippedRelicIds.Length; i++)
        {
            _equippedRelicIds[i] = (saveData.relicData.equippedRelicIds != null && i < saveData.relicData.equippedRelicIds.Count)
                ? saveData.relicData.equippedRelicIds[i]
                : string.Empty;
        }

        OnRelicDataUpdated?.Invoke();
    }
}