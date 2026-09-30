using UnityEngine;
using TMPro;

public class EconomyBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _gemText;
    [SerializeField] private TextMeshProUGUI _energyText;

    private void Start()
    {
        // Guarantee the EconomyManager.Instance is awake before being used
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnEconomyChanged += UpdateDisplay;
            UpdateDisplay(EconomyManager.Instance.CurrentData);
        }
    }
    
    // Using Start as subs need OnDestroy as unsubs
    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnEconomyChanged -= UpdateDisplay;
        }
    }

    private void UpdateDisplay(EconomyManager.EconomyData data)
    {
        _goldText.text = data.gold.ToString("N0");
        _gemText.text = data.gem.ToString("N0");
        _energyText.text = $"{data.currentEnergy}/{data.maxEnergy}";
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
