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

    public void Setup(ResourceItemData data)
    {
        _itemName.text = data.name;
        _itemAmount.text = data.amount.ToString();
        _itemPrice.text = data.price.ToString();

        _itemIcon.sprite = data.sprite;
    }
}