using UnityEngine;
using TMPro;

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
        _goldText.text = data.gold.ToString("N0");
        _gemText.text = data.gem.ToString("N0");
        _energyText.text = $"{data.currentEnergy}/{data.maxEnergy}";
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