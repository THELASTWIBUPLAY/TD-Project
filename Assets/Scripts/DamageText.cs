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

    public void Setup(float damageAmount, bool isCrit = false)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        if (textMesh != null)
        {
            int roundedDamage = Mathf.RoundToInt(damageAmount);

            if (isCrit)
            {
                textMesh.text = $"{roundedDamage}!";
                textMesh.fontSize = 6.5f; 
                textMesh.color = new Color(1f, 0.8f, 0.1f); 
            }
            else
            {
                textMesh.text = roundedDamage.ToString();
                textMesh.fontSize = 4.5f; 
                textMesh.color = startColor;
            }
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