using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    [Header("UI Dependencies")]
    [SerializeField] private TextMeshProUGUI _levelText;

    [Header("Settings")]
    [Tooltip("Input the level number")]
    [SerializeField] private int _level;

    private void Start()
    {
        _levelText.text = _level.ToString();
    }

    public void OpenScene()
    {
        string sceneName = "Level " + _level.ToString();
        Debug.Log("Attempting to load: " + sceneName);

        // Cek jika scene sudah terdaftar dalam build
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found!");
        }
    }
}