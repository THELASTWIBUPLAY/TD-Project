using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 originalPos;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        originalPos = transform.localPosition;
    }

    /// <summary>
    /// Memulai guncangan kamera.
    /// </summary>
    /// <param name="duration">Durasi guncangan (detik)</param>
    /// <param name="magnitude">Kekuatan getaran</param>
    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalPos;
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Gunakan unscaledDeltaTime agar shake tetap bekerja normal walau di speed 0.5x atau 5x
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }
}