using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicDetailModalUI : MonoBehaviour
{
    [Header("Identity & Visual")]
    [SerializeField] private Image _relicIcon;
    [SerializeField] private TextMeshProUGUI _relicNameText;
    [SerializeField] private TextMeshProUGUI _elementText;
    [SerializeField] private Image _elementBadge;
    [SerializeField] private TextMeshProUGUI _typeText;

    [Header("Bonus Effect Display")]
    [SerializeField] private TextMeshProUGUI _bonusTitleText;
    [SerializeField] private TextMeshProUGUI _bonusValueText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("Element Colors (Visual Feedback)")]
    [SerializeField] private Color _fireColor = new Color(0.95f, 0.35f, 0.25f);
    [SerializeField] private Color _earthColor = new Color(0.45f, 0.75f, 0.35f);
    [SerializeField] private Color _waterColor = new Color(0.25f, 0.65f, 0.95f);
    [SerializeField] private Color _windColor = new Color(0.65f, 0.95f, 0.75f);

    [Header("Modal Controls")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _backgroundOverlayCloseBtn;

    private string _currentRelicId;

    private void Awake()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(CloseModal);

        if (_backgroundOverlayCloseBtn != null)
            _backgroundOverlayCloseBtn.onClick.AddListener(CloseModal);
    }

    public void SetupModal(string relicId)
    {
        _currentRelicId = relicId;
        gameObject.SetActive(true);
        RefreshDisplay();
    }

    public void CloseModal()
    {
        gameObject.SetActive(false);
    }

    public void RefreshDisplay()
    {
        if (RelicManager.Instance == null || RelicManager.Instance.Database == null) return;

        var relic = RelicManager.Instance.Database.GetRelicById(_currentRelicId);
        if (relic == null) return;

        // 1. Tampilkan Identitas Dasar
        if (_relicIcon != null) _relicIcon.sprite = relic.relicIcon;
        if (_relicNameText != null) _relicNameText.text = relic.relicName;
        if (_descriptionText != null) _descriptionText.text = relic.relicDescription;

        // 2. Tampilkan Tipe Relik (jika ada properti type di RelicSO)
        if (_typeText != null)
        {
            // Menampilkan nama tipe atau kategori relik
            _typeText.text = $"Type: {relic.activationType}";
        }

        // 3. Konfigurasi Elemen & Pewarnaan Badge
        SetupElementBadge(relic.element);

        // 4. Format Efek Pasif Sesuai Elemen
        SetupPassiveEffect(relic);
    }

    private void SetupElementBadge(RelicElement element)
    {
        Color targetColor = Color.white;
        string elementName = element.ToString();

        switch (element)
        {
            case RelicElement.Fire:
                targetColor = _fireColor;
                break;
            case RelicElement.Earth:
                targetColor = _earthColor;
                break;
            case RelicElement.Water:
                targetColor = _waterColor;
                break;
            case RelicElement.Wind:
                targetColor = _windColor;
                break;
        }

        if (_elementText != null)
        {
            _elementText.text = elementName.ToUpper();
            _elementText.color = targetColor;
        }

        if (_elementBadge != null)
        {
            _elementBadge.color = targetColor;
        }
    }

    private void SetupPassiveEffect(RelicSO relic)
    {
        string title = "Active Bonus";
        string value = string.Empty;

        // Membaca stat yang relevan berdasarkan elemen tempur relik
        switch (relic.element)
        {
            case RelicElement.Fire:
                title = "Global Attack Bonus";
                value = $"+{relic.fireAttackBonusPercent:F1}% ATK";
                break;

            case RelicElement.Earth:
                title = "Base Max HP Bonus";
                value = $"+{relic.earthBaseHpBonus:N0} HP";
                break;

            case RelicElement.Water:
                title = "Enemy Movement Slow";
                value = $"-{relic.waterSlowPercent * 100f:F0}% SPD";
                break;

            case RelicElement.Wind:
                title = "Unit Attack Speed";
                value = $"+{relic.windAtkSpeedBonusPercent:F0}% ASPD";
                break;
        }

        if (_bonusTitleText != null) _bonusTitleText.text = title;
        if (_bonusValueText != null) _bonusValueText.text = value;
    }
}