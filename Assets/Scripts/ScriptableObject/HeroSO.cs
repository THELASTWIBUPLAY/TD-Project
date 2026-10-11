using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct HeroAscendPassive
{
    [Range(1, 3)]
    [Tooltip("Terbuka pada tingkat bintang keberapa (1, 2, atau 3)")]
    public int unlockAtStar;

    public string passiveName;
    [TextArea(2, 3)] public string passiveDescription;

    [Header("1. Attack Modifiers")]
    [Tooltip("Tambahan ATK persentase (0.15 = +15%)")]
    public float bonusAttackPercent;
    [Tooltip("Tambahan ATK flat langsung")]
    public float flatAttackBonus;

    [Header("2. Cooldown Modifiers")]
    [Tooltip("Persentase pemotongan jeda serang (0.1 = 10% lebih cepat menyerang)")]
    [Range(0f, 0.7f)] public float cooldownReductionPercent;

    [Header("3. Range Modifiers")]
    [Tooltip("Tambahan jarak jangkauan serang (unit satuan Unity)")]
    public float flatAttackRangeBonus;

    [Header("4. Critical Modifiers")]
    [Tooltip("Tambahan Critical Rate (5 = +5%)")]
    public float bonusCritRate;
    [Tooltip("Tambahan pengali Critical Damage (0.25 = +25% ekstra damage crit)")]
    public float bonusCritDamage;
}

[CreateAssetMenu(fileName = "Hero_New", menuName = "Deck/Hero Data")]
public class HeroSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Format ID unik: type_name_id (contoh: meele_mika_1)")]
    public string heroId;
    public string heroName;
    [TextArea(2, 4)] public string heroDescription;
    public Sprite heroIcon;
    public HeroRarity rarity;

    [Header("Unlock Requirements")]
    [Tooltip("Jumlah fragmen dasar yang dibutuhkan untuk membuka hero")]
    [Min(1)] public int requiredFragments = 50;

    [Header("Base Combat Stats (Level 1)")]
    public float baseDamage = 20f;
    public float baseAttackCooldown = 0.8f;
    public float baseAttackRange = 5.5f;

    [Header("Base Critical Stats")]
    [Range(0f, 100f)] public float baseCritRate = 5f;
    public float baseCritDamage = 1.5f;

    [Header("Stat Growth Per Level")]
    [Tooltip("Penambahan base damage setiap hero naik 1 level")]
    public float damageGrowthPerLevel = 2.5f;

    [Header("Ascend Passives (★1 s/d ★3)")]
    [SerializeField] private List<HeroAscendPassive> _ascendPassives = new List<HeroAscendPassive>();
    public IReadOnlyList<HeroAscendPassive> AscendPassives => _ascendPassives;

    [Header("Prefab In-Game")]
    [Tooltip("Prefab unit hero yang akan di-spawn saat pertempuran dimulai")]
    public GameObject inGamePrefab;
}