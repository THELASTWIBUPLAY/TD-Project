using System.Collections;
using UnityEngine;
using TMPro;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmNormalSource;
    [SerializeField] private AudioSource bgmBossSource;

    [Header("Mute UI Feedback")]
    public TextMeshProUGUI muteButtonText; 

    [Header("Class Shoot SFX")]
    public AudioClip sfxRanger;
    public AudioClip sfxSniper;
    public AudioClip sfxBombardier;
    public AudioClip sfxCryo;
    public AudioClip sfxGunslinger;

    [Header("Game Event SFX")]
    public AudioClip sfxIncomingBoss;
    public AudioClip bgmMusic;
    public AudioClip bossBgm;

    [Header("BGM Settings")]
    public float defaultBgmVolume = 0.45f;
    private Coroutine transitionCoroutine;

    private float lastShootSoundTime;
    public float minSoundInterval = 0.05f; 
    private bool isMuted = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (bgmNormalSource == null)
        {
            bgmNormalSource = gameObject.AddComponent<AudioSource>();
            bgmNormalSource.loop = true;
            bgmNormalSource.playOnAwake = false;
            bgmNormalSource.priority = 0;
        }

        if (bgmBossSource == null)
        {
            bgmBossSource = gameObject.AddComponent<AudioSource>();
            bgmBossSource.loop = true;
            bgmBossSource.playOnAwake = false;
            bgmBossSource.priority = 0;
        }
    }

    void Start()
    {
        UpdateMuteUI();
        PlayBGM();
    }

    public void PlayBGM()
    {
        if (bgmMusic != null && bgmNormalSource != null)
        {
            bgmNormalSource.clip = bgmMusic;
            bgmNormalSource.volume = defaultBgmVolume;
            bgmNormalSource.Play();
        }
    }

    public void PlayBossBGM()
    {
        if (bossBgm == null) return;

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(ToBossRoutine());
    }

    public void ReturnToNormalBGM()
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(ToNormalRoutine());
    }

    private IEnumerator ToBossRoutine()
    {
        float duration = 0.6f;
        float elapsed = 0f;
        float startNormalVol = bgmNormalSource.volume;

        bgmBossSource.clip = bossBgm;
        bgmBossSource.volume = 0f;
        bgmBossSource.Play();

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            bgmNormalSource.volume = Mathf.Lerp(startNormalVol, 0f, t);
            bgmBossSource.volume = Mathf.Lerp(0f, defaultBgmVolume, t);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        bgmBossSource.volume = defaultBgmVolume;
        bgmNormalSource.volume = 0f;
        bgmNormalSource.Pause();
        transitionCoroutine = null;
    }

    private IEnumerator ToNormalRoutine()
    {
        float duration = 1.0f;
        float elapsed = 0f;
        float startBossVol = bgmBossSource.volume;

        bgmNormalSource.UnPause();

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            bgmBossSource.volume = Mathf.Lerp(startBossVol, 0f, t);
            bgmNormalSource.volume = Mathf.Lerp(0f, defaultBgmVolume, t);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        bgmNormalSource.volume = defaultBgmVolume;
        bgmBossSource.volume = 0f;
        bgmBossSource.Stop();
        transitionCoroutine = null;
    }

    public void PlayClassShootSFX(CharacterClassType classType)
    {
        if (isMuted) return;

        if (Time.unscaledTime - lastShootSoundTime < minSoundInterval) return;
        lastShootSoundTime = Time.unscaledTime;

        AudioClip clip = classType switch
        {
            CharacterClassType.Ranger => sfxRanger,
            CharacterClassType.Sniper => sfxSniper,
            CharacterClassType.Bombardier => sfxBombardier,
            CharacterClassType.Cryo => sfxCryo,
            CharacterClassType.Gunslinger => sfxGunslinger,
            _ => sfxRanger
        };

        if (clip != null)
        {
            sfxSource.pitch = Random.Range(0.92f, 1.08f);
            sfxSource.PlayOneShot(clip, 0.6f);
        }
    }

    public void PlayBossIncomingSFX()
    {
        if (isMuted || sfxIncomingBoss == null) return;
        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(sfxIncomingBoss, 1f);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        if (bgmNormalSource != null) bgmNormalSource.mute = isMuted;
        if (bgmBossSource != null) bgmBossSource.mute = isMuted;
        if (sfxSource != null) sfxSource.mute = isMuted;

        UpdateMuteUI();
    }

    void UpdateMuteUI()
    {
        if (muteButtonText != null)
        {
            muteButtonText.text = isMuted ? "Mute: ON" : "Mute: OFF";
            muteButtonText.ForceMeshUpdate(); 
        }
        else
        {
            Debug.LogWarning("[AudioManager] Slot Mute Button Text masih kosong di Inspector!");
        }
    }
}