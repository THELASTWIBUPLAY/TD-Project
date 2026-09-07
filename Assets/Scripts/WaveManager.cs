using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Spawner Setup")]
    public GameObject enemyPrefab;
    public float spawnYPosition = 6f;
    public float minX = -2f;
    public float maxX = 2f;

    [Header("Wave Progression")]
    public int currentWave = 1;
    public int baseEnemyCount = 5;
    public float baseSpawnInterval = 2.2f;
    public float spawnInterval;

    [Header("Wave UI")]
    public Slider waveProgressBar;
    public TextMeshProUGUI waveText;
    public GameObject waveClearPanel;

    private int totalEnemiesThisWave;
    private int spawnedCount = 0;
    private bool isSpawning = false;
    private bool isWaveClearing = false;

    // Simpan daftar musuh yang sedang hidup di arena
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

        totalEnemiesThisWave = baseEnemyCount + ((currentWave - 1) * 2);
        spawnInterval = Mathf.Max(0.8f, baseSpawnInterval - ((currentWave - 1) * 0.1f));

        spawnedCount = 0;

        if (waveProgressBar != null)
        {
            waveProgressBar.maxValue = totalEnemiesThisWave;
            waveProgressBar.value = 0;
        }

        if (waveText != null)
        {
            waveText.text = $"Wave {currentWave}";
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

        // Cek darurat jika semua musuh ternyata sudah mati saat spawn terakhir selesai
        CheckWaveCompletion();
    }

    // Dipanggil saat musuh hancur atau mati
    public void OnEnemyDefeated(GameObject enemyGO)
    {
        if (activeEnemies.Contains(enemyGO))
        {
            activeEnemies.Remove(enemyGO);
        }

        // Bersihkan referensi null yang mungkin tersisa
        activeEnemies.RemoveAll(item => item == null);

        CheckWaveCompletion();
    }

    void CheckWaveCompletion()
    {
        // Bersihkan objek musuh yang sudah hancur
        activeEnemies.RemoveAll(item => item == null);

        // Jika semua musuh wave ini SUDAH di-spawn DAN tidak ada lagi musuh di arena
        if (!isSpawning && spawnedCount >= totalEnemiesThisWave && activeEnemies.Count == 0 && !isWaveClearing)
        {
            StartCoroutine(WaveClearRoutine());
        }
    }

    IEnumerator WaveClearRoutine()
    {
        isWaveClearing = true;
        Debug.Log($"<color=green>WAVE {currentWave} BERHASIL DILEWATI!</color>");

        if (waveClearPanel != null)
        {
            waveClearPanel.SetActive(true);
        }

        // Pakai WaitForSecondsRealtime agar tidak nyangkut saat game ter-pause
        yield return new WaitForSecondsRealtime(2.5f);

        if (waveClearPanel != null)
        {
            waveClearPanel.SetActive(false);
        }

        currentWave++;
        StartNewWave();
    }
}