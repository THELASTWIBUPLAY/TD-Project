using UnityEngine;
using UnityEngine.UI;

public class MainPanelController : MonoBehaviour
{
    [SerializeField] private Button _battleBtn;
    private CanvasGroup _myPanel;

    private void Awake()
    {
        // Ambil CanvasGroup di objek ini jika ada, sebagai fallback
        _myPanel = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (_battleBtn != null)
        {
            _battleBtn.onClick.AddListener(OpenLevelSelector);
        }
        else
        {
            Debug.LogWarning("[MainPanelController] Slot '_battleBtn' di Inspector belum dihubungkan!");
        }
    }

    private void OnDisable()
    {
        if (_battleBtn != null)
        {
            _battleBtn.onClick.RemoveListener(OpenLevelSelector);
        }
    }

    public void OnSettingsButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            CanvasGroup panelToHide = _myPanel != null ? _myPanel : MainMenuManager.Instance.MainPanel;
            MainMenuManager.Instance.TogglePanel(panelToHide, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.SettingsPanel, true);
        }
    }

    public void OpenLevelSelector()
    {
        Debug.Log("[Battle Click] Tombol Battle ditekan, membuka Level Selector...");

        if (MainMenuManager.Instance == null)
        {
            Debug.LogError("[MainPanelController] MainMenuManager.Instance tidak ditemukan!");
            return;
        }

        CanvasGroup panelToHide = _myPanel != null ? _myPanel : MainMenuManager.Instance.MainPanel;
        MainMenuManager.Instance.TogglePanel(panelToHide, false);
        MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LevelSelectorPanel, true);
        MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.BottomMenu, true);
        MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LeftMenu, true);
        MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ProfileMenu, true);
        MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.EconomyBar, true);
    }
}