using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseHealth : MonoBehaviour
{
    public float maxHp = 100f;
    public float currentHp;

    [Header("UI References")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    void Start()
    {
        currentHp = maxHp;
        UpdateUI();
    }

    public void TakeBaseDamage(float damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);
        UpdateUI();

        if (currentHp <= 0)
        {
            Debug.Log("Game Over!");
        }
    }

    void UpdateUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHp} / {maxHp}";
        }
    }
}