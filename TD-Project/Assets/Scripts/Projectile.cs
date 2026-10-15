using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 10f;
    public float damage = 10f;
    public float lifetime = 4f;

    [Header("5-Step Acceleration Config")]
    public float[] speedSteps = { 8f, 14f, 20f, 26f, 32f };
    public float stepDuration = 0.12f;

    [Header("Critical & Class")]
    public bool isCrit = false;
    public CharacterClassType shooterClass = CharacterClassType.Fighter;
    public EvolutionPath evolution = EvolutionPath.None;

    [Header("AoE Configuration")]
    public bool isAoE = false;
    public float aoeRadius = 1.5f;

    [Header("Soft Homing Config")]
    public float turnSpeed = 280f; 
    public float leadPredictionTime = 0.15f;

    [Header("Ricochet Mechanic")]
    public static bool GlobalRicochetUnlocked = false;
    private int ricochetRemaining = 0;
    private Transform lastHitTarget;

    [Header("Piercing (Fighter Path A)")]
    public bool isPiercing = false;
    private int pierceCount = 0;

    [Header("Mage Path A Burn Puddle Prefab")]
    public GameObject burnPuddlePrefab;

    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailRenderer trailRenderer;

    private Transform targetEnemy;
    private Rigidbody2D targetRb;
    private Vector2 currentDirection = Vector2.up;

    private float currentSpeed;
    private float timeAlive = 0f;

    private Vector3 lastPosition;
    private bool hasHitProcessed = false;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (trailRenderer == null) trailRenderer = GetComponentInChildren<TrailRenderer>();
    }

    public void Setup(Transform target, bool isAreaDamage = false, float splashRadius = 1.5f)
    {
        targetEnemy = target;
        isAoE = isAreaDamage;
        aoeRadius = splashRadius;
        ricochetRemaining = 0;
        hasHitProcessed = false;

        timeAlive = 0f;
        ResetTrail();

        if (speedSteps != null && speedSteps.Length > 0)
        {
            currentSpeed = speedSteps[0];
        }

        if (GlobalRicochetUnlocked && shooterClass == CharacterClassType.Fighter && !isAoE)
        {
            ricochetRemaining = 1;
        }

        if (targetEnemy != null)
        {
            targetRb = targetEnemy.GetComponent<Rigidbody2D>();
            currentDirection = (targetEnemy.position - transform.position).normalized;
            UpdateRotation(currentDirection);
        }

        lastPosition = transform.position;
    }

    public void SetColor(Color targetColor)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }

        if (trailRenderer != null)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(targetColor, 0.0f), 
                    new GradientColorKey(targetColor, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );

            trailRenderer.colorGradient = gradient;
        }
    }

    public void ResetTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    void Start()
    {
        lastPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timeAlive += Time.deltaTime;

        if (speedSteps != null && speedSteps.Length > 0)
        {
            int currentStepIndex = Mathf.FloorToInt(timeAlive / stepDuration);
            currentStepIndex = Mathf.Clamp(currentStepIndex, 0, speedSteps.Length - 1);
            currentSpeed = speedSteps[currentStepIndex];
        }

        if (!isPiercing && targetEnemy != null)
        {
            Vector3 predictedTargetPos = targetEnemy.position;

            if (targetRb != null)
            {
                Vector2 vel = targetRb.linearVelocity; 
                predictedTargetPos += (Vector3)(vel * leadPredictionTime);
            }
            else
            {
                Enemy enemyScript = targetEnemy.GetComponent<Enemy>();
                float enemySpeed = enemyScript != null ? enemyScript.moveSpeed : 1.3f;
                predictedTargetPos += Vector3.down * (enemySpeed * leadPredictionTime);
            }

            Vector2 desiredDirection = (predictedTargetPos - transform.position).normalized;
            float step = turnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            currentDirection = Vector3.RotateTowards(currentDirection, desiredDirection, step, 0f);
            UpdateRotation(currentDirection);
        }

        Vector3 moveDelta = (Vector3)(currentDirection * currentSpeed * Time.deltaTime);
        transform.position += moveDelta;

        if (!hasHitProcessed)
        {
            Vector3 currentPos = transform.position;
            float dist = Vector3.Distance(lastPosition, currentPos);
            if (dist > 0.001f)
            {
                RaycastHit2D[] hits = Physics2D.LinecastAll(lastPosition, currentPos);
                foreach (var hit in hits)
                {
                    if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                    {
                        ProcessEnemyHit(hit.collider);
                        break;
                    }
                }
            }
            lastPosition = currentPos;
        }
    }

    void UpdateRotation(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasHitProcessed && collision.CompareTag("Enemy"))
        {
            ProcessEnemyHit(collision);
        }
    }

    private void ProcessEnemyHit(Collider2D collision)
    {
        if (hasHitProcessed) return;

        Enemy enemy = collision.GetComponent<Enemy>();

        if (isAoE)
        {
            hasHitProcessed = true;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayClassShootSFX(CharacterClassType.Mage);
            }

            ExplosionEffect.Create(transform.position, aoeRadius);

            if (shooterClass == CharacterClassType.Mage && evolution == EvolutionPath.PathA)
            {
                GameObject puddleObj = new GameObject("Ignis_BurnPuddle");
                puddleObj.transform.position = transform.position;

                BurnPuddle bp = puddleObj.AddComponent<BurnPuddle>();
                bp.radius = aoeRadius;
                bp.dpsDamage = damage * 0.35f; 
                bp.duration = 4.0f;            
            }

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
            foreach (Collider2D col in hitEnemies)
            {
                if (col.CompareTag("Enemy"))
                {
                    Enemy e = col.GetComponent<Enemy>();
                    if (e != null)
                    {
                        ApplyHitEffects(e, damage);
                    }
                }
            }

            Destroy(gameObject);
            return;
        }

        if (isPiercing)
        {
            if (enemy != null)
            {
                ApplyHitEffects(enemy, damage);
                damage *= 0.90f;
                pierceCount++;
                lastPosition = transform.position; 
                if (pierceCount >= 4) Destroy(gameObject);
            }
            return;
        }

        if (enemy != null)
        {
            ApplyHitEffects(enemy, damage);
        }

        if (ricochetRemaining > 0)
        {
            ricochetRemaining--;
            lastHitTarget = collision.transform;
            Transform nextTarget = FindNextBounceTarget();

            if (nextTarget != null)
            {
                targetEnemy = nextTarget;
                targetRb = targetEnemy.GetComponent<Rigidbody2D>();
                currentDirection = (targetEnemy.position - transform.position).normalized;
                damage *= 0.60f;
                ResetTrail(); 
                lastPosition = transform.position; 
                return;
            }
        }

        hasHitProcessed = true;
        Destroy(gameObject);
    }

    void ApplyHitEffects(Enemy enemy, float dmg)
{
    if (enemy == null) return;

    if (GameManager.Instance != null)
    {
        GameManager.Instance.RecordDamage(shooterClass, dmg);
    }

    if (isCrit && CameraShake.Instance != null)
    {
        CameraShake.Instance.Shake(0.08f, 0.06f);
    }

    if (shooterClass == CharacterClassType.Tank && evolution == EvolutionPath.PathB)
    {
        enemy.ApplyKnockback(2.5f, 0.08f); 
    }

    if (shooterClass == CharacterClassType.Mage && evolution == EvolutionPath.PathB)
    {
        enemy.ApplyKnockback(3.5f, 0.1f);
    }

    if (shooterClass == CharacterClassType.Ranged && evolution == EvolutionPath.PathB)
    {
        if (enemy.archetype != EnemyArchetype.Boss)
        {
            if (enemy.currentHp <= (enemy.maxHp * 0.10f))
            {
                dmg = enemy.currentHp + 999f;
            }
        }
    }

    if (shooterClass == CharacterClassType.Support && evolution == EvolutionPath.PathA)
    {
        float splashRadius = 1.3f;

        GameObject fx = new GameObject("FrostNovaFX");
        fx.transform.position = transform.position;
        FrostNovaVisual nova = fx.AddComponent<FrostNovaVisual>();
        nova.Play(splashRadius, 0.25f); 

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splashRadius);
        foreach (var col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                Enemy nearbyEnemy = col.GetComponent<Enemy>();
                if (nearbyEnemy != null)
                {
                    nearbyEnemy.ApplyChilledSlow(0.45f, 2.5f);
                }
            }
        }
    }

    enemy.TakeDamage(dmg, isCrit);
}

    Transform FindNextBounceTarget()
    {
        Collider2D[] candidates = Physics2D.OverlapCircleAll(transform.position, 4.5f);
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var col in candidates)
        {
            if (col.CompareTag("Enemy") && col.transform != lastHitTarget)
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = col.transform;
                }
            }
        }
        return closest;
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    public void SetupDirection(Vector2 direction)
    {
        targetEnemy = null; 
        currentDirection = direction.normalized;
        UpdateRotation(currentDirection);
        ResetTrail();
        lastPosition = transform.position;
    }
}