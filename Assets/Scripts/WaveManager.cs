using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Stage Configuration (ScriptableObject)")]
    public StageConfig stageConfig;

    [Header("Spawner Setup")]
    public GameObject enemyPrefab;
    public float spawnYPosition = 6f;
    public float minX = -2f;
    public float maxX = 2f;

    [Header("Wave Progression State")]
    public int currentWave = 1;
    public float spawnInterval;

    [Header("Wave UI")]
    public Slider waveProgressBar;
    public TextMeshProUGUI waveText;
    public GameObject waveClearPanel;

    private int totalEnemiesThisWave;
    private int spawnedCount = 0;
    private bool isSpawning = false;
    private bool isWaveClearing = false;

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Enemy.ResetGlobalStats();
    }

    void Start()
    {
        if (waveClearPanel != null) waveClearPanel.SetActive(false);
        StartNewWave();
    }

    public void StartNewWave()
    {
        isWaveClearing = false;
        activeEnemies.Clear();

        // Mengambil formula dari StageConfig
        int baseEnemies = stageConfig != null ? stageConfig.baseEnemyCount : 5;
        float mult = stageConfig != null ? stageConfig.enemyCountWaveMultiplier : 2f;
        totalEnemiesThisWave = Mathf.RoundToInt(baseEnemies + ((currentWave - 1) * mult));

        float baseInterval = stageConfig != null ? stageConfig.baseSpawnInterval : 2.2f;
        float minInterval = stageConfig != null ? stageConfig.minSpawnInterval : 0.6f;
        spawnInterval = Mathf.Max(minInterval, baseInterval - ((currentWave - 1) * 0.08f));

        spawnedCount = 0;

        if (waveProgressBar != null)
        {
            waveProgressBar.maxValue = totalEnemiesThisWave;
            waveProgressBar.value = 0;
        }

        if (waveText != null)
        {
            string modeLabel = (stageConfig != null && stageConfig.isEndless) ? " (Endless)" : "";
            waveText.text = $"Wave {currentWave}{modeLabel}";
        }

        StartCoroutine(SpawnEnemiesRoutine());
    }

    IEnumerator SpawnEnemiesRoutine()
    {
        isSpawning = true;
        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < totalEnemiesThisWave; i++)
        {
            float randomX = Random.Range(minX, maxX);
            Vector2 spawnPos = new Vector2(randomX, spawnYPosition);

            if (enemyPrefab != null)
            {
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                Enemy enemyComp = newEnemy.GetComponent<Enemy>();

                if (enemyComp != null)
                {
                    EnemyArchetype chosenType = PickEnemyTypeForWave(currentWave, i);
                    enemyComp.ApplyArchetype(chosenType, currentWave);

                    if (chosenType == EnemyArchetype.Boss && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayBossIncomingSFX();
                    }
                }

                activeEnemies.Add(newEnemy);
            }

            spawnedCount++;
            if (waveProgressBar != null)
            {
                waveProgressBar.value = spawnedCount;
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
        CheckWaveCompletion();
    }

    EnemyArchetype PickEnemyTypeForWave(int wave, int enemyIndex)
    {
        int finalBossInt = stageConfig != null ? stageConfig.finalBossInterval : 10;
        int miniBossInt = stageConfig != null ? stageConfig.miniBossInterval : 5;

        int finalCount = stageConfig != null ? stageConfig.finalBossCount : 1;
        int miniCount = stageConfig != null ? stageConfig.miniBossCount : 1;

        // 1. Cek Wave Boss Utama (misal: wave 10, 20, 30...)
        if (finalBossInt > 0 && wave % finalBossInt == 0)
        {
            if (enemyIndex >= totalEnemiesThisWave - finalCount)
            {
                return EnemyArchetype.Boss;
            }
        }
        // 2. Cek Wave Miniboss (misal: wave 5, 15, 25...)
        else if (miniBossInt > 0 && wave % miniBossInt == 0)
        {
            if (enemyIndex >= totalEnemiesThisWave - miniCount)
            {
                return EnemyArchetype.Boss;
            }
        }

        // Variasi mob reguler
        if (wave < 3)
        {
            return EnemyArchetype.Normal;
        }
        else if (wave < 5)
        {
            return (Random.value < 0.3f) ? EnemyArchetype.Scout : EnemyArchetype.Normal;
        }
        else
        {
            float roll = Random.value;
            if (roll < 0.28f) return EnemyArchetype.Tank;
            if (roll < 0.58f) return EnemyArchetype.Scout;
            return EnemyArchetype.Normal;
        }
    }

    public void OnEnemyDefeated(GameObject enemyGO)
    {
        if (activeEnemies.Contains(enemyGO))
        {
            activeEnemies.Remove(enemyGO);
        }

        activeEnemies.RemoveAll(item => item == null);
        CheckWaveCompletion();
    }

    void CheckWaveCompletion()
    {
        activeEnemies.RemoveAll(item => item == null);

        if (!isSpawning && spawnedCount >= totalEnemiesThisWave && activeEnemies.Count == 0 && !isWaveClearing)
        {
            StartCoroutine(WaveClearRoutine());
        }
    }

    IEnumerator WaveClearRoutine()
    {
        isWaveClearing = true;
        Debug.Log($"<color=green>WAVE {currentWave} CLEAR!</color>");

        // Tambah bonus skor wave clear
        // Tambah bonus skor wave clear HANYA jika Endless Mode
        if (GameManager.Instance != null && stageConfig != null && stageConfig.isEndless)
        {
            GameManager.Instance.AddScore(stageConfig.scoreWaveClearBonus);
        }

        // Kondisi menang hanya jika BUKAN mode Endless
        bool isEndless = stageConfig != null && stageConfig.isEndless;
        int maxWave = stageConfig != null ? stageConfig.maxWave : 10;

        if (!isEndless && currentWave >= maxWave)
        {
            yield return new WaitForSecondsRealtime(1.0f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameWin();
            }
            yield break;
        }

        if (waveClearPanel != null)
        {
            waveClearPanel.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(2.0f);

        if (waveClearPanel != null)
        {
            waveClearPanel.SetActive(false);
        }

        currentWave++;
        StartNewWave();
    }
}