using UnityEngine;

public enum BuffType
{
    AddRandomCharacter,
    AddSpecificCharacter,
    BoostAttack,
    BoostAttackSpeed,
    SlowMob,
    ExpGain,
    Ricochet,
    EmergencyRepair,
    Overdrive
}

[System.Serializable]
public class UpgradeCard
{
    public string cardName;
    public BuffType buffType;
    public CharacterClassType targetClassType;
    public int currentLevel = 0;

    public float[] playerValues = new float[0];
    public float[] mobValues = new float[0];

    public bool IsMaxLevel
    {
        get
        {
            if (buffType == BuffType.AddRandomCharacter || buffType == BuffType.AddSpecificCharacter || buffType == BuffType.EmergencyRepair)
            {
                return false;
            }
            if (playerValues == null || playerValues.Length == 0) return false;
            return currentLevel >= playerValues.Length;
        }
    }

    public float GetNextPlayerValue()
    {
        if (playerValues == null || playerValues.Length == 0) return 0f;
        int idx = Mathf.Clamp(currentLevel, 0, playerValues.Length - 1);
        return playerValues[idx];
    }

    public float GetNextMobValue()
    {
        if (mobValues == null || mobValues.Length == 0) return 0f;
        int idx = Mathf.Clamp(currentLevel, 0, mobValues.Length - 1);
        return mobValues[idx];
    }
}