using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;

public class MailConnectionTest : MonoBehaviour
{
    [SerializeField] private MailInboxUI mailInbox;
    private struct UserAttributes { }
    private struct AppAttributes { }
    

    private async void Start()
    {
        try
        {
            // This test should start directly from the empty MailTest scene.
            if (UnityServices.State != ServicesInitializationState.Uninitialized)
            {
                Debug.LogWarning(
                    "[MailTest] Services already initialized. " +
                    "Stop Play Mode and run directly from MailTest."
                );
                return;
            }

            // Use the same production environment as the main menu.
            var options = new InitializationOptions();
            options.SetEnvironmentName("production");

            await UnityServices.InitializeAsync(options);

            // Give this test player an identity.
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            // Download the published configuration.
            var config = await RemoteConfigService.Instance.FetchConfigsAsync(
                new UserAttributes(),
                new AppAttributes()
            );

            Debug.Log($"[MailTest] Config source: {config.origin}");

            // Cached data wouldn't prove the cloud connection worked.
            if (config.origin != ConfigOrigin.Remote)
            {
                Debug.LogWarning(
                    "[MailTest] No fresh cloud response. " +
                    "Check your connection and try again."
                );
                return;
            }

            if (!config.HasKey("mail_catalog"))
            {
                Debug.LogError(
                    "[MailTest] mail_catalog was not found. " +
                    "Check the linked cloud project, environment, " +
                    "key spelling, and publication."
                );
                return;
            }

            string json = config.GetJson("mail_catalog");

MailCatalog catalog = JsonUtility.FromJson<MailCatalog>(json);

if (catalog == null || catalog.messages == null)
{
    Debug.LogError("[MailTest] Expected a messages array in mail_catalog.");
    return;
}
if (mailInbox != null)
{
    mailInbox.ShowInbox(catalog);
}
else
{
    Debug.LogWarning("[MailTest] Assign the Mail Inbox reference.");
}

Debug.Log($"[MailTest] Loaded {catalog.messages.Length} message(s).");

foreach (MailMessage mail in catalog.messages)
{
    if (mail == null)
        continue;

    int rewardCount = mail.rewards != null ? mail.rewards.Length : 0;

    Debug.Log(
        $"[MailTest]\n" +
        $"ID: {mail.id}\n" +
        $"Title: {mail.title}\n" +
        $"Body: {mail.body}\n" +
        $"Attached rewards: {rewardCount}"
    );
}
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
