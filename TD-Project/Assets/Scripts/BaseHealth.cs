using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
public class BaseHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI References")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;

    [Header("Base Line Sprite Variants")]
    [Tooltip("Sprite baseline untuk layar standar (~9:16)")]
    public Sprite baselineSprite916;

    [Tooltip("Sprite baseline untuk layar jangkung (~9:19, 9:20)")]
    public Sprite baselineSprite919;

    [Header("Debuff Multipliers")]
    public float repairEffectivenessMultiplier = 1.0f;

    [Header("Collision Settings")]
    [Tooltip("Tinggi fisik collider dalam World Unit (misal: 0.5f)")]
    public float fixedColliderHeight = 0.5f;

    [Header("Base Line Size Settings")]
    [Range(0.5f, 1.0f)]
    [Tooltip("Persentase lebar gambar dibanding lebar layar (1.0 = penuh, 0.85 = 85% lebar layar)")]
    public float widthPercentage = 0.88f; 

    private bool isGameOverTriggered = false;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyBaselineSprite();
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void ApplyBaselineSprite()
    {
        if (spriteRenderer == null) return;

        float screenAspect = (float)Screen.height / Screen.width;

        if (screenAspect >= 1.95f && baselineSprite919 != null)
        {
            spriteRenderer.sprite = baselineSprite919;
        }
        else if (baselineSprite916 != null)
        {
            spriteRenderer.sprite = baselineSprite916;
        }

        if (spriteRenderer.sprite != null)
        {
            transform.localScale = Vector3.one;

            float spriteWidth = spriteRenderer.sprite.bounds.size.x;
            Camera mainCam = Camera.main;

            if (mainCam != null)
            {
                float worldScreenHeight = mainCam.orthographicSize * 2.0f;
                float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

                float scaleX = (worldScreenWidth * widthPercentage) / spriteWidth;

                transform.localScale = new Vector3(scaleX, scaleX, 1f);

                BoxCollider2D col = GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    float unscaledWidth = spriteWidth;
                    col.size = new Vector2(unscaledWidth, fixedColliderHeight / scaleX);
                    col.offset = Vector2.zero;
                }
            }
        }
    }

    public void TakeBaseDamage(float damage)
    {
        if (isGameOverTriggered) return;

        if (BaseShieldBarrier.Instance != null && BaseShieldBarrier.Instance.TryAbsorbHit())
        {
            Debug.Log($"[BaseHealth] {damage} damage berhasil ditangkis 100% oleh perisai Tank!");
            return; 
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateUI();

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.18f, 0.12f); 
        }

        if (currentHealth <= 0)
        {
            isGameOverTriggered = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }

    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {maxHealth}";
        }
    }

    public void ReduceMaxHpPermanently(float percentLoss)
    {
        float amount = maxHealth * (percentLoss / 100f);
        maxHealth = Mathf.Max(1f, maxHealth - amount);
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        UpdateUI(); 
    }

    public void ReduceMaxHpFlat(float flatLoss)
    {
        maxHealth = Mathf.Max(1f, maxHealth - flatLoss);
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        UpdateUI();
    }

    public void HealBase(float amount)
    {
        float adjustedAmount = amount * repairEffectivenessMultiplier;
        currentHealth = Mathf.Min(maxHealth, currentHealth + adjustedAmount);
        UpdateUI();
    }
}