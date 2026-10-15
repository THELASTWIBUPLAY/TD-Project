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

    // =========================================================================
    // EQUIPPED DECK SYSTEM (5 SLOTS)
    // =========================================================================
    [Header("Equipped Deck Slots")]
    private readonly string[] _equippedHeroIds = new string[5];
    public IReadOnlyList<string> EquippedHeroIds => _equippedHeroIds;

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


    public string GetEquippedHeroAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _equippedHeroIds.Length) return string.Empty;
        return _equippedHeroIds[slotIndex];
    }

    public void SetEquippedHero(int slotIndex, string heroId)
    {
        if (slotIndex < 0 || slotIndex >= _equippedHeroIds.Length) return;
        _equippedHeroIds[slotIndex] = heroId;
        OnHeroDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
    }

    public void SwapEquippedHeroes(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _equippedHeroIds.Length ||
            toIndex < 0 || toIndex >= _equippedHeroIds.Length) return;

        string temp = _equippedHeroIds[toIndex];
        _equippedHeroIds[toIndex] = _equippedHeroIds[fromIndex];
        _equippedHeroIds[fromIndex] = temp;

        OnHeroDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
    }

    public void UnequipHero(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _equippedHeroIds.Length) return;
        _equippedHeroIds[slotIndex] = string.Empty;
        OnHeroDataUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
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

        float talentAtkPercent = 0f;
        if (TalentManager.Instance != null)
        {
            // Misal Talent Attack Lv 2 (2.5% x 2 = 5%) -> diubah ke desimal (5 / 100 = 0.05)
            talentAtkPercent = TalentManager.Instance.CurrentTalentAttackBuff / 100f;
        }

        stats.totalDamage = calculatedDamage * (1f + totalDmgPercentBonus + talentAtkPercent);
        float cdMultiplier = Mathf.Clamp(1f - totalCdReductionPercent, 0.3f, 1f);
        stats.attackCooldown = Mathf.Max(0.15f, calculatedCooldown * cdMultiplier);
        stats.attackRange = Mathf.Max(1f, calculatedRange);
        stats.totalCritRate = Mathf.Clamp(calculatedCritRate, 0f, 100f);
        stats.totalCritDamage = calculatedCritDmg;

        return stats;
    }

    /// <summary>
    /// Mengambil daftar seluruh HeroSO yang sedang terpasang di 5 slot Deck.
    /// </summary>
    public List<HeroSO> GetEquippedHeroSOList()
    {
        List<HeroSO> list = new List<HeroSO>();
        if (_database == null) return list;

        for (int i = 0; i < _equippedHeroIds.Length; i++)
        {
            string id = _equippedHeroIds[i];
            if (!string.IsNullOrEmpty(id))
            {
                var hero = _database.GetHeroById(id);
                if (hero != null) list.Add(hero);
            }
        }
        return list;
    }

    /// <summary>
    /// Mencari HeroSO yang sedang dipasang di Deck berdasarkan kelasnya (Fighter, Ranged, dll).
    /// </summary>
    public HeroSO GetEquippedHeroByClass(CharacterClassType targetClass)
    {
        if (_database == null) return null;

        // 1. Prioritaskan mencari dari 5 slot Deck yang sedang dipakai
        for (int i = 0; i < _equippedHeroIds.Length; i++)
        {
            string id = _equippedHeroIds[i];
            if (!string.IsNullOrEmpty(id))
            {
                var hero = _database.GetHeroById(id);
                if (hero != null && hero.classType == targetClass)
                    return hero;
            }
        }

        // 2. Fallback: Jika tidak ada di deck, cari dari database hero yang memiliki class tersebut
        foreach (var hero in _database.AllHeroes)
        {
            if (hero != null && hero.classType == targetClass)
                return hero;
        }

        return null;
    }

    /// <summary>
    /// Menentukan HeroSO untuk unit di arena. 
    /// Jika unit belum punya heroId, pilih salah satu dari 5 slot Deck yang cocok dengan classType-nya lalu kunci ID-nya.
    /// Jika unit sudah punya heroId (misal saat naik bintang/merge), pertahankan hero yang sama.
    /// </summary>
    public HeroSO ResolveHeroForCharacter(CharacterClassType targetClass, ref string assignedHeroId)
    {
        if (_database == null) return null;

        // CEK APAKAH KARAKTER SUDAH PUNYA IDENTITAS HERO YANG VALID
        // (Berguna saat unit melakukan Merge dari Bintang 1 ke Bintang 2 agar karakternya tidak berubah)
        if (!string.IsNullOrEmpty(assignedHeroId))
        {
            HeroSO existingHero = _database.GetHeroById(assignedHeroId);
            if (existingHero != null && existingHero.classType == targetClass)
            {
                return existingHero;
            }
        }

        // JIKA BELUM PUNYA ID (Baru di-spawn), CARI SEMUA HERO DI DECK YANG KELASNYA COCOK
        List<HeroSO> matchingDeckHeroes = new List<HeroSO>();

        for (int i = 0; i < _equippedHeroIds.Length; i++)
        {
            string id = _equippedHeroIds[i];
            if (!string.IsNullOrEmpty(id))
            {
                HeroSO hero = _database.GetHeroById(id);
                if (hero != null && hero.classType == targetClass)
                {
                    matchingDeckHeroes.Add(hero);
                }
            }
        }

        // Jika pemain memasang 1 s/d 5 Hero dengan class yang sama di Deck,
        // pilih salah satu secara acak dan kunci ID-nya ke unit tersebut!
        if (matchingDeckHeroes.Count > 0)
        {
            HeroSO chosenHero = matchingDeckHeroes[UnityEngine.Random.Range(0, matchingDeckHeroes.Count)];
            assignedHeroId = chosenHero.heroId; // Mengisi otomatis variabel heroId di Character.cs!
            return chosenHero;
        }

        // FALLBACK Jika di Deck tidak ada class tersebut, ambil dari database yang class-nya cocok
        foreach (var hero in _database.AllHeroes)
        {
            if (hero != null && hero.classType == targetClass)
            {
                assignedHeroId = hero.heroId;
                return hero;
            }
        }

        return null;
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
        if (saveData?.heroData == null) return;

        // 1. Simpan daftar 5 hero yang dipasang di deck
        saveData.heroData.equippedHeroIds.Clear();
        saveData.heroData.equippedHeroIds.AddRange(_equippedHeroIds);

        // 2. Simpan progres level, fragmen, dan bintang hero
        saveData.heroData.heroProgressList.Clear();
        foreach (var pair in _progressMap.Values)
        {
            saveData.heroData.heroProgressList.Add(pair);
        }
    }

    public void LoadFromSaveData(GameSaveData saveData)
    {
        if (saveData?.heroData == null) return;

        // 1. Muat kembali slot deck yang tersimpan
        for (int i = 0; i < _equippedHeroIds.Length; i++)
        {
            _equippedHeroIds[i] = (saveData.heroData.equippedHeroIds != null && i < saveData.heroData.equippedHeroIds.Count)
                ? saveData.heroData.equippedHeroIds[i]
                : string.Empty;
        }

        // 2. Muat progres status hero
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