using System.Collections;
using UnityEngine;
using TMPro;

public class Character : MonoBehaviour
{
    [Header("Class & Star Config")]
    public CharacterClassType classType = CharacterClassType.Fighter;
    [Range(1, 3)]
    public int starLevel = 1;
    public TextMeshPro starText3D;

    [Header("Base Stats")]
    public float baseAttackDamage = 10f;
    public float baseAttackCooldown = 0.7f;
    public float attackRange = 7f;

    [Header("Critical Stats")]
    public float critRate = 0.05f;     
    public float critDamage = 1.5f;    

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

    [Header("Evolution")]
    public EvolutionPath currentEvolution = EvolutionPath.None;

    [Header("Evolution Indicator")]
    public TextMeshPro evolutionBadgeText;

    private int fighterAttackCount = 0;
    private Transform lastFighterTarget = null;

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
            case CharacterClassType.Fighter: 
                baseAttackDamage = 14f;
                baseAttackCooldown = 0.6f;
                attackRange = 5.2f;
                critRate = 0.10f;
                critDamage = 1.5f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.4f, 0.8f); 
                break;

            case CharacterClassType.Ranged: 
                baseAttackDamage = 50f;
                baseAttackCooldown = 1.8f;
                attackRange = 10f;
                critRate = 0.20f;
                critDamage = 2.0f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(0.2f, 0.85f, 0.3f); 
                break;

            case CharacterClassType.Mage: 
                baseAttackDamage = 30f;
                baseAttackCooldown = 1.25f;
                attackRange = 7.5f;
                critRate = 0.05f;
                critDamage = 1.5f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.45f, 0.1f); 
                break;

            case CharacterClassType.Support:
                baseAttackDamage = 8f;
                baseAttackCooldown = 0.8f;
                attackRange = 7.5f;
                critRate = 0.05f;
                critDamage = 1.3f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(0.4f, 0.8f, 1f); 
                break;

            case CharacterClassType.Tank: 
                baseAttackDamage = 20f;
                baseAttackCooldown = 0.95f; 
                attackRange = 4.0f;
                critRate = 0.05f;
                critDamage = 1.4f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.85f, 0.15f); 
                break;
        }
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        fireCountdown -= Time.deltaTime;

        float starSpeedMult = starLevel == 1 ? 1f : (starLevel == 2 ? 1.3f : 1.8f);

        float effectiveCooldown = baseAttackCooldown;

        if (classType == CharacterClassType.Fighter && currentEvolution == EvolutionPath.PathA)
        {
            effectiveCooldown = 1f; 
        }

        bool isCritical = Random.value < critRate;

        if (classType == CharacterClassType.Ranged && currentEvolution == EvolutionPath.PathB)
        {
            isCritical = false;
        }

        float currentCooldown = effectiveCooldown / (GlobalAtkSpeedMultiplier * starSpeedMult);

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

        if (classType == CharacterClassType.Ranged)
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

    private int attackCount = 0;

    void Shoot(Transform target)
    {
        if (target == null || projectilePrefab == null) return;

        if (classType == CharacterClassType.Fighter)
        {
            if (lastFighterTarget != target)
            {
                fighterAttackCount = 0;
                lastFighterTarget = target;
            }
            fighterAttackCount++;
        }

        attackCount++;

        if (classType != CharacterClassType.Mage && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClassShootSFX(classType);
        }

        float starDamageMult = starLevel == 1 ? 1f : (starLevel == 2 ? 2.2f : 4.5f);
        float calculatedDamage = baseAttackDamage * starDamageMult * (1f + (GlobalDamageBonusPercent / 100f));

        bool isCritical = Random.value < critRate;

        if (classType == CharacterClassType.Ranged && currentEvolution == EvolutionPath.PathB)
        {
            isCritical = false;
        }
        else if (classType == CharacterClassType.Ranged && currentEvolution == EvolutionPath.PathA)
        {
            Enemy targetEnemy = target.GetComponent<Enemy>();
            if (targetEnemy != null && (targetEnemy.archetype == EnemyArchetype.Tank || targetEnemy.archetype == EnemyArchetype.Boss))
            {
                isCritical = true;
                calculatedDamage *= (critDamage * 1.5f);
            }
        }
        else if (isCritical)
        {
            calculatedDamage *= critDamage;
        }

        if (classType == CharacterClassType.Fighter && currentEvolution == EvolutionPath.PathA)
        {
            SpawnPiercingClaw(target, calculatedDamage, isCritical);
        }
        else if (classType == CharacterClassType.Fighter && currentEvolution == EvolutionPath.PathB && fighterAttackCount % 4 == 0)
        {
            StartCoroutine(TripleClawRoutine(target, calculatedDamage, isCritical));
        }
        else if (classType == CharacterClassType.Tank && currentEvolution == EvolutionPath.PathB)
        {
            SpawnShotgunCone(target, calculatedDamage, isCritical);
        }
        else
        {
            SpawnProjectile(target, calculatedDamage, isCritical);
        }

        if (bounceCoroutine != null) StopCoroutine(bounceCoroutine);
        bounceCoroutine = StartCoroutine(BounceEffect());
    }

    void SpawnProjectile(Transform target, float dmg, bool isCritical)
    {
        GameObject projGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectile = projGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage = dmg;
            projectile.isCrit = isCritical;
            projectile.shooterClass = classType;
            projectile.evolution = currentEvolution;

            bool isAoE = (classType == CharacterClassType.Mage);
            float splashRadius = (starLevel >= 3) ? 2.2f : 1.5f;

            if (classType == CharacterClassType.Mage && currentEvolution == EvolutionPath.PathB)
            {
                splashRadius = 3.8f;
            }

            projectile.Setup(target, isAoE, splashRadius);
        }
    }

    void SpawnPiercingClaw(Transform target, float dmg, bool isCritical)
    {
        GameObject projGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projGO.transform.localScale = new Vector3(0.5f, 2.0f, 1f);
        Projectile projectile = projGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage = dmg;
            projectile.isCrit = isCritical;
            projectile.isPiercing = true;
            projectile.shooterClass = classType;
            projectile.evolution = currentEvolution;
            projectile.Setup(target, false);
        }
    }

    void SpawnShotgunCone(Transform target, float dmg, bool isCritical)
    {
        float[] angles = { -15f, 0f, 15f };

        foreach (float ang in angles)
        {
            GameObject projGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile p = projGO.GetComponent<Projectile>();
            if (p != null)
            {
                p.damage = dmg * 0.7f;
                p.isCrit = isCritical;
                p.shooterClass = classType;
                p.evolution = currentEvolution;
                p.Setup(target, false);
            }
        }
    }

    IEnumerator TripleClawRoutine(Transform target, float dmg, bool isCritical)
    {
        for (int i = 0; i < 3; i++)
        {
            if (target != null) SpawnProjectile(target, dmg * 0.75f, isCritical);
            yield return new WaitForSeconds(0.08f);
        }
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

    public void ApplyEvolution(EvolutionPath path)
    {
        currentEvolution = path;
        Debug.Log($"[{classType}] Berevolusi ke {path}!");

        if (spriteRenderer != null)
        {
            if (path == EvolutionPath.PathA) spriteRenderer.color = Color.magenta;
            else if (path == EvolutionPath.PathB) spriteRenderer.color = Color.cyan;
        }

        if (evolutionBadgeText != null)
        {
            evolutionBadgeText.text = (path == EvolutionPath.PathA) ? "[A]" : "[B]";
            evolutionBadgeText.color = (path == EvolutionPath.PathA) ? Color.magenta : Color.cyan;
        }
        else if (starText3D != null)
        {
            string badge = (path == EvolutionPath.PathA) ? "A" : "B";
            starText3D.text = $"{starLevel} ({badge})";
        }
    }
}