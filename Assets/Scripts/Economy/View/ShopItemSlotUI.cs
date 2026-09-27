using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlotUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemAmount;
    [SerializeField] private TextMeshProUGUI _itemPrice;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private Button _buyButton;

    private ResourceItemData _currentData;

    private void Awake()
    {
        if (_buyButton != null)
        {
            _buyButton.onClick.AddListener(OnBuyClicked);
        }
    }

    private void OnDestroy()
    {
        if (_buyButton != null)
        {
            _buyButton.onClick.RemoveListener(OnBuyClicked);
        }
    }

    public void Setup(ResourceItemData data)
    {
        _currentData = data;

        if (_itemName != null) _itemName.text = data.name;
        if (_itemAmount != null) _itemAmount.text = $"+{data.amount:N0}";
        if (_itemIcon != null && data.sprite != null) _itemIcon.sprite = data.sprite;

        // Format label harga berdasarkan mata uang pembayaran
        if (_itemPrice != null)
        {
            if (data.type == ResourceItemType.Gold)
            {
                // Pembelian Gold dibayar dengan Gem
                _itemPrice.text = $"{data.price:N0} Gem";
            }
            else
            {
                // Pembelian Gem dibayar dengan Uang Nyata
                _itemPrice.text = $"${data.price:F2}";
            }
        }
    }

    private void OnBuyClicked()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.BuyResource(_currentData);
        }
        else
        {
            Debug.LogWarning("[ShopItemSlotUI] ShopManager.Instance tidak ditemukan!");
        }
    }
}