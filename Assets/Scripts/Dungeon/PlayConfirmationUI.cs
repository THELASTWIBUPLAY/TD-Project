using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayConfirmationUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _energyCostText;
    //[SerializeField] private RewardsSlotUI[] rewardsSlots;
    [SerializeField] private Button _playBtn;
    [SerializeField] private Button _backBtn;

    private string _dungeonName;
    private string _sceneName;
    private int _energyCost;

    private void OnEnable()
    {
        _playBtn.onClick.AddListener(OnPlayButtonClicked);
        _backBtn.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnDisable()
    {
        _playBtn.onClick.RemoveListener(OnPlayButtonClicked);
        _backBtn.onClick.RemoveListener(OnBackButtonClicked);
    }

    public void Setup(DungeonData data)
    {
        _energyCost = data.energyCost;
        _dungeonName = data.dungeonName;
        _sceneName = data.dungeonScene;

        if (_energyCostText != null)
            _energyCostText.text = data.energyCost.ToString();

        if (_titleText != null)
            _titleText.text = data.dungeonName;
    }

    private void OnPlayButtonClicked()
    {
        if (string.IsNullOrEmpty(_sceneName))
        {
            Debug.LogWarning("Scene name isnt registered in DungeonDB");
            return;
        }

        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded(_sceneName))
        {
            if (EconomyManager.Instance != null && EconomyManager.Instance.CurrentData.currentEnergy < _energyCost)
            {
                AlertManager.Instance.Show("Not enough energy!");
                return;
            }
            EconomyManager.Instance?.ModifyEnergy(-_energyCost);
            DailyQuestManager.Instance?.AddProgress(DailyQuestType.SpendEnergy, _energyCost);

            if (_playBtn != null) _playBtn.interactable = false;

            SaveManager.Instance?.SaveLocal();
            SceneManager.LoadScene(_sceneName);
        }
        else
        {
            AlertManager.Instance.Show($"Dungeon {_dungeonName} is not available");
            Debug.LogWarning($"Scene {_sceneName} cannot be loaded. Please check the scene name and ensure it is added to the build settings.");
        }
    }

    private void OnBackButtonClicked()
    {
        gameObject.SetActive(false);
    }
}