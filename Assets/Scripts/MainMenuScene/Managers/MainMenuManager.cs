using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    [Header("Game Version (Format: a.b.c - Ex: 1.3.21)")]
    [SerializeField] private string _gameVerMajor;
    [SerializeField] private string _gameVerMinor;
    [SerializeField] private string _gameVerPatches;

    [Header("UI Settings")]
    [SerializeField] private string _gameTitle;

    // Panels
    [Header("Panels")]
    [SerializeField] private CanvasGroup _mainPanel;
    [SerializeField] private CanvasGroup _levelSelectorPanel;
    [SerializeField] private CanvasGroup _loadPanel;
    [SerializeField] private CanvasGroup _settingPanel;
    [SerializeField] private CanvasGroup _creditsPanel;

    public CanvasGroup MainPanel => _mainPanel;
    public CanvasGroup LevelSelectorPanel => _levelSelectorPanel;
    public CanvasGroup LoadPanel => _loadPanel;
    public CanvasGroup SettingsPanel => _settingPanel;
    public CanvasGroup CreditsPanel => _creditsPanel;

    // UI Dependencies
    [Header("UI Dependencies")]
    [SerializeField] private TextMeshProUGUI _gameTitleText;
    [SerializeField] private TextMeshProUGUI _gameVersionText;
    [SerializeField] private Button _playBtn;
    [SerializeField] private Button _loadBtn;
    [SerializeField] private Button _settingsBtn;
    [SerializeField] private Button _creditsBtn;
    [SerializeField] private Button _exitBtn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        _gameTitleText.text = _gameTitle;
        _gameVersionText.text = $"Version {_gameVerMajor}.{_gameVerMinor}.{_gameVerPatches}";
    }

    public void TogglePanel(CanvasGroup panel, bool isVisible)
    {
        panel.alpha = isVisible ? 1 : 0;
        panel.interactable = isVisible;
        panel.blocksRaycasts = isVisible;
    }
}
