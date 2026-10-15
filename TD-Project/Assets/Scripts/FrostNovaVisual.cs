using System.Collections;
using UnityEngine;

public class FrostNovaVisual : MonoBehaviour
{
    private LineRenderer lr;
    private int segments = 24;

    void Awake()
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = segments;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = 3;

        float angleStep = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float rad = Mathf.Deg2Rad * (i * angleStep);
            lr.SetPosition(i, new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f));
        }
    }

    public void Play(float maxRadius, float duration)
    {
        StartCoroutine(AnimateRing(maxRadius, duration));
    }

    private IEnumerator AnimateRing(float maxRadius, float duration)
    {
        float elapsed = 0f;
        Color iceCyan = new Color(0.3f, 0.85f, 1f, 0.9f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentScale = Mathf.Lerp(0.1f, maxRadius, Mathf.Sin(t * Mathf.PI * 0.5f));
            transform.localScale = new Vector3(currentScale, currentScale, 1f);

            float width = Mathf.Lerp(0.12f, 0.01f, t);
            lr.startWidth = width;
            lr.endWidth = width;

            Color c = iceCyan;
            c.a = Mathf.Lerp(0.85f, 0f, t);
            lr.startColor = c;
            lr.endColor = c;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}