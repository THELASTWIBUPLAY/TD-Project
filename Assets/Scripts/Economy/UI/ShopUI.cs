using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [Header("Dependencies")]
    private ShopManager _shopManager;
    [SerializeField] private ShopItemSlotUI[] _goldSlots;
    [SerializeField] private ShopItemSlotUI[] _gemSlot;

    public void Awake()
    {
        _shopManager = GetComponent<ShopManager>();
    }

    private void Start()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_shopManager == null || _shopManager.Resources == null) return;

        var resources = _shopManager.Resources;

        // Gold Slot
        for (int i = 0; i < _goldSlots.Length; i++)
        {
            if (_goldSlots[i] == null) continue;

            if (i < resources.GoldItem.Count)
            {
                _goldSlots[i].gameObject.SetActive(true);
                _goldSlots[i].Setup(resources.GoldItem[i]);
            }
            else
            {
                _goldSlots[i].gameObject.SetActive(false);
            }
        }

        // Gem Slot
        for (int i = 0; i < _gemSlot.Length; i++)
        {
            if (_gemSlot[i] == null) continue;

            if (i < resources.GemItem.Count)
            {
                _gemSlot[i].gameObject.SetActive(true);
                _gemSlot[i].Setup(resources.GemItem[i]);
            }
            else
            {
                _gemSlot[i].gameObject.SetActive(false);
            }
        }
    }
}
