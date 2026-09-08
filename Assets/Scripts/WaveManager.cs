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
                Enemy enemyComp = newEnemy.GetComponent<Enemy>();

                if (enemyComp != null)
                {
                    // Tentukan tipe musuh secara dinamis
                    EnemyArchetype chosenType = PickEnemyTypeForWave(currentWave, i);
                    enemyComp.ApplyArchetype(chosenType, currentWave);
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

    // Penentu komposisi tipe musuh berdasarkan nomor Wave
    EnemyArchetype PickEnemyTypeForWave(int wave, int enemyIndex)
    {
        // Wave 5 dan 10: Musuh terakhir yang muncul adalah BOSS
        if ((wave == 5 || wave == 10) && enemyIndex == totalEnemiesThisWave - 1)
        {
            return EnemyArchetype.Boss;
        }

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
            if (roll < 0.25f) return EnemyArchetype.Tank;
            if (roll < 0.55f) return EnemyArchetype.Scout;
            return EnemyArchetype.Normal;
        }
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

    [Header("Win Condition")]
    public int maxWaveToWin = 10; // Selesaikan Wave 10 untuk menang

    IEnumerator WaveClearRoutine()
    {
        isWaveClearing = true;
        Debug.Log($"<color=green>WAVE {currentWave} CLEAR!</color>");

        // Cek apakah pemain sudah menamatkan wave terakhir
        if (currentWave >= maxWaveToWin)
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

        yield return new WaitForSecondsRealtime(2.5f);

        if (waveClearPanel != null)
        {
            waveClearPanel.SetActive(false);
        }

        currentWave++;
        StartNewWave();
    }
}