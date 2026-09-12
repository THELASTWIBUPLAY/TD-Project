using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Attributes")]
    public float maxHp = 15f;
    private float currentHp;
    public float moveSpeed = 1.3f;
    public float damageToBase = 5f;
    public int expReward = 8;


    [Header("Visual Feedback")]
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color hitFlashColor = Color.white;
    public float flashDuration = 0.08f;
    private Coroutine flashCoroutine;


    public static float GlobalHpMultiplier = 1f;
    public static float GlobalSpeedMultiplier = 1f;
    public static float GlobalDamageMultiplier = 1f;
    public static float GlobalExpMultiplier = 1f;


    [Header("Archetype Setup")]
    public EnemyArchetype archetype = EnemyArchetype.Normal;


    [Header("Base Safety Boundary")]
    public float baseLineY = -2f;


    public static System.Action<float, float> OnBossHpChanged; // (currentHp, maxHp)
    public static System.Action OnBossDefeatedEvent;
    private bool isDead = false;


    public static void ResetGlobalStats()
    {
        GlobalHpMultiplier = 1f;
        GlobalSpeedMultiplier = 1f;
        GlobalDamageMultiplier = 1f;
        GlobalExpMultiplier = 1f;
    }


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }


    void Start()
    {
        maxHp *= GlobalHpMultiplier;
        moveSpeed *= GlobalSpeedMultiplier;
        damageToBase *= GlobalDamageMultiplier;
        currentHp = maxHp;
    }


    void Update()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        // PENGAMAN: Jika musuh terlalu cepat sampai melompati Collider Base
        if (!isDead && transform.position.y <= baseLineY)
        {
            HitBaseDirectly();
        }
    }


    private void HitBaseDirectly()
    {
        if (isDead) return;
        isDead = true;

        BaseHealth baseHealth = FindFirstObjectByType<BaseHealth>();
        if (baseHealth != null)
        {
            baseHealth.TakeBaseDamage(damageToBase);
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDefeated(gameObject);
        }

        if (archetype == EnemyArchetype.Boss)
        {
            OnBossDefeatedEvent?.Invoke();
        }

        Destroy(gameObject);
    }


    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHp -= damageAmount;

        // Picu efek flash
        if (gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlashRoutine());
        }

        // Spawn teks damage melayang
        if (DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.SpawnDamageText(transform.position, damageAmount);
        }

        if (archetype == EnemyArchetype.Boss)
        {
            OnBossHpChanged?.Invoke(currentHp, maxHp);
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }


    IEnumerator HitFlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }


    private void Die()
    {
        if (isDead) return;
        isDead = true;

        int currentWave = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 1;

        // ==========================================
        // 1. Berikan EXP ke Player (DISEIMBAGKAN)
        // ==========================================
        if (GameManager.Instance != null)
        {
            // Base EXP dari archetype
            float baseExp = expReward;

            // Wave Bonus yang lebih stabil: naik perlahan, tapi ada cap di 200
            float waveExpBonus = Mathf.Min((currentWave - 1) * 1.5f, 200f);

            // Total EXP sebelum multiplier kartu
            float totalBaseExp = baseExp + waveExpBonus;

            // Terapkan multiplier kartu global (jika ada)
            int finalExp = Mathf.RoundToInt(totalBaseExp * Enemy.GlobalExpMultiplier);

            Debug.Log($"[Enemy.Die] Wave {currentWave}: Base={baseExp}, Bonus={waveExpBonus}, Total={totalBaseExp}, Final={finalExp}");

            GameManager.Instance.AddExp(finalExp);
        }


        // 2. Berikan Skor sesuai StageConfig (HANYA di Endless Mode)
        if (GameManager.Instance != null && WaveManager.Instance != null && WaveManager.Instance.stageConfig != null)
        {
            StageConfig cfg = WaveManager.Instance.stageConfig;

            if (cfg.isEndless)
            {
                int scoreGiven = archetype switch
                {
                    EnemyArchetype.Normal => cfg.scoreNormalMob,
                    EnemyArchetype.Scout => cfg.scoreScoutMob,
                    EnemyArchetype.Tank => cfg.scoreTankMob,
                    EnemyArchetype.Boss => (currentWave >= 10) ? cfg.scoreFinalBoss : cfg.scoreMiniBoss,
                    _ => 10
                };
                GameManager.Instance.AddScore(scoreGiven);
            }
        }


        // 3. Notifikasi musuh mati ke WaveManager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDefeated(gameObject);
        }

        if (archetype == EnemyArchetype.Boss)
        {
            OnBossDefeatedEvent?.Invoke();
        }


        // Catat musuh yang mati ke GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterKill();
        }

        Destroy(gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Base"))
        {
            HitBaseDirectly();
        }
    }


    public void ApplyArchetype(EnemyArchetype type, int wave)
    {
        archetype = type;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Scaling dasar per wave
        float waveHpFactor = 1f + ((wave - 1) * 0.12f);


        switch (archetype)
        {
            case EnemyArchetype.Normal:
                maxHp = 15f * waveHpFactor;
                moveSpeed = 1.3f;
                damageToBase = 5f;
                expReward = 8;
                transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                originalColor = Color.red;
                break;

            case EnemyArchetype.Scout:
                maxHp = 9f * waveHpFactor;
                moveSpeed = 2.3f;
                damageToBase = 4f;
                expReward = 10;
                transform.localScale = new Vector3(0.35f, 0.35f, 1f);
                originalColor = new Color(1f, 0.8f, 0.1f); // Kuning terang
                break;

            case EnemyArchetype.Tank:
                maxHp = 45f * waveHpFactor;
                moveSpeed = 0.75f;
                damageToBase = 15f;
                expReward = 22;
                transform.localScale = new Vector3(0.85f, 0.85f, 1f);
                originalColor = new Color(0.5f, 0.1f, 0.7f); // Ungu gelap
                break;

            case EnemyArchetype.Boss:
                // Cek apakah ini Miniboss (Wave <= 5) atau Final Boss (Wave >= 10)
                bool isFinalBoss = (wave >= 10);

                moveSpeed = 0.5f;
                damageToBase = isFinalBoss ? 50f : 30f;
                expReward = isFinalBoss ? 100 : 50;

                maxHp = (isFinalBoss ? 450f : 200f) + (wave * 30f);

                transform.localScale = isFinalBoss ? new Vector3(2.2f, 2.2f, 1f) : new Vector3(1.7f, 1.7f, 1f);
                originalColor = isFinalBoss ? new Color(0.8f, 0f, 0.2f) : new Color(0.9f, 0.3f, 0f);
                break;
        }

        // Terapkan multiplier kartu global
        maxHp *= GlobalHpMultiplier;
        moveSpeed *= GlobalSpeedMultiplier;
        damageToBase *= GlobalDamageMultiplier;
        currentHp = maxHp;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
