using UnityEngine;
using UnityEngine.UI;

public class BottomMenuController : MonoBehaviour
{
    [SerializeField] private Button _shopBtn;
    [SerializeField] private Button _deckBtn;
    [SerializeField] private Button _homeBtn;
    [SerializeField] private Button _talentBtn;
    [SerializeField] private Button _dungeonBtn;

    private void OnEnable()
    {
        AddButtonListener();
    }

    private void OnDisable()
    {
        RemoveButtonListener();
    }

    private void AddButtonListener()
    {
        _shopBtn.onClick.AddListener(OpenShopMenu);   
        _deckBtn.onClick.AddListener(OpenDeckMenu);   
        _homeBtn.onClick.AddListener(OpenHomeMenu);   
        _talentBtn.onClick.AddListener(OpenTalentMenu);   
        _dungeonBtn.onClick.AddListener(OpenDungeonMenu);   
    }

    private void RemoveButtonListener()
    {
        _shopBtn.onClick.RemoveListener(OpenShopMenu);
        _deckBtn.onClick.RemoveListener(OpenDeckMenu);
        _homeBtn.onClick.RemoveListener(OpenHomeMenu);
        _talentBtn.onClick.RemoveListener(OpenTalentMenu);
        _dungeonBtn.onClick.RemoveListener(OpenDungeonMenu);
    }

    // OnClick
    private void OpenShopMenu()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LevelSelectorPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ShopPanel, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DeckPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DungeonPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.TalentPanel, false);
        }
    }

    private void OpenDeckMenu()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LevelSelectorPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DeckPanel, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ShopPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DungeonPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.TalentPanel, false);
        }
    }

    private void OpenHomeMenu()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LevelSelectorPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ShopPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DeckPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DungeonPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.TalentPanel, false);
        }
    }

    private void OpenTalentMenu()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LevelSelectorPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.TalentPanel, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ShopPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DeckPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DungeonPanel, false);

            FindFirstObjectByType<TalentUI>()?.RefreshAllUI();
        }
    }

    private void OpenDungeonMenu()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LevelSelectorPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DungeonPanel, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ShopPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.DeckPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.TalentPanel, false);
        }
    }
}