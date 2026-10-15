using System;
using System.Collections.Generic;
using UnityEngine;

public enum DailyQuestState
{
    Progress,
    Clear,
    Claimed,
}

public enum DailyQuestType
{
    Gather,
    Kill,
    ClearDungeon,
    SpendGem,
    SpendGold,
    SpendEnergy,
}

public enum MilestoneState
{
    Locked,
    ReadyToClaim,
    Claimed,
}

[Serializable]
public struct DailyQuestData
{
    [Header("Quest Settings")]
    public string questID;
    public DailyQuestType questType;
    public string questName;
    [Min(1)] public int targetGoal;
    // Milestones
    [Min(1)] public int questPoints;

    [Header("Rewards")]
    [Min(0)] public int gold;
    [Min(0)] public int gem;
}

[Serializable]
public struct QuestMilestoneTier
{
    public int milestoneID;
    [Min(1)] public int requiredPoints;
    [Min(0)] public int gold;
    [Min(0)] public int gem;
    public Sprite icon;
}

[CreateAssetMenu(fileName = "DailyQuestDB", menuName = "DailyQuest/DailyQuestDB")]
public class DailyQuestSO : ScriptableObject
{
    [Header("Quest List")]
    [SerializeField] private List<DailyQuestData> _quests = new List<DailyQuestData>();
    public IReadOnlyList<DailyQuestData> Quests => _quests;

    [Header("Milestone Rewards")]
    [SerializeField] private List<QuestMilestoneTier> _milestones = new List<QuestMilestoneTier>();
    public IReadOnlyList<QuestMilestoneTier> Milestones => _milestones;

    // get max point for max slider value
    public int GetMaxMilestonePoints()
    {
        if (_milestones == null || _milestones.Count == 0) return 100;
        return _milestones[_milestones.Count - 1].requiredPoints;
    }
}