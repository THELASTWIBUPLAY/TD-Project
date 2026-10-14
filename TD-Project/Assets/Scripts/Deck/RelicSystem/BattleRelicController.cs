using UnityEngine;

public class BattleRelicController : MonoBehaviour
{
    private void Start()
    {
        ApplyEquippedRelicPassives();
    }

    private void ApplyEquippedRelicPassives()
    {
        if (RelicManager.Instance == null || RelicManager.Instance.Database == null) return;

        var db = RelicManager.Instance.Database;
        var equippedIds = RelicManager.Instance.EquippedRelicIds;

        BaseHealth baseHp = FindFirstObjectByType<BaseHealth>();

        foreach (var id in equippedIds)
        {
            var relic = db.GetRelicById(id);
            if (relic == null) continue;

            switch (relic.element)
            {
                case RelicElement.Fire:
                    Character.GlobalDamageBonusPercent += relic.fireAttackBonusPercent;
                    Debug.Log($"[Relic Active] Fire: Unit ATK +{relic.fireAttackBonusPercent}%");
                    break;

                case RelicElement.Earth:
                    if (baseHp != null)
                    {
                        baseHp.maxHealth += relic.earthBaseHpBonus;
                        baseHp.currentHealth += relic.earthBaseHpBonus;
                        Debug.Log($"[Relic Active] Earth: Base Max HP +{relic.earthBaseHpBonus}");
                    }
                    break;

                case RelicElement.Water:
                    Enemy.GlobalSpeedMultiplier = Mathf.Max(0.2f, Enemy.GlobalSpeedMultiplier - relic.waterSlowPercent);
                    Debug.Log($"[Relic Active] Water: Musuh diperlambat {relic.waterSlowPercent * 100}%");
                    break;

                case RelicElement.Wind:
                    Character.GlobalAtkSpeedMultiplier += (relic.windAtkSpeedBonusPercent / 100f);
                    Debug.Log($"[Relic Active] Wind: Unit ASPD +{relic.windAtkSpeedBonusPercent}%");
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        // PEMBERSIHAN MUTLAK:
        // Kembalikan stat global tempur ke default saat pemain keluar dari scene arena
        Character.GlobalDamageBonusPercent = 0f;
        Character.GlobalAtkSpeedMultiplier = 1f;
        Enemy.ResetGlobalStats();
    }
}