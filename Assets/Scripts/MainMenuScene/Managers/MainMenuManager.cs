using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("Game Version (Format: a.b.c - Ex: 1.3.21)")]
    [SerializeField] private string _gameVerMajor = "1";
    [SerializeField] private string _gameVerMinor = "0";
    [SerializeField] private string _gameVerPatches = "0";
    [SerializeField] private TextMeshProUGUI _gameVersionText;

    // =========================================================================
    // 1. UI REFERENCES (TERSTRUKTUR PER LAYER)
    // =========================================================================

    [Header("Persistent HUD")]
    [SerializeField] private CanvasGroup _economyBar;
    [SerializeField] private CanvasGroup _profileBar;
    [SerializeField] private CanvasGroup _leftMenu;
    [SerializeField] private CanvasGroup _bottomMenu;

    [Header("Fullscreen Pages (Hanya 1 Aktif di Satu Waktu)")]
    [SerializeField] private CanvasGroup _mainScreen;
    [SerializeField] private CanvasGroup _levelSelectorScreen;
    [SerializeField] private CanvasGroup _shopScreen;
    [SerializeField] private CanvasGroup _deckScreen;
    [SerializeField] private CanvasGroup _talentScreen;
    [SerializeField] private CanvasGroup _dungeonScreen;

    [Header("Pop Ups (Memicu Dark BG)")]
    [SerializeField] private CanvasGroup _mail;
    [SerializeField] private CanvasGroup _settings;
    [SerializeField] private CanvasGroup _achievement;
    [SerializeField] private CanvasGroup _dailyAttendance;
    [SerializeField] private CanvasGroup _dailyQuest;
    [SerializeField] private CanvasGroup _profileCard;

    [Header("System Overlays (Sub-Modal di Atas Popup/Screen)")]
    [SerializeField] private CanvasGroup _heroCard;
    [SerializeField] private CanvasGroup _relicCard;
    [SerializeField] private CanvasGroup _rewardClaim;
    [SerializeField] private CanvasGroup _editProfile;
    [SerializeField] private CanvasGroup _credits;

    [Header("Dynamic Backgrounds")]
    [Tooltip("Background gelap di belakang Pop-up / Modal")]
    [SerializeField] private Image _darkBg;
    [Tooltip("Background bar atas yang menyala saat membuka Fullscreen UI selain Home")]
    [SerializeField] private Image _topHudBarBg;

    // =========================================================================
    // 2. CENTRALIZED BUTTONS
    // =========================================================================

    [Header("Navigation & Action Buttons")]
    [SerializeField] private Button _battleBtn;          // Buka Level Selector
    [SerializeField] private Button _profileBtn;         // Buka Profile Card
    [SerializeField] private Button _shopBtn;            // Buka Shop Screen
    [SerializeField] private Button _deckBtn;            // Buka Deck Screen
    [SerializeField] private Button _mainBtn;            // Buka Main/Home Screen
    [SerializeField] private Button _talentBtn;          // Buka Talent Screen
    [SerializeField] private Button _dungeonBtn;         // Buka Dungeon Screen
    [SerializeField] private Button _mailBtn;            // Buka Mail Popup
    [SerializeField] private Button _achievementBtn;     // Buka Achievement Popup
    [SerializeField] private Button _dailyQuestBtn;      // Buka Daily Quest Popup
    [SerializeField] private Button _dailyAttendanceBtn; // Buka Daily Attendance Popup
    [SerializeField] private Button _settingsBtn;        // Buka Settings Popup
    [SerializeField] private Button _creditsBtn;         // Buka Credits Overlay
    [SerializeField] private Button _exitBtn;            // Keluar Game

    [Header("Global Back Buttons (Semua Tombol Close / X / Back)")]
    [Tooltip("Masukkan semua tombol X, Close, atau Back dari berbagai panel ke daftar ini")]
    [SerializeField] private Button[] _backButtons;

    [Tooltip("Jika dicentang, menekan tombol Back di HP Android (atau ESC di PC) otomatis memanggil fungsi Back()")]
    [SerializeField] private bool _enableHardwareBackButton = true;

    // =========================================================================
    // 3. BACKWARD COMPATIBILITY PROPERTIES
    // (Agar script lama seperti BottomMenuController, DailyQuestUI, dll tidak error)
    // =========================================================================
    public CanvasGroup MainPanel => _mainScreen;
    public CanvasGroup LevelSelectorPanel => _levelSelectorScreen;
    public CanvasGroup SettingsPanel => _settings;
    public CanvasGroup CreditsPanel => _credits;
    public CanvasGroup ShopPanel => _shopScreen;
    public CanvasGroup TalentPanel => _talentScreen;
    public CanvasGroup DeckPanel => _deckScreen;
    public CanvasGroup DungeonPanel => _dungeonScreen;

    public CanvasGroup BottomMenu => _bottomMenu;
    public CanvasGroup LeftMenu => _leftMenu;
    public CanvasGroup EconomyBar => _economyBar;
    public CanvasGroup ProfileMenu => _profileBar;

    // =========================================================================
    // 4. INTERNAL STATE TRACKING
    // =========================================================================
    private CanvasGroup _currentFullscreen;
    private readonly Stack<CanvasGroup> _popupStack = new Stack<CanvasGroup>();
    private CanvasGroup[] _allFullscreens;
    private CanvasGroup[] _allPopupsAndOverlays;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Kumpulkan daftar grup untuk memudahkan iterasi pembersihan tanpa kode berulang
        _allFullscreens = new CanvasGroup[]
        {
            _mainScreen, _levelSelectorScreen, _shopScreen,
            _deckScreen, _talentScreen, _dungeonScreen
        };

        _allPopupsAndOverlays = new CanvasGroup[]
        {
            _mail, _settings, _achievement, _dailyAttendance, _dailyQuest,
            _profileCard, _heroCard, _relicCard, _rewardClaim, _editProfile, _credits
        };

        RegisterAllButtons();
    }

    private void Start()
    {
        if (_gameVersionText != null)
        {
            _gameVersionText.text = $"Version {_gameVerMajor}.{_gameVerMinor}.{_gameVerPatches}";
        }

        InitializeDefaultUIState();
    }

    private void Update()
    {
        // Fitur kualitas hidup (QoL): Tombol Back di HP Android / Escape di PC otomatis memanggil Back()
        if (_enableHardwareBackButton && Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }

    // =========================================================================
    // 5. INITIALIZATION & BUTTON WIRING
    // =========================================================================

    private void InitializeDefaultUIState()
    {
        _popupStack.Clear();

        // 1. Pastikan seluruh HUD utama menyala
        SetCanvasGroupState(_economyBar, true);
        SetCanvasGroupState(_profileBar, true);
        SetCanvasGroupState(_leftMenu, true);
        SetCanvasGroupState(_bottomMenu, true);

        // 2. Matikan paksa semua Popup & Overlay di awal
        foreach (var popup in _allPopupsAndOverlays)
        {
            SetCanvasGroupState(popup, false);
        }

        // 3. Buka layar utama (Main/Home Screen)
        OpenFullscreen(_mainScreen);

        // 4. Sinkronkan status background gelap
        RefreshDarkBackground();
    }

    private void RegisterAllButtons()
    {
        // Fullscreen Navigation Buttons
        BindButton(_mainBtn, () => OpenFullscreen(_mainScreen));
        BindButton(_battleBtn, () => OpenFullscreen(_levelSelectorScreen));
        BindButton(_shopBtn, () => OpenFullscreen(_shopScreen));
        BindButton(_deckBtn, () => OpenFullscreen(_deckScreen));
        BindButton(_talentBtn, () =>
        {
            OpenFullscreen(_talentScreen);
            FindFirstObjectByType<TalentUI>()?.RefreshAllUI();
        });
        BindButton(_dungeonBtn, () => OpenFullscreen(_dungeonScreen));

        // Popup & Overlay Buttons
        BindButton(_profileBtn, () => OpenPopup(_profileCard));
        BindButton(_mailBtn, () => OpenPopup(_mail));
        BindButton(_achievementBtn, () =>
        {
            FindFirstObjectByType<AchievementUI>()?.RefreshUI();
            OpenPopup(_achievement);
        });
        BindButton(_dailyQuestBtn, () =>
        {
            DailyQuestManager.Instance?.CheckDailyReset();
            OpenPopup(_dailyQuest);
        });
        BindButton(_dailyAttendanceBtn, () => OpenPopup(_dailyAttendance));
        BindButton(_settingsBtn, () => OpenPopup(_settings));
        BindButton(_creditsBtn, () =>
        {
            OpenPopup(_credits);
            FindFirstObjectByType<CreditsPanelController>()?.StartScroll();
        });

        BindButton(_exitBtn, ExitGame);

        // Daftarkan semua tombol Back/Close di dalam array ke fungsi Back()
        if (_backButtons != null)
        {
            foreach (var btn in _backButtons)
            {
                BindButton(btn, Back);
            }
        }

        // Opsional: Jika _darkBg diberi komponen Button di Inspector, klik di luar popup akan menutup popup!
        if (_darkBg != null && _darkBg.TryGetComponent<Button>(out Button darkBgBtn))
        {
            BindButton(darkBgBtn, Back);
        }
    }

    private void BindButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveListener(action);
        btn.onClick.AddListener(action);
    }

    // =========================================================================
    // 6. CORE UI STATE MANAGEMENT (FULLSCREEN, POPUP, & BACK)
    // =========================================================================

    /// <summary>
    /// Membuka halaman Fullscreen (Home, Shop, Deck, Talent, Dungeon, LevelSelector).
    /// Otomatis menyalakan _topHudBarBg jika bukan di Home (_mainScreen).
    /// </summary>
    public void OpenFullscreen(CanvasGroup targetScreen)
    {
        if (targetScreen == null) return;

        // Jika berpindah tab utama dari Bottom Menu, bersihkan semua popup yang masih menggantung
        CloseAllPopups();

        // Matikan semua layar fullscreen lainnya, nyalakan hanya targetScreen
        foreach (var screen in _allFullscreens)
        {
            if (screen == null) continue;
            SetCanvasGroupState(screen, screen == targetScreen);
        }

        _currentFullscreen = targetScreen;

        // ATURAN TOP HUD BG: Menyala di semua Fullscreen KECUALI saat berada di _mainScreen (Home)
        bool isSubFullscreen = (targetScreen != _mainScreen);
        if (_topHudBarBg != null)
        {
            _topHudBarBg.gameObject.SetActive(isSubFullscreen);
        }

        // Sembunyikan Left Floating Menu saat tidak di Home agar tidak menutupi Shop/Deck/LevelSelector
        if (_leftMenu != null)
        {
            SetCanvasGroupState(_leftMenu, !isSubFullscreen);
        }
    }

    /// <summary>
    /// Membuka Popup / Modal di atas layar saat ini dan menyalakan Dark Background.
    /// </summary>
    public void OpenPopup(CanvasGroup popup)
    {
        if (popup == null) return;

        // Jika popup noch belum ada di tumpukan, masukkan ke Stack
        if (!_popupStack.Contains(popup))
        {
            _popupStack.Push(popup);
        }

        SetCanvasGroupState(popup, true);

        // Urutkan agar DarkBg berada tepat di belakang popup teratas
        RefreshDarkBackground();
        popup.transform.SetAsLastSibling();
    }

    /// <summary>
    /// Menutup Popup spesifik (atau gunakan Back() untuk menutup yang paling atas).
    /// </summary>
    public void ClosePopup(CanvasGroup popup)
    {
        if (popup == null) return;

        SetCanvasGroupState(popup, false);

        // Jika popup yang ditutup adalah yang paling atas di stack, keluarkan dari stack
        if (_popupStack.Count > 0 && _popupStack.Peek() == popup)
        {
            _popupStack.Pop();
        }
        else if (_popupStack.Contains(popup))
        {
            // Rebuild stack jika menutup popup di tengah tumpukan
            var temp = new List<CanvasGroup>(_popupStack);
            temp.Remove(popup);
            temp.Reverse();
            _popupStack.Clear();
            foreach (var item in temp) _popupStack.Push(item);
        }

        RefreshDarkBackground();
    }

    public void CloseAllPopups()
    {
        while (_popupStack.Count > 0)
        {
            CanvasGroup top = _popupStack.Pop();
            SetCanvasGroupState(top, false);
        }

        foreach (var p in _allPopupsAndOverlays)
        {
            SetCanvasGroupState(p, false);
        }

        RefreshDarkBackground();
    }

    /// <summary>
    /// FUNGSI TOMBOL BACK UNIVERSAL:
    /// 1. Jika ada Popup/Overlay yang terbuka -> Tutup popup paling atas.
    /// 2. Jika tidak ada Popup, tapi sedang di Fullscreen selain Home (misal LevelSelector/Shop) -> Kembali ke Home.
    /// </summary>
    public void Back()
    {
        // Prioritas 1: Tutup Popup / Overlay yang sedang terbuka paling depan
        if (_popupStack.Count > 0)
        {
            CanvasGroup topPopup = _popupStack.Pop();
            SetCanvasGroupState(topPopup, false);

            // Jika yang ditutup adalah Credits, hentikan animasi scroll-nya
            if (topPopup == _credits)
            {
                FindFirstObjectByType<CreditsPanelController>()?.StopScroll();
            }

            RefreshDarkBackground();
            return;
        }

        // Prioritas 2: Jika sedang berada di halaman Fullscreen selain Home, kembali ke Home (_mainScreen)
        if (_currentFullscreen != null && _currentFullscreen != _mainScreen)
        {
            OpenFullscreen(_mainScreen);
            return;
        }
    }

    private void RefreshDarkBackground()
    {
        if (_darkBg == null) return;

        bool hasActivePopup = _popupStack.Count > 0;
        _darkBg.gameObject.SetActive(hasActivePopup);

        // Jika ada popup bertumpuk (misal EditProfile di atas ProfileCard),
        // pastikan popup yang paling atas tetap berada di depan DarkBg
        if (hasActivePopup)
        {
            CanvasGroup topPopup = _popupStack.Peek();
            if (topPopup != null && _darkBg.transform.parent == topPopup.transform.parent)
            {
                _darkBg.transform.SetAsLastSibling();
                topPopup.transform.SetAsLastSibling();
            }
        }
    }

    // =========================================================================
    // 7. HELPER & LEGACY BRIDGE (JEMBATAN UNTUK SCRIPT LAMA)
    // =========================================================================

    /// <summary>
    /// Jembatan cerdas untuk panggilan TogglePanel() dari script lama.
    /// </summary>
    public void TogglePanel(CanvasGroup panel, bool isVisible)
    {
        if (panel == null) return;

        // Cek apakah panel ini termasuk kategori Fullscreen
        if (System.Array.IndexOf(_allFullscreens, panel) >= 0)
        {
            if (isVisible)
            {
                OpenFullscreen(panel);
            }
            else if (panel == _currentFullscreen && panel != _mainScreen)
            {
                // Jika script lama mematikan Fullscreen (misal menutup LevelSelector), kembalikan ke Home
                OpenFullscreen(_mainScreen);
            }
            return;
        }

        // Cek apakah panel ini termasuk kategori Popup / Overlay
        if (System.Array.IndexOf(_allPopupsAndOverlays, panel) >= 0)
        {
            if (isVisible) OpenPopup(panel);
            else ClosePopup(panel);
            return;
        }

        // Untuk elemen HUD biasa
        SetCanvasGroupState(panel, isVisible);
    }

    private void SetCanvasGroupState(CanvasGroup group, bool isVisible)
    {
        if (group == null) return;

        group.gameObject.SetActive(isVisible);
        group.alpha = isVisible ? 1f : 0f;
        group.interactable = isVisible;
        group.blocksRaycasts = isVisible;
    }

    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}