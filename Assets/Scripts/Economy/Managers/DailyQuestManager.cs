using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance { get; private set; }

    [SerializeField] private DailyQuestSO _questDatabase;
    public DailyQuestSO QuestDatabase => _questDatabase;

    private readonly Dictionary<string, int> _questProgress = new Dictionary<string, int>();
    private readonly HashSet<string> _claimedQuest = new HashSet<string>();

    public event Action OnQuestUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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

        AlertManager.Instance?.Show($"Hadiah {quest.questName} berhasil diambil!");
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

    [ContextMenu("Debug: Reset All Quests")]
    public void DebugResetAllQuests()
    {
        _questProgress.Clear();
        _claimedQuest.Clear();

        OnQuestUpdated?.Invoke();
    }
}