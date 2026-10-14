using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayConfirmationUI : MonoBehaviour
{
    private enum ConfirmationMode { None, Dungeon, Level }
    private ConfirmationMode _currentMode = ConfirmationMode.None;

    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _energyCostText;
    //[SerializeField] private RewardsSlotUI[] rewardsSlots;
    [SerializeField] private Button _playBtn;
    [SerializeField] private Button _backBtn;

    private string _dungeonName;
    private string _sceneName;
    private int _energyCost;

    private string _stageName;
    private string _levelSceneName;

    private void OnEnable()
    {
        if (_playBtn != null) _playBtn.interactable = true;

        _playBtn.onClick.AddListener(OnPlayButtonClicked);
        _backBtn.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnDisable()
    {
        _playBtn.onClick.RemoveListener(OnPlayButtonClicked);
        _backBtn.onClick.RemoveListener(OnBackButtonClicked);
    }

    public void SetupDungeon(DungeonData data)
    {
        _currentMode = ConfirmationMode.Dungeon;

        _energyCost = data.energyCost;
        _dungeonName = data.dungeonName;
        _sceneName = data.dungeonScene;

        if (_energyCostText != null)
            _energyCostText.text = data.energyCost.ToString();

        if (_titleText != null)
            _titleText.text = data.dungeonName;
    }

    public void SetupLevel(StageConfig data)
    {
        _currentMode = ConfirmationMode.Level;

        _energyCost = data.energyCost;
        _stageName = data.stageName;
        _levelSceneName = $"Level {data.stageLevel}";

        if (_energyCostText != null)
            _energyCostText.text = data.energyCost.ToString();

        if (_titleText != null)
            _titleText.text = data.stageName;
    }

    private void OnPlayButtonClicked()
    {
        switch (_currentMode)
        {
            case ConfirmationMode.Dungeon:
                if (string.IsNullOrEmpty(_sceneName))
                {
                    Debug.LogWarning("Scene name isnt registered in DungeonDB");
                    return;
                }
                LoadDungeonScene();
                break;

            case ConfirmationMode.Level:
                if (string.IsNullOrEmpty(_levelSceneName))
                {
                    Debug.LogWarning("Stage level isnt registered in StageConfig");
                    return;
                }
                LoadLevelScene();
                break;

            default:
                Debug.LogWarning("Play Confirmation opened without proper Setup!");
                break;
        }
    }

    private void LoadDungeonScene()
    {
        if (!HasEnoughEnergy()) return;

        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded(_sceneName))
        {
            ProcessEnergyAndSave();
            SceneManager.LoadScene(_sceneName);
        }
        else
        {
            AlertManager.Instance?.Show($"Dungeon {_dungeonName} is not available");
            Debug.LogWarning($"Scene {_sceneName} cannot be loaded. Ensure it is added to Build Settings.");
        }
    }

    private void LoadLevelScene()
    {
        if (!HasEnoughEnergy()) return;

        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded(_levelSceneName))
        {
            ProcessEnergyAndSave();
            SceneManager.LoadScene(_levelSceneName);
        }
        else
        {
            AlertManager.Instance?.Show($"Level {_levelSceneName} is not available");
            Debug.LogWarning($"Scene {_levelSceneName} cannot be loaded. Ensure it is added to Build Settings.");
        }
    }

    private bool HasEnoughEnergy()
    {
        if (EconomyManager.Instance != null && EconomyManager.Instance.CurrentData.currentEnergy < _energyCost)
        {
            AlertManager.Instance?.Show("Not enough energy!");
            return false;
        }
        return true;
    }

    private void ProcessEnergyAndSave()
    {
        EconomyManager.Instance?.ModifyEnergy(-_energyCost);
        DailyQuestManager.Instance?.AddProgress(DailyQuestType.SpendEnergy, _energyCost);

        if (_playBtn != null) _playBtn.interactable = false;

        SaveManager.Instance?.SaveLocal();
    }

    private void OnBackButtonClicked()
    {
        gameObject.SetActive(false);
    }
}