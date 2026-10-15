using TMPro;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [Header("UI Dependencies")]
    [SerializeField] private TextMeshProUGUI _playerId;
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private TextMeshProUGUI _accountLevel;
    [SerializeField] private TextMeshProUGUI _accountExp;
    [SerializeField] private Slider _expProgress;
    [SerializeField] private Button _backBtn;
    [SerializeField] private CanvasGroup _cardCanvas;
    [SerializeField] private Button _copyIdBtn;
    [SerializeField] private Button _openEditBtn;

    [Header("UI Dependencies (Btn)")]
    [SerializeField] private TextMeshProUGUI _playerNameBtnText;
    [SerializeField] private TextMeshProUGUI _playerLevelBtnText;
    [SerializeField] private Button _playerProfileBtn;


    [Header("UI Edit Modal")]
    [SerializeField] private CanvasGroup _editPanel;
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private Button _saveBtn;
    [SerializeField] private Button _cancelBtn;

    private bool _isSubscribed = false;

    private void Awake()
    {
        if (_copyIdBtn != null) _copyIdBtn.onClick.AddListener(OnCopyIdClicked);
        if (_openEditBtn != null) _openEditBtn.onClick.AddListener(OpenEditModal);

        if (_saveBtn != null) _saveBtn.onClick.AddListener(OnSaveProfileClicked);
        if (_cancelBtn != null) _cancelBtn.onClick.AddListener(CloseEditModal);
    }

    private void OnEnable()
    {  
        if (_backBtn != null)
        {
            _backBtn.onClick.AddListener(OnBackButtonClicked);
        }

        if (_playerProfileBtn != null)
        {
            _playerProfileBtn.onClick.AddListener(OnOpenPlayerProfileClicked);
        }

        TrySubscribe();
        CloseEditModal();
        RefreshUI();
    }

    private void OnDisable()
    {
        if (_backBtn != null)
        {
            _backBtn.onClick.RemoveListener(OnBackButtonClicked);
        }

        if (_playerProfileBtn != null)
        {
            _playerProfileBtn.onClick.RemoveListener(OnOpenPlayerProfileClicked);
        }
    }

    private void Start()
    {
        TrySubscribe();
        RefreshUI();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (!_isSubscribed && ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnProfileChanged += RefreshUI;
            _isSubscribed = true;
        }
    }

    private void Unsubscribe()
    {
        if (_isSubscribed && ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnProfileChanged -= RefreshUI;
            _isSubscribed = false;
        }
    }

    public void RefreshUI()
    {
        if (ProfileManager.Instance != null)
        {
            var profile = ProfileManager.Instance;

            _playerId.text = profile.PlayerId;
            _playerName.text = profile.PlayerName;
            _accountLevel.text = profile.AccountLevel.ToString();
            _accountExp.text = $"{profile.AccountExp} / {profile.ExpToNextLevel}";

            _expProgress.value = Mathf.Clamp01(profile.AccountExp / profile.ExpToNextLevel);

            _playerNameBtnText.text = profile.PlayerName;
            _playerLevelBtnText.text = profile.AccountLevel.ToString();
        }
    }

    public void OpenEditModal()
    {
        if (ProfileManager.Instance == null || _editPanel == null) return;

        if (_nameInput != null) _nameInput.text = ProfileManager.Instance.PlayerName;

        _editPanel.gameObject.SetActive(true);
    }

    public void CloseEditModal()
    {
        if (_editPanel != null)
        {
            _editPanel.gameObject.SetActive(false);
        }
    }

    private void OnSaveProfileClicked()
    {
        if (ProfileManager.Instance == null) return;

        string targetName = _nameInput != null ? _nameInput.text : string.Empty;

        // 1. Validasi & simpan Nama
        bool nameSuccess = ProfileManager.Instance.SetPlayerName(targetName);
        if (!nameSuccess) return;

        AlertManager.Instance?.Show("Profil saved!");
        CloseEditModal();
    }

    private void OnCopyIdClicked()
    {
        if (ProfileManager.Instance == null) return;

        GUIUtility.systemCopyBuffer = ProfileManager.Instance.PlayerId;
        AlertManager.Instance?.Show("Copied ID!");
    }

    private void OnBackButtonClicked()
    {
        _cardCanvas.alpha = 0;
        _cardCanvas.blocksRaycasts = false;
        _cardCanvas.interactable = false;
    }

    private void OnOpenPlayerProfileClicked()
    {
        _cardCanvas.alpha = 1;
        _cardCanvas.blocksRaycasts = true;
        _cardCanvas.interactable = true;
    }
}