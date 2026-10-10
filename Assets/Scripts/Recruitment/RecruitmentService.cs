using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using UnityEngine;

[Serializable] public class OwnedHero { public string heroId; public int quantity; }
[Serializable] public class OwnedRelic { public string relicId; public int quantity; }
// Keep numeric values stable for existing prefab settings and pending requests.
public enum RecruitmentChestType { Rare = 0, Legendary = 1, RareRelic = 2, EpicRelic = 3 }
[Serializable] public class HeroOpening { public string requestId; public string chestType; public string heroId; public string relicId; public int quantity; public int sequence; }
[Serializable] public class RecruitmentState
{
    public int schemaVersion = 1;
    public int rareHeroKeys;
    public int legendaryHeroKeys;
    public int rareRelicKeys;
    public int epicRelicKeys;
    public int sequence;
    public OwnedHero[] heroes = Array.Empty<OwnedHero>();
    public OwnedRelic[] relics = Array.Empty<OwnedRelic>();
    public HeroOpening lastOpening;
    public int QuantityOf(string heroId) => Array.Find(heroes, h => h.heroId == heroId)?.quantity ?? 0;
    public int RelicQuantityOf(string relicId) => relics == null ? 0 : Array.Find(relics, r => r.relicId == relicId)?.quantity ?? 0;
}
[Serializable] public class RecruitmentReply
{
    public bool ok;
    public string error;
    public string environmentId;
    public RecruitmentState state;
    public HeroOpening reward;
}

// Deck can observe CollectionChanged and read Current; it does not need shop UI references.
public class RecruitmentService : MonoBehaviour
{
    public RecruitmentState Current { get; private set; }
    public bool Busy { get; private set; }
    public event Action CollectionChanged;
    string scope;
    string playerId;
    [Serializable] class Pending { public string requestId; public int sequence; public RecruitmentChestType chestType; }
    public int KeysFor(RecruitmentChestType chest) => chest switch
    {
        RecruitmentChestType.Rare => Current?.rareHeroKeys ?? 0,
        RecruitmentChestType.Legendary => Current?.legendaryHeroKeys ?? 0,
        RecruitmentChestType.RareRelic => Current?.rareRelicKeys ?? 0,
        RecruitmentChestType.EpicRelic => Current?.epicRelicKeys ?? 0,
        _ => 0
    };
    static string ActionFor(RecruitmentChestType chest) => chest switch
    {
        RecruitmentChestType.Rare => "open",
        RecruitmentChestType.Legendary => "open_legendary",
        RecruitmentChestType.RareRelic => "open_rare_relic",
        RecruitmentChestType.EpicRelic => "open_epic_relic",
        _ => throw new ArgumentOutOfRangeException(nameof(chest))
    };
    public bool HasPendingFor(RecruitmentChestType chest) => HasPending && ReadPending()?.chestType == chest;
    Pending ReadPending() => HasPending ? JsonUtility.FromJson<Pending>(PlayerPrefs.GetString(scope)) : null;

    async Task WaitForSignIn()
    {
        // Reuse the project's existing authentication/bootstrap and its selected environment.
        for (int i = 0; i < 100; i++)
        {
            if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn) return;
            await Task.Delay(100);
        }
        throw new InvalidOperationException("Sign in through the normal game login, then press Refresh.");
    }

    public async Task<RecruitmentReply> RefreshAsync()
    {
        if (Busy) throw new InvalidOperationException("Recruitment is busy.");
        Busy = true;
        CollectionChanged?.Invoke();
        try { await WaitForSignIn(); return await Status(); }
        finally { Busy = false; CollectionChanged?.Invoke(); }
    }

    async Task<RecruitmentReply> Status()
    {
        playerId = AuthenticationService.Instance.PlayerId;
        var reply = await Call("status", "", 0);
        Apply(reply);
        scope = "Recruitment.Pending." + Application.cloudProjectId + "." + reply.environmentId + "." + playerId;
        Debug.Log($"[Recruitment] Player: {playerId}; Environment: {reply.environmentId}; Rare Hero Keys: {Current.rareHeroKeys}");
        return reply;
    }

    public Task<RecruitmentReply> OpenRareHeroAsync() => OpenHeroAsync(RecruitmentChestType.Rare);
    public Task<RecruitmentReply> OpenHeroAsync(RecruitmentChestType chest) => OpenChestAsync(chest);

    public async Task<RecruitmentReply> OpenChestAsync(RecruitmentChestType chest)
    {
        string action = ActionFor(chest);
        if (Busy) throw new InvalidOperationException("Recruitment is busy.");
        Busy = true;
        CollectionChanged?.Invoke();
        try
        {
            await WaitForSignIn();
            // Refresh authoritative state before creating a new request. Pending requests retain their original sequence.
            await Status();
            var pending = ReadPending();
            if (pending != null && pending.chestType != chest)
                throw new InvalidOperationException("Retry the pending opening on the other chest first.");
            if (pending == null)
            {
                if (KeysFor(chest) < 1) return new RecruitmentReply { ok = false, error = "NO_KEYS", state = Current };
                pending = new Pending { requestId = Guid.NewGuid().ToString("N"), sequence = Current.sequence, chestType = chest };
                PlayerPrefs.SetString(scope, JsonUtility.ToJson(pending));
                PlayerPrefs.Save();
            }
            var reply = await Call(action, pending.requestId, pending.sequence);
            Apply(reply);
            // A transport/server exception keeps the pending request for a safe retry, including after restarting.
            PlayerPrefs.DeleteKey(scope);
            PlayerPrefs.Save();
            return reply;
        }
        finally { Busy = false; CollectionChanged?.Invoke(); }
    }

    public bool HasPending => !string.IsNullOrEmpty(scope) && PlayerPrefs.HasKey(scope);
    Task<RecruitmentReply> Call(string action, string requestId, int sequence) =>
        CloudCodeService.Instance.CallEndpointAsync<RecruitmentReply>("RecruitHero", new Dictionary<string, object>
        { { "action", action }, { "requestId", requestId }, { "expectedSequence", sequence } });

    void Apply(RecruitmentReply reply)
    {
        if (reply?.state?.heroes == null || string.IsNullOrEmpty(reply.environmentId))
            throw new InvalidOperationException("Invalid recruitment response.");
        if (!AuthenticationService.Instance.IsSignedIn || AuthenticationService.Instance.PlayerId != playerId)
            throw new InvalidOperationException("Account changed. Refresh recruitment before continuing.");
        Current = reply.state;
        CollectionChanged?.Invoke();
    }
}
