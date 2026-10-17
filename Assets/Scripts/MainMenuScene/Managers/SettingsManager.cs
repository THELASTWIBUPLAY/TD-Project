using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Button _creditBtn;
    [SerializeField] private Button _exitBtn;
    [SerializeField] private CreditsPanelController _creditsPanelController;

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
        PlayerPrefs.Save();

        // Teruskan perubahan slider langsung ke AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplySettingsVolume(volume);
        }
    }
}
