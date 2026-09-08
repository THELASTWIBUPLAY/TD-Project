using UnityEngine;

[CreateAssetMenu(fileName = "NewStageConfig", menuName = "TowerDefense/Stage Config")]
public class StageConfig : ScriptableObject
{
    [Header("Mode Configuration")]
    public string stageName = "Stage 1";
    public bool isEndless = false;
    public int maxWave = 10; // Hanya berlaku jika isEndless = false

    [Header("Wave Enemies Progression")]
    public int baseEnemyCount = 5;
    public float enemyCountWaveMultiplier = 2f; // Tambahan musuh tiap naik wave
    public float baseSpawnInterval = 2.2f;
    public float minSpawnInterval = 0.6f;

    [Header("Boss Wave Rules")]
    [Tooltip("Kelipatan wave untuk Miniboss (misal: tiap kelipatan 5)")]
    public int miniBossInterval = 5;
    [Tooltip("Kelipatan wave untuk Boss Utama (misal: tiap kelipatan 10)")]
    public int finalBossInterval = 10;
    public int miniBossCount = 1;
    public int finalBossCount = 1;

    [Header("Scoring Values")]
    public int scoreNormalMob = 10;
    public int scoreScoutMob = 15;
    public int scoreTankMob = 30;
    public int scoreMiniBoss = 120;
    public int scoreFinalBoss = 350;
    public int scoreWaveClearBonus = 50;
}