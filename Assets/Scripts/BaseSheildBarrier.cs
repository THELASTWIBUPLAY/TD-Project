using UnityEngine;

public class BaseShieldBarrier : MonoBehaviour
{
    public static BaseShieldBarrier Instance { get; private set; }

    [Header("Shield Config")]
    public float rechargeTime = 60f; 
    public bool isShieldActive = true;
    public Vector2 barrierSize = new Vector2(6.5f, 0.1f);

    private float currentCooldown = 0f;
    private SpriteRenderer sr;

    void Awake()
    {
        Instance = this;
        sr = gameObject.AddComponent<SpriteRenderer>();

        Texture2D tex = Texture2D.whiteTexture;
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
        sr.sortingOrder = 2;
        transform.localScale = new Vector3(barrierSize.x, barrierSize.y, 1f);
    }

    void Start()
    {
        ActivateShield();
    }

    void Update()
    {
        if (isShieldActive)
        {
            float alpha = 0.5f + Mathf.PingPong(Time.time * 2f, 0.35f);
            sr.color = new Color(0.2f, 0.8f, 1f, alpha); 
        }
        else
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0f)
            {
                ActivateShield();
            }
        }
    }

    public void ActivateShield()
    {
        isShieldActive = true;
        currentCooldown = 0f;
        sr.enabled = true;
        Debug.Log("[Shield] Perisai Aegis Base siap menahan 1 serangan!");
    }

    public bool TryAbsorbHit()
    {
        if (isShieldActive)
        {
            isShieldActive = false;
            currentCooldown = rechargeTime;
            sr.enabled = false;

            Debug.Log("[Shield] Perisai pecah menahan serangan musuh! Mulai recharge 60 detik.");

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.15f, 0.05f);
            }

            return true;
        }

        return false;
    }
}