using UnityEngine;
using TMPro; // Tambahkan ini

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("Mute UI Feedback")]
    public TextMeshProUGUI muteButtonText; // Slot untuk teks tombol Mute

    [Header("Class Shoot SFX")]
    public AudioClip sfxRanger;
    public AudioClip sfxSniper;
    public AudioClip sfxBombardier;
    public AudioClip sfxCryo;
    public AudioClip sfxGunslinger;

    [Header("Game Event SFX")]
    public AudioClip sfxIncomingBoss;
    public AudioClip bgmMusic;

    private float lastShootSoundTime;
    public float minSoundInterval = 0.05f; // Jeda 50ms antar tembakan
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

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.priority = 0; // Prioritas absolut, BGM tidak akan pernah kena cut!
        }
        else
        {
            bgmSource.priority = 0;
        }
    }

    void Start()
    {
        UpdateMuteUI();
        PlayBGM();
    }

    public void PlayBGM()
    {
        if (bgmMusic != null && bgmSource != null)
        {
            bgmSource.clip = bgmMusic;
            bgmSource.volume = 0.45f;
            bgmSource.Play();
        }
    }

    public void PlayClassShootSFX(CharacterClassType classType)
    {
        if (isMuted) return;

        // Cegah penumpukan suara di frame yang sama
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
        if (bgmSource != null) bgmSource.mute = isMuted;
        if (sfxSource != null) sfxSource.mute = isMuted;

        UpdateMuteUI();
    }

    void UpdateMuteUI()
    {
        if (muteButtonText != null)
        {
            muteButtonText.text = isMuted ? "Mute: ON" : "Mute: OFF";
            muteButtonText.ForceMeshUpdate(); // Memaksa TMP me-render ulang seketika
        }
        else
        {
            Debug.LogWarning("[AudioManager] Slot Mute Button Text masih kosong di Inspector!");
        }
    }
}