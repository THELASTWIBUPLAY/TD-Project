using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance { get; private set; }

    [SerializeField] private DailyQuestSO _questDatabase;
    public DailyQuestSO QuestDatabase => _questDatabase;

    // Quest Runtime
    private readonly Dictionary<string, int> _questProgress = new Dictionary<string, int>();
    private readonly HashSet<string> _claimedQuest = new HashSet<string>();

    // Milestone Runtime
    private int _currentMilestonePoints = 0;
    public int CurrentMilestonePoints => _currentMilestonePoints;
    private readonly HashSet<int> _claimedMilestones = new HashSet<int>();

    // Reset tracking
    private DateTime _lastQuestDate;
    private const string LastQuestResetKey = "DailyQuest_LastResetDateUtc";

    public event Action OnQuestUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Load last reset date when game started
        LoadLastResetDate();
    }

    private void Start()
    {
        // Check if quest need reset when game first started
        CheckDailyReset();
    }

    // Count cycle date of today quest
    public DateTime GetCurrentQuestCycleDate()
    {
        // UTC + 7 jam (WIB) - 4 jam (Offset Reset) = UTC + 3 jam
        return DateTime.UtcNow.AddHours(3).Date;
    }

    public void CheckDailyReset()
    {
        DateTime currentCycle = GetCurrentQuestCycleDate();

        // if today cycle date is different with saved cycle date
        if (currentCycle > _lastQuestDate)
        {
            ExecuteDailyReset(currentCycle);
        }
    }

    private void ExecuteDailyReset(DateTime newCycleDate)
    {
        // Clear all quest progress
        _questProgress.Clear();
        _claimedQuest.Clear();

        //clear all milestone progress
        _currentMilestonePoints = 0;
        _claimedMilestones.Clear();

        // save new cycle date to memory and playerprefs
        _lastQuestDate = newCycleDate;
        PlayerPrefs.SetString(LastQuestResetKey, _lastQuestDate.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();

        Debug.Log($"[DailyQuest] Successfully reset cycle date: {{_lastQuestDate:yyyy-MM-dd}} (04:00 UTC+7)\")");
    
        OnQuestUpdated?.Invoke();
    }

    private void LoadLastResetDate()
    {
        if (PlayerPrefs.HasKey(LastQuestResetKey))
        {
            string savedDateStr = PlayerPrefs.GetString(LastQuestResetKey);
            if (DateTime.TryParse(savedDateStr, out DateTime savedDate))
            {
                _lastQuestDate = savedDate.Date;
                return;
            }
        }

        // If new player, set the last quest date to yesterday
        _lastQuestDate = DateTime.MinValue;
    }

    public int GetProgress(string questID)
    {
        return _questProgress.TryGetValue(questID, out int progress) ? progress : 0;
    }

    public DailyQuestState GetQuestState(DailyQuestData quest)
    {
        if (_claimedQuest.Contains(quest.questID))
        {
            return DailyQuestState.Claimed;
        }

        int currentProgress = GetProgress(quest.questID);

        if (currentProgress >= quest.targetGoal)
        {
            return DailyQuestState.Clear;
        }

        return DailyQuestState.Progress;
    }

    // Called by another system to update the daily quest progress
    public void AddProgress(DailyQuestType type, int amount = 1)
    {
        if (_questDatabase == null) return;

        bool isAnyUpdated = false;

        foreach (var quest in _questDatabase.Quests)
        {
            if (quest.questType == type && !_claimedQuest.Contains(quest.questID))
            {
                int current = GetProgress(quest.questID);
                if (current < quest.targetGoal)
                {
                    _questProgress[quest.questID] = Mathf.Min(current + amount, quest.targetGoal);
                    isAnyUpdated = true;
                }
            }
        }

        if (isAnyUpdated)
        {
            OnQuestUpdated?.Invoke();
        }
    }

    public void ClaimQuest(DailyQuestData quest)
    {
        if (GetQuestState(quest) != DailyQuestState.Clear)
        {
            AlertManager.Instance?.Show("Quest is not cleared or been claimed");
            return;
        }

        _claimedQuest.Add(quest.questID);

        if (EconomyManager.Instance != null)
        {
            if (quest.gold > 0) EconomyManager.Instance.ModifyGold(quest.gold);
            if (quest.gem > 0) EconomyManager.Instance.ModifyGem(quest.gem);
        }

        _currentMilestonePoints += quest.questPoints;

        AlertManager.Instance?.Show($"Hadiah {quest.questName} berhasil diambil!");
        OnQuestUpdated?.Invoke();
    }

    // Milestone point
    public MilestoneState GetMilestoneState(QuestMilestoneTier milestone)
    {
        if (_claimedMilestones.Contains(milestone.milestoneID))
        {
            return MilestoneState.Claimed;
        }

        if (_currentMilestonePoints >= milestone.requiredPoints)
            return MilestoneState.ReadyToClaim;

        return MilestoneState.Locked;
    }

    public void ClaimMilestone(QuestMilestoneTier milestone)
    {
        if (GetMilestoneState(milestone) != MilestoneState.ReadyToClaim)
        {
            AlertManager.Instance?.Show("Milestone is not reached or already been claimed");
            return;
        }

        _claimedMilestones.Add(milestone.milestoneID);

        if (EconomyManager.Instance != null)
        {
            if (milestone.gold > 0) EconomyManager.Instance.ModifyGold(milestone.gold);
            if (milestone.gold > 0) EconomyManager.Instance.ModifyGem(milestone.gem);
        }

        OnQuestUpdated?.Invoke();
    }

    // Debug
    [ContextMenu("Debug: Complete All Quests")]
    public void DebugCompleteAllQuests()
    {
        if (_questDatabase == null) return;

        foreach (var quest in _questDatabase.Quests)
        {
            if (!_claimedQuest.Contains(quest.questID))
            {
                _questProgress[quest.questID] = quest.targetGoal;
            }
        }

        OnQuestUpdated?.Invoke();
    }

    [ContextMenu("Debug: Add 20 Milestone Points")]
    public void DebugAddPoints()
    {
        _currentMilestonePoints += 20;
        OnQuestUpdated?.Invoke();
    }

    [ContextMenu("Debug: Reset All Quests & Milestones")]
    public void DebugResetAllQuests()
    {
        _questProgress.Clear();
        _claimedQuest.Clear();
        _claimedMilestones.Clear();
        _currentMilestonePoints = 0;

        OnQuestUpdated?.Invoke();
    }

    [ContextMenu("Debug: Simulate Pass 04:00AM (Trigger Reset)")]
    public void DebugSimulateNextDayReset()
    {
        // Mundurkan tanggal pencatatan ke 2 hari lalu seolah-olah sudah lewat hari
        _lastQuestDate = DateTime.UtcNow.AddDays(-2);
        PlayerPrefs.SetString(LastQuestResetKey, _lastQuestDate.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();

        CheckDailyReset();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            CheckDailyReset();
        }
    }
}