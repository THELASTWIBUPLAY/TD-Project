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
            if (UnityServices.State != ServicesInitializationState.Uninitialized)
            {
                Debug.LogWarning(
                    "[MailTest] Services already initialized. " +
                    "Stop Play Mode and run directly from MailTest."
                );
                return;
            }

            var options = new InitializationOptions();
            options.SetEnvironmentName("development");

            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var config = await RemoteConfigService.Instance.FetchConfigsAsync(
                new UserAttributes(),
                new AppAttributes()
            );

            Debug.Log($"[MailTest] Config source: {config.origin}");

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