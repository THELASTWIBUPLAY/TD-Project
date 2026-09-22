using System;
using UnityEngine;

public class PlayTimeRewardController : MonoBehaviour
{ 
    public enum RewardState
    {
        CountingDown,
        ReadyToClaim,
    }

    [Header("Reward Configurations")]
    [Tooltip("Reward duration in seconds. Default 30 Minutes = 1800 seconds.")]
    [SerializeField] private float _claimInterval = 1800f; // In seconds
    [SerializeField] private int _baseGoldReward = 100;

    // Events
    public event Action OnRewardReady;
    public event Action OnRewardClaimed;

    // State
    private float _targetTime;
    private RewardState _currentState;

    // UI Access
    public RewardState CurrentState => _currentState;
    public float RemainingSeconds => MathF.Max(0f, _targetTime - Time.time);
    public float ProgressNormalized => Mathf.Clamp01(1f - (RemainingSeconds / _claimInterval));

    public string SourceName => GetType().Name;

    private void Start()
    {
        StartNewCycle();
    }

    private void Update()
    {
        if (_currentState == RewardState.CountingDown)
        {
            if (Time.time >= _targetTime)
            {
                SetReadyToClaim();
            }
        }
    }

    private void StartNewCycle()
    {
        _targetTime = Time.time + _claimInterval;
        _currentState = RewardState.CountingDown;
    }

    private void SetReadyToClaim()
    {
        _currentState = RewardState.ReadyToClaim;
        OnRewardReady?.Invoke();
    }

    /// <summary>
    /// Called via OnClick()
    /// </summary>
    public void ClaimReward()
    {
        if (_currentState != RewardState.ReadyToClaim)
        {
            AlertManager.Instance.Show("Reward is not ready to claim!");
            return;
        }

        DistributeReward();
        OnRewardClaimed?.Invoke();
        StartNewCycle();
    }

    private void DistributeReward()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.ModifyGold(_baseGoldReward);
        }

        AlertManager.Instance.Show("Reward is claimed!");
    }
}