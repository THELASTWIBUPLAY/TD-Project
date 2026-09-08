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

    [Header("Scoring System")]
    public TextMeshProUGUI scoreText;
    public int currentScore = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (PausePanel != null) PausePanel.SetActive(false);
        UpdateExpUI();

        // Tampilkan HUD skor HANYA jika masuk Endless Mode
        bool isEndless = (WaveManager.Instance != null && 
                          WaveManager.Instance.stageConfig != null && 
                          WaveManager.Instance.stageConfig.isEndless);

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(isEndless);
            if (isEndless) UpdateScoreUI();
        }
    }

    public void AddScore(int amount)
    {
        // Tolak penambahan skor jika bukan Endless Mode
        if (WaveManager.Instance != null && WaveManager.Instance.stageConfig != null)
        {
            if (!WaveManager.Instance.stageConfig.isEndless) return;
        }

        currentScore += amount;
        UpdateScoreUI();
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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
        }
        else
        {
            isMuted = !isMuted;
            AudioListener.pause = isMuted;
        }
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

        // Formula Kuadratik Bertahap (Ditingkatkan 1.5x lipat):
        // Lv 1: ~65 EXP
        // Lv 10: ~360 EXP
        // Lv 20: ~1.150 EXP
        // Lv 50: ~7.000 EXP
        // Lv 100: ~27.000 EXP
        float baseCurve = 25f + (currentLevel * 18f) + (Mathf.Pow(currentLevel, 1.85f) * 1.8f);
        expToNextLevel = Mathf.Round(baseCurve * 1.5f);

        // Pengaman: kuras sisa EXP berlebih jika naik level beruntun dalam 1 frame
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

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:N0}";
        }
    }
}