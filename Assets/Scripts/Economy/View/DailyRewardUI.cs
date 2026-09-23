using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyRewardUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DailyRewardViewModel _viewModel;
    [SerializeField] private DailyRewardSlotUI[] _slots; // Array 7 slot di Canvas

    [Header("UI Elements")]
    [SerializeField] private CanvasGroup _myCanvas;
    [SerializeField] private Button _claimButton;
    [SerializeField] private TextMeshProUGUI _claimButtonText;
    [SerializeField] private TextMeshProUGUI _cooldownTimerText;

    private void Awake()
    {
        _myCanvas = GetComponentInChildren<CanvasGroup>();
    }

    private void OnEnable()
    {
        _viewModel.OnDataInitialized += RefreshUI;
        _viewModel.OnRewardClaimedSuccess += HandleRewardSuccess;
        _viewModel.OnErrorMessage += HandleError;
        _viewModel.OnCountdownUpdated += HandleCountdownUpdated;

        _claimButton.onClick.AddListener(_viewModel.ClaimRewardCommand);
    }

    private void OnDisable()
    {
        _viewModel.OnDataInitialized -= RefreshUI;
        _viewModel.OnRewardClaimedSuccess -= HandleRewardSuccess;
        _viewModel.OnErrorMessage -= HandleError;
        _viewModel.OnCountdownUpdated -= HandleCountdownUpdated;

        _claimButton.onClick.RemoveListener(_viewModel.ClaimRewardCommand);
    }

    private void RefreshUI()
    {
        var schedule = _viewModel.Schedule;
        if (schedule == null) return;

        for (int i = 0; i < _slots.Length && i < schedule.Rewards.Count; i++)
        {
            var tier = schedule.Rewards[i];
            var state = _viewModel.GetSlotState(tier.day);
            _slots[i].Setup(tier, state);
        }

        bool canClaim = _viewModel.CanClaimToday;
        _claimButton.interactable = canClaim;

        if (canClaim)
        {
            _claimButtonText.text = "CLAIM";
            if (_cooldownTimerText != null) _cooldownTimerText.gameObject.SetActive(false);
        }
        else
        {
            _claimButtonText.text = "LOCKED";
            if (_cooldownTimerText != null) _cooldownTimerText.gameObject.SetActive(true);
        }
    }
    private void HandleCountdownUpdated(string timeFormatted)
    {
        if (_cooldownTimerText != null && !_viewModel.CanClaimToday)
        {
            _cooldownTimerText.text = $"Next Reward in: {timeFormatted}";
        }
    }

    private void HandleRewardSuccess(DailyRewardTier tier)
    {
        AlertManager.Instance?.Show($"Claimed day {tier.day}!");
    }

    private void HandleError(string message)
    {
        AlertManager.Instance?.Show(message);
    }

    public void OpenDailyPanel()
    {
        MainMenuManager.Instance.TogglePanel(_myCanvas, true);
        MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, false);
    }

    public void CloseDailyPanel()
    {
        MainMenuManager.Instance.TogglePanel(_myCanvas, false);
        MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, true);
    }
}