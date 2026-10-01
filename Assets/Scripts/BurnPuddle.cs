using UnityEngine;

public class BurnPuddle : MonoBehaviour
{
    public float duration = 4.0f;
    public float tickInterval = 0.5f;
    public float dpsDamage = 15f;
    public float radius = 2.4f;

    private float timer = 0f;
    private float tickTimer = 0f;
    private SpriteRenderer sr;
    private Color baseColor = new Color(1f, 0.35f, 0.05f, 0.45f); 

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        sr.sprite = CreateCircleSprite(128);
        sr.color = baseColor;
        sr.sortingOrder = 1;
    }

    void Start()
    {
        transform.localScale = new Vector3(radius, radius, 1f);
    }
    void Update()
    {
        timer += Time.deltaTime;
        tickTimer += Time.deltaTime;

        if (sr != null)
        {
            float alpha = 0.35f + Mathf.PingPong(Time.time * 2.5f, 0.2f);
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }

        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            DealPuddleDamage();
        }

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    void DealPuddleDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                Enemy e = col.GetComponent<Enemy>();
                if (e != null)
                {
                    e.TakeDamage(dpsDamage, false);
                }
            }
        }
    }

    Sprite CreateCircleSprite(int resolution)
    {
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        float center = resolution / 2f;
        float radiusPixel = center - 1f;

        Color[] colors = new Color[resolution * resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radiusPixel)
                {
                    float edgeAlpha = Mathf.Clamp01((radiusPixel - dist) / 3f);
                    colors[y * resolution + x] = new Color(1f, 1f, 1f, edgeAlpha);
                }
                else
                {
                    colors[y * resolution + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}