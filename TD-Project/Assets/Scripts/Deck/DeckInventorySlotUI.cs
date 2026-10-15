using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckInventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private GameObject _equippedBadge;
    [SerializeField] private Button _selectButton;

    private System.Action _onClickAction;

    private void Awake()
    {
        if (_selectButton != null)
            _selectButton.onClick.AddListener(() => _onClickAction?.Invoke());
    }

    public void SetupHero(HeroSO hero, bool isEquipped, System.Action onClickAction)
    {
        _onClickAction = onClickAction;
        if (_iconImage != null) _iconImage.sprite = hero.heroIcon;
        if (_nameText != null) _nameText.text = hero.heroName;
        if (_equippedBadge != null) _equippedBadge.SetActive(isEquipped);
    }

    public void SetupRelic(RelicSO relic, bool isEquipped, System.Action onClickAction)
    {
        _onClickAction = onClickAction;
        if (_iconImage != null) _iconImage.sprite = relic.relicIcon;
        if (_nameText != null) _nameText.text = relic.relicName;
        if (_equippedBadge != null) _equippedBadge.SetActive(isEquipped);
    }
}