using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI References")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;

    private bool isGameOverTriggered = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeBaseDamage(float damage)
    {
        if (isGameOverTriggered) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateUI();

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.18f, 0.12f); 
        }

        if (currentHealth <= 0)
        {
            isGameOverTriggered = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }

    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {maxHealth}";
        }
    }

    public void HealBase(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
    }
}