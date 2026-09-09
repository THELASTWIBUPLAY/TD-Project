using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    private SpriteRenderer sr;
    private float duration = 0.25f;
    private float timer = 0f;
    private Vector3 initialScale;
    private Vector3 targetScale;
    private Color startColor = new Color(1f, 0.45f, 0.1f, 0.85f); // Oranye ledakan transparan

    public static void Create(Vector3 position, float radius)
    {
        GameObject fxObj = new GameObject("ExplosionEffect");
        fxObj.transform.position = position;

        ExplosionEffect fx = fxObj.AddComponent<ExplosionEffect>();
        fx.Init(radius);
    }

    public void Init(float radius)
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 15;

        // Buat tekstur lingkaran instan tanpa memanggil resource bawaan Unity yang sudah usang
        Texture2D tex = CreateCircleTexture(64);
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));

        sr.color = startColor;

        float diameter = radius * 2f;
        initialScale = Vector3.zero;
        targetScale = new Vector3(diameter, diameter, 1f);
        transform.localScale = initialScale;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / duration;

        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Membesar cepat lalu memudar (Fade out)
        transform.localScale = Vector3.Lerp(initialScale, targetScale, Mathf.Sin(progress * Mathf.PI * 0.5f));
        
        Color c = startColor;
        c.a = Mathf.Lerp(startColor.a, 0f, progress);
        sr.color = c;
    }

    Texture2D CreateCircleTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float r = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, dist <= r ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return tex;
    }
}