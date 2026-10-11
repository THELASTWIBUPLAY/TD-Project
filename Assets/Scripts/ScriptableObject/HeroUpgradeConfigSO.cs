using System;
using UnityEngine;

[Serializable]
public struct AscendRequirement
{
    [Range(1, 3)] public int starLevel;
    [Tooltip("Level maksimal hero di tier bintang ini")]
    public int maxLevelCap;
    [Tooltip("Biaya Gem untuk naik KE tier bintang ini (★1 ke ★2, ★2 ke ★3)")]
    public int gemCost;
}

[CreateAssetMenu(fileName = "HeroUpgradeConfig", menuName = "Deck/Hero Upgrade Config")]
public class HeroUpgradeConfigSO : ScriptableObject
{
    [Header("Ascend Tiers Configuration")]
    [Tooltip("Daftar konfigurasi level cap dan biaya ascend per bintang")]
    [SerializeField]
    private AscendRequirement[] _ascendTiers = new AscendRequirement[]
    {
        new AscendRequirement { starLevel = 1, maxLevelCap = 30, gemCost = 0 },
        new AscendRequirement { starLevel = 2, maxLevelCap = 40, gemCost = 150 },
        new AscendRequirement { starLevel = 3, maxLevelCap = 50, gemCost = 300 }
    };

    [Header("Level Up Cost (Gold)")]
    [Tooltip("Biaya dasar Gold di Level 1")]
    [SerializeField] private int _baseGoldCost = 100;
    [Tooltip("Kenaikan biaya Gold per level (Contoh: Cost = Base + (Level * Multiplier))")]
    [SerializeField] private float _goldCostMultiplier = 50f;

    /// <summary>
    /// Mendapatkan batas level maksimal berdasarkan tingkat bintang saat ini.
    /// </summary>
    public int GetMaxLevelForStar(int star)
    {
        foreach (var tier in _ascendTiers)
        {
            if (tier.starLevel == star) return tier.maxLevelCap;
        }
        return 30; // Fallback default
    }

    /// <summary>
    /// Mendapatkan biaya Gem untuk ascend ke targetStar berikutnya.
    /// </summary>
    public int GetGemCostForAscend(int targetStar)
    {
        foreach (var tier in _ascendTiers)
        {
            if (tier.starLevel == targetStar) return tier.gemCost;
        }
        return 0;
    }

    /// <summary>
    /// Menghitung biaya Gold untuk naik dari currentLevel ke currentLevel + 1.
    /// </summary>
    public int GetGoldCostForLevelUp(int currentLevel)
    {
        return Mathf.RoundToInt(_baseGoldCost + (currentLevel * _goldCostMultiplier));
    }
}