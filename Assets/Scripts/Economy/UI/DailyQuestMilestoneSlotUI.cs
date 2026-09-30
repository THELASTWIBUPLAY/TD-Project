using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Economy.View
{
    public class DailyQuestMilestoneSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _pointText;
        [SerializeField] private Button _chestButton;
        [SerializeField] private Image _chestIcon;
        [SerializeField] private GameObject _highlightGlow;
        [SerializeField] private GameObject _claimedMark;

        private QuestMilestoneTier _currentTier;

        private void Awake()
        {
            if (_chestButton != null)
            {
                _chestButton.onClick.AddListener(OnChestClicked);
            }
        }

        private void OnDestroy()
        {
            if (_chestButton != null)
            {
                _chestButton.onClick.RemoveListener(OnChestClicked);
            }
        }

        public void Setup(QuestMilestoneTier tier, MilestoneState state)
        {
            _currentTier = tier;

            if (_pointText != null)
                _pointText.text = tier.requiredPoints.ToString();
            if (_chestIcon != null)
                _chestIcon.sprite = tier.icon;

            switch (state)
            {
                case MilestoneState.Locked:
                    _chestButton.interactable = false;
                    if (_highlightGlow != null) _highlightGlow.SetActive(false);
                    if (_claimedMark != null) _claimedMark.SetActive(false);
                    break;

                case MilestoneState.ReadyToClaim:
                    _chestButton.interactable = true;
                    if (_highlightGlow != null) _highlightGlow.SetActive(true);
                    if (_claimedMark != null) _claimedMark.SetActive(false);
                    break;

                case MilestoneState.Claimed:
                    _chestButton.interactable = false;
                    if (_highlightGlow != null) _highlightGlow.SetActive(false);
                    if (_claimedMark != null) _claimedMark.SetActive(true);
                    break;
            }
        }

        private void OnChestClicked()
        {
            DailyQuestManager.Instance?.ClaimMilestone(_currentTier);
        }
    }
}