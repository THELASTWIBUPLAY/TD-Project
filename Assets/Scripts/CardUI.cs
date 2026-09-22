using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;
    public Button actionButton;

    [Header("Star System")]
    public Transform starContainer;
    public Image[] starImages;

    private Action onCardClicked;

    void Awake()
    {
        if (actionButton == null) actionButton = GetComponent<Button>();
    }

    // Menggunakan optional parameter (= 1) agar pemanggilan lama tidak error CS7036
    public void SetupCard(string title, string desc, Sprite icon, int requiredStars, Action onClickAction)
    {
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = desc;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }

        UpdateStars(requiredStars);

        onCardClicked = onClickAction;

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => {
                Debug.Log($"[CardUI] Kartu ditekan: {title}");
                onCardClicked?.Invoke();
            });
        }
    }

    private void UpdateStars(int count)
    {
        if (starImages != null && starImages.Length > 0)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].gameObject.SetActive(i < count);
                }
            }
        }
    }
}