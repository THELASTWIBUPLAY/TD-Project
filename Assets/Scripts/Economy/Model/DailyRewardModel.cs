using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Unity.Services.Authentication; // For server to idenitfy player ID
using Unity.Services.CloudCode; // To use server side script created
using Unity.Services.Core; // To use services
using UnityEngine;

public class DailyRewardModel
{
    [Serializable]
    public class ServerTimeResponse
    {
        public string serverTimeUtc;
    }

    private const string LastClaimDateKey = "DailyReward_LastDateUtc";
    private const string CurrentStreakKey = "DailyReward_CurrentStreak";

    private DateTime? _cachedLastClaimTimeUtc;
    private int _cachedStreak = 0;

    public async Task EnsureAuthenticatedAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async Task<DateTime> FetchServerTimeUtcAsync()
    {
        var response = await CloudCodeService.Instance.CallEndpointAsync<ServerTimeResponse>(
            "GetServerTime",
            new Dictionary<string, object>()
            );

        // Gunakan InvariantCulture dan RoundtripKind agar parsing ISO 8601 selalu presisi di semua region HP
        return DateTime.Parse(response.serverTimeUtc, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
    }

    public DateTime? GetLastClaimTimeUtc()
    {
        return _cachedLastClaimTimeUtc;
    }

    public int GetCurrentStreak()
    {
        return _cachedStreak;
    }

    public void SaveClaimProgress(DateTime claimTimeUtc, int newStreak)
    {
        _cachedLastClaimTimeUtc = claimTimeUtc;
        _cachedStreak = newStreak;

        // Panggil auto-save lokal setiap kali reward harian diklaim
        SaveManager.Instance?.SaveLocal();
    }

    public void PopulateSaveData(GameSaveData saveData)
    {
        saveData.dailyReward.lastClaimTimeUtc = _cachedLastClaimTimeUtc.HasValue
            ? _cachedLastClaimTimeUtc.Value.ToString("o")
            : string.Empty;

        saveData.dailyReward.currentStreak = _cachedStreak;
    }

    public void LoadFromSaveData(GameSaveData saveData)
    {
        if (!string.IsNullOrEmpty(saveData.dailyReward.lastClaimTimeUtc) &&
            DateTime.TryParse(saveData.dailyReward.lastClaimTimeUtc, out DateTime parsedTime))
        {
            _cachedLastClaimTimeUtc = parsedTime;
        }
        else
        {
            _cachedLastClaimTimeUtc = null;
        }

        _cachedStreak = saveData.dailyReward.currentStreak;
    }

    [ContextMenu("Debug: Reset Daily Progress")]
    public void ResetDailyProgressDebug()
    {
        PlayerPrefs.DeleteKey(LastClaimDateKey);
        PlayerPrefs.DeleteKey(CurrentStreakKey);
        PlayerPrefs.Save();
    }
}