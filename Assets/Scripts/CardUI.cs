using UnityEngine;
using TMPro;

public class CardUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public int cardIndex = 0;

    public void Setup(string title, string desc, int index)
    {
        cardIndex = index;
        if (titleText != null) titleText.text = title;
        if (descText != null) descText.text = desc;
    }

    public void OnCardClicked()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.ApplyUpgradeByIndex(cardIndex);
        }
    }
}