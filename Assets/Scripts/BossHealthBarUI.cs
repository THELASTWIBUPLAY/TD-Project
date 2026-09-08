using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    public GameObject bossBarContainer;
    public Slider bossSlider;
    public TextMeshProUGUI bossNameText;

    void Awake()
    {
        // Daftarkan event di Awake agar tetap menangkap sinyal
        Enemy.OnBossHpChanged += HandleBossHpChanged;
        Enemy.OnBossDefeatedEvent += HandleBossDefeated;

        if (bossBarContainer != null)
        {
            bossBarContainer.SetActive(false);
        }
    }

    void OnDestroy()
    {
        Enemy.OnBossHpChanged -= HandleBossHpChanged;
        Enemy.OnBossDefeatedEvent -= HandleBossDefeated;
    }

    public void ShowBossBar(string bossName, float maxHp)
    {
        if (bossBarContainer != null) bossBarContainer.SetActive(true);
        if (bossNameText != null) bossNameText.text = bossName;
        if (bossSlider != null)
        {
            bossSlider.maxValue = maxHp;
            bossSlider.value = maxHp;
        }
    }

    void HandleBossHpChanged(float currentHp, float maxHp)
    {
        if (bossBarContainer != null && !bossBarContainer.activeSelf)
        {
            int wave = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 5;
            string title = wave >= 10 ? "FINAL BOSS: OVERLORD" : "MINI-BOSS: CRUSHER";
            ShowBossBar(title, maxHp);
        }

        if (bossSlider != null)
        {
            bossSlider.maxValue = maxHp;
            bossSlider.value = Mathf.Max(0, currentHp);
        }
    }

    void HandleBossDefeated()
    {
        if (bossBarContainer != null)
        {
            bossBarContainer.SetActive(false);
        }
    }
}