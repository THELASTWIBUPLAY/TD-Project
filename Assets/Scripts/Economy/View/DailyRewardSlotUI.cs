using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyRewardSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private TextMeshProUGUI _rewardAmountText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private GameObject _claimedCheckmark; // Gambar centang
    [SerializeField] private GameObject _highlightBorder;   // Efek border menyala jika ready

    public void Setup(DailyRewardTier tier, DailyRewardViewModel.SlotState state)
    {
        _dayText.text = $"Day {tier.day}";

        // Format teks hadiah
        if (tier.gold > 0 && tier.gem > 0)
            _rewardAmountText.text = $"{tier.gold}G + {tier.gem}D";
        else if (tier.gold > 0)
            _rewardAmountText.text = $"{tier.gold} Gold";
        else
            _rewardAmountText.text = $"{tier.gem} Gem";

        if (tier.rewardIcon != null)
            _iconImage.sprite = tier.rewardIcon;

        // Visual State
        _claimedCheckmark.SetActive(state == DailyRewardViewModel.SlotState.Claimed);
        _highlightBorder.SetActive(state == DailyRewardViewModel.SlotState.ReadyToClaim);
    }
}