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

[Serializable]
public struct DailyQuestData
{
    [Header("Quest Settings")]
    public string questID;
    public DailyQuestType questType;
    public string questName;
    [Min(1)] public int targetGoal;

    [Header("Rewards")]
    [Min(0)] public int gold;
    [Min(0)] public int gem;
}

[CreateAssetMenu(fileName = "DailyQuestDB", menuName = "DailyQuest/DailyQuestDB")]
public class DailyQuestSO : ScriptableObject
{
    [SerializeField] private List<DailyQuestData> _quests = new List<DailyQuestData>();
    public IReadOnlyList<DailyQuestData> Quests => _quests;
}