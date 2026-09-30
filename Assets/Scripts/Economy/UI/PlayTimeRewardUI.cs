using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class PlayTimeRewardUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayTimeRewardController _controller;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private Button _claimBtn;

    // Dirty Checking
    private int _lastDisplayedSecond = -1;

    private void OnEnable()
    {
        _controller.OnRewardReady += HandleRewardReady;
        _controller.OnRewardClaimed += HandleRewardClaimed;

        _claimBtn.onClick.AddListener(_controller.ClaimReward);
    }

    private void OnDisable()
    {
        _controller.OnRewardReady -= HandleRewardReady;
        _controller.OnRewardClaimed -= HandleRewardClaimed;

        _claimBtn.onClick.RemoveListener(_controller.ClaimReward);
    }

    private void Start()
    {
        RefreshUIState();
    }

    private void Update()
    {
        if (_controller.CurrentState != PlayTimeRewardController.RewardState.CountingDown)
            return;

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int currentSecond = Mathf.CeilToInt(_controller.RemainingSeconds);

        if (currentSecond == _lastDisplayedSecond) return;

        _lastDisplayedSecond = currentSecond;

        int minutes = currentSecond / 60;
        int seconds = currentSecond % 60;

        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Event Listener
    private void HandleRewardReady()
    {
        RefreshUIState();
    }

    private void HandleRewardClaimed()
    {
        _lastDisplayedSecond = -1;
        RefreshUIState();
    }

    private void RefreshUIState()
    {
        bool isReady = _controller.CurrentState == PlayTimeRewardController.RewardState.ReadyToClaim;

        _claimBtn.interactable = isReady;

        if (isReady)
        {
            _timerText.text = "CLAIM!";
        }
        else
        {
            UpdateTimerDisplay();
        }
    }
}