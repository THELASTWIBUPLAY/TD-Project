using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshPro textMesh;
    public float moveSpeed = 1.2f;
    public float fadeDuration = 0.5f;

    private Color startColor;
    private float elapsed = 0f;

    void Awake()
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null) startColor = textMesh.color;
    }

    public void Setup(float damageAmount)
    {
        if (textMesh != null)
        {
            textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
            textMesh.color = startColor;
        }
        elapsed = 0f;
    }

    void Update()
    {

        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        elapsed += Time.deltaTime;
        if (textMesh != null)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            Color c = textMesh.color;
            c.a = alpha;
            textMesh.color = c;
        }

        if (elapsed >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }
}