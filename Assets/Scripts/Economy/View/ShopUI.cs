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
        var resources = _shopManager.Resources;

        for (int i = 0; i < _goldSlots.Length; i++)
        {
            var slot = _goldSlots[i];
            var data = resources.GoldItem[i];
            slot.Setup(data);
        }

        for (int i = 0; i < _gemSlot.Length; i++)
        {
            var slot = _gemSlot[i];
            var data = resources.GemItem[i];
            slot.Setup(data);
        }
    }
}
