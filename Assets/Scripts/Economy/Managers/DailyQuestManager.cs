using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyQuestManager : MonoBehaviour, ISaveable
{
    public static DailyQuestManager Instance { get; private set; }

    [SerializeField] private DailyQuestSO _questDatabase;
    public DailyQuestSO QuestDatabase => _questDatabase;

    [Header("Random Quest Settings")]
    [Tooltip("Quest total for each day")]
    [SerializeField] private int _questCountToPick = 3;

    private readonly List<DailyQuestData> _activeQuest = new List<DailyQuestData>();
    public IReadOnlyList<DailyQuestData> ActiveQuest => _activeQuest;

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
    private const string ActiveQuestsKey = "DailyQuest_ActiveQuestIDs";

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
        LoadActiveQuests();
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
        if (currentCycle > _lastQuestDate || _activeQuest.Count == 0)
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

        // Randomize quest from database
        GenerateRandomQuests();

        // save new cycle date to memory and playerprefs
        _lastQuestDate = newCycleDate;
        PlayerPrefs.SetString(LastQuestResetKey, _lastQuestDate.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();

        Debug.Log($"[DailyQuest] Successfully reset cycle date: {_lastQuestDate:yyyy-MM-dd} (04:00 UTC+7)\")");

        OnQuestUpdated?.Invoke();
        SaveManager.Instance?.SaveLocal();
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

    // Random Quest Selection
    private void GenerateRandomQuests()
    {
        _activeQuest.Clear();

        if (_questDatabase == null || _questDatabase.Quests.Count == 0)
        {
            Debug.LogWarning("[DailyQuestManager] DB quest empty");
            return;
        }

        // Copy all quest from database to temporary list for randomize
        List<DailyQuestData> pool = new List<DailyQuestData>(_questDatabase.Quests);

        // Algoritma Fisher-Yates Shuffle untuk menjamin keacakan yang merata
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int randIndex = UnityEngine.Random.Range(0, i + 1);
            var temp = pool[i];
            pool[i] = pool[randIndex];
            pool[randIndex] = temp;
        }

        int count = Mathf.Min(_questCountToPick, pool.Count);
        List<string> pickedIDs = new List<string>();

        for (int i = 0; i < count; i++)
        {
            _activeQuest.Add(pool[i]);
            pickedIDs.Add(pool[i].questID);
        }

        // Save selected ID
        PlayerPrefs.SetString(ActiveQuestsKey, string.Join(",", pickedIDs));
        PlayerPrefs.Save();
    }

    private void LoadActiveQuests()
    {
        _activeQuest.Clear();

        if (!PlayerPrefs.HasKey(ActiveQuestsKey) || _questDatabase == null) return;

        string savedIDs = PlayerPrefs.GetString(ActiveQuestsKey);
        if (string.IsNullOrEmpty(savedIDs)) return;

        string[] idArray = savedIDs.Split(',');
        HashSet<string> idSet = new HashSet<string>(idArray);

        // Cocokkan ID yang tersimpan dengan definisi di db
        foreach (var quest in _questDatabase.Quests)
        {
            if (idSet.Contains(quest.questID))
            {
                _activeQuest.Add(quest);
            }
        }
    }

    // Quest Logic
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

        // Only add on active quest
        foreach (var quest in _activeQuest)
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
            SaveManager.Instance?.SaveLocal();
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

        SaveManager.Instance?.SaveLocal();
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

        AlertManager.Instance?.Show($"Milestone chest {milestone.requiredPoints} Pts collected!");
        OnQuestUpdated?.Invoke();

        SaveManager.Instance?.SaveLocal();
    }

    // Debug
    [ContextMenu("Debug: Re-Roll Quests Now")]
    public void DebugRerollQuests()
    {
        _questProgress.Clear();
        _claimedQuest.Clear();
        GenerateRandomQuests();
        OnQuestUpdated?.Invoke();
        Debug.Log("[DailyQuest DEBUG] Re-roll quest...");
    }

    [ContextMenu("Debug: Complete All Active Quests")]
    public void DebugCompleteAllQuests()
    {
        if (_questDatabase == null) return;

        foreach (var quest in _activeQuest)
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

    [ContextMenu("Debug: Reset All")]
    public void DebugResetAll()
    {
        _questProgress.Clear();
        _claimedQuest.Clear();
        _claimedMilestones.Clear();
        _currentMilestonePoints = 0;
        PlayerPrefs.DeleteKey(LastQuestResetKey);
        PlayerPrefs.DeleteKey(ActiveQuestsKey);
        PlayerPrefs.Save();
        CheckDailyReset();
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

    public void PopulateSaveData(GameSaveData saveData)
    {
        var d = saveData.dailyQuest;
        d.cycleDateUtc = _lastQuestDate.ToString("yyyy-MM-dd");
        d.milestonePoints = _currentMilestonePoints;

        d.activeQuestIDs.Clear();
        foreach (var q in _activeQuest) d.activeQuestIDs.Add(q.questID);

        d.questProgress.Clear();
        foreach (var pair in _questProgress)
        {
            d.questProgress.Add(new QuestProgressEntry { questID = pair.Key, progress = pair.Value });
        }

        d.claimedQuests.Clear();
        d.claimedQuests.AddRange(_claimedQuest);

        d.claimedMilestones.Clear();
        d.claimedMilestones.AddRange(_claimedMilestones);
    }

    public void LoadFromSaveData(GameSaveData saveData)
    {
        var d = saveData.dailyQuest;

        if (DateTime.TryParse(d.cycleDateUtc, out DateTime cycle))
            _lastQuestDate = cycle.Date;

        _currentMilestonePoints = d.milestonePoints;

        _questProgress.Clear();
        foreach (var entry in d.questProgress) _questProgress[entry.questID] = entry.progress;

        _claimedQuest.Clear();
        foreach (var id in d.claimedQuests) _claimedQuest.Add(id);

        _claimedMilestones.Clear();
        foreach (var id in d.claimedMilestones) _claimedMilestones.Add(id);

        // Pulihkan quest aktif berdasarkan ID
        _activeQuest.Clear();
        if (_questDatabase != null)
        {
            var idSet = new HashSet<string>(d.activeQuestIDs);
            foreach (var q in _questDatabase.Quests)
            {
                if (idSet.Contains(q.questID)) _activeQuest.Add(q);
            }
        }

        OnQuestUpdated?.Invoke();
    }
}