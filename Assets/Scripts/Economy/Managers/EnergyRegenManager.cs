using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.CloudCode;
using System.Threading.Tasks;
using UnityEditor.Rendering;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using Unity.Services.CloudSave;

public class EnergyRegenManager : MonoBehaviour, ISaveable
{
    public static EnergyRegenManager Instance { get; private set; }

    [Serializable]
    public struct ServerTimeResponse
    {
        public string serverTimeUtc;
    }

    [Header("Settings (Interval in seconds)")]
    [SerializeField] private float _energyRegenInterval = 10f;
    [SerializeField] private int _regenAmount = 5;
    [SerializeField] private float _retryDelay = 5f;

    // Called when the countdown changes, passing the remaining time in seconds
    public event Action<int> OnCountdownChanged;

    private DateTime _serverTimeAtSync;
    private double _realtimeAtSync;

    private bool _isSynced;
    private bool _isSyncing;
    private double _nextRetryTime;

    private DateTime? _lastRegenUtc;

    private int _lastReportedSeconds = int.MinValue;

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
        // If savemanager done loading but this script not yet
        var sm = SaveManager.Instance;
        if (sm != null && sm.IsLoaded)
            LoadFromSaveData(sm.CurrentData);
    }

    private void Update()
    {
        if (!_isSynced)
        {
            TrySync();
            return;
        }

        if (SaveManager.Instance == null || !SaveManager.Instance.IsLoaded) return;
        if (EconomyManager.Instance == null) return;

        Tick();
    }

    // Server Time Sync
    private void TrySync()
    {
        if (_isSyncing || Time.unscaledTimeAsDouble < _nextRetryTime) return;
        _ = SyncAsync();
    }

    private async Task SyncAsync()
    {
        _isSyncing = true;

        try
        {
            // if not initialized then add current time with retry delay
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                _nextRetryTime = Time.unscaledTimeAsDouble + _retryDelay;
                return;
            }

            DateTime serverTime = await FetchServerTimeUtcAsync();
            _serverTimeAtSync = serverTime;
            _realtimeAtSync = Time.unscaledTimeAsDouble;
            _isSynced = true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[EnergyRegen] Failed to sync server time, trying in {_retryDelay}s: {ex.Message}");
            _nextRetryTime = Time.unscaledTimeAsDouble + _retryDelay;
        }
        finally
        {
            _isSyncing = false;
        }
    }

    public async Task<DateTime> FetchServerTimeUtcAsync()
    {
        var response = await CloudCodeService.Instance.CallEndpointAsync<ServerTimeResponse>(
            "GetServerTime",
            new Dictionary<string, object>()
            );

        return ParseUtc(response.serverTimeUtc);
    }

    private static DateTime ParseUtc(string s)
    {
        return DateTime.Parse(
            s,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }

    private DateTime GetCurrentServerTime()
    {
        if (AuthManager.Instance != null && AuthManager.Instance.IsServerTimeSynced)
        {
            return AuthManager.Instance.CurrentServerTimeUtc;
        }

        return DateTime.UtcNow;
    }

    // Regen Logic
    private void Tick()
    {
        var eco = EconomyManager.Instance;
        int current = eco.CurrentData.currentEnergy;
        int max = eco.CurrentData.maxEnergy;
        DateTime now = GetCurrentServerTime();

        // If energy already full timer wont update
        if (current >= max)
        {
            _lastRegenUtc = now;
            Report(-1);
            return;
        }

        if (_lastRegenUtc == null) _lastRegenUtc = now;

        double elapsed = (now - _lastRegenUtc.Value).TotalSeconds;
        if (elapsed < 0) // if data is weird (ex. save from future) then reset
        {
            _lastRegenUtc = now;
            elapsed = 0;
        }

        // Offline Regen
        int ticks = (int)(elapsed / _energyRegenInterval);
        if (ticks > 0)
        {
            int gain = Mathf.Min(ticks * _regenAmount, max - current);
            if (gain > 0) eco.ModifyEnergy(gain);

            if (current + gain >= max)
            {
                _lastRegenUtc = now; //if full, sisa waktu buang
            }
            else
            {
                _lastRegenUtc = _lastRegenUtc.Value.AddSeconds(ticks * (double)_energyRegenInterval);
            }

            elapsed = (now - _lastRegenUtc.Value).TotalSeconds;
            SaveManager.Instance?.SaveLocal();
        }

        double remaining = _energyRegenInterval - elapsed;
        Report(Mathf.Max(0, Mathf.CeilToInt((float)remaining)));
    }

    // Send event only when seconds changed
    private void Report(int seconds)
    {
        if (seconds == _lastReportedSeconds) return;
        _lastReportedSeconds = seconds;
        OnCountdownChanged?.Invoke(seconds);
    }

    // Sync again when player opens game again
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) _isSynced = false;
    }

    // ISaveable
    public void PopulateSaveData(GameSaveData saveData)
    {
        if (saveData == null) return;
        if (saveData.economy == null) saveData.economy = new EconomySaveData();

        saveData.economy.lastEnergyRegenUtc = _lastRegenUtc.HasValue
            ? _lastRegenUtc.Value.ToString("o", CultureInfo.InvariantCulture)
            : string.Empty;
    }

    public void LoadFromSaveData(GameSaveData saveData)
    {
        if (saveData == null || saveData.economy == null) return;

        string s = saveData.economy.lastEnergyRegenUtc;
        if (!string.IsNullOrEmpty(s) &&
            DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed))
        {
            _lastRegenUtc = parsed;
        }
        else
        {
            _lastRegenUtc = null;
        }
    }
}