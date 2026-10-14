using UnityEngine;

// Compatibility with already-published pools and existing Cloud Save collections.
public static class RecruitmentHeroes
{
    public static string CanonicalId(string id) => id switch
    {
        "hero_fighter_001" => "01",
        "hero_ranger_001" => "Ranger_Alex_01",
        "hero_mage_001" => "Ranger_Lisa_01",
        "hero_support_001" => "Support_Ely_01",
        "hero_tank_001" => "Tank_Bubu_01",
        _ => id
    };

    public static HeroSO Find(string id) => HeroManager.Instance?.Database?.GetHeroById(CanonicalId(id));
}
