using System;
using System.Collections.Generic;

[Serializable]
public class ProfileSaveData
{
    public string playerName = "The Hero";
    public string playerBio = "I am ready!";
    public int accountLevel = 1;
    public int accountExp = 0;
}

[Serializable]
public class EconomySaveData
{
    public int gold;
    public int gem;
    public int currentEnergy;
    public string lastEnergyRegenUtc;
}

[Serializable]
public class DailyRewardSaveData
{
    public string lastClaimTimeUtc;
    public int currentStreak;
}

[Serializable]
public class QuestProgressEntry
{
    public string questID;
    public int progress;
}

[Serializable]
public class DailyQuestSaveData
{
    public string cycleDateUtc;
    public List<string> activeQuestIDs = new List<string>();
    public List<QuestProgressEntry> questProgress = new List<QuestProgressEntry>();
    public List<string> claimedQuests = new List<string>();
    public int milestonePoints;
    public List<int> claimedMilestones = new List<int>();
}

[Serializable]
public class AchievementProgressEntry
{
    public string achievementID;
    public int progress;
}

[Serializable]
public class AchievementSaveData
{
    public List<AchievementProgressEntry> progressList = new List<AchievementProgressEntry>();
    public List<string> claimedList = new List<string>();
}

[Serializable]
public class TalentSaveData
{
    public int attackLevel;
    public int hpLevel;
    public int coinLevel;
    public List<int> claimedRewardTiers = new List<int>();
}

[Serializable]
public class GameSaveData
{
    public int saveVersion = 1;
    public string lastSavedTimestampUtc;
    public ProfileSaveData profile = new ProfileSaveData();
    public EconomySaveData economy = new EconomySaveData();
    public DailyRewardSaveData dailyReward = new DailyRewardSaveData();
    public DailyQuestSaveData dailyQuest = new DailyQuestSaveData();
    public AchievementSaveData achievement = new AchievementSaveData();
    public TalentSaveData talent = new TalentSaveData();
}