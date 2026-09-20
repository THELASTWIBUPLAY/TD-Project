using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        }
    }

    public void OnEndlessButtonClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
