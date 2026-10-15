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

    // Panels
    [Header("Panels")]
    [SerializeField] private CanvasGroup _mainPanel;
    [SerializeField] private CanvasGroup _levelSelectorPanel;
    [SerializeField] private CanvasGroup _settingPanel;
    [SerializeField] private CanvasGroup _creditsPanel;
    [SerializeField] private CanvasGroup _shopPanel;
    [SerializeField] private CanvasGroup _talentPanel;
    [SerializeField] private CanvasGroup _deckPanel;
    [SerializeField] private CanvasGroup _dungeonPanel;

    public CanvasGroup MainPanel => _mainPanel;
    public CanvasGroup LevelSelectorPanel => _levelSelectorPanel;
    public CanvasGroup SettingsPanel => _settingPanel;
    public CanvasGroup CreditsPanel => _creditsPanel;
    public CanvasGroup ShopPanel => _shopPanel;
    public CanvasGroup TalentPanel => _talentPanel;
    public CanvasGroup DeckPanel => _deckPanel;
    public CanvasGroup DungeonPanel => _dungeonPanel;

    [Header("UI Groups")]
    [SerializeField] private CanvasGroup _bottomMenu;
    [SerializeField] private CanvasGroup _leftMenu;
    [SerializeField] private CanvasGroup _economyBar;
    [SerializeField] private CanvasGroup _profileMenu;

    public CanvasGroup BottomMenu => _bottomMenu;
    public CanvasGroup LeftMenu => _leftMenu;
    public CanvasGroup EconomyBar => _economyBar;
    public CanvasGroup ProfileMenu => _profileMenu;
   
    [Header("UI Dependencies")]
    [SerializeField] private TextMeshProUGUI _gameTitleText;
    [SerializeField] private TextMeshProUGUI _gameVersionText;
    [SerializeField] private Button _playBtn;
    [SerializeField] private Button _settingsBtn;
    [SerializeField] private Button _creditsBtn;
    [SerializeField] private Button _exitBtn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _gameVersionText.text = $"Version {_gameVerMajor}.{_gameVerMinor}.{_gameVerPatches}";

        TogglePanel(_settingPanel, false);
        TogglePanel(_creditsPanel, false);
        TogglePanel(_levelSelectorPanel, false);
        TogglePanel(_shopPanel, false);
        TogglePanel(_talentPanel, false);
        TogglePanel(_deckPanel, false);
        TogglePanel(_dungeonPanel, false);

        TogglePanel(_mainPanel, true);
    }

    public void TogglePanel(CanvasGroup panel, bool isVisible)
    {
        if (panel == null) return;

        panel.alpha = isVisible ? 1f : 0f;
        panel.interactable = isVisible;
        panel.blocksRaycasts = isVisible;

        panel.gameObject.SetActive(isVisible);
    }
}
