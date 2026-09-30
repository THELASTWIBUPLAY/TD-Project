using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Button _creditBtn;
    [SerializeField] private Button _exitBtn;
    [SerializeField] private CreditsPanelController _creditsPanelController;

    private void OnEnable()
    {
        _creditBtn.onClick.AddListener(OpenCreditPanel);
        _exitBtn.onClick.AddListener(ExitGame);
    }

    private void OnDisable()
    {
        _creditBtn.onClick.RemoveListener(OpenCreditPanel);
        _exitBtn.onClick.RemoveListener(ExitGame);
    }

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        _volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        _volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("GameVolume", volume);
    }

    private void OpenCreditPanel()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.CreditsPanel, true);
            _creditsPanelController.StartScroll();
        }
    }

    private void ExitGame()
    {
        Application.Quit();
        SaveManager.Instance.SaveLocal();

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
