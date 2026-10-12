using System;
using System.Collections.Generic;
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

    // State Equipped Array
    private readonly string[] _equippedHeroes = new string[5];
    private readonly string[] _equippedRelics = new string[3];

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
        RefreshEquippedView();
        RefreshInventoryRows();

        if (HeroManager.Instance != null)
            HeroManager.Instance.OnHeroDataUpdated += RefreshInventoryRows;
        if (RelicManager.Instance != null)
            RelicManager.Instance.OnRelicDataUpdated += RefreshInventoryRows;
    }

    private void OnDisable()
    {
        if (HeroManager.Instance != null)
            HeroManager.Instance.OnHeroDataUpdated -= RefreshInventoryRows;
        if (RelicManager.Instance != null)
            RelicManager.Instance.OnRelicDataUpdated -= RefreshInventoryRows;
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

            // BACA STATUS KEPEMILIKAN DARI HEROMANAGER
            bool isOwned = HeroManager.Instance != null && HeroManager.Instance.IsHeroUnlocked(hero.heroId);
            bool isEquipped = Array.Exists(_equippedHeroes, id => id == hero.heroId);

            if (isOwned)
            {
                if (isEquipped) continue;
                CreateDraggableSlot(_ownedContent, hero.heroIcon, hero.heroId, DeckDraggableItem.ItemType.Hero, true, false);
            }
            else
            {
                // Unit yang belum terbuka masuk ke ScrollView Locked
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
            bool isEquipped = Array.Exists(_equippedRelics, id => id == relic.relicId);

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
        string[] targetArray = (type == DeckDraggableItem.ItemType.Hero) ? _equippedHeroes : _equippedRelics;

        if (fromEquipped && sourceIndex >= 0)
        {
            HandleSlotToSlotSwap(type, sourceIndex, targetSlotIndex);
        }
        else
        {
            targetArray[targetSlotIndex] = incomingId;
            RefreshEquippedView();
            RefreshInventoryRows();
            SaveManager.Instance?.SaveLocal();
        }
    }

    public void HandleSlotToSlotSwap(DeckDraggableItem.ItemType type, int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;

        string[] targetArray = (type == DeckDraggableItem.ItemType.Hero) ? _equippedHeroes : _equippedRelics;

        string temp = targetArray[toIndex];
        targetArray[toIndex] = targetArray[fromIndex];
        targetArray[fromIndex] = temp;

        RefreshEquippedView();
        RefreshInventoryRows();
        SaveManager.Instance?.SaveLocal();
    }

    public void UnequipItemFromSlot(DeckDraggableItem.ItemType type, int slotIndex)
    {
        string[] targetArray = (type == DeckDraggableItem.ItemType.Hero) ? _equippedHeroes : _equippedRelics;

        if (slotIndex >= 0 && slotIndex < targetArray.Length)
        {
            targetArray[slotIndex] = string.Empty;

            RefreshEquippedView();
            RefreshInventoryRows();
            SaveManager.Instance?.SaveLocal();
        }
    }

    public void RefreshEquippedView()
    {
        for (int i = 0; i < _heroDropSlots.Length; i++)
        {
            string id = _equippedHeroes[i];
            var hero = !string.IsNullOrEmpty(id) ? _heroDatabase?.GetHeroById(id) : null;
            _heroDropSlots[i].SetupSlot(hero != null ? hero.heroIcon : null, id);
        }

        for (int i = 0; i < _relicDropSlots.Length; i++)
        {
            string id = _equippedRelics[i];
            var relic = !string.IsNullOrEmpty(id) ? _relicDatabase?.GetRelicById(id) : null;
            _relicDropSlots[i].SetupSlot(relic != null ? relic.relicIcon : null, id);
        }
    }
}