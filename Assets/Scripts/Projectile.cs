using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 10f;
    public float damage = 10f;
    public float lifetime = 4f;

    [Header("Critical & Class")]
    public bool isCrit = false;
    public CharacterClassType shooterClass = CharacterClassType.Fighter;
    public EvolutionPath evolution = EvolutionPath.None;

    [Header("AoE Configuration")]
    public bool isAoE = false;
    public float aoeRadius = 1.5f;

    [Header("Soft Homing Config")]
    public float turnSpeed = 280f; 
    public float leadPredictionTime = 0.25f;

    [Header("Ricochet Mechanic")]
    public static bool GlobalRicochetUnlocked = false;
    private int ricochetRemaining = 0;
    private Transform lastHitTarget;

    [Header("Piercing (Fighter Path A)")]
    public bool isPiercing = false;
    private int pierceCount = 0;

    [Header("Mage Path A Burn Puddle Prefab")]
    public GameObject burnPuddlePrefab;

    private Transform targetEnemy;
    private Rigidbody2D targetRb;
    private Vector2 currentDirection = Vector2.up;

    public void Setup(Transform target, bool isAreaDamage = false, float splashRadius = 1.5f)
    {
        targetEnemy = target;
        isAoE = isAreaDamage;
        aoeRadius = splashRadius;

        ricochetRemaining = 0;

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
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!isPiercing && targetEnemy != null)
        {
            Vector3 predictedTargetPos = targetEnemy.position;

            if (targetRb != null)
            {
                predictedTargetPos += (Vector3)(targetRb.linearVelocity * leadPredictionTime);
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

        transform.position += (Vector3)(currentDirection * speed * Time.deltaTime);
    }

    void UpdateRotation(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();

            if (isAoE)
            {
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
                    return;
                }
            }

            Destroy(gameObject);
        }
    }

    void ApplyHitEffects(Enemy enemy, float dmg)
    {
        if (enemy == null) return;

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

        if (shooterClass == CharacterClassType.Support)
        {
            float slowFactor = (evolution == EvolutionPath.PathA) ? 0.4f : 0.6f;
            enemy.moveSpeed = Mathf.Max(0.3f, enemy.moveSpeed * slowFactor);
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
    }
}