using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image classIconImage;
    public TextMeshProUGUI classNameText;
    public TextMeshProUGUI unitDetailText;
    public Image damageFillBar;
    public TextMeshProUGUI damageText;

    public void Setup(Sprite icon, string className, string detail, float damageValue, float fillPercent)
    {
        if (classIconImage != null && icon != null) classIconImage.sprite = icon;
        if (classNameText != null) classNameText.text = className;
        if (unitDetailText != null) unitDetailText.text = detail;
        
        if (damageText != null) 
            damageText.text = $"{GameManager.FormatNumber(damageValue)} ({fillPercent * 100f:F1}%)";
            
        if (damageFillBar != null) damageFillBar.fillAmount = fillPercent;
    }
}