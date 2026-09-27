using Assets.Scripts.Economy.Managers;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] private ResourcesItemSO _resources;
    public ResourcesItemSO Resources => _resources;

    [Header("Panels")]
    [SerializeField] private CanvasGroup _recruitPanel;
    [SerializeField] private CanvasGroup _specialPanel;
    [SerializeField] private CanvasGroup _resourcesPanel;
    private CanvasGroup myPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        myPanel = GetComponent<CanvasGroup>();
    }

    public void BuyResource(ResourceItemData item)
    {
        if (EconomyManager.Instance == null)
        {
            Debug.LogError("[ShopManager] EconomyManager is missing!");
            return;
        }

        switch (item.type)
        {
            case ResourceItemType.Gold:
                ProcessGoldPurchase(item);
                break;

            case ResourceItemType.Gem:
                ProcessGemPurchase(item);
                break;
        }
    }
    private void ProcessGoldPurchase(ResourceItemData item)
    {
        int gemCost = Mathf.RoundToInt(item.price);
        int currentGem = EconomyManager.Instance.CurrentData.gem;

        // Validation 
        if (currentGem < gemCost)
        {
            AlertManager.Instance?.Show("Not enough gem");
            return;
        }

        // economy mutation
        EconomyManager.Instance.ModifyGem(-gemCost);
        EconomyManager.Instance.ModifyGold(item.amount);

        // Gameplay integration
        DailyQuestManager.Instance?.AddProgress(DailyQuestType.SpendGem, gemCost);
        AchievementManager.Instance?.AddProgress(AchievementType.SpendGem, gemCost);

        AlertManager.Instance?.Show($"Successfully bought {item.amount:N0} Gold!");
    }

    private void ProcessGemPurchase(ResourceItemData item)
    {
        // Catatan: Paket Gem berbayar uang sungguhan (USD / Rupiah) normalnya terhubung ke Unity IAP.
        // Untuk tahap prototyping & testing saat ini, kita simulasikan transaksi berhasil.
        EconomyManager.Instance.ModifyGem(item.amount);

        AlertManager.Instance?.Show($"[TEST IAP] Berhasil membeli {item.amount:N0} Gem!");
    }

    // Navigation
    public void OpenRecruitShop()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_recruitPanel, true);
            MainMenuManager.Instance.TogglePanel(_specialPanel, false);
            MainMenuManager.Instance.TogglePanel(_resourcesPanel, false);
        }
    }

    public void OpenSpecialShop()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_recruitPanel, false);
            MainMenuManager.Instance.TogglePanel(_specialPanel, true);
            MainMenuManager.Instance.TogglePanel(_resourcesPanel, false);
        }
    }

    public void OpenResourcesShop()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_recruitPanel, false);
            MainMenuManager.Instance.TogglePanel(_specialPanel, false);
            MainMenuManager.Instance.TogglePanel(_resourcesPanel, true);
        }
    }
}