using System;
using UnityEngine;

[Serializable]
public struct AscendRequirement
{
    [Range(1, 3)] public int starLevel;
    [Tooltip("Level maksimal hero di tier bintang ini")]
    public int maxLevelCap;

    [Header("Material Requirements")]
    public int gemCost;
    [Tooltip("Jumlah fragmen hero spesifik yang dibutuhkan untuk ascend")]
    public int requiredHeroFragments;
    [Tooltip("Jumlah batu evolusi yang dibutuhkan (disiapkan untuk fitur mendatang)")]
    public int requiredEvolutionStones;
}

[CreateAssetMenu(fileName = "HeroUpgradeConfig", menuName = "Deck/Hero Upgrade Config")]
public class HeroUpgradeConfigSO : ScriptableObject
{
    [Header("Ascend Tiers Configuration")]
    [SerializeField]
    private AscendRequirement[] _ascendTiers = new AscendRequirement[]
    {
        new AscendRequirement { starLevel = 1, maxLevelCap = 30, gemCost = 0,   requiredHeroFragments = 0,  requiredEvolutionStones = 0 },
        new AscendRequirement { starLevel = 2, maxLevelCap = 40, gemCost = 150, requiredHeroFragments = 20, requiredEvolutionStones = 1 },
        new AscendRequirement { starLevel = 3, maxLevelCap = 50, gemCost = 300, requiredHeroFragments = 50, requiredEvolutionStones = 3 }
    };

    [Header("Level Up Cost (Gold)")]
    [SerializeField] private int _baseGoldCost = 100;
    [SerializeField] private float _goldCostMultiplier = 50f;

    public AscendRequirement GetRequirementForStar(int star)
    {
        foreach (var tier in _ascendTiers)
        {
            if (tier.starLevel == star) return tier;
        }
        return default;
    }

    public int GetMaxLevelForStar(int star) => GetRequirementForStar(star).maxLevelCap;
    public int GetGoldCostForLevelUp(int currentLevel) => Mathf.RoundToInt(_baseGoldCost + (currentLevel * _goldCostMultiplier));
}