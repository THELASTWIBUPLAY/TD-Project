using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DailyRewardTier
{
    [Range(1, 7)]
    public int day;
    [Min(0)] public int gold;
    [Min(0)] public int gem;
    public Sprite rewardIcon;
}

[CreateAssetMenu(fileName = "DailyReward", menuName = "Economy/DailyReward")]
public class DailyRewardScheduleSO : ScriptableObject
{
    [Tooltip("Daily Reward from day 1 to 7")]
    [SerializeField] private List<DailyRewardTier> _rewards = new List<DailyRewardTier>();

    public IReadOnlyList<DailyRewardTier> Rewards => _rewards;

    public DailyRewardTier GetRewardForDay(int dayNumber)
    {
        int index = Mathf.Clamp(dayNumber - 1, 0, _rewards.Count - 1);
        return _rewards[index];
    }

    // Defensive if Game Designer incorrectly add data
    private void OnValidate()
    {
        if (_rewards == null) return;

        // Making sure slot = 7
        while (_rewards.Count < 7)
        {
            _rewards.Add(new DailyRewardTier { day = _rewards.Count + 1});
        }
        while (_rewards.Count > 7)
        {
            _rewards.RemoveAt(_rewards.Count - 1);
        }

        // Auto-assign day number
        for (int i = 0; i < _rewards.Count; i++)
        {
            var tier = _rewards[i];
            tier.day = i + 1;
            _rewards[i] = tier;
        }
    }
}
