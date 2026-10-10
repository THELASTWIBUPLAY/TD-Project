using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [Serializable]
    public struct ServerTimeResponse
    {
        public string serverTimeUtc;
    }

    // Auth Props
    public bool IsAuthenticated => AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn;
    public string PlayerId => IsAuthenticated ? AuthenticationService.Instance.PlayerId : string.Empty;

    // Cache, server time
    private DateTime _serverTimeAtSync;
    private double _realtimeAtSync;
    public bool IsServerTimeSynced { get; private set; }

    /// <summary>
    /// Returning server utc time without network
    /// </summary>
    public DateTime CurrentServerTimeUtc
    {
        get
        {
            if (!IsServerTimeSynced) return DateTime.UtcNow;
            double elapsed = Time.unscaledTimeAsDouble - _realtimeAtSync;
            return _serverTimeAtSync.AddSeconds(elapsed);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Initiate unity services
    /// </summary>
    public async Task InitializeServiceAsync()
    {
        // If not init yet wait until initialized
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await GameServicesBootstrap.InitializeAsync();
        }
    }

    /// <summary>
    /// Guest Login Flow (Anonymous)
    /// </summary>
    public async Task<bool> SignInAsGuestAsync()
    {
        try
        {
            await InitializeServiceAsync();
            
            // Do login if not logged in yet
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await GameServicesBootstrap.SignInAnonymouslyAsync();
            }
            Debug.Log($"[AuthManager] Login Success! PlayerID: {AuthenticationService.Instance.PlayerId}");
            Debug.Log($"[AuthManager] PlayerName: {AuthenticationService.Instance.PlayerName}");

            await SyncServerTimeAsync();

            return true;
        }
        catch (AuthenticationException authEx)
        {
            Debug.LogError($"[AuthManager] Auth Error: {authEx.ErrorCode} - {authEx.Message}");
            return false;
        }
        catch (RequestFailedException reqEx)
        {
            Debug.LogError($"[AuthManager] Request Error: {reqEx.ErrorCode} - {reqEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AuthManager] Login Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Sync game time with UGS Cloud Code
    /// </summary>
    public async Task SyncServerTimeAsync()
    {
        try
        {
            var response = await CloudCodeService.Instance.CallEndpointAsync<ServerTimeResponse>(
                "GetServerTime",
                new Dictionary<string, object>()
                );

            _serverTimeAtSync = DateTime.Parse(
                response.serverTimeUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal
                );

            _realtimeAtSync = Time.unscaledTimeAsDouble; // Start Game time after fetching time from ugs
            IsServerTimeSynced = true;

            Debug.Log($"[AuthManager] Server Time Sychronized: {_serverTimeAtSync:u}");
        }
        catch (Exception ex)
        {
            // Fallback using devices time
            Debug.LogWarning($"[AuthManager] Failed to fetch time from server, fallback using devices time: {ex.Message}");

            _serverTimeAtSync = DateTime.UtcNow;
            _realtimeAtSync = Time.unscaledTimeAsDouble;
            IsServerTimeSynced = false;
        }
    }
}
