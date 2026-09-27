using Unity.Microsoft.GDK;
using UnityEngine;

public class MainPanelController : MonoBehaviour
{
    [SerializeField] private CreditsPanelController _creditsPanelController;
    private CanvasGroup _myPanel;

    private void Awake()
    {
        _myPanel = GetComponent<CanvasGroup>();
    }

    public void OnHomeButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ShopPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.HeroPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DungeonPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.BasePanel, false);
        }
    }

    public void OnPlayButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LevelSelectorPanel, true);
        }
    }

    public void OnShopButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ShopPanel, true);
        }
    }

    public void OnBaseButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.BasePanel, true);
        }
    }

    public void OnHeroButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.HeroPanel, true);
        }
    }

    public void OnDungeonButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DungeonPanel, true);
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
