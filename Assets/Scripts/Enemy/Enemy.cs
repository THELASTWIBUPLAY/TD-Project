using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Attributes")]
    public float maxHp = 15f;
    public float currentHp;
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

    [Header("Support Debuff")]
    public bool isChilled = false;
    private Coroutine chilledCoroutine;

    [Header("Animation Setup")]
    [SerializeField] private RuntimeAnimatorController normalController;
    [SerializeField] private RuntimeAnimatorController scoutController;
    [SerializeField] private RuntimeAnimatorController tankMobController;
    [SerializeField] private RuntimeAnimatorController bossController;

    private Animator anim;

    [Header("Sway Movement (Kiri-Kanan)")]
    public float swayFrequency = 0.5f; 
    public float swayAmplitude = 0.05f;  
    private float swayOffset = 0f;

    public static System.Action<float, float> OnBossHpChanged;
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
        anim = GetComponent<Animator>();

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

        swayOffset = Random.Range(0f, 10f);
    }


    void Update()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        float verticalMove = -moveSpeed * Time.deltaTime;

        float horizontalMove = Mathf.Sin((Time.time + swayOffset) * swayFrequency) * swayAmplitude * Time.deltaTime;

        transform.Translate(new Vector3(horizontalMove, verticalMove, 0f));

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


    public void TakeDamage(float damageAmount, bool isCrit = false)
    {
        if (isDead) return;

        if (isChilled)
        {
            damageAmount *= 1.15f;
        }

        currentHp -= damageAmount;

        if (gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlashRoutine());
        }

        if (DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.SpawnDamageText(transform.position, damageAmount, isCrit);
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

        Color burstColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        DeathBurstEffect.Create(transform.position, burstColor, transform.localScale.x);

        int currentWave = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 1;

        if (GameManager.Instance != null)
        {
            float baseExp = expReward;
            float waveExpBonus = Mathf.Min((currentWave - 1) * 1.5f, 200f);
            float totalBaseExp = baseExp + waveExpBonus;
            int finalExp = Mathf.RoundToInt(totalBaseExp * Enemy.GlobalExpMultiplier);

            Debug.Log($"[Enemy.Die] Wave {currentWave}: Base={baseExp}, Bonus={waveExpBonus}, Total={totalBaseExp}, Final={finalExp}");

            GameManager.Instance.AddExp(finalExp);
        }

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

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDefeated(gameObject);
        }

        if (archetype == EnemyArchetype.Boss)
        {
            OnBossDefeatedEvent?.Invoke();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ReturnToNormalBGM(); 
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterKill();
        }

        if (archetype == EnemyArchetype.Boss && CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.2f, 0.08f); 
        }
        else if (archetype == EnemyArchetype.Tank && CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.1f, 0.06f);
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
        if (anim == null) anim = GetComponent<Animator>();

        float waveHpFactor = 1f + ((wave - 1) * 0.12f);

        float baseScaleSize = 0.12f; 

        switch (archetype)
        {
            case EnemyArchetype.Normal:
                maxHp = 15f * waveHpFactor;
                moveSpeed = 1.3f;
                damageToBase = 5f;
                expReward = 8;
                transform.localScale = new Vector3(baseScaleSize, baseScaleSize, 1f);
                if (anim != null && normalController != null) anim.runtimeAnimatorController = normalController;
                break;

            case EnemyArchetype.Scout:
                maxHp = 9f * waveHpFactor;
                moveSpeed = 2.3f;
                damageToBase = 4f;
                expReward = 10;
                // Scout bisa dibuat sedikit lebih kecil
                transform.localScale = new Vector3(baseScaleSize * 0.8f, baseScaleSize * 0.8f, 1f);
                if (anim != null && scoutController != null) anim.runtimeAnimatorController = scoutController;
                break;

            case EnemyArchetype.Tank:
                maxHp = 45f * waveHpFactor;
                moveSpeed = 0.75f;
                damageToBase = 15f;
                expReward = 22;
                // Tank mob sedikit lebih besar
                transform.localScale = new Vector3(baseScaleSize * 1.3f, baseScaleSize * 1.3f, 1f);
                if (anim != null && tankMobController != null) anim.runtimeAnimatorController = tankMobController;
                break;

            case EnemyArchetype.Boss:
                bool isFinalBoss = (wave >= 10);
                moveSpeed = 0.5f;
                damageToBase = isFinalBoss ? 50f : 30f;
                expReward = isFinalBoss ? 100 : 50;
                maxHp = (isFinalBoss ? 450f : 200f) + (wave * 30f);
                
                // Boss jauh lebih besar dari mob biasa
                float bossScale = isFinalBoss ? baseScaleSize * 2.5f : baseScaleSize * 1.8f;
                transform.localScale = new Vector3(bossScale, bossScale, 1f);
                
                if (anim != null && bossController != null) anim.runtimeAnimatorController = bossController;
                break;
        }

        maxHp *= GlobalHpMultiplier;
        moveSpeed *= GlobalSpeedMultiplier;
        damageToBase *= GlobalDamageMultiplier;
        currentHp = maxHp;

    }

    public void ApplyKnockback(float force, float duration = 0.15f)
    {
        if (isDead) return;

        if (archetype == EnemyArchetype.Boss)
        {
            force *= 0.15f; 
        }

        StartCoroutine(KnockbackRoutine(force, duration));
    }

    private IEnumerator KnockbackRoutine(float force, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position += Vector3.up * force * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void ApplyChilledSlow(float slowPercent, float duration)
    {
        if (isDead) return;

        if (chilledCoroutine != null)
        {
            StopCoroutine(chilledCoroutine);
        }

        chilledCoroutine = StartCoroutine(ChilledRoutine(slowPercent, duration));
    }

    private IEnumerator ChilledRoutine(float slowPercent, float duration)
    {
        isChilled = true;

        float originalSpeed = moveSpeed;

        moveSpeed = Mathf.Max(0.35f, originalSpeed * (1f - slowPercent));

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;
        if (sr != null) sr.color = new Color(0.4f, 0.75f, 1f, originalColor.a);

        yield return new WaitForSeconds(duration);

        moveSpeed = originalSpeed;
        if (sr != null) sr.color = originalColor;
        isChilled = false;
        chilledCoroutine = null;
    }
}
