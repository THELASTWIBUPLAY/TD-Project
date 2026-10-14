using System.Collections;
using TMPro;
using UnityEngine;

public class AlertManager : MonoBehaviour
{
    // Encapsulation: Hanya bisa diisi dari dalam class ini sendiri
    public static AlertManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup _alertCanvasGroup;
    [SerializeField] private TextMeshProUGUI _messageText;

    [Header("Animation Settings")]
    [SerializeField] private float _fadeDuration = 0.25f;

    [Header("Cooldown Settings")]
    [Tooltip("Minimal pause interval between alert")]
    [SerializeField] private float _alertCooldown = 1.5f;

    private Coroutine _currentAlertCoroutine;
    private float _nextAllowedAlertTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Pastikan objek berada di Root sebelum menerapkan DontDestroyOnLoad
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        DontDestroyOnLoad(gameObject);

        // Inisialisasi awal UI
        if (_alertCanvasGroup != null)
        {
            _alertCanvasGroup.alpha = 0f;
            _alertCanvasGroup.blocksRaycasts = false;
        }
    }

    public void Show(string message, float duration = 2f)
    {
        if (_alertCanvasGroup == null || _messageText == null)
        {
            Debug.LogWarning("[AlertManager] Referensi UI belum dipasang di Inspector!");
            return;
        }

        // Guard Clause Cooldown
        if (Time.unscaledTime < _nextAllowedAlertTime) return;

        _nextAllowedAlertTime = Time.unscaledTime + _alertCooldown;

        // Stop Animation if still on
        if (_currentAlertCoroutine != null)
        {
            StopCoroutine(_currentAlertCoroutine);
            _currentAlertCoroutine = null;
        }

        _currentAlertCoroutine = StartCoroutine(ShowAlertRoutine(message, duration));
    }

    private IEnumerator ShowAlertRoutine(string message, float displayDuration)
    {
        _messageText.text = message;

        // Fade In , stop when coroutine is called
        yield return FadeCanvasGroup(_alertCanvasGroup.alpha, 1f, _fadeDuration);

        // Hold di layar
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade Out
        yield return FadeCanvasGroup(_alertCanvasGroup.alpha, 0f, _fadeDuration);

        _currentAlertCoroutine = null;
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float targetAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _alertCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        _alertCanvasGroup.alpha = targetAlpha;
    }
}