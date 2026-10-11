using System;
using System.Collections.Generic;
using UnityEngine;

public enum HeroUpgradeButtonState
{
    LevelUp,   // Level belum mentok cap bintang saat ini (Bayar Gold)
    Ascend,    // Level sudah mentok cap bintang, bintang < 3 (Bayar Gem & Fragmen)
    MaxedOut   // Bintang sudah 3 dan level sudah 50 (Tombol nonaktif)
}

public struct CalculatedHeroStats
{
    public float totalDamage;
    public float attackCooldown;
    public float attackRange;
    public float totalCritRate;
    public float totalCritDamage;
    public List<string> activePassiveNames;
}

public class HeroManager : MonoBehaviour, ISaveable
{
    public static HeroManager Instance { get; private set; }

    [SerializeField] private HeroDatabaseSO _database;
    [SerializeField] private HeroUpgradeConfigSO _upgradeConfig;
    public HeroDatabaseSO Database => _database;
    public HeroUpgradeConfigSO UpgradeConfig => _upgradeConfig;

    [Header("Debug Testing Tool")]
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

    public HeroProgressEntry GetOrCreateEntry(string heroId)
    {
        if (!_progressMap.TryGetValue(heroId, out var entry))
        {
            entry = new HeroProgressEntry
            {
                heroId = heroId,
                currentFragments = 0,
                isUnlocked = false,
                level = 1,
                star = 1
            };
            _progressMap[heroId] = entry;
        }
        return entry;
    }

    public int GetFragments(string heroId) => GetOrCreateEntry(heroId).currentFragments;
    public int GetHeroLevel(string heroId) => GetOrCreateEntry(heroId).level;
    public int GetHeroStar(string heroId) => GetOrCreateEntry(heroId).star;
    public bool IsHeroUnlocked(string heroId) => GetOrCreateEntry(heroId).isUnlocked;

    public void AddFragments(string heroId, int amount)
    {
        if (_database == null || amount <= 0) return;
        var hero = _database.GetHeroById(heroId);
        if (hero == null) return;

        var entry = GetOrCreateEntry(heroId);
        entry.currentFragments += amount;

        if (!entry.isUnlocked && entry.currentFragments >= hero.requiredFragments)
        {
            entry.isUnlocked = true;
            AlertManager.Instance?.Show($"Hero '{hero.heroName}' Terbuka!");
        }

        OnHeroDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
    }

    // =========================================================================
    // DYNAMIC BUTTON STATE
    // =========================================================================

    public HeroUpgradeButtonState GetUpgradeActionState(string heroId)
    {
        if (_upgradeConfig == null) return HeroUpgradeButtonState.MaxedOut;

        var entry = GetOrCreateEntry(heroId);
        int maxLevelForCurrentStar = _upgradeConfig.GetMaxLevelForStar(entry.star);

        if (entry.star >= 3 && entry.level >= maxLevelForCurrentStar)
        {
            return HeroUpgradeButtonState.MaxedOut;
        }

        if (entry.level >= maxLevelForCurrentStar)
        {
            return HeroUpgradeButtonState.Ascend;
        }

        return HeroUpgradeButtonState.LevelUp;
    }

    // =========================================================================
    // LEVEL UP SYSTEM (GOLD)
    // =========================================================================

    public bool CanLevelUp(string heroId, out string failureReason)
    {
        failureReason = string.Empty;
        var hero = _database?.GetHeroById(heroId);
        if (hero == null || !IsHeroUnlocked(heroId))
        {
            failureReason = "Hero belum dimiliki!";
            return false;
        }

        var entry = GetOrCreateEntry(heroId);
        int maxLevel = _upgradeConfig.GetMaxLevelForStar(entry.star);

        if (entry.level >= maxLevel)
        {
            failureReason = entry.star < 3 ? "Batas level tercapai! Lakukan Ascend." : "Hero sudah mencapai level maksimal!";
            return false;
        }

        int goldCost = _upgradeConfig.GetGoldCostForLevelUp(entry.level);
        if (EconomyManager.Instance != null && EconomyManager.Instance.CurrentData.gold < goldCost)
        {
            failureReason = "Gold tidak cukup!";
            return false;
        }

        return true;
    }

    public bool TryLevelUp(string heroId)
    {
        if (!CanLevelUp(heroId, out string reason))
        {
            AlertManager.Instance?.Show(reason);
            return false;
        }

        var entry = GetOrCreateEntry(heroId);
        int cost = _upgradeConfig.GetGoldCostForLevelUp(entry.level);

        EconomyManager.Instance?.ModifyGold(-cost);
        entry.level++;
        AlertManager.Instance?.Show($"Level Up! Sekarang Lv. {entry.level}");

        OnHeroDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
        return true;
    }

    // =========================================================================
    // ASCEND SYSTEM (GEM & FRAGMENTS)
    // =========================================================================

    public bool CanAscend(string heroId, out string failureReason)
    {
        failureReason = string.Empty;
        var entry = GetOrCreateEntry(heroId);

        if (entry.star >= 3)
        {
            failureReason = "Hero sudah mencapai bintang maksimal (★3)!";
            return false;
        }

        int nextStar = entry.star + 1;
        var req = _upgradeConfig.GetRequirementForStar(nextStar);

        int currentMaxLevel = _upgradeConfig.GetMaxLevelForStar(entry.star);
        if (entry.level < currentMaxLevel)
        {
            failureReason = $"Tingkatkan hero ke level {currentMaxLevel} untuk Ascend!";
            return false;
        }

        if (EconomyManager.Instance != null && EconomyManager.Instance.CurrentData.gem < req.gemCost)
        {
            failureReason = "Gem tidak cukup!";
            return false;
        }

        if (entry.currentFragments < req.requiredHeroFragments)
        {
            failureReason = $"Fragmen tidak cukup! Butuh {req.requiredHeroFragments} fragmen.";
            return false;
        }

        return true;
    }

    public bool TryAscend(string heroId)
    {
        if (!CanAscend(heroId, out string reason))
        {
            AlertManager.Instance?.Show(reason);
            return false;
        }

        var entry = GetOrCreateEntry(heroId);
        int nextStar = entry.star + 1;
        var req = _upgradeConfig.GetRequirementForStar(nextStar);

        if (req.gemCost > 0)
            EconomyManager.Instance?.ModifyGem(-req.gemCost);

        if (req.requiredHeroFragments > 0)
            entry.currentFragments -= req.requiredHeroFragments;

        entry.star = nextStar;
        AlertManager.Instance?.Show($"Ascend Berhasil! Hero mencapai Bintang ★{entry.star}!");

        OnHeroDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
        return true;
    }

    // =========================================================================
    // STAT CALCULATION ENGINE
    // =========================================================================

    public CalculatedHeroStats GetCalculatedStats(string heroId)
    {
        CalculatedHeroStats stats = new CalculatedHeroStats
        {
            activePassiveNames = new List<string>()
        };

        var hero = _database?.GetHeroById(heroId);
        if (hero == null) return stats;

        var entry = GetOrCreateEntry(heroId);

        float calculatedDamage = hero.baseDamage + ((entry.level - 1) * hero.damageGrowthPerLevel);
        float calculatedCooldown = hero.baseAttackCooldown;
        float calculatedRange = hero.baseAttackRange;
        float calculatedCritRate = hero.baseCritRate;
        float calculatedCritDmg = hero.baseCritDamage;

        float totalDmgPercentBonus = 0f;
        float totalCdReductionPercent = 0f;

        foreach (var passive in hero.AscendPassives)
        {
            if (passive.unlockAtStar <= entry.star)
            {
                stats.activePassiveNames.Add(passive.passiveName);
                totalDmgPercentBonus += passive.bonusAttackPercent;
                calculatedDamage += passive.flatAttackBonus;
                totalCdReductionPercent += passive.cooldownReductionPercent;
                calculatedRange += passive.flatAttackRangeBonus;
                calculatedCritRate += passive.bonusCritRate;
                calculatedCritDmg += passive.bonusCritDamage;
            }
        }

        stats.totalDamage = calculatedDamage * (1f + totalDmgPercentBonus);
        float cdMultiplier = Mathf.Clamp(1f - totalCdReductionPercent, 0.3f, 1f);
        stats.attackCooldown = Mathf.Max(0.15f, calculatedCooldown * cdMultiplier);
        stats.attackRange = Mathf.Max(1f, calculatedRange);
        stats.totalCritRate = Mathf.Clamp(calculatedCritRate, 0f, 100f);
        stats.totalCritDamage = calculatedCritDmg;

        return stats;
    }

    // =========================================================================
    // DEBUG TOOLS
    // =========================================================================

    [ContextMenu("Debug: Toggle Test Hero Lock Status")]
    public void DebugToggleHeroStatus()
    {
        if (string.IsNullOrEmpty(_testHeroId)) return;

        var entry = GetOrCreateEntry(_testHeroId);
        var hero = _database != null ? _database.GetHeroById(_testHeroId) : null;

        entry.isUnlocked = !entry.isUnlocked;

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

    /// <summary>
    /// Toggle status kepemilikan hero yang sedang dibuka di modal untuk keperluan testing runtime.
    /// </summary>
    public void DebugToggleHeroUnlock(string heroId)
    {
        if (string.IsNullOrEmpty(heroId)) return;

        var entry = GetOrCreateEntry(heroId);
        var hero = _database != null ? _database.GetHeroById(heroId) : null;

        entry.isUnlocked = !entry.isUnlocked;

        if (entry.isUnlocked)
        {
            int target = hero != null ? hero.requiredFragments : 50;
            entry.currentFragments = Mathf.Max(entry.currentFragments, target);
            Debug.Log($"<color=green>[Debug UI]</color> Hero {heroId} berhasil di-UNLOCK!");
        }
        else
        {
            entry.currentFragments = 0;
            entry.level = 1;
            entry.star = 1;
            Debug.Log($"<color=red>[Debug UI]</color> Hero {heroId} berhasil di-LOCK!");
        }

        OnHeroDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
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
            if (entry.level < 1) entry.level = 1;
            if (entry.star < 1) entry.star = 1;
            _progressMap[entry.heroId] = entry;
        }
        OnHeroDataUpdated?.Invoke();
    }
}