using UnityEngine;

public enum RelicElement
{
    Fire,   // Ofensif: Unit ATK / Ekstra Damage
    Earth,  // Defensif: Daya tahan Base / Shield
    Water,  // Crowd Control: Slow / Freeze musuh
    Wind    // Utility: Attack Speed / Distraksi
}

public enum RelicActivationType
{
    PassiveBattle,  // Langsung aktif sejak Wave 1 dimulai sampai game selesai
    ActiveSkill     // Memiliki cooldown dan tombol klik aktif di UI Gameplay
}

[CreateAssetMenu(fileName = "Relic_New", menuName = "Deck/Relic Data")]
public class RelicSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Format ID: relic_element_name (contoh: relic_fire_phoenix_feather)")]
    public string relicId;
    public string relicName;
    [TextArea(2, 3)] public string relicDescription;
    public Sprite relicIcon;
    public RelicElement element;
    public RelicActivationType activationType = RelicActivationType.PassiveBattle;

    [Header("Unlock Requirements")]
    [Tooltip("Jumlah fragmen yang dibutuhkan untuk membuka relik (default: 50)")]
    [Min(1)] public int requiredFragments = 50;

    [Header("Effect Balancing (Diisi sesuai elemen)")]
    [Tooltip("Fire: Tambahan % ATK untuk semua unit di arena")]
    public float fireAttackBonusPercent;

    [Tooltip("Earth: Tambahan Flat Max HP ke Base Health saat match dimulai")]
    public float earthBaseHpBonus;

    [Tooltip("Water: Persentase slow pergerakan musuh (contoh: 0.2 untuk 20% slow)")]
    [Range(0f, 0.8f)] public float waterSlowPercent;

    [Tooltip("Wind: Tambahan % ASPD (Attack Speed) untuk semua unit")]
    public float windAtkSpeedBonusPercent;

    [Header("Active Skill Parameters (Hanya jika tipe = ActiveSkill)")]
    public float skillCooldown = 30f;
    public float skillDuration = 8f;
}