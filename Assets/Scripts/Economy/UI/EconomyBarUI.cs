using UnityEngine;
using TMPro;
using System.Globalization;
public class EconomyBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _gemText;
    [SerializeField] private TextMeshProUGUI _energyText;
    [SerializeField] private TextMeshProUGUI _energyCountdownText;

    private void Start()
    {
        // Guarantee the EconomyManager.Instance is awake before being used
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnEconomyChanged += UpdateDisplay;
            UpdateDisplay(EconomyManager.Instance.CurrentData);
        }

        if (EnergyRegenManager.Instance != null)
        {
            EnergyRegenManager.Instance.OnCountdownChanged += UpdateCountdown;
        }
    }
    
    // Using Start as subs need OnDestroy as unsubs
    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnEconomyChanged -= UpdateDisplay;
        }

        if (EnergyRegenManager.Instance != null)
        {
            EnergyRegenManager.Instance.OnCountdownChanged -= UpdateCountdown;
        }
    }

    private void UpdateDisplay(EconomyManager.EconomyData data)
    {
        _goldText.text = FormatBigNumber(data.gold);
        _gemText.text = FormatBigNumber(data.gem);
        _energyText.text = $"{data.currentEnergy}/{data.maxEnergy}";
    }

    private string FormatBigNumber(long number)
    {
        if (number >= 1000000000) // 1 Miliar+
        {
            return (number / 1000000000f).ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + "B";
        }
        if (number >= 1000000) // 1 Juta+
        {
            return (number / 1000000f).ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + "M";
        }
        if (number >= 1000) // 1 Ribu+
        {
            return (number / 1000f).ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + "K";
        }

        return number.ToString(); // Di bawah 1000 tetap tampil angka biasa
    }

    private void UpdateCountdown(int seconds)
    {
        int minute = seconds / 60;
        int second = seconds % 60;

        string countdown = string.Format("{0:D2}:{1:D2}", minute, second);

        if (seconds == -1) countdown = "Full";

        _energyCountdownText.text = countdown;
    }

    // OnClick
    public void Buy()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ShopPanel, true);
        }
    }
}