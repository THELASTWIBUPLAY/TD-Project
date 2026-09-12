using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    [Header("Speed & Pause Controls")]
    public TextMeshProUGUI speedButtonText;
    private int currentSpeedIndex = 1; 
    private readonly float[] speedMultipliers = { 0.5f, 1f, 2f, 3f, 5f };
    private bool isPaused = false;
    private bool isMuted = false;


    [Header("Pause Modal")]
    public GameObject PausePanel;


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


    [Header("End Game Summary References")]
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverKillsText;
    public TextMeshProUGUI gameOverHighscoreText;


    public TextMeshProUGUI gameWinScoreText;
    public TextMeshProUGUI gameWinKillsText;
    public TextMeshProUGUI gameWinHighscoreText;


    public int totalEnemiesKilled = 0;


    void Awake()
    {
        Instance = this;
        Projectile.GlobalRicochetUnlocked = false;
    }


    void Start()
    {
        UpdateSpeedUI(); 
        if (PausePanel != null) PausePanel.SetActive(false);
        UpdateExpUI();

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
        if (WaveManager.Instance != null && WaveManager.Instance.stageConfig != null)
        {
            if (!WaveManager.Instance.stageConfig.isEndless) return;
        }

        currentScore += amount;
        UpdateScoreUI();
    }


    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:N0}";
        }
    }


    public void RegisterKill()
    {
        totalEnemiesKilled++;
    }

   public void ToggleSpeed()
    {
        if (isPaused) return;
        currentSpeedIndex = (currentSpeedIndex + 1) % speedMultipliers.Length;
        Time.timeScale = speedMultipliers[currentSpeedIndex];

        UpdateSpeedUI();
    }


    private void UpdateSpeedUI()
    {
        if (speedButtonText != null)
        {

            speedButtonText.text = $"{speedMultipliers[currentSpeedIndex]:0.#}x";
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

        float baseCurve = 30f + (currentLevel * 15f) + (Mathf.Pow(currentLevel, 1.4f) * 1.5f);
        expToNextLevel = Mathf.Round(baseCurve * 1.2f);

        Debug.Log($"[GameManager.LevelUp] Level {currentLevel}: Base={baseCurve:F0}, NextExp={expToNextLevel}");

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
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        int finalWave = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 1;
        if (gameOverWaveText != null)
        {
            gameOverWaveText.text = $"Bertahan Hingga:\n<size=120%>Wave {finalWave}</size>";
        }

        UpdateSummaryUI(gameOverScoreText, gameOverKillsText, gameOverHighscoreText);
    }


    public void TriggerGameWin()
    {
        Time.timeScale = 0f;

        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);
        }

        UpdateSummaryUI(gameWinScoreText, gameWinKillsText, gameWinHighscoreText);
    }


    private void UpdateSummaryUI(TextMeshProUGUI txtScore, TextMeshProUGUI txtKills, TextMeshProUGUI txtHighscore)
    {
        bool isEndless = (WaveManager.Instance != null && 
                          WaveManager.Instance.stageConfig != null && 
                          WaveManager.Instance.stageConfig.isEndless);

        int savedHighscore = PlayerPrefs.GetInt("Endless_Highscore", 0);
        bool isNewRecord = false;

        if (isEndless && currentScore > savedHighscore)
        {
            savedHighscore = currentScore;
            PlayerPrefs.SetInt("Endless_Highscore", savedHighscore);
            PlayerPrefs.Save();
            isNewRecord = true;
        }

        if (txtKills != null) txtKills.text = $"Total Musuh Dikalahkan: \n<size=120%>{totalEnemiesKilled:N0}</size>";


        if (txtScore != null)
        {
            txtScore.gameObject.SetActive(isEndless);
            txtScore.text = $"Skor Akhir: \n<size=120%>{currentScore:N0}</size>";
        }

        if (txtHighscore != null)
        {
            txtHighscore.gameObject.SetActive(isEndless);
            txtHighscore.text = isNewRecord ? $"<color=yellow>NEW HIGH SCORE!</color> {savedHighscore:N0}" 
                                          : $"High Score: \n<size=120%>{savedHighscore:N0}</size>";
        }
    }


    public void RestartGame()
    {
        Time.timeScale = 1f;
        Enemy.ResetGlobalStats();
        Character.GlobalDamageBonusPercent = 0f;
        Character.GlobalAtkSpeedMultiplier = 1f;
        Projectile.GlobalRicochetUnlocked = false; 

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
