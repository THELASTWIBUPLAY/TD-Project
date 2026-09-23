using JetBrains.Annotations;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class DailyRewardViewModel : MonoBehaviour
{
    public enum SlotState
    {
        Claimed,
        ReadyToClaim,
        Locked
    }

    [Header("Configuration")]
    [SerializeField] private DailyRewardScheduleSO _rewardSchedule;

    [Header("Debug/Testing")]
    [Tooltip("Check for testing interval")]
    [SerializeField] private bool _enableTestMode = false;
    [Tooltip("Interval in seconds. Ex: 60 for 1 minute")]
    [SerializeField] private float _testIntervalSeconds = 60f;

    private DailyRewardModel _model;
    private DateTime _cachedServerTimeUtc;
    private float _timeWhenServerReceived;

    private int _targetClaimDay = 1;
    private bool _canClaimToday;

    // Events untuk View
    public event Action OnDataInitialized;
    public event Action<DailyRewardTier> OnRewardClaimedSuccess;
    public event Action<string> OnErrorMessage;
    public event Action<string> OnCountdownUpdated;

    public DailyRewardScheduleSO Schedule => _rewardSchedule;
    public int TargetClaimDay => _targetClaimDay;
    public bool CanClaimToday => _canClaimToday;

    private void Awake()
    {
        _model = new DailyRewardModel();
    }

    private void Start()
    {
        _ = InitializeAsync();
    }

    private void Update()
    {
        if (!_canClaimToday)
        {
            UpdateCountdown();
        }
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _model.EnsureAuthenticatedAsync();
            _cachedServerTimeUtc = await _model.FetchServerTimeUtcAsync();
            _timeWhenServerReceived = Time.unscaledTime;

            EvaluateStreakProgress();
            OnDataInitialized?.Invoke();
        }
        catch (Exception ex)
        {
            OnErrorMessage?.Invoke("Connection failed " + ex.Message);
        }
    }

    private DateTime CurrentEstimatedServerTimeUtc
    {
        get
        {
            float elapsed = Time.unscaledTime - _timeWhenServerReceived;
            return _cachedServerTimeUtc.AddSeconds(elapsed);
        }
    }

    private DateTime CurrentServerDateUtc
    {
        get
        {
            // Hitung berapa detik waktu lokal berjalan sejak kita menerima respon server
            float elapsedSeconds = Time.unscaledTime - _timeWhenServerReceived;

            // Tambahkan detik tersebut ke waktu server awal, lalu ambil .Date-nya saja (jam 00:00:00)
            return _cachedServerTimeUtc.AddSeconds(elapsedSeconds).Date;
        }
    }

    private void EvaluateStreakProgress()
    {
        DateTime? lastClaimTime = _model.GetLastClaimTimeUtc();
        int savedStreak = _model.GetCurrentStreak();

        if (lastClaimTime == null)
        {
            // Start streak from 1
            _targetClaimDay = 1;
            _canClaimToday = true;
            return;
        }

        if (_enableTestMode)
        {
            double elapsedSeconds = (CurrentEstimatedServerTimeUtc - lastClaimTime.Value).TotalSeconds;

            if (elapsedSeconds < _testIntervalSeconds)
            {
                _targetClaimDay = savedStreak;
                _canClaimToday = false;
            }
            else if (elapsedSeconds < _testIntervalSeconds * 2)
            {
                // Streak continued
                _targetClaimDay = (savedStreak >= 7) ? 1 : savedStreak + 1;
                _canClaimToday = true;
            }
            else
            {
                // Streak broke
                _targetClaimDay = 1;
                _canClaimToday = true;
            }
        }
        else
        {
            int daysDifference = (CurrentServerDateUtc - lastClaimTime.Value).Days;

            if (daysDifference == 0)
            {
                _targetClaimDay = savedStreak;
                _canClaimToday = false;
            }
            else if (daysDifference == 1)
            {
                // Countinue last streak, if on day 7 get back to day 1
                _targetClaimDay = (savedStreak >= 7) ? 1 : savedStreak + 1;
                _canClaimToday = true;
            }
            else
            {
                // Streak gone if more than a day didnt login
                _targetClaimDay = 1;
                _canClaimToday = true;
            }
        }
    }

    private DateTime GetNextClaimTimeUtc()
    {
        DateTime? lastClaimTime = _model.GetLastClaimTimeUtc();
        if (lastClaimTime == null) return CurrentEstimatedServerTimeUtc;

        if (_enableTestMode)
        {
            return lastClaimTime.Value.AddSeconds(_testIntervalSeconds);
        }

        return lastClaimTime.Value.Date.AddDays(1);
    }

    private void UpdateCountdown()
    {
        TimeSpan remaining = GetNextClaimTimeUtc() - CurrentEstimatedServerTimeUtc;

        if (remaining.TotalSeconds <= 0)
        {
            EvaluateStreakProgress();
            OnDataInitialized?.Invoke();
            return;
        }

        string formattedTime = string.Format(
            "{0:D2}:{1:D2}:{2:D2}",
            (int)remaining.TotalHours,
            remaining.Minutes,
            remaining.Seconds);

        OnCountdownUpdated?.Invoke(formattedTime);
    }

    // Visual rule
    public SlotState GetSlotState(int dayNumber)
    {
        if (!_canClaimToday)
        {
            // if todays claimed then day less than target is claimed
            return (dayNumber <= _targetClaimDay) ? SlotState.Claimed : SlotState.Locked;
        }
        // if today is not claimed
        if (dayNumber < _targetClaimDay) return SlotState.Claimed;
        if (dayNumber == _targetClaimDay) return SlotState.ReadyToClaim;
        return SlotState.Locked;
    }

    public void ClaimRewardCommand()
    {
        if (!_canClaimToday)
        {
            OnErrorMessage?.Invoke("You have claimed today's reward, come again tommorow!");
            return;
        }

        DailyRewardTier reward = _rewardSchedule.GetRewardForDay(_targetClaimDay);

        // Save progress
        _model.SaveClaimProgress(CurrentEstimatedServerTimeUtc, _targetClaimDay);

        // Economy mutation
        if (EconomyManager.Instance != null)
        {
            if (reward.gold > 0) EconomyManager.Instance.ModifyGold(reward.gold);
            if (reward.gem > 0) EconomyManager.Instance.ModifyGem(reward.gem);
        }

        // Update locl state
        _canClaimToday = false;

        OnRewardClaimedSuccess?.Invoke(reward);
        OnDataInitialized?.Invoke();
    }

    [ContextMenu("Debug: Reset Daily Progress")]
    public void ResetProgresDebug()
    {
        if (_model == null) _model = new DailyRewardModel();
        _model.ResetDailyProgressDebug();
        _ = InitializeAsync();
        Debug.Log("[DailyReward] Progress PlayerPrefs reset");
    }
}