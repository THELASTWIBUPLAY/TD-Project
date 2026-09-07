using System.Collections;
using UnityEngine;
using TMPro;

public class Character : MonoBehaviour
{
    [Header("Star Level")]
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

    [Header("Bouncy Animation")]
    public float bounceDuration = 0.15f;
    private Vector3 basePresetScale = new Vector3(0.6f, 0.6f, 1f); // Ukuran standar karakter
    private Coroutine bounceCoroutine;
    private float fireCountdown = 0f;

    void Awake()
    {
        // Pastikan skala dasar tersimpan aman sejak awal instansiasi
        if (transform.localScale != Vector3.zero)
        {
            basePresetScale = transform.localScale;
        }
    }

    void Start()
    {
        UpdateStarDisplay();
    }

    void Update()
    {
        fireCountdown -= Time.deltaTime;

        float starSpeedMult = starLevel == 1 ? 1f : (starLevel == 2 ? 1.3f : 1.8f);
        float currentCooldown = baseAttackCooldown / (GlobalAtkSpeedMultiplier * starSpeedMult);

        if (fireCountdown <= 0f)
        {
            Transform target = FindClosestEnemy();
            if (target != null)
            {
                Shoot(target);
                fireCountdown = Mathf.Max(0.1f, currentCooldown);
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

        // Sedikit perbesar karakter saat naik bintang, tapi tetap berbasis pada ukuran dasar
        float scaleMult = 1f + ((starLevel - 1) * 0.2f);
        transform.localScale = basePresetScale * scaleMult;
    }

    Transform FindClosestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        Transform lowestEnemy = null;
        float lowestY = Mathf.Infinity;

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                if (col.transform.position.y < lowestY)
                {
                    lowestY = col.transform.position.y;
                    lowestEnemy = col.transform;
                }
            }
        }
        return lowestEnemy;
    }

    void Shoot(Transform target)
    {
        if (target == null || projectilePrefab == null) return;

        GameObject projGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectile = projGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            float starDamageMult = starLevel == 1 ? 1f : (starLevel == 2 ? 2.2f : 4.5f);
            float finalDamage = baseAttackDamage * starDamageMult * (1f + (GlobalDamageBonusPercent / 100f));

            projectile.damage = finalDamage;
            projectile.Setup(target);
        }

        if (bounceCoroutine != null) StopCoroutine(bounceCoroutine);
        bounceCoroutine = StartCoroutine(BounceEffect());
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}