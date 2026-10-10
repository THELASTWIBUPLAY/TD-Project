using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

// Keep login and direct MainMenu testing on the same environment.
public static class GameServicesBootstrap
{
    static Task initialization;
    static Task signIn;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetTasks() { initialization = null; signIn = null; }

    public static Task InitializeAsync()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
            return Task.CompletedTask;
        if (initialization == null || initialization.IsFaulted || initialization.IsCanceled)
        {
            var options = new InitializationOptions();
            options.SetEnvironmentName("production");
            initialization = UnityServices.InitializeAsync(options);
        }
        return initialization;
    }

    public static async Task SignInAnonymouslyAsync()
    {
        await InitializeAsync();
        if (AuthenticationService.Instance.IsSignedIn) return;
        if (signIn == null || signIn.IsCompleted)
            signIn = AuthenticationService.Instance.SignInAnonymouslyAsync();
        await signIn;
    }
}
