using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour, ISaveable
{
    public static HeroManager Instance { get; private set; }

    [SerializeField] private HeroDatabaseSO _database;
    public HeroDatabaseSO Database => _database;

    [Header("Debug Testing Tool")]
    [Tooltip("Ketik HeroID di sini lalu klik kanan script ini di Inspector untuk eksekusi")]
    [SerializeField] private string _testHeroId = "meele_mika_1";

    private readonly Dictionary<string, HeroProgressEntry> _progressMap = new Dictionary<string, HeroProgressEntry>();
    public event Action OnHeroDataUpdated;

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

    public int GetFragments(string heroId)
    {
        return _progressMap.TryGetValue(heroId, out var entry) ? entry.currentFragments : 0;
    }

    public bool IsHeroUnlocked(string heroId)
    {
        return _progressMap.TryGetValue(heroId, out var entry) && entry.isUnlocked;
    }

    public void AddFragments(string heroId, int amount)
    {
        if (_database == null || amount <= 0) return;
        var hero = _database.GetHeroById(heroId);
        if (hero == null) return;

        var entry = GetOrCreateEntry(heroId);
        entry.currentFragments += amount;

        // Buka otomatis jika fragmen menyentuh target
        if (!entry.isUnlocked && entry.currentFragments >= hero.requiredFragments)
        {
            entry.isUnlocked = true;
            AlertManager.Instance?.Show($"Hero '{hero.heroName}' Terbuka!");
        }

        OnHeroDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
    }

    private HeroProgressEntry GetOrCreateEntry(string heroId)
    {
        if (!_progressMap.TryGetValue(heroId, out var entry))
        {
            entry = new HeroProgressEntry { heroId = heroId, currentFragments = 0, isUnlocked = false };
            _progressMap[heroId] = entry;
        }
        return entry;
    }

    // =========================================================================
    // TESTING TOOLS (Bisa dijalankan lewat Klik Kanan di Inspector saat Play Mode)
    // =========================================================================

    [ContextMenu("Debug: Toggle Test Hero Lock Status")]
    public void DebugToggleHeroStatus()
    {
        if (string.IsNullOrEmpty(_testHeroId)) return;

        var entry = GetOrCreateEntry(_testHeroId);
        var hero = _database != null ? _database.GetHeroById(_testHeroId) : null;

        // Balikkan status unlock
        entry.isUnlocked = !entry.isUnlocked;

        // Sesuaikan fragmen visualnya agar realistis
        if (entry.isUnlocked)
        {
            int target = hero != null ? hero.requiredFragments : 50;
            entry.currentFragments = Mathf.Max(entry.currentFragments, target);
            Debug.Log($"<color=green>[Hero DEBUG]</color> {_testHeroId} diubah menjadi UNLOCKED!");
        }
        else
        {
            entry.currentFragments = 0;
            Debug.Log($"<color=yellow>[Hero DEBUG]</color> {_testHeroId} diubah menjadi LOCKED!");
        }

        OnHeroDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
    }

    [ContextMenu("Debug: Unlock All Heroes")]
    public void DebugUnlockAll()
    {
        if (_database == null) return;

        foreach (var hero in _database.AllHeroes)
        {
            if (hero == null) continue;
            var entry = GetOrCreateEntry(hero.heroId);
            entry.isUnlocked = true;
            entry.currentFragments = hero.requiredFragments;
        }

        OnHeroDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
        Debug.Log("<color=green>[Hero DEBUG]</color> Seluruh hero telah di-UNLOCK!");
    }

    [ContextMenu("Debug: Lock All Heroes")]
    public void DebugLockAll()
    {
        if (_database == null) return;

        foreach (var hero in _database.AllHeroes)
        {
            if (hero == null) continue;
            var entry = GetOrCreateEntry(hero.heroId);
            entry.isUnlocked = false;
            entry.currentFragments = 0;
        }

        OnHeroDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
        Debug.Log("<color=red>[Hero DEBUG]</color> Seluruh hero telah di-LOCK!");
    }

    // =========================================================================
    // ISAVEABLE IMPLEMENTATION
    // =========================================================================

    public void PopulateSaveData(GameSaveData saveData)
    {
        if (saveData == null || saveData.heroData == null) return;
        saveData.heroData.heroProgressList.Clear();
        foreach (var pair in _progressMap.Values)
        {
            saveData.heroData.heroProgressList.Add(pair);
        }
    }

    public void LoadFromSaveData(GameSaveData saveData)
    {
        if (saveData == null || saveData.heroData == null) return;
        _progressMap.Clear();
        foreach (var entry in saveData.heroData.heroProgressList)
        {
            _progressMap[entry.heroId] = entry;
        }
        OnHeroDataUpdated?.Invoke();
    }
}