using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyQuestSlotUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _layoutCanvas;
    [SerializeField] private TextMeshProUGUI _questName;
    [SerializeField] private TextMeshProUGUI _questTarget;
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private Button _claimBtn;
    [SerializeField] private GameObject _questCheckmark;

    private DailyQuestData _currentData;

    private void Awake()
    {
        _claimBtn.onClick.AddListener(OnClaimClicked);
    }

    private void OnDestroy()
    {
        _claimBtn.onClick.RemoveListener(OnClaimClicked);
    }

    public void Setup(DailyQuestData data, int currentProgress, DailyQuestState currentState)
    {
        _currentData = data;

        _questName.text = data.questName;
        _questTarget.text = $"{currentProgress} / {data.targetGoal}";

        _progressSlider.value = Mathf.Clamp01((float)currentProgress / data.targetGoal);

        switch (currentState)
        {
            case DailyQuestState.Progress:
                _claimBtn.gameObject.SetActive(false);
                _questCheckmark.SetActive(false);
                _progressSlider.gameObject.SetActive(true);
                break;

            case DailyQuestState.Clear:
                _claimBtn.gameObject.SetActive(true);
                _claimBtn.interactable = true;
                _questCheckmark.SetActive(false);
                _progressSlider.gameObject.SetActive(false);
                break;
            case DailyQuestState.Claimed:
                _claimBtn.gameObject.SetActive(false);
                _questCheckmark.SetActive(true);
                _progressSlider.gameObject.SetActive(false);
                break;
        }
    }

    private void OnClaimClicked()
    {
        DailyQuestManager.Instance?.ClaimQuest(_currentData);
    }
}