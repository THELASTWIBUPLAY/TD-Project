using System;
using System.Collections.Generic;
using UnityEngine;

public class TalentManager : MonoBehaviour, ISaveable
{
    public static TalentManager Instance { get; private set; }

    public event Action<TalentType, int, float, int> OnTalentUpdated;
    public event Action<int, TalentReward, TalentRewardState> OnTalentRewardUpdated;

    public enum TalentType
    {
        Attack,
        Hp,
        Coin,
    }

    public enum TalentRewardState
    {
        Progress,
        ReadyToClaim,
        Claimed
    }

    [Serializable]
    public struct TalentReward
    {
        public int gold;
        public int gem;
    }
    
    [Header("Settings (Talent Buff Per Level in %)")]
    [SerializeField] private float _talentAttackBuff = 2.5f;
    [SerializeField] private float _talentHpBuff = 1.5f;
    [SerializeField] private float _talentCoinBuff = 5f;

    [Header("Price Settings")]
    [SerializeField] private int _startingPrice = 500;
    [SerializeField] private float _priceInflationMultiplier = 2.5f;

    [SerializeField] private int _maxTalentBuffLevel = 20;

    [Header("Reward Settings")]
    [Tooltip("The interval at which rewards are given based on talent levels.")]
    [SerializeField] private int _rewardInterval = 15;
    [SerializeField] private List<TalentReward> _rewards = new List<TalentReward>();

    public int MaxTalentLevel => _maxTalentBuffLevel;
    public int RewardInterval => _rewardInterval;
    public List<TalentReward> Rewards => _rewards;

    // Levels
    private int _currentTalentAttackLevel;
    private int _currentTalentHpLevel;
    private int _currentTalentCoinLevel;

    public int AttackLevel => _currentTalentAttackLevel;
    public int HpLevel => _currentTalentHpLevel;
    public int CoinLevel => _currentTalentCoinLevel;

    // Buff totals
    public float CurrentTalentAttackBuff => _currentTalentAttackLevel * _talentAttackBuff;
    public float CurrentTalentHpBuff => _currentTalentHpLevel * _talentHpBuff;
    public float CurrentTalentCoinBuff => _currentTalentCoinLevel * _talentCoinBuff;

    public int TotalTalentLevel => _currentTalentAttackLevel + _currentTalentHpLevel + _currentTalentCoinLevel;

    // Reward Tracking
    private readonly HashSet<int> _claimedRewardTiers = new HashSet<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        var sm = SaveManager.Instance;
        if (sm != null && sm.IsLoaded)
            LoadFromSaveData(sm.CurrentData);
    }

    private float GetInflationRate() => _priceInflationMultiplier / 100f;

    public int GetUpgradeCost(int currentLevel)
    {
        return (int)(_startingPrice * Math.Pow(1 + GetInflationRate(), currentLevel));
    }

    // Reward Logic
    public int GetRequiredLevelForTier(int tierIndex)
    {
        return (tierIndex + 1) * _rewardInterval;
    }

    public TalentRewardState GetRewardState(int tierIndex)
    {
        if (_claimedRewardTiers.Contains(tierIndex))
        {
            return TalentRewardState.Claimed;
        }

        int requiredLevel = GetRequiredLevelForTier(tierIndex);
        if ( TotalTalentLevel >= requiredLevel)
        {
            return TalentRewardState.ReadyToClaim;
        }

        return TalentRewardState.Progress;
    }

    // Check tier and notify if ready to claim using helper method above
    private void CheckAndNotifyRewards()
    {
        for (int i = 0; i < _rewards.Count; i++)
        {
            TalentRewardState state = GetRewardState(i);
            OnTalentRewardUpdated?.Invoke(i, _rewards[i], state);
        }
    }

    public void ClaimReward(int tierIndex)
    {
        if (tierIndex < 0 || tierIndex >= _rewards.Count) return;

        if (GetRewardState(tierIndex) != TalentRewardState.ReadyToClaim)
        {
            AlertManager.Instance?.Show("Reward not ready to claim!");
            return;
        }

        // Add to hashset
        _claimedRewardTiers.Add(tierIndex);

        // Distribute
        TalentReward reward = _rewards[tierIndex];
        if (EconomyManager.Instance != null)
        {
            if (reward.gold > 0) EconomyManager.Instance.ModifyGold(reward.gold);
            if (reward.gem > 0) EconomyManager.Instance.ModifyGem(reward.gem);
        }

        AlertManager.Instance?.Show($"Level {GetRequiredLevelForTier(tierIndex)} reward claimed!");

        OnTalentRewardUpdated?.Invoke(tierIndex, reward, TalentRewardState.Claimed);
        SaveManager.Instance?.SaveLocal();
    }

    // Talent Upgrade
    public void ModifyAttackBuff()
    {
        if (_currentTalentAttackLevel >= _maxTalentBuffLevel)
        {
            AlertManager.Instance?.Show("Max level reached!");
            return;
        }

        int cost = GetUpgradeCost(_currentTalentAttackLevel);

        if (CheckPlayerGold(cost))
        {
            _currentTalentAttackLevel++;
            EconomyManager.Instance.ModifyGold(-cost);

            int nextCost = GetUpgradeCost(_currentTalentAttackLevel);
            OnTalentUpdated?.Invoke(TalentType.Attack, _currentTalentAttackLevel, CurrentTalentAttackBuff, nextCost);
            SaveManager.Instance?.SaveLocal();
            CheckAndNotifyRewards();
        }
    }

    public void ModifyHpBuff()
    {
        if (_currentTalentHpLevel >= _maxTalentBuffLevel)
        {
            AlertManager.Instance?.Show("Max level reached!");
            return;
        }

        int cost = GetUpgradeCost(_currentTalentHpLevel);

        if (CheckPlayerGold(cost))
        {
            _currentTalentHpLevel++;
            EconomyManager.Instance.ModifyGold(-cost);

            int nextCost = GetUpgradeCost(_currentTalentHpLevel);
            OnTalentUpdated?.Invoke(TalentType.Hp, _currentTalentHpLevel, CurrentTalentHpBuff, nextCost);
            SaveManager.Instance?.SaveLocal();
            CheckAndNotifyRewards();
        }
    }

    public void ModifyCoinBuff()
    {
        if (_currentTalentCoinLevel >= _maxTalentBuffLevel)
        {
            AlertManager.Instance?.Show("Max level reached!");
            return;
        }

        int cost = GetUpgradeCost(_currentTalentCoinLevel);

        if (CheckPlayerGold(cost))
        {
            _currentTalentCoinLevel++;
            EconomyManager.Instance.ModifyGold(-cost);

            int nextCost = GetUpgradeCost(_currentTalentCoinLevel);
            OnTalentUpdated?.Invoke(TalentType.Coin, _currentTalentCoinLevel, CurrentTalentCoinBuff, nextCost);
            SaveManager.Instance?.SaveLocal();
            CheckAndNotifyRewards();
        }
    }

    private bool CheckPlayerGold(int cost)
    {
        if (EconomyManager.Instance == null) return false;

        int currentGold = EconomyManager.Instance.CurrentData.gold;
        if (currentGold < cost)
        {
            AlertManager.Instance?.Show("Not enough gold");
            return false;
        }
        return true;
    }

    // =========================================================================
    // ISAVEABLE IMPLEMENTATION
    // =========================================================================
    public void PopulateSaveData(GameSaveData saveData)
    {
        if (saveData == null) return;
        if (saveData.talent == null) saveData.talent = new TalentSaveData();

        saveData.talent.attackLevel = _currentTalentAttackLevel;
        saveData.talent.hpLevel = _currentTalentHpLevel;
        saveData.talent.coinLevel = _currentTalentCoinLevel;

        saveData.talent.claimedRewardTiers.Clear();
        saveData.talent.claimedRewardTiers.AddRange(_claimedRewardTiers);
    }

    public void LoadFromSaveData(GameSaveData saveData)
    {
        Debug.Log($"[Talent] Load: atk={saveData.talent.attackLevel} hp={saveData.talent.hpLevel} coin={saveData.talent.coinLevel}");

        if (saveData == null || saveData.talent == null) return;

        _currentTalentAttackLevel = saveData.talent.attackLevel;
        _currentTalentHpLevel = saveData.talent.hpLevel;
        _currentTalentCoinLevel = saveData.talent.coinLevel;

        _claimedRewardTiers.Clear();
        if (saveData.talent.claimedRewardTiers != null)
        {
            foreach (int tier in saveData.talent.claimedRewardTiers)
            {
                _claimedRewardTiers.Add(tier);
            }
        }

        OnTalentUpdated?.Invoke(TalentType.Attack, _currentTalentAttackLevel, CurrentTalentAttackBuff, GetUpgradeCost(_currentTalentAttackLevel));
        OnTalentUpdated?.Invoke(TalentType.Hp, _currentTalentHpLevel, CurrentTalentHpBuff, GetUpgradeCost(_currentTalentHpLevel));
        OnTalentUpdated?.Invoke(TalentType.Coin, _currentTalentCoinLevel, CurrentTalentCoinBuff, GetUpgradeCost(_currentTalentCoinLevel));

        CheckAndNotifyRewards();
    }

    // Debug
    [ContextMenu("Reset All Talent & Rewards")]
    public void ResetAllTalent()
    {
        _currentTalentAttackLevel = 0;
        _currentTalentCoinLevel = 0;
        _currentTalentHpLevel = 0;
        _claimedRewardTiers.Clear();

        OnTalentUpdated?.Invoke(TalentType.Attack, _currentTalentAttackLevel, CurrentTalentAttackBuff, GetUpgradeCost(_currentTalentAttackLevel));
        OnTalentUpdated?.Invoke(TalentType.Hp, _currentTalentHpLevel, CurrentTalentHpBuff, GetUpgradeCost(_currentTalentHpLevel));
        OnTalentUpdated?.Invoke(TalentType.Coin, _currentTalentCoinLevel, CurrentTalentCoinBuff, GetUpgradeCost(_currentTalentCoinLevel));

        CheckAndNotifyRewards();
        SaveManager.Instance?.SaveLocal();
    }
}