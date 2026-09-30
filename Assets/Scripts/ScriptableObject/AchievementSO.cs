using System;
using System.Collections.Generic;
using UnityEngine;

public enum AchievementType
{
    TotalKills,         // Kumulatif: Total membunuh musuh
    ReachWave,          // Rekor: Mencapai wave tertentu
    MergeCharacters,    // Kumulatif: Total melakukan merge unit
    SpendGold,          // Kumulatif: Total gold yang dihabiskan
    SpendGem,           // Kumulatif: Total gem yang dihabiskan
    ClearEndlessStage   // Kumulatif/Trigger: Menyelesaikan stage tertentu
}

public enum AchievementState
{
    InProgress,     
    ReadyToClaim,   
    Claimed,
}

[Serializable]
public struct AchievementData
{
    [Header("Identity")]
    public string achievementID;
    public string title;
    [TextArea(2, 3)] public string description;
    public AchievementType type;
    public Sprite icon;

    [Header("Goal")]
    [Min(1)] public int targetGoal;

    [Header("One-Time Rewards")]
    [Min(0)] public int rewardGold;
    [Min(0)] public int rewardGem;
}

[CreateAssetMenu(fileName = "AchievementDB", menuName = "Achievement/Achievement Database")]
public class AchievementSO : ScriptableObject
{
    [SerializeField] private List<AchievementData> _achievements = new List<AchievementData>();
    public IReadOnlyList<AchievementData> Achievements => _achievements;
}