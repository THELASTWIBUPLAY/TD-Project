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
    public float baseLineY = -2f; // Sesuaikan dengan posisi Y BaseLine kamu

    // Tambahkan variabel event statis untuk UI Boss
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

        if (GameManager.Instance != null)
        {
            int wave = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 1;
            float waveExpBonus = (wave - 1) * 3f;
            float totalBaseExp = expReward + waveExpBonus;
            int finalExp = Mathf.RoundToInt(totalBaseExp * GlobalExpMultiplier);

            GameManager.Instance.AddExp(finalExp);
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
        float waveHpFactor = 1f + ((wave - 1) * 0.12f); // Musuh makin tebal tiap wave

        switch (archetype)
        {
            case EnemyArchetype.Normal:
                maxHp = 15f * waveHpFactor;
                moveSpeed = 1.3f;
                damageToBase = 5f;
                expReward = 8;
                transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                if (spriteRenderer != null) originalColor = Color.red;
                break;

            case EnemyArchetype.Scout:
                maxHp = 9f * waveHpFactor; // Darah lebih tipis
                moveSpeed = 2.3f;          // Gerakan sangat kencang
                damageToBase = 4f;
                expReward = 10;
                transform.localScale = new Vector3(0.35f, 0.35f, 1f); // Lebih kecil
                if (spriteRenderer != null) originalColor = new Color(1f, 0.8f, 0.1f); // Kuning terang
                break;

            case EnemyArchetype.Tank:
                maxHp = 45f * waveHpFactor; // Darah 3x lipat
                moveSpeed = 0.75f;         // Gerakan lambat
                damageToBase = 15f;        // Hantaman ke base lebih sakit
                expReward = 22;
                transform.localScale = new Vector3(0.85f, 0.85f, 1f); // Bodi besar
                if (spriteRenderer != null) originalColor = new Color(0.5f, 0.1f, 0.7f); // Ungu gelap
                break;

            case EnemyArchetype.Boss:
                maxHp = 180f * waveHpFactor; // HP luar biasa tebal
                moveSpeed = 0.5f;           // Gerak sangat lambat dan mengintimidasi
                damageToBase = 50f;         // Hantaman fatal ke Base (setengah HP Base)
                expReward = 80;
                transform.localScale = new Vector3(1.3f, 1.3f, 1f); // Ukuran raksasa
                if (spriteRenderer != null) originalColor = new Color(0.9f, 0.1f, 0.2f); // Merah membara
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