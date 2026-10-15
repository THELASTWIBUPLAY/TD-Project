using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroAscendSkillItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _starRequirementText; // Contoh: "★ 2"
    [SerializeField] private TextMeshProUGUI _skillNameText;        // Contoh: "Flame Focus"
    [SerializeField] private TextMeshProUGUI _skillDescriptionText; // Rincian stat
    [SerializeField] private GameObject _lockedBadge;               // Tulisan/Ikon Gembok
    [SerializeField] private CanvasGroup _canvasGroup;

    public void Setup(HeroAscendPassive passive, bool isUnlocked)
    {
        if (_starRequirementText != null)
            _starRequirementText.text = $"★ {passive.unlockAtStar}";

        if (_skillNameText != null)
            _skillNameText.text = passive.passiveName;

        if (_skillDescriptionText != null)
            _skillDescriptionText.text = passive.passiveDescription;

        if (_lockedBadge != null)
            _lockedBadge.SetActive(!isUnlocked);

        // Jika skill belum terbuka, redupkan seluruh kartu menjadi 40% alpha
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = isUnlocked ? 1f : 0.4f;
        }
    }
}