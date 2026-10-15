using UnityEngine;
using UnityEngine.UI;

public class DeckHeroSlotUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private GameObject _emptyPlaceholder;
    [SerializeField] private Button _slotButton;

    private HeroSO _currentHero;
    private int _slotIndex;
    private System.Action<int> _onSlotClicked;

    private void Awake()
    {
        if (_slotButton != null)
            _slotButton.onClick.AddListener(() => _onSlotClicked?.Invoke(_slotIndex));
    }

    public void Setup(int slotIndex, HeroSO hero, System.Action<int> onClicked)
    {
        _slotIndex = slotIndex;
        _currentHero = hero;
        _onSlotClicked = onClicked;

        bool hasHero = hero != null;
        if (_iconImage != null)
        {
            _iconImage.gameObject.SetActive(hasHero);
            if (hasHero) _iconImage.sprite = hero.heroIcon;
        }

        if (_emptyPlaceholder != null)
            _emptyPlaceholder.SetActive(!hasHero);
    }
}