using System.Collections;
using UnityEngine;
using TMPro;

public class Character : MonoBehaviour
{
    [Header("Class & Star Config")]
    public CharacterClassType classType = CharacterClassType.Melee;
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

    private Vector3 basePresetScale = new Vector3(0.6f, 0.6f, 1f);
    private float fireCountdown = 0f;

    private CharacterAnimator charAnim;

    public bool isAttacking = false;

    private GameObject projectileVfx;
    private GameObject auraVfx;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (transform.localScale != Vector3.zero)
        {
            basePresetScale = transform.localScale;
        }
        charAnim = GetComponentInChildren<CharacterAnimator>();
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

        if (charAnim != null)
        {
            charAnim.InitializeAnimator();
        }
    }

    public void ApplyClassStats()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        switch (classType)
        {
            case CharacterClassType.Melee: //Ranger
                baseAttackDamage = 12f;
                baseAttackCooldown = 0.65f;
                attackRange = 7.5f;
                break;

            case CharacterClassType.Ranger: //Sniper
                baseAttackDamage = 45f;
                baseAttackCooldown = 1.6f;
                attackRange = 10f; 
                break;

            case CharacterClassType.Mage: //Bombardier
                baseAttackDamage = 25f;
                baseAttackCooldown = 1.1f;
                attackRange = 6.5f; 
                break;

            case CharacterClassType.Support: //Cryo
                baseAttackDamage = 8f;
                baseAttackCooldown = 0.8f;
                attackRange = 7f; 
                break;

            case CharacterClassType.Tank: //Gunslinger
                baseAttackDamage = 6f;
                baseAttackCooldown = 0.25f; 
                attackRange = 5.2f;
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
                charAnim.PlayAnim();
                fireCountdown = Mathf.Max(0.08f, currentCooldown);
            }
        }
    }

    public void SetStarLevel(int newLevel)
    {
        starLevel = Mathf.Clamp(newLevel, 1, 3);
        UpdateStarDisplay();

        if (charAnim != null)
        {
            charAnim.InitializeAnimator();
        }
    }

    public void UpdateStarDisplay()
    {
        if (starText3D != null)
        {
            starText3D.text = starLevel.ToString();
        }

        float scaleMult = 1f + ((starLevel - 1) * 0.2f);
        transform.localScale = basePresetScale * scaleMult;

        UpdateVfx();
    }

    Transform PickTargetForClass()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        Transform bestEnemy = null;

        if (classType == CharacterClassType.Ranger)
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

    public void UpdateVfx()
    {
        projectileVfx = VfxManager.Instance.GetProjectilePrefab(classType, starLevel);

        UpdateAuraVfx();
    }

    private void UpdateAuraVfx()
    {
        if (auraVfx != null)
        {
            Destroy(auraVfx);
        }

        GameObject auraPrefab = VfxManager.Instance.GetAuraPrefab(classType, starLevel);
        if (auraPrefab != null)
        {
            auraVfx = Instantiate(auraPrefab, transform.position, Quaternion.identity, transform);
            auraVfx.transform.localPosition = Vector3.zero;
            auraVfx.transform.localRotation = Quaternion.identity;
        }
    }

    void Shoot(Transform target)
    {
        if (target == null || projectilePrefab == null) return;

        if (classType != CharacterClassType.Mage && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClassShootSFX(classType);
        }

        float starDamageMult = starLevel == 1 ? 1f : (starLevel == 2 ? 2.2f : 4.5f);
        float finalDamage = baseAttackDamage * starDamageMult * (1f + (GlobalDamageBonusPercent / 100f));

        if (starLevel >= 3 && classType == CharacterClassType.Ranger)
        {
            Enemy targetEnemyComp = target.GetComponent<Enemy>();
            if (targetEnemyComp != null && (targetEnemyComp.archetype == EnemyArchetype.Tank || targetEnemyComp.archetype == EnemyArchetype.Boss))
            {
                finalDamage *= 1.8f;
            }
        }

        if (starLevel >= 3 && classType == CharacterClassType.Melee)
        {
            StartCoroutine(DoubleTapRoutine(target, finalDamage));
        }
        else
        {
            SpawnProjectile(target, finalDamage);
        }
    }

    void SpawnProjectile(Transform target, float dmg)
    {
        GameObject projGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectile = projGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage = dmg;

            bool isAoE = (classType == CharacterClassType.Mage);
            float splashRadius = (starLevel >= 3) ? 2.2f : 1.5f;

            projectile.Setup(target, isAoE, splashRadius, this, projectileVfx);
        }
    }

    IEnumerator DoubleTapRoutine(Transform target, float dmg)
    {
        SpawnProjectile(target, dmg);
        yield return new WaitForSeconds(0.12f);
        if (target != null) SpawnProjectile(target, dmg);
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