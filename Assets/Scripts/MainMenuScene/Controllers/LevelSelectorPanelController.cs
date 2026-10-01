using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class LevelSelectorController : MonoBehaviour
{
    private CanvasGroup _myPanel;

    private void Awake()
    {
        _myPanel = GetComponent<CanvasGroup>();
    }
        
    public void OnBackButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.BottomMenu, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LeftMenu, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ProfileMenu, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.EconomyBar, true);
        }
    }

    public void OnEndlessButtonClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
