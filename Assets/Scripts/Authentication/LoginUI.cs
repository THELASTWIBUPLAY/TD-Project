using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public enum LoginStatus
    {
        StandBy,
        Loading,
        Success,
        Failed,
    }

    [Header("UI Dependencies")]
    [SerializeField] private Button _guestLoginBtn;
    [SerializeField] private Button _googleLoginBtn;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private GameObject _loadingSpinner;

    [Header("Scene Transition Target")]
    [SerializeField] private string _nextSceneName = "MainMenu";

    private void Awake()
    {
        if (_guestLoginBtn != null)
        {
            _guestLoginBtn.onClick.AddListener(OnGuestButtonClicked);
        }

        if ( _googleLoginBtn != null)
        {
            _googleLoginBtn.onClick.AddListener(OnGoogleButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (_guestLoginBtn != null)
        {
            _guestLoginBtn.onClick.RemoveListener(OnGuestButtonClicked);
        }

        if (_googleLoginBtn != null)
        {
            _googleLoginBtn.onClick.RemoveListener(OnGoogleButtonClicked);
        }
    }

    private void Start()
    {
        SetLoading(false);
        SetStatus(LoginStatus.StandBy);
    }

    private async void OnGuestButtonClicked()
    {
        SetLoading(true);
        SetStatus(LoginStatus.Loading);

        if (AuthManager.Instance == null)
        {
            Debug.LogError("AuthManager is missing");
            SetLoading(false);
            return;
        }

        bool success = await AuthManager.Instance.SignInAsGuestAsync();

        if (success)
        {
            SetStatus(LoginStatus.Success);

            // Load local save data before changin scene
            if (SaveManager.Instance != null && !SaveManager.Instance.IsLoaded)
            {
                SaveManager.Instance.LoadLocal();
            }

            SceneManager.LoadScene(_nextSceneName);
        }
        else
        {
            SetStatus(LoginStatus.Failed);
            SetLoading(false);
        }
    }

    private void OnGoogleButtonClicked()
    {
        _statusText.text = "Google sign-in is not implemented yet";
    }

    private void SetLoading(bool isLoading)
    {
        if (_guestLoginBtn != null) _guestLoginBtn.interactable = !isLoading;
        if (_googleLoginBtn != null) _googleLoginBtn.interactable = !isLoading;
        if (_loadingSpinner != null) _loadingSpinner.SetActive(isLoading);
    }

    private void SetStatus(LoginStatus status)
    {
        switch (status)
        {
            case LoginStatus.StandBy:
                if (_statusText != null) _statusText.text = "Choose login option";
                break;
            case LoginStatus.Loading:
                if (_statusText != null) _statusText.text = "Connecting to server...";
                break;
            case LoginStatus.Success:
                if (_statusText != null) _statusText.text = "Login success! Loading your account";
                break;
            case LoginStatus.Failed:
                if (_statusText != null) _statusText.text = "Login failed. Check your internet connection";
                break;
            default:
                if (_statusText != null) _statusText.text = "Check your internet connection";
                break;
        }
    }
}