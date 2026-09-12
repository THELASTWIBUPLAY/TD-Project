using System.Collections;
using UnityEngine;
using TMPro;

public class Character : MonoBehaviour
{
    [Header("Class & Star Config")]
    public CharacterClassType classType = CharacterClassType.Ranger;
    [Range(1, 3)]
    public int starLevel = 1;
    public TextMeshPro starText3D;

    [Header("Base Stats")]
    public float baseAttackDamage = 10f;
    public float baseAttackCooldown = 0.7f;
    public float attackRange = 7f;

    [Header("Buff Multipliers (Global)")]
    public static float GlobalDamageBonusPercent = 0f;
    public static float GlobalAtkSpeedMultiplier = 1f;

    [Header("References")]
    public GameObject projectilePrefab;
    private SpriteRenderer spriteRenderer;

    [Header("Bouncy Animation")]
    public float bounceDuration = 0.15f;
    private Vector3 basePresetScale = new Vector3(0.6f, 0.6f, 1f);
    private Coroutine bounceCoroutine;
    private float fireCountdown = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (transform.localScale != Vector3.zero)
        {
            basePresetScale = transform.localScale;
        }
    }

    void Start()
    {
        ApplyClassStats();
        UpdateStarDisplay();
    }

    public void SetupClass(CharacterClassType type, int star = 1)
    {
        classType = type;
        starLevel = star;
        ApplyClassStats();
        UpdateStarDisplay();
    }

    public void ApplyClassStats()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        switch (classType)
        {
            case CharacterClassType.Ranger:
                baseAttackDamage = 12f;
                baseAttackCooldown = 0.65f;
                attackRange = 7.5f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.4f, 0.8f); 
                break;

            case CharacterClassType.Sniper:
                baseAttackDamage = 45f;
                baseAttackCooldown = 1.6f;
                attackRange = 10f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(0.2f, 0.85f, 0.3f); 
                break;

            case CharacterClassType.Bombardier:
                baseAttackDamage = 25f;
                baseAttackCooldown = 1.1f;
                attackRange = 6.5f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.45f, 0.1f); 
                break;

            case CharacterClassType.Cryo:
                baseAttackDamage = 8f;
                baseAttackCooldown = 0.8f;
                attackRange = 7f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(0.4f, 0.8f, 1f); 
                break;

            case CharacterClassType.Gunslinger:
                baseAttackDamage = 6f;
                baseAttackCooldown = 0.25f; 
                attackRange = 5.2f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.85f, 0.15f); 
                break;
        }
    }

    void Update()
    {
        fireCountdown -= Time.deltaTime;

        float starSpeedMult = starLevel == 1 ? 1f : (starLevel == 2 ? 1.3f : 1.8f);
        float currentCooldown = baseAttackCooldown / (GlobalAtkSpeedMultiplier * starSpeedMult);

        if (fireCountdown <= 0f)
        {
            Transform target = PickTargetForClass();
            if (target != null)
            {
                Shoot(target);
                fireCountdown = Mathf.Max(0.08f, currentCooldown);
            }
        }
    }

    public void SetStarLevel(int newLevel)
    {
        starLevel = Mathf.Clamp(newLevel, 1, 3);
        UpdateStarDisplay();
    }

    public void UpdateStarDisplay()
    {
        if (starText3D != null)
        {
            starText3D.text = starLevel.ToString();
        }

        float scaleMult = 1f + ((starLevel - 1) * 0.2f);
        transform.localScale = basePresetScale * scaleMult;
    }

    Transform PickTargetForClass()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        Transform bestEnemy = null;

        if (classType == CharacterClassType.Sniper)
        {
            float maxFoundHp = -1f;
            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    Enemy e = col.GetComponent<Enemy>();
                    if (e != null && e.maxHp > maxFoundHp)
                    {
                        maxFoundHp = e.maxHp;
                        bestEnemy = col.transform;
                    }
                }
            }
            if (bestEnemy != null) return bestEnemy;
        }

        float lowestY = Mathf.Infinity;
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                if (col.transform.position.y < lowestY)
                {
                    lowestY = col.transform.position.y;
                    bestEnemy = col.transform;
                }
            }
        }
        return bestEnemy;
    }

    void Shoot(Transform target)
    {
        if (target == null || projectilePrefab == null) return;

        if (classType != CharacterClassType.Bombardier && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClassShootSFX(classType);
        }

        float starDamageMult = starLevel == 1 ? 1f : (starLevel == 2 ? 2.2f : 4.5f);
        float finalDamage = baseAttackDamage * starDamageMult * (1f + (GlobalDamageBonusPercent / 100f));

        if (starLevel >= 3 && classType == CharacterClassType.Sniper)
        {
            Enemy targetEnemyComp = target.GetComponent<Enemy>();
            if (targetEnemyComp != null && (targetEnemyComp.archetype == EnemyArchetype.Tank || targetEnemyComp.archetype == EnemyArchetype.Boss))
            {
                finalDamage *= 1.8f;
            }
        }

        if (starLevel >= 3 && classType == CharacterClassType.Ranger)
        {
            StartCoroutine(DoubleTapRoutine(target, finalDamage));
        }
        else
        {
            SpawnProjectile(target, finalDamage);
        }

        if (bounceCoroutine != null) StopCoroutine(bounceCoroutine);
        bounceCoroutine = StartCoroutine(BounceEffect());
    }

    void SpawnProjectile(Transform target, float dmg)
    {
        GameObject projGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectile = projGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage = dmg;

            bool isAoE = (classType == CharacterClassType.Bombardier);
            float splashRadius = (starLevel >= 3) ? 2.2f : 1.5f;

            projectile.Setup(target, isAoE, splashRadius);
        }
    }

    IEnumerator DoubleTapRoutine(Transform target, float dmg)
    {
        SpawnProjectile(target, dmg);
        yield return new WaitForSeconds(0.12f);
        if (target != null) SpawnProjectile(target, dmg);
    }

    IEnumerator BounceEffect()
    {
        Vector3 targetScale = basePresetScale * (1f + ((starLevel - 1) * 0.2f));
        Vector3 squashScale = new Vector3(targetScale.x * 1.2f, targetScale.y * 0.8f, targetScale.z);
        Vector3 stretchScale = new Vector3(targetScale.x * 0.85f, targetScale.y * 1.15f, targetScale.z);

        float halfDuration = bounceDuration / 2f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            transform.localScale = Vector3.Lerp(squashScale, stretchScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            transform.localScale = Vector3.Lerp(stretchScale, targetScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    public void PlayMergeCelebration()
    {
        MergeSparkleEffect.Create(transform.position, starLevel);
        
        StartCoroutine(MergePopRoutine());
    }

    IEnumerator MergePopRoutine()
    {
        Vector3 baseScale = basePresetScale * (1f + ((starLevel - 1) * 0.2f));
        Vector3 bigScale = baseScale * 1.45f;
        float duration = 0.2f;
        float t = 0f;

        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(bigScale, baseScale, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = baseScale;
    }
}