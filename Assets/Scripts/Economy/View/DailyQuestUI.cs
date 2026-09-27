using Assets.Scripts.Economy.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuestUI : MonoBehaviour
{
    [Header("Quest List Slots")]
    [SerializeField] private DailyQuestSlotUI[] _slots;

    [Header("Milesone Progress Bar")]
    [SerializeField] private Slider _milestoneSlider;
    [SerializeField] private TextMeshProUGUI _milestonePointsText;
    [SerializeField] private DailyQuestMilestoneSlotUI[] _milestoneSlots;

    private CanvasGroup _myCanvas;

    private void Awake()
    {
        _myCanvas = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        RefreshUI();
    }

    private void OnEnable()
    {
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.OnQuestUpdated += RefreshUI;
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.OnQuestUpdated -= RefreshUI;
        }
    }

    private void RefreshUI()
    {
        if (DailyQuestManager.Instance == null || DailyQuestManager.Instance.QuestDatabase == null)
            return;

        var db = DailyQuestManager.Instance.QuestDatabase;

        // Quest
        var questList = db.Quests;
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null)
            {
                Debug.LogError($"[DailyQuestUI] Slot pada index {i} belum di-drag ke array _slots di Inspector!");
                continue;
            }

            if (i < questList.Count)
            {
                _slots[i].gameObject.SetActive(true);

                var quest = questList[i];
                int progress = DailyQuestManager.Instance.GetProgress(quest.questID);
                var state = DailyQuestManager.Instance.GetQuestState(quest);

                _slots[i].Setup(quest, progress, state);
            }
            else
            {
                _slots[i].gameObject.SetActive(false);
            }
        }

        // Milestone
        int currentPoints = DailyQuestManager.Instance.CurrentMilestonePoints;
        int maxPoints = db.GetMaxMilestonePoints();

        if (_milestoneSlider != null)
        {
            _milestoneSlider.maxValue = maxPoints;
            _milestoneSlider.value = currentPoints;
        }

        if (_milestonePointsText != null)
        {
            _milestonePointsText.text = $"{currentPoints} / {maxPoints}";
        }

        // refresh chest milestones
        var milestoneList = db.Milestones;
        for (int i = 0; i < _milestoneSlots.Length; i++)
        {
            if (_milestoneSlots[i] == null) continue;

            if (i < milestoneList.Count)
            {
                _milestoneSlots[i].gameObject.SetActive(true);
                var tier = milestoneList[i];
                var state = DailyQuestManager.Instance.GetMilestoneState(tier);
                _milestoneSlots[i].Setup(tier, state);
            }
            else
            {
                _milestoneSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void OpenDailyPanel()
    {
        DailyQuestManager.Instance?.CheckDailyReset();

        RefreshUI();
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myCanvas, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, false);
        }
    }

    public void CloseDailyPanel()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myCanvas, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, true);
        }
    }
}