using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffItemUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image buffIcon;
    public TMP_Text titleText;
    public TMP_Text descText;

    [Header("Star System (Dynamic Spawn)")]
    public Transform starContainer; 
    public GameObject starPrefab;    

    /// <summary>
    /// Setup data UI Buff dan spawn bintang sesuai level
    /// </summary>
    public void SetupBuff(Sprite icon, string title, string desc, int starLevel)
    {
        if (buffIcon != null) buffIcon.sprite = icon;
        if (titleText != null) titleText.text = title;
        if (descText != null) descText.text = desc;

        foreach (Transform child in starContainer)
        {
            Destroy(child.gameObject);
        }

        if (starPrefab != null && starContainer != null)
        {
            for (int i = 0; i < starLevel; i++)
            {
                Instantiate(starPrefab, starContainer);
            }
        }
    }
}