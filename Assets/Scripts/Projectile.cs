using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 10f;
    public float damage = 10f;
    public float lifetime = 4f;

    [Header("Soft Homing Config")]
    [Tooltip("Semakin tinggi angkanya, semakin tajam beloknya. Nilai 200-350 terasa sangat natural.")]
    public float turnSpeed = 280f; 

    [Tooltip("Waktu prediksi pergerakan musuh ke depan (dalam detik).")]
    public float leadPredictionTime = 0.25f;

    private Transform targetEnemy;
    private Rigidbody2D targetRb;
    private Vector2 currentDirection = Vector2.up;

    public void Setup(Transform target)
    {
        targetEnemy = target;

        if (targetEnemy != null)
        {
            // Ambil Rigidbody2D musuh jika ada untuk membaca kecepatannya secara akurat
            targetRb = targetEnemy.GetComponent<Rigidbody2D>();

            // Arahkan tembakan awal langsung ke target
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
        // 1. Tentukan arah target yang ingin dituju
        if (targetEnemy != null)
        {
            Vector3 predictedTargetPos = targetEnemy.position;

            // Tambahkan prediksi ke mana musuh akan berada
            if (targetRb != null)
            {
                predictedTargetPos += (Vector3)(targetRb.linearVelocity * leadPredictionTime);
            }
            else
            {
                // Jika musuh bergerak via Transform.Translate ke bawah
                Enemy enemyScript = targetEnemy.GetComponent<Enemy>();
                float enemySpeed = enemyScript != null ? enemyScript.moveSpeed : 1.3f;
                predictedTargetPos += Vector3.down * (enemySpeed * leadPredictionTime);
            }

            Vector2 desiredDirection = (predictedTargetPos - transform.position).normalized;

            // 2. Berbelok bertahap menggunakan RotateTowards (tidak patah/lengket)
            float step = turnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            currentDirection = Vector3.RotateTowards(currentDirection, desiredDirection, step, 0f);
            
            UpdateRotation(currentDirection);
        }

        // 3. Maju terus searah orientasi peluru saat ini
        transform.position += (Vector3)(currentDirection * speed * Time.deltaTime);
    }

    void UpdateRotation(Vector2 dir)
    {
        // Ujung proyektil selalu menghadap ke arah jalurnya
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}