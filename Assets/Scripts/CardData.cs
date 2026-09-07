using UnityEngine;

public enum BuffType
{
    AddCharacter,       // +1 Karakter
    BoostAttack,        // Attack % (15% -> 25% -> 40%)
    BoostAttackSpeed,   // ASPD % (15% -> 30% -> 50%)
    SlowMob,            // Slow Mob % (15% -> 25% -> 35%)
    ExpGain             // EXP Gain % (20% -> 40% -> 70%)
}

[System.Serializable]
public class UpgradeCard
{
    public string cardName;
    public BuffType buffType;
    public int currentLevel = 0; // 0 = belum punya, max = 3
    
    // Nilai stat pemain tiap level [Lv1, Lv2, Lv3]
    public float[] playerValues = new float[3];
    // Nilai buff musuh tiap level [Lv1, Lv2, Lv3]
    public float[] mobValues = new float[3];

    public bool IsMaxLevel => (buffType != BuffType.AddCharacter) && currentLevel >= 3;

    public float GetNextPlayerValue()
    {
        return currentLevel < 3 ? playerValues[currentLevel] : playerValues[2];
    }

    public float GetNextMobValue()
    {
        return currentLevel < 3 ? mobValues[currentLevel] : mobValues[2];
    }
}