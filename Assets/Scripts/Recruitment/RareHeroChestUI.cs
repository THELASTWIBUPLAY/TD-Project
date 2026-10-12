using System;
using TMPro;
using UnityEngine;

public class RareHeroChestUI : MonoBehaviour
{
    // Shared chest view; keep the class name to preserve existing prefab references.
    [SerializeField] RecruitmentChestType chestType;
    [SerializeField] RecruitmentService service;
    [SerializeField] HeroCatalog catalog;
    [SerializeField] RelicCatalog relicCatalog;
    [SerializeField] UnityEngine.UI.Button keyButton;
    [SerializeField] TMP_Text keyLabel;
    [SerializeField] UnityEngine.UI.Button refreshButton;
    [SerializeField] GameObject resultPanel;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text statusText;
    bool busy;
    bool IsRelic => chestType == RecruitmentChestType.RareRelic || chestType == RecruitmentChestType.EpicRelic;
    string ChestName => chestType == RecruitmentChestType.RareRelic ? "Rare Relic" :
        chestType == RecruitmentChestType.EpicRelic ? "Epic Relic" : chestType + " Hero";

    void Awake()
    {
        keyButton.onClick.AddListener(Open);
        refreshButton.onClick.AddListener(Refresh);
        resultPanel.SetActive(false);
    }
    void OnEnable()
    {
        service.CollectionChanged += UpdateButtons;
        statusText.text = ChestName + " Chest · 1 key";
        UpdateButtons();
        if (!service.Busy) Refresh();
    }
    void OnDisable()
    {
        service.CollectionChanged -= UpdateButtons;
        if (resultPanel) resultPanel.SetActive(false);
    }
    void OnDestroy()
    {
        keyButton.onClick.RemoveListener(Open);
        refreshButton.onClick.RemoveListener(Refresh);
    }
    public async void Refresh()
    {
        if (busy || service.Busy) return;
        busy = true; UpdateButtons(); statusText.text = "Loading collection...";
        try { await service.RefreshAsync(); if (this) statusText.text = ChestName + " Chest · 1 key"; }
        catch (Exception e) { if (this) ShowFailure(e); }
        finally { if (this) { busy = false; UpdateButtons(); } }
    }
    public async void Open()
    {
        if (busy || service.Busy) return;
        busy = true; UpdateButtons(); statusText.text = "Opening chest...";
        try
        {
            var reply = await service.OpenChestAsync(chestType);
            if (!this) return;
            if (!reply.ok)
            {
                statusText.text = reply.error == "NO_KEYS" ? "You need a " + ChestName + " Key." :
                    reply.error == "INVALID_REQUEST" ? "Update the RecruitHero Cloud Code script." :
                    reply.error == "STATE_CHANGED" ? "Collection updated on another request. Review your keys and try again." : "This chest is currently unavailable.";
                return;
            }
            var reward = reply.reward;
            if (reward == null) throw new InvalidOperationException("The server returned no reward.");
            string id = IsRelic ? reward.relicId : reward.heroId;
            if (string.IsNullOrEmpty(id)) throw new InvalidOperationException("The server returned the wrong reward type.");
            string name = IsRelic ? relicCatalog.Find(id)?.displayName ?? id : catalog.Find(id)?.displayName ?? id;
            int quantity = IsRelic ? reply.state.RelicQuantityOf(id) : reply.state.QuantityOf(id);
            resultText.text = name + " +1\n\nOwned: " + quantity;
            resultPanel.SetActive(true);
            statusText.text = (IsRelic ? "Relic" : "Hero") + " added to your collection.";
        }
        catch (Exception e) { if (this) ShowFailure(e); }
        finally { if (this) { busy = false; UpdateButtons(); } }
    }
    void ShowFailure(Exception e)
    {
        statusText.text = service.HasPendingFor(chestType) ? "Connection interrupted. Press Retry to recover this opening." :
            "Recruitment unavailable. Check sign-in / cloud setup, then Refresh.";
        Debug.LogWarning("[Recruitment] " + e.Message);
    }
    void UpdateButtons()
    {
        bool waiting = busy || service.Busy;
        bool retry = service.HasPendingFor(chestType);
        keyLabel.text = waiting ? "Please wait" : retry ? "Retry opening" : service.KeysFor(chestType) + "/1 Key";
        keyButton.interactable = !waiting && service.Current != null &&
            (retry || (!service.HasPending && service.KeysFor(chestType) > 0));
        refreshButton.interactable = !waiting;
    }
}
