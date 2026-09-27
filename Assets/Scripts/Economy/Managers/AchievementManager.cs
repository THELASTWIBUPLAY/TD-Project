using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Economy.Managers
{
    public class AchievementManager : MonoBehaviour, ISaveable
    {
        public static AchievementManager Instance { get; private set; }

        [SerializeField] private AchievementSO _database;
        public AchievementSO Database => _database;

        // Runtime state
        private readonly Dictionary<string, int> _progressMap = new Dictionary<string, int>();
        private readonly HashSet<string> _claimedSet = new HashSet<string>();

        public event Action OnAchievementUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public int GetProgress(string achievementID)
        {
            return _progressMap.TryGetValue(achievementID, out int progress) ? progress : 0;
        }

        public AchievementState GetAchievementState(AchievementData data)
        {
            if (_claimedSet.Contains(data.achievementID))
            {
                return AchievementState.Claimed;
            }

            int currentProgress = GetProgress(data.achievementID);
            if (currentProgress >= data.targetGoal)
            {
                return AchievementState.ReadyToClaim;
            }

            return AchievementState.InProgress;
        }

        // Cummulative
        public void AddProgress(AchievementType type, int amount = 1)
        {
            if (_database == null || amount <= 0) return;

            bool updated = false;

            foreach (var ach in _database.Achievements)
            {
                if (ach.type == type && !_claimedSet.Contains(ach.achievementID))
                {
                    int current = GetProgress(ach.achievementID);
                    if (current < ach.targetGoal)
                    {
                        _progressMap[ach.achievementID] = Mathf.Min(current + amount, ach.targetGoal);
                        updated = true;
                    }
                }
            }

            if (updated)
            {
                OnAchievementUpdated?.Invoke();
            }
        }

        // Record
        public void UpdateMaxProgress(AchievementType type, int peakValue)
        {
            if (_database == null) return;

            bool updated = false;

            foreach (var ach in _database.Achievements)
            {
                if (ach.type == type && !_claimedSet.Contains(ach.achievementID))
                {
                    int current = GetProgress(ach.achievementID);
                    if (peakValue > current)
                    {
                        _progressMap[ach.achievementID] = Mathf.Min(peakValue, ach.targetGoal);
                        updated = true;
                    }
                }
            }

            if (updated)
            {
                OnAchievementUpdated?.Invoke();
            }
        }

        public void ClaimAchievement(AchievementData data)
        {
            if (GetAchievementState(data) != AchievementState.ReadyToClaim)
            {
                AlertManager.Instance?.Show("Finish the achievement first!");
                return;
            }

            _claimedSet.Add(data.achievementID);

            if (EconomyManager.Instance != null)
            {
                if (data.rewardGold > 0) EconomyManager.Instance.ModifyGold(data.rewardGold);
                if (data.rewardGem > 0) EconomyManager.Instance.ModifyGem(data.rewardGem);
            }

            AlertManager.Instance?.Show($"Achievement '{data.title}' finished!");

            OnAchievementUpdated?.Invoke();

            SaveManager.Instance?.SaveLocal();
        }

        public void PopulateSaveData(GameSaveData saveData)
        {
            var a = saveData.achievement;

            // 1. Ekspor progres dictionary ke List
            a.progressList.Clear();
            foreach (var pair in _progressMap)
            {
                a.progressList.Add(new AchievementProgressEntry
                {
                    achievementID = pair.Key,
                    progress = pair.Value
                });
            }

            // 2. Ekspor daftar klaim permanen
            a.claimedList.Clear();
            a.claimedList.AddRange(_claimedSet);
        }

        public void LoadFromSaveData(GameSaveData saveData)
        {
            var a = saveData.achievement;

            _progressMap.Clear();
            foreach (var item in a.progressList)
            {
                _progressMap[item.achievementID] = item.progress;
            }

            _claimedSet.Clear();
            foreach (var id in a.claimedList)
            {
                _claimedSet.Add(id);
            }

            OnAchievementUpdated?.Invoke(); // Refresh UI Achievement
        }

        // DEBUG
        [ContextMenu("Debug: Complete All Achievements")]
        public void DebugCompleteAll()
        {
            if (_database == null) return;

            foreach (var ach in _database.Achievements)
            {
                if (!_claimedSet.Contains(ach.achievementID))
                {
                    _progressMap[ach.achievementID] = ach.targetGoal;
                }
            }
            OnAchievementUpdated?.Invoke();
            Debug.Log("[Achievement DEBUG] All achievement is claimable!");
        }

        [ContextMenu("Debug: Reset All Achievements")]
        public void DebugResetAll()
        {
            _progressMap.Clear();
            _claimedSet.Clear();
            OnAchievementUpdated?.Invoke();
            Debug.Log("[Achievement DEBUG] Reset achievement data!");
        }
    }
}