using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TalentRewardSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _requiredLevelText;
    [SerializeField] private Button _claimButton;
    [SerializeField] private GameObject _highlightGlow;
    [SerializeField] private GameObject _claimedMark;

    private int _tierIndex;

    private void Awake()
    {
        if (_claimButton != null)
        {
            _claimButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (_claimButton != null)
        {
            _claimButton.onClick.RemoveListener(OnButtonClicked);
        }
    }

    public void Setup(int tierIndex, int requiredLevel, TalentManager.TalentRewardState state)
    {
        _tierIndex = tierIndex;

        if (_requiredLevelText != null)
            _requiredLevelText.text = requiredLevel.ToString();

        ApplyVisualState(state);
    }

    public void ApplyVisualState(TalentManager.TalentRewardState state)
    {
        switch (state)
        {
            case TalentManager.TalentRewardState.Progress:
                if (_claimButton != null) _claimButton.interactable = false;
                if (_highlightGlow != null) _highlightGlow.SetActive(false);
                if (_claimedMark != null) _claimedMark.SetActive(false);
                break;

            case TalentManager.TalentRewardState.ReadyToClaim:
                if (_claimButton != null) _claimButton.interactable = true;
                if (_highlightGlow != null) _highlightGlow.SetActive(true);
                if (_claimedMark != null) _claimedMark.SetActive(false);
                break;

            case TalentManager.TalentRewardState.Claimed:
                if (_claimButton != null) _claimButton.interactable = false;
                if (_highlightGlow != null) _highlightGlow.SetActive(false);
                if (_claimedMark != null) _claimedMark.SetActive(true);
                break;
        }
    }

    private void OnButtonClicked()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        TalentManager.Instance?.ClaimReward(_tierIndex);
    }
}