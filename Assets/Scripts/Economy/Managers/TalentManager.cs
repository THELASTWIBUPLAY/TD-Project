using System;
using System.Collections.Generic;
using UnityEngine;

public class TalentManager : MonoBehaviour, ISaveable
{
    public static TalentManager Instance { get; private set; }

    public event Action<TalentType, int, float, int> OnTalentUpdated;
    public event Action<TalentReward> OnTalentRewardUpdated;

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
        [HideInInspector] TalentRewardState state;
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
    [SerializeField] private int _rewardInterval = 15;
    [SerializeField] private List<TalentReward> _rewards = new List<TalentReward>();

    public int MaxTalentLevel => _maxTalentBuffLevel;

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private float GetInflationRate() => _priceInflationMultiplier / 100f;

    public int GetUpgradeCost(int currentLevel)
    {
        return (int)(_startingPrice * Math.Pow(1 + GetInflationRate(), currentLevel));
    }

    private void Reward()
    {
       // To Do
    }

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
        }

        Reward();
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
        }

        Reward();
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
        }

        Reward();
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

    // Debug
    [ContextMenu("Reset All Talent")]
    public void ResetAllTalent()
    {
        _currentTalentAttackLevel = 0;
        _currentTalentCoinLevel = 0;
        _currentTalentHpLevel = 0;

        OnTalentUpdated?.Invoke(TalentType.Attack, _currentTalentAttackLevel, CurrentTalentAttackBuff, GetUpgradeCost(_currentTalentAttackLevel));
        OnTalentUpdated?.Invoke(TalentType.Hp, _currentTalentHpLevel, CurrentTalentHpBuff, GetUpgradeCost(_currentTalentHpLevel));
        OnTalentUpdated?.Invoke(TalentType.Coin, _currentTalentCoinLevel, CurrentTalentCoinBuff, GetUpgradeCost(_currentTalentCoinLevel));
    }

    // =========================================================================
    // ISAVEABLE IMPLEMENTATION
    // =========================================================================

    public void PopulateSaveData(GameSaveData saveData)
    {
        saveData.talent.attackLevel = _currentTalentAttackLevel;
        saveData.talent.hpLevel = _currentTalentHpLevel;
        saveData.talent.coinLevel = _currentTalentCoinLevel;
    }

    public void LoadFromSaveData(GameSaveData saveData)
    {
        _currentTalentAttackLevel = saveData.talent.attackLevel;
        _currentTalentHpLevel = saveData.talent.hpLevel;
        _currentTalentCoinLevel = saveData.talent.coinLevel;

        OnTalentUpdated?.Invoke(TalentType.Attack, _currentTalentAttackLevel, CurrentTalentAttackBuff, GetUpgradeCost(_currentTalentAttackLevel));
        OnTalentUpdated?.Invoke(TalentType.Hp, _currentTalentHpLevel, CurrentTalentHpBuff, GetUpgradeCost(_currentTalentHpLevel));
        OnTalentUpdated?.Invoke(TalentType.Coin, _currentTalentCoinLevel, CurrentTalentCoinBuff, GetUpgradeCost(_currentTalentCoinLevel));
    }
}