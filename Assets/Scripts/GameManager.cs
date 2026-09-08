using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Speed & Pause Controls")]
    public TextMeshProUGUI speedButtonText;
    private int currentSpeedIndex = 0;
    private readonly float[] speedMultipliers = { 1f, 2f, 3f };
    private bool isPaused = false;
    private bool isMuted = false;

    [Header("Pause Modal")]
    public GameObject PausePanel; // Panel berisi tombol Resume & Quit

    [Header("EXP & Level System")]
    public Slider expSlider;
    public TextMeshProUGUI expText;
    public int currentLevel = 1;
    public float currentExp = 0f;
    public float expToNextLevel = 20f;

    [Header("End Game UI Panels")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverWaveText;
    public GameObject gameWinPanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (PausePanel != null) PausePanel.SetActive(false);
        UpdateExpUI();
    }

    public void ToggleSpeed()
    {
        if (isPaused) return;
        currentSpeedIndex = (currentSpeedIndex + 1) % speedMultipliers.Length;
        Time.timeScale = speedMultipliers[currentSpeedIndex];

        if (speedButtonText != null)
        {
            speedButtonText.text = speedMultipliers[currentSpeedIndex] + "x";
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : speedMultipliers[currentSpeedIndex];
        
        if (PausePanel != null)
        {
            PausePanel.SetActive(isPaused);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (PausePanel != null) PausePanel.SetActive(false);
        Time.timeScale = speedMultipliers[currentSpeedIndex];
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        // Jika ada MainMenu scene nanti, bisa panggil SceneManager.LoadScene("MainMenu");
        Application.Quit();
        Debug.Log("Quit Game dipanggil!");
    }

    public void RestoreSpeedAfterModal()
    {
        Time.timeScale = speedMultipliers[currentSpeedIndex];
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        AudioListener.pause = isMuted;
    }

    public void AddExp(float amount)
    {
        currentExp += amount;
        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
        UpdateExpUI();
    }

    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        currentLevel++;
        expToNextLevel = Mathf.Round(expToNextLevel * 1.5f);

        // Panggil pilihan 3 kartu acak
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.ShowUpgradeSelection();
        }
    }

    private void UpdateExpUI()
    {
        if (expSlider != null)
        {
            expSlider.maxValue = expToNextLevel;
            expSlider.value = currentExp;
        }

        if (expText != null)
        {
            expText.text = $"Lv.{currentLevel} ({currentExp}/{expToNextLevel})";
        }
    }

    public void TriggerGameOver()
    {
        Time.timeScale = 0f; // Hentikan game

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverWaveText != null && WaveManager.Instance != null)
        {
            gameOverWaveText.text = $"Bertahan Sampai: Wave {WaveManager.Instance.currentWave}";
        }
    }

    public void TriggerGameWin()
    {
        Time.timeScale = 0f; // Hentikan game

        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);
        }
    }

    // Dipanggil oleh tombol Retry / Main Lagi di UI
    public void RestartGame()
    {
        Time.timeScale = 1f; // Kembalikan waktu normal
        Enemy.ResetGlobalStats(); // Reset multiplier musuh
        Character.GlobalDamageBonusPercent = 0f;
        Character.GlobalAtkSpeedMultiplier = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}