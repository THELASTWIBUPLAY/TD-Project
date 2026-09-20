using UnityEngine;

public class MainPanelController : MonoBehaviour
{
    [SerializeField] private CreditsPanelController _creditsPanelController;
    private CanvasGroup _myPanel;

    private void Awake()
    {
        _myPanel = GetComponent<CanvasGroup>();
    }

    public void OnPlayButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LevelSelectorPanel, true);
        }
    }
    public void OnLoadButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LoadPanel, true);
        }
    }

    public void OnSettingsButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.SettingsPanel, true);
        }
    }

    public void OnCreditsButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.CreditsPanel, true);
            _creditsPanelController.StartScroll();
        }
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
