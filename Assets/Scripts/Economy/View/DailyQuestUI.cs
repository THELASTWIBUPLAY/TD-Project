using UnityEngine;

public class DailyQuestUI : MonoBehaviour
{
    [SerializeField] private DailyQuestSlotUI[] _slots;
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

        var questList = DailyQuestManager.Instance.QuestDatabase.Quests;

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
    }

    public void OpenDailyPanel()
    {
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