using UnityEngine;

[CreateAssetMenu(fileName = "Hero_New", menuName = "Deck/Hero Data")]
public class HeroSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Format ID: type_name_id (contoh: fighter_mika_01)")]
    public string heroId;
    public string heroName;
    [TextArea(2, 3)] public string heroDescription;
    public Sprite heroIcon;
    public CharacterClassType classType;
    public HeroRarity rarity;

    [Header("Base Combat Stats")]
    public float baseDamage = 15f;
    public float baseAttackCooldown = 0.8f;
    public float baseAttackRange = 5.5f;

    [Header("Base Critical Stats")]
    [Range(0f, 100f)] public float baseCritRate = 5f;
    public float baseCritDamage = 1.5f;

    [Header("Unlock Requirements")]
    [Tooltip("Jumlah fragmen untuk membuka hero ini")]
    [Min(1)] public int requiredFragments = 50;

    [Header("Prefab In-Game")]
    public GameObject inGamePrefab;
}