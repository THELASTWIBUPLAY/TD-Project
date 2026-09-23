using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Unity.Services.Authentication; // For server to idenitfy player ID
using Unity.Services.CloudCode; // To use server side script created
using Unity.Services.Core; // To use services
using UnityEngine;

public class DailyRewardModel : MonoBehaviour
{
    [Serializable]
    public class ServerTimeResponse
    {
        public string serverTimeUtc;
    }

    private const string LastClaimDateKey = "DailyReward_LastDateUtc";
    private const string CurrentStreakKey = "DailyReward_CurrentStreak";

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
        if (!PlayerPrefs.HasKey(LastClaimDateKey)) return null;

        string savedString = PlayerPrefs.GetString(LastClaimDateKey);
        if (DateTime.TryParse(savedString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime result))
        {
            return result;
        }
        return null;
    }

    public int GetCurrentStreak()
    {
        return PlayerPrefs.GetInt(CurrentStreakKey, 0);
    }

    public void SaveClaimProgress(DateTime claimTimeUtc, int newStreak)
    {
        // Format "o" menyimpan tahun, bulan, tanggal, jam, menit, hingga detik secara standar ISO 8601
        PlayerPrefs.SetString(LastClaimDateKey, claimTimeUtc.ToString("o", CultureInfo.InvariantCulture));
        PlayerPrefs.SetInt(CurrentStreakKey, newStreak);
        PlayerPrefs.Save();
    }

    // Helper method for testing
    public void ResetDailyProgressDebug()
    {
        PlayerPrefs.DeleteKey(LastClaimDateKey);
        PlayerPrefs.DeleteKey(CurrentStreakKey);
        PlayerPrefs.Save();
    }
}