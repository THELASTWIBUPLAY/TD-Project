using UnityEngine;
using UnityEngine.UI;

public class DeckRelicSlotUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private GameObject _emptyPlaceholder;
    [SerializeField] private Button _slotButton;

    private RelicSO _currentRelic;
    private int _slotIndex;
    private System.Action<int> _onSlotClicked;

    private void Awake()
    {
        if (_slotButton != null)
            _slotButton.onClick.AddListener(() => _onSlotClicked?.Invoke(_slotIndex));
    }

    public void Setup(int slotIndex, RelicSO relic, System.Action<int> onClicked)
    {
        _slotIndex = slotIndex;
        _currentRelic = relic;
        _onSlotClicked = onClicked;

        bool hasRelic = relic != null;
        if (_iconImage != null)
        {
            _iconImage.gameObject.SetActive(hasRelic);
            if (hasRelic) _iconImage.sprite = relic.relicIcon;
        }

        if (_emptyPlaceholder != null)
            _emptyPlaceholder.SetActive(!hasRelic);
    }
}