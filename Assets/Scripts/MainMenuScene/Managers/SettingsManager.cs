using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider _volumeSlider;

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
        PlayerPrefs.SetFloat("GameVolume", volume); // Simpan permanen
    }
}
