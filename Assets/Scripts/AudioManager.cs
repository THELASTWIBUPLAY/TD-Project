using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmNormalSource;
    [SerializeField] private AudioSource bgmBossSource;
    [SerializeField] private AudioSource bgmMainMenuSource;

    [Header("Mute UI Feedback")]
    public TextMeshProUGUI muteButtonText;

    [Header("Class Shoot SFX")]
    public AudioClip sfxRanger;
    public AudioClip sfxSniper;
    public AudioClip sfxBombardier;
    public AudioClip sfxCryo;
    public AudioClip sfxGunslinger;

    [Header("Game Event SFX & BGM Clips")]
    public AudioClip sfxIncomingBoss;
    [Tooltip("Musik saat di BootScene dan MainMenu")]
    public AudioClip menuBgm;
    [Tooltip("Musik saat pertarungan normal di Gameplay")]
    public AudioClip bgmMusic;
    [Tooltip("Musik saat Boss muncul di Gameplay")]
    public AudioClip bossBgm;

    [Header("BGM Volume Settings")]
    [Range(0f, 1f)] public float defaultBgmVolume = 0.45f;
    [Tooltip("Persentase volume BGM saat di BootScene (0.35 = 35% dari volume normal)")]
    [Range(0.05f, 1f)] public float bootVolumeMultiplier = 0.8f;

    private float userSettingsVolume = 1f;
    private float currentSceneMultiplier = 1f;
    private Coroutine transitionCoroutine;

    private float lastShootSoundTime;
    public float minSoundInterval = 0.05f;
    private bool isMuted = false;

    private float EffectiveBgmVolume => defaultBgmVolume * userSettingsVolume * currentSceneMultiplier;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Matikan semua suara pada objek duplikat sebelum dihancurkan agar tidak bocor
            AudioSource[] duplicateSources = GetComponentsInChildren<AudioSource>();
            foreach (var src in duplicateSources) src.Stop();

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        userSettingsVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        AudioListener.volume = userSettingsVolume;

        SetupAudioSources();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void SetupAudioSources()
    {
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        if (bgmNormalSource == null) bgmNormalSource = gameObject.AddComponent<AudioSource>();
        bgmNormalSource.loop = true;
        bgmNormalSource.playOnAwake = false;
        bgmNormalSource.priority = 0;

        if (bgmBossSource == null) bgmBossSource = gameObject.AddComponent<AudioSource>();
        bgmBossSource.loop = true;
        bgmBossSource.playOnAwake = false;
        bgmBossSource.priority = 0;

        if (bgmMainMenuSource == null) bgmMainMenuSource = gameObject.AddComponent<AudioSource>();
        bgmMainMenuSource.loop = true;
        bgmMainMenuSource.playOnAwake = false;
        bgmMainMenuSource.priority = 0;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Start()
    {
        UpdateMuteUI();
        // Evaluasi scene tempat AudioManager pertama kali dinyalakan
        EvaluateSceneBgm(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EvaluateSceneBgm(scene.name);
    }

    private void EvaluateSceneBgm(string sceneName)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        // Segarkan nilai volume dari PlayerPrefs setiap ganti scene
        userSettingsVolume = PlayerPrefs.GetFloat("GameVolume", 1f);

        string cleanName = sceneName.Replace(" ", "").ToLower();

        if (cleanName.Contains("boot"))
        {
            currentSceneMultiplier = bootVolumeMultiplier;
            PlayMenuBgm();
        }
        else if (cleanName.Contains("mainmenu") || cleanName == "menu")
        {
            currentSceneMultiplier = 1f;
            PlayMenuBgm();
        }
        else
        {
            currentSceneMultiplier = 1f;
            PlayBGM();
        }
    }

    // =========================================================================
    // BGM CONTROLLERS (MENU, NORMAL GAMEPLAY, BOSS)
    // =========================================================================

    public void PlayMenuBgm()
    {
        // Matikan mutlak musik Gameplay Normal & Boss
        StopAndClearSource(bgmNormalSource);
        StopAndClearSource(bgmBossSource);

        if (menuBgm == null)
        {
            Debug.LogWarning("[AudioManager] Clip 'menuBgm' belum diisi di Inspector AudioManager!");
            return;
        }

        // Jika lagu menu suda berputar (misal transisi dari BootScene ke MainMenu),
        // jangan di-restart! Cukup sesuaikan volumenya dengan mulus.
        if (bgmMainMenuSource.isPlaying && bgmMainMenuSource.clip == menuBgm)
        {
            transitionCoroutine = StartCoroutine(FadeAudioSourceVolume(bgmMainMenuSource, EffectiveBgmVolume, 0.5f));
            return;
        }

        // Jika belum berputar, mainkan dari awal
        bgmMainMenuSource.clip = menuBgm;
        bgmMainMenuSource.volume = EffectiveBgmVolume;
        bgmMainMenuSource.Play();
    }

    public void PlayBGM()
    {
        // Matikan mutlak musik Menu & Boss
        StopAndClearSource(bgmMainMenuSource);
        StopAndClearSource(bgmBossSource);

        if (bgmMusic == null)
        {
            Debug.LogWarning("[AudioManager] Clip 'bgmMusic' (Gameplay BGM) belum diisi di Inspector AudioManager!");
            return;
        }

        // Mainkan musik Gameplay Normal
        bgmNormalSource.clip = bgmMusic;
        bgmNormalSource.volume = EffectiveBgmVolume;
        bgmNormalSource.Play();
    }

    public void PlayBossBGM()
    {
        if (bossBgm == null)
        {
            Debug.LogWarning("[AudioManager] Clip 'bossBgm' belum diisi di Inspector AudioManager!");
            return;
        }

        // Pastikan lagu menu mati jika karena alasan tertentu masih aktif
        StopAndClearSource(bgmMainMenuSource);

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(ToBossRoutine());
    }

    public void ReturnToNormalBGM()
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(ToNormalRoutine());
    }

    private void StopAndClearSource(AudioSource source)
    {
        if (source != null)
        {
            source.Stop();
            source.clip = null;
        }
    }

    // =========================================================================
    // TRANSITION COROUTINES
    // =========================================================================

    private IEnumerator FadeAudioSourceVolume(AudioSource source, float targetVol, float duration)
    {
        if (source == null) yield break;
        float startVol = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVol, targetVol, elapsed / duration);
            yield return null;
        }

        source.volume = targetVol;
        transitionCoroutine = null;
    }

    private IEnumerator ToBossRoutine()
    {
        float duration = 0.6f;
        float elapsed = 0f;
        float startNormalVol = bgmNormalSource.volume;
        float targetVol = EffectiveBgmVolume;

        bgmBossSource.clip = bossBgm;
        bgmBossSource.volume = 0f;
        bgmBossSource.Play();

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            bgmNormalSource.volume = Mathf.Lerp(startNormalVol, 0f, t);
            bgmBossSource.volume = Mathf.Lerp(0f, targetVol, t);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        bgmBossSource.volume = targetVol;
        bgmNormalSource.volume = 0f;
        bgmNormalSource.Pause();
        transitionCoroutine = null;
    }

    private IEnumerator ToNormalRoutine()
    {
        float duration = 1.0f;
        float elapsed = 0f;
        float startBossVol = bgmBossSource.volume;
        float targetVol = EffectiveBgmVolume;

        if (!bgmNormalSource.isPlaying)
        {
            bgmNormalSource.UnPause();
            if (!bgmNormalSource.isPlaying && bgmMusic != null)
            {
                bgmNormalSource.clip = bgmMusic;
                bgmNormalSource.Play();
            }
        }

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            bgmBossSource.volume = Mathf.Lerp(startBossVol, 0f, t);
            bgmNormalSource.volume = Mathf.Lerp(0f, targetVol, t);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        bgmNormalSource.volume = targetVol;
        bgmBossSource.volume = 0f;
        bgmBossSource.Stop();
        transitionCoroutine = null;
    }

    // =========================================================================
    // SFX & VOLUME SETTINGS SYNC
    // =========================================================================

    /// <summary>
    /// Dipanggil langsung oleh SettingsManager saat pemain menggeser slider volume.
    /// </summary>
    public void ApplySettingsVolume(float sliderVolume)
    {
        userSettingsVolume = Mathf.Clamp01(sliderVolume);
        AudioListener.volume = userSettingsVolume;

        // Perbarui langsung volume BGM yang sedang aktif saat ini
        float targetVol = EffectiveBgmVolume;
        if (bgmMainMenuSource != null && bgmMainMenuSource.isPlaying)
            bgmMainMenuSource.volume = targetVol;
        if (bgmNormalSource != null && bgmNormalSource.isPlaying)
            bgmNormalSource.volume = targetVol;
        if (bgmBossSource != null && bgmBossSource.isPlaying)
            bgmBossSource.volume = targetVol;
    }

    public void PlayClassShootSFX(CharacterClassType classType)
    {
        if (isMuted) return;

        if (Time.unscaledTime - lastShootSoundTime < minSoundInterval) return;
        lastShootSoundTime = Time.unscaledTime;

        AudioClip clip = classType switch
        {
            CharacterClassType.Fighter => sfxRanger,
            CharacterClassType.Ranged => sfxSniper,
            CharacterClassType.Mage => sfxBombardier,
            CharacterClassType.Support => sfxCryo,
            CharacterClassType.Tank => sfxGunslinger,
            _ => sfxRanger
        };

        if (clip != null)
        {
            sfxSource.pitch = Random.Range(0.92f, 1.08f);
            sfxSource.PlayOneShot(clip, 0.6f * userSettingsVolume);
        }
    }

    public void PlayBossIncomingSFX()
    {
        if (isMuted || sfxIncomingBoss == null) return;
        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(sfxIncomingBoss, 1f * userSettingsVolume);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        if (bgmNormalSource != null) bgmNormalSource.mute = isMuted;
        if (bgmBossSource != null) bgmBossSource.mute = isMuted;
        if (bgmMainMenuSource != null) bgmMainMenuSource.mute = isMuted;
        if (sfxSource != null) sfxSource.mute = isMuted;

        UpdateMuteUI();
    }

    public void SetMuteButtonText(TextMeshProUGUI newTextComponent)
    {
        muteButtonText = newTextComponent;
        UpdateMuteUI();
    }

    void UpdateMuteUI()
    {
        if (muteButtonText != null)
        {
            muteButtonText.text = isMuted ? "ON" : "OFF";
            muteButtonText.ForceMeshUpdate();
        }
    }
}