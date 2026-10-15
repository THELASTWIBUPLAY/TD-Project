using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckPanelUI : MonoBehaviour
{
    public static DeckPanelUI Instance { get; private set; }

    private enum InventoryTab { Hero, Relic }
    private InventoryTab _currentTab = InventoryTab.Hero;

    [Header("Modal Panels")]
    [SerializeField] private HeroUpgradeModalUI _heroUpgradeModal;
    [SerializeField] private RelicDetailModalUI _relicDetailModal;

    [Header("Top Box: Equipped Slots")]
    [SerializeField] private DeckDropSlot[] _heroDropSlots;   // Ukuran: 5
    [SerializeField] private DeckDropSlot[] _relicDropSlots;  // Ukuran: 3

    [Header("Bottom Box: Tabs")]
    [SerializeField] private Button _tabHeroBtn;
    [SerializeField] private Button _tabRelicBtn;

    [Header("Bottom Box: Section Titles")]
    [SerializeField] private TextMeshProUGUI _ownedTitleText;
    [SerializeField] private TextMeshProUGUI _lockedTitleText;

    [Header("Bottom Box: Separate ScrollView Containers")]
    [SerializeField] private Transform _ownedContent;
    [SerializeField] private Transform _lockedContent;
    [SerializeField] private GameObject _itemSlotPrefab;

    [Header("Databases")]
    [SerializeField] private HeroDatabaseSO _heroDatabase;
    [SerializeField] private RelicDatabaseSO _relicDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_heroDropSlots != null)
        {
            for (int i = 0; i < _heroDropSlots.Length; i++)
            {
                if (_heroDropSlots[i] != null)
                    _heroDropSlots[i].Configure(DeckDraggableItem.ItemType.Hero, i);
            }
        }

        if (_relicDropSlots != null)
        {
            for (int i = 0; i < _relicDropSlots.Length; i++)
            {
                if (_relicDropSlots[i] != null)
                    _relicDropSlots[i].Configure(DeckDraggableItem.ItemType.Relic, i);
            }
        }

        if (_tabHeroBtn != null) _tabHeroBtn.onClick.AddListener(() => SwitchTab(InventoryTab.Hero));
        if (_tabRelicBtn != null) _tabRelicBtn.onClick.AddListener(() => SwitchTab(InventoryTab.Relic));
    }

    private void OnEnable()
    {
        RefreshAllViews();

        if (HeroManager.Instance != null)
            HeroManager.Instance.OnHeroDataUpdated += RefreshAllViews;
        if (RelicManager.Instance != null)
            RelicManager.Instance.OnRelicDataUpdated += RefreshAllViews;
    }

    private void OnDisable()
    {
        if (HeroManager.Instance != null)
            HeroManager.Instance.OnHeroDataUpdated -= RefreshAllViews;
        if (RelicManager.Instance != null)
            RelicManager.Instance.OnRelicDataUpdated -= RefreshAllViews;
    }

    private void RefreshAllViews()
    {
        RefreshEquippedView();
        RefreshInventoryRows();
    }

    public void OpenHeroUpgradeModal(string heroId)
    {
        if (_heroUpgradeModal != null)
        {
            _heroUpgradeModal.SetupModal(heroId);
        }
    }

    public void OpenRelicDetailModal(string relicId)
    {
        if (_relicDetailModal != null)
        {
            _relicDetailModal.SetupModal(relicId);
        }
    }

    private void SwitchTab(InventoryTab targetTab)
    {
        _currentTab = targetTab;
        RefreshInventoryRows();
    }

    // =========================================================================
    // INVENTORY RENDERING (DUAL SCROLL VIEW)
    // =========================================================================

    public void RefreshInventoryRows()
    {
        ClearContainer(_ownedContent);
        ClearContainer(_lockedContent);

        if (_currentTab == InventoryTab.Hero)
        {
            if (_ownedTitleText != null) _ownedTitleText.text = "Your Unit";
            if (_lockedTitleText != null) _lockedTitleText.text = "Locked Hero";

            PopulateHeroRows();
        }
        else
        {
            if (_ownedTitleText != null) _ownedTitleText.text = "Your Relic";
            if (_lockedTitleText != null) _lockedTitleText.text = "Locked Relic";

            PopulateRelicRows();
        }
    }

    private void PopulateHeroRows()
    {
        if (_heroDatabase == null) return;

        foreach (var hero in _heroDatabase.AllHeroes)
        {
            if (hero == null) continue;

            bool isOwned = HeroManager.Instance != null && HeroManager.Instance.IsHeroUnlocked(hero.heroId);

            bool isEquipped = false;
            if (HeroManager.Instance != null && HeroManager.Instance.EquippedHeroIds != null)
            {
                for (int i = 0; i < HeroManager.Instance.EquippedHeroIds.Count; i++)
                {
                    if (HeroManager.Instance.EquippedHeroIds[i] == hero.heroId)
                    {
                        isEquipped = true;
                        break;
                    }
                }
            }

            if (isOwned)
            {
                if (isEquipped) continue;
                CreateDraggableSlot(_ownedContent, hero.heroIcon, hero.heroId, DeckDraggableItem.ItemType.Hero, true, false);
            }
            else
            {
                CreateDraggableSlot(_lockedContent, hero.heroIcon, hero.heroId, DeckDraggableItem.ItemType.Hero, false, false);
            }
        }
    }

    private void PopulateRelicRows()
    {
        if (_relicDatabase == null) return;

        foreach (var relic in _relicDatabase.AllRelics)
        {
            if (relic == null) continue;

            bool isOwned = RelicManager.Instance != null && RelicManager.Instance.IsRelicUnlocked(relic.relicId);

            bool isEquipped = false;
            if (RelicManager.Instance != null && RelicManager.Instance.EquippedRelicIds != null)
            {
                for (int i = 0; i < RelicManager.Instance.EquippedRelicIds.Count; i++)
                {
                    if (RelicManager.Instance.EquippedRelicIds[i] == relic.relicId)
                    {
                        isEquipped = true;
                        break;
                    }
                }
            }

            if (isOwned)
            {
                if (isEquipped) continue;
                CreateDraggableSlot(_ownedContent, relic.relicIcon, relic.relicId, DeckDraggableItem.ItemType.Relic, true, false);
            }
            else
            {
                CreateDraggableSlot(_lockedContent, relic.relicIcon, relic.relicId, DeckDraggableItem.ItemType.Relic, false, false);
            }
        }
    }

    private void CreateDraggableSlot(Transform parent, Sprite icon, string id, DeckDraggableItem.ItemType type, bool owned, bool equipped)
    {
        GameObject slotObj = Instantiate(_itemSlotPrefab, parent);
        var itemComp = slotObj.GetComponent<DeckDraggableItem>();
        if (itemComp != null)
        {
            itemComp.Setup(icon, id, type, owned, equipped, false, -1);
        }
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    // =========================================================================
    // DROP & SWAP HANDLING
    // =========================================================================

    public void HandleItemDroppedOnSlot(DeckDraggableItem.ItemType type, int targetSlotIndex, string incomingId, bool fromEquipped, int sourceIndex)
    {
        if (type == DeckDraggableItem.ItemType.Hero)
        {
            if (fromEquipped && sourceIndex >= 0)
                HeroManager.Instance?.SwapEquippedHeroes(sourceIndex, targetSlotIndex);
            else
                HeroManager.Instance?.SetEquippedHero(targetSlotIndex, incomingId);
        }
        else
        {
            if (fromEquipped && sourceIndex >= 0)
                RelicManager.Instance?.SwapEquippedRelics(sourceIndex, targetSlotIndex);
            else
                RelicManager.Instance?.SetEquippedRelic(targetSlotIndex, incomingId);
        }

        RefreshAllViews();
    }

    public void HandleSlotToSlotSwap(DeckDraggableItem.ItemType type, int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;

        if (type == DeckDraggableItem.ItemType.Hero)
        {
            HeroManager.Instance?.SwapEquippedHeroes(fromIndex, toIndex);
        }
        else
        {
            RelicManager.Instance?.SwapEquippedRelics(fromIndex, toIndex);
        }

        // Diletakkan di luar if-else agar baik Hero maupun Relic langsung menyegarkan tampilan
        RefreshAllViews();
    }

    public void UnequipItemFromSlot(DeckDraggableItem.ItemType type, int slotIndex)
    {
        if (type == DeckDraggableItem.ItemType.Hero)
        {
            HeroManager.Instance?.UnequipHero(slotIndex);
        }
        else
        {
            RelicManager.Instance?.UnequipRelicAt(slotIndex);
        }

        RefreshAllViews();
    }

    public void RefreshEquippedView()
    {
        for (int i = 0; i < _heroDropSlots.Length; i++)
        {
            string id = HeroManager.Instance != null ? HeroManager.Instance.GetEquippedHeroAt(i) : string.Empty;
            var hero = !string.IsNullOrEmpty(id) ? _heroDatabase?.GetHeroById(id) : null;
            _heroDropSlots[i].SetupSlot(hero != null ? hero.heroIcon : null, id);
        }

        for (int i = 0; i < _relicDropSlots.Length; i++)
        {
            string id = RelicManager.Instance != null ? RelicManager.Instance.GetEquippedRelicAt(i) : string.Empty;
            var relic = !string.IsNullOrEmpty(id) ? _relicDatabase?.GetRelicById(id) : null;
            _relicDropSlots[i].SetupSlot(relic != null ? relic.relicIcon : null, id);
        }
    }
}