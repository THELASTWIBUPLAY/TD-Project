using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Attributes")]
    public float maxHp = 15f;          // Dibuat lebih empuk (awalnya 20f)
    private float currentHp;
    public float moveSpeed = 1.3f;      // Gerakan sedikit diperlambat (awalnya 2f)
    public float damageToBase = 5f;     // Damage ke base dikurangi (awalnya 10f)
    public int expReward = 8;           // EXP dinaikkan sedikit biar cepat level-up awal

    // Multiplier global dari buff kartu
    public static float GlobalHpMultiplier = 1f;
    public static float GlobalSpeedMultiplier = 1f;
    public static float GlobalDamageMultiplier = 1f;
    public static float GlobalExpMultiplier = 1f;

    // Reset saat ganti scene / game mulai ulang
    public static void ResetGlobalStats()
    {
        GlobalHpMultiplier = 1f;
        GlobalSpeedMultiplier = 1f;
        GlobalDamageMultiplier = 1f;
        GlobalExpMultiplier = 1f;
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
    }

    private bool isDead = false;

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHp -= damageAmount;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null)
        {
            // Ambil nomor wave aktif (default ke 1 jika WaveManager belum siap)
            int wave = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 1;

            // Rumus EXP: Base EXP + bonus flat per wave (misal +3 EXP per kenaikan wave)
            // Contoh base 8: Wave 1 = 8, Wave 2 = 11, Wave 3 = 14, Wave 5 = 20, dst.
            float waveExpBonus = (wave - 1) * 3f;
            float totalBaseExp = expReward + waveExpBonus;

            // Kalikan dengan buff EXP dari kartu Bounty Hunter jika ada
            int finalExp = Mathf.RoundToInt(totalBaseExp * GlobalExpMultiplier);

            GameManager.Instance.AddExp(finalExp);
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDefeated(gameObject);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Base"))
        {
            isDead = true;

            BaseHealth baseHealth = collision.GetComponent<BaseHealth>();
            if (baseHealth != null)
            {
                baseHealth.TakeBaseDamage(damageToBase);
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnEnemyDefeated(gameObject);
            }

            Destroy(gameObject);
        }
    }
}