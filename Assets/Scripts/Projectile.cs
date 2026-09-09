using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 10f;
    public float damage = 10f;
    public float lifetime = 4f;

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

    private Transform targetEnemy;
    private Rigidbody2D targetRb;
    private Vector2 currentDirection = Vector2.up;

    // Overload 1: jika hanya passing target tunggal
    public void Setup(Transform target)
    {
        Setup(target, false, 1.5f);
    }

    // Overload 2: implementasi utama
    public void Setup(Transform target, bool isAreaDamage, float splashRadius = 1.5f)
    {
        targetEnemy = target;
        isAoE = isAreaDamage;
        aoeRadius = splashRadius;

        // Kunci ke 0 dulu agar tidak membawa data sisa prefab
        ricochetRemaining = 0;

        // Beri jatah 1x pantulan HANYA jika kartu Ricochet memang sudah aktif
        if (GlobalRicochetUnlocked && !isAoE)
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
        if (targetEnemy != null)
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
            if (isAoE)
            {
                // Mainkan SFX ledakan
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayClassShootSFX(CharacterClassType.Bombardier);
                }

                // --- MUNCULKAN VISUAL RADIUS LEDAKAN ---
                ExplosionEffect.Create(transform.position, aoeRadius);
                // ----------------------------------------

                // Berikan damage ke seluruh musuh di radius ledakan
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
                foreach (Collider2D col in hitEnemies)
                {
                    if (col.CompareTag("Enemy"))
                    {
                        Enemy e = col.GetComponent<Enemy>();
                        if (e != null)
                        {
                            e.TakeDamage(damage);
                        }
                    }
                }

                Destroy(gameObject);
                return;
            }

            // Hit target tunggal untuk kelas selain Bombardier
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Cek pantulan Ricochet jika bukan AoE
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
                    damage *= 0.75f;
                    return;
                }
            }

            Destroy(gameObject);
        }
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

    private void OnDrawGizmosSelected()
    {
        if (isAoE)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }
}