using UnityEngine;
using UnityEngine.UI;

public class MainPanelController : MonoBehaviour
{
    [SerializeField] private Button _battleBtn;
    private CanvasGroup _myPanel;

    private void Awake()
    {
        _myPanel = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        _battleBtn.onClick.AddListener(OpenLevelSelector);
    }

    private void OnDisable()
    {
        _battleBtn.onClick.RemoveListener(OpenLevelSelector);
    }

    public void OnSettingsButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.SettingsPanel, true);
        }
    }

    private void OpenLevelSelector()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.BottomMenu, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LeftMenu, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ProfileMenu, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.EconomyBar, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LevelSelectorPanel, true);
        }
    }
}
