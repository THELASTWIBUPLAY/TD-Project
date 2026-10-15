using UnityEngine;
using UnityEngine.UI;

public class BottomMenuController : MonoBehaviour
{
    [SerializeField] private Button _shopBtn;
    [SerializeField] private Button _deckBtn;
    [SerializeField] private Button _homeBtn;
    [SerializeField] private Button _talentBtn;
    [SerializeField] private Button _dungeonBtn;

    [Header("Tab visuals (Shop, Deck, Home, Talent, Dungeon)")]
    [SerializeField] private RectTransform[] _tabVisuals;
    [SerializeField, Range(0f, 0.9f)] private float _inactiveDropFraction = 0.25f;
    [SerializeField, Range(0f, 1f)] private float _inactiveBrightness = 0.65f;
    private UnityEngine.UI.Image[] _tabImages;
    private Color[] _tabColors;

    private void Awake()
    {
        if (_tabVisuals == null) return;
        _tabImages = new UnityEngine.UI.Image[_tabVisuals.Length];
        _tabColors = new Color[_tabVisuals.Length];
        for (int i = 0; i < _tabVisuals.Length; i++)
        {
            if (!_tabVisuals[i]) continue;
            _tabImages[i] = _tabVisuals[i].GetComponent<UnityEngine.UI.Image>();
            if (_tabImages[i]) _tabColors[i] = _tabImages[i].color;
        }
    }

    private void LateUpdate()
    {
        var menu = MainMenuManager.Instance;
        if (menu == null || _tabVisuals == null) return;
        // Read the visible page so shortcuts (such as the currency bar) stay in sync too.
        int selected = Visible(menu.ShopPanel) ? 0 : Visible(menu.DeckPanel) ? 1 :
            Visible(menu.TalentPanel) ? 3 : Visible(menu.DungeonPanel) ? 4 : 2;
        for (int i = 0; i < _tabVisuals.Length; i++)
        {
            var visual = _tabVisuals[i];
            if (!visual) continue;
            var position = visual.anchoredPosition;
            position.y = i == selected ? 0f : -visual.rect.height * _inactiveDropFraction;
            visual.anchoredPosition = position;
            if (_tabImages != null && i < _tabImages.Length && _tabImages[i])
            {
                Color color = _tabColors[i];
                float brightness = i == selected ? 1f : _inactiveBrightness;
                _tabImages[i].color = new Color(color.r * brightness, color.g * brightness,
                    color.b * brightness, color.a);
            }
        }
    }

    private static bool Visible(CanvasGroup panel) => panel && panel.gameObject.activeInHierarchy && panel.alpha > 0.5f;

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
