using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckPanelUI : MonoBehaviour
{
    public static DeckPanelUI Instance { get; private set; }

    public enum InventoryTab { Hero, Relic }
    public enum InventoryFilter { All, Unlocked, Locked }

    private InventoryTab _currentTab = InventoryTab.Hero;
    private InventoryFilter _currentFilter = InventoryFilter.All;

    [Header("Modal Panels")]
    [SerializeField] private HeroUpgradeModalUI _heroUpgradeModal;
    [SerializeField] private RelicDetailModalUI _relicDetailModal;

    [Header("Top Box: Equipped Slots")]
    [SerializeField] private DeckDropSlot[] _heroDropSlots;   // Ukuran: 5
    [SerializeField] private DeckDropSlot[] _relicDropSlots;  // Ukuran: 3

    [Header("Bottom Box: Tabs")]
    [SerializeField] private Button _tabHeroBtn;
    [SerializeField] private Button _tabRelicBtn;

    [Header("Bottom Box: Filter Buttons")]
    [SerializeField] private Button _filterAllBtn;
    [SerializeField] private Button _filterUnlockedBtn;
    [SerializeField] private Button _filterLockedBtn;

    [Header("Filter Visual Feedback")]
    [SerializeField] private Color _activeFilterColor = Color.white;
    [SerializeField] private Color _inactiveFilterColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Bottom Box: Single Horizontal ScrollView")]
    [SerializeField] private Transform _inventoryContent;
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

        // Inisialisasi slot equipped
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

        // Listener Tab Utama
        if (_tabHeroBtn != null) _tabHeroBtn.onClick.AddListener(() => SwitchTab(InventoryTab.Hero));
        if (_tabRelicBtn != null) _tabRelicBtn.onClick.AddListener(() => SwitchTab(InventoryTab.Relic));

        // Listener Filter
        if (_filterAllBtn != null) _filterAllBtn.onClick.AddListener(() => SetFilter(InventoryFilter.All));
        if (_filterUnlockedBtn != null) _filterUnlockedBtn.onClick.AddListener(() => SetFilter(InventoryFilter.Unlocked));
        if (_filterLockedBtn != null) _filterLockedBtn.onClick.AddListener(() => SetFilter(InventoryFilter.Locked));
    }

    private void OnEnable()
    {
        UpdateFilterButtonsVisual();
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

    public void SetFilter(InventoryFilter filter)
    {
        _currentFilter = filter;
        UpdateFilterButtonsVisual();
        RefreshInventoryRows();
    }

    private void UpdateFilterButtonsVisual()
    {
        SetButtonVisual(_filterAllBtn, _currentFilter == InventoryFilter.All);
        SetButtonVisual(_filterUnlockedBtn, _currentFilter == InventoryFilter.Unlocked);
        SetButtonVisual(_filterLockedBtn, _currentFilter == InventoryFilter.Locked);
    }

    private void SetButtonVisual(Button btn, bool isActive)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = isActive ? _activeFilterColor : _inactiveFilterColor;
        }
    }

    // =========================================================================
    // INVENTORY RENDERING (SINGLE SCROLL VIEW WITH FILTER)
    // =========================================================================

    public void RefreshInventoryRows()
    {
        ClearContainer(_inventoryContent);

        if (_currentTab == InventoryTab.Hero)
        {
            PopulateHeroRows();
        }
        else
        {
            PopulateRelicRows();
        }
    }

    private void PopulateHeroRows()
    {
        if (_heroDatabase == null || _inventoryContent == null) return;

        List<HeroSO> unlockedList = new List<HeroSO>();
        List<HeroSO> lockedList = new List<HeroSO>();

        foreach (var hero in _heroDatabase.AllHeroes)
        {
            if (hero == null) continue;

            // Cek apakah hero sedang terpasang di 5 slot Deck
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

            // Jika sedang dipakai di slot atas, sembunyikan dari daftar inventory bawah
            if (isEquipped) continue;

            bool isOwned = HeroManager.Instance != null && HeroManager.Instance.IsHeroUnlocked(hero.heroId);

            if (isOwned)
                unlockedList.Add(hero);
            else
                lockedList.Add(hero);
        }

        // Tampilkan item Unlocked jika filter All atau Unlocked
        if (_currentFilter == InventoryFilter.All || _currentFilter == InventoryFilter.Unlocked)
        {
            foreach (var hero in unlockedList)
            {
                CreateDraggableSlot(_inventoryContent, hero.heroIcon, hero.heroId, DeckDraggableItem.ItemType.Hero, true, false);
            }
        }

        // Tampilkan item Locked jika filter All atau Locked
        if (_currentFilter == InventoryFilter.All || _currentFilter == InventoryFilter.Locked)
        {
            foreach (var hero in lockedList)
            {
                CreateDraggableSlot(_inventoryContent, hero.heroIcon, hero.heroId, DeckDraggableItem.ItemType.Hero, false, false);
            }
        }
    }

    private void PopulateRelicRows()
    {
        if (_relicDatabase == null || _inventoryContent == null) return;

        List<RelicSO> unlockedList = new List<RelicSO>();
        List<RelicSO> lockedList = new List<RelicSO>();

        foreach (var relic in _relicDatabase.AllRelics)
        {
            if (relic == null) continue;

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

            if (isEquipped) continue;

            bool isOwned = RelicManager.Instance != null && RelicManager.Instance.IsRelicUnlocked(relic.relicId);

            if (isOwned)
                unlockedList.Add(relic);
            else
                lockedList.Add(relic);
        }

        if (_currentFilter == InventoryFilter.All || _currentFilter == InventoryFilter.Unlocked)
        {
            foreach (var relic in unlockedList)
            {
                CreateDraggableSlot(_inventoryContent, relic.relicIcon, relic.relicId, DeckDraggableItem.ItemType.Relic, true, false);
            }
        }

        if (_currentFilter == InventoryFilter.All || _currentFilter == InventoryFilter.Locked)
        {
            foreach (var relic in lockedList)
            {
                CreateDraggableSlot(_inventoryContent, relic.relicIcon, relic.relicId, DeckDraggableItem.ItemType.Relic, false, false);
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

        // Tampilkan badge jumlah salinan hasil gacha/rekrutmen jika hero sudah terbuka
        if (type == DeckDraggableItem.ItemType.Hero && owned && HeroManager.Instance != null)
        {
            int copies = HeroManager.Instance.GetRecruitedCopies(id);
            if (copies > 0)
            {
                var badge = new GameObject("CopyCount", typeof(RectTransform), typeof(TextMeshProUGUI));
                badge.transform.SetParent(slotObj.transform, false);
                var text = badge.GetComponent<TextMeshProUGUI>();
                text.text = "x" + copies;
                text.fontSize = 16;
                text.color = Color.white;
                text.alignment = TextAlignmentOptions.BottomRight;
                text.raycastTarget = false;
                var rect = text.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(3, 3);
                rect.offsetMax = new Vector2(-3, -3);
            }
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