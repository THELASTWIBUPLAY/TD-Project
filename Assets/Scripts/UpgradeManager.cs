using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject cardChoicePanel;
    public CardUI[] cardButtons;
    public GameObject buffListPanel;
    public TextMeshProUGUI buffListText;
    public TextMeshProUGUI autoBattleText;
    [Header("Character Prefab")]
    public GameObject characterPrefab;
    [Header("Auto Battle")]
    public bool isAutoBattle = false;

    private List<UpgradeCard> cardPool = new List<UpgradeCard>();
    private List<UpgradeCard> currentOptions = new List<UpgradeCard>();

    void Awake()
    {
        Instance = this;
        InitCardPool();
    }

    void Start()
    {
        if (cardChoicePanel != null) cardChoicePanel.SetActive(false);
        if (buffListPanel != null) buffListPanel.SetActive(false);
        UpdateAutoBattleUI();
    }

    void InitCardPool()
    {
        cardPool.Clear();

        // Kartu Tambah Karakter (bisa diambil berulang selama slot 15 belum penuh)
        cardPool.Add(new UpgradeCard {
            cardName = "+1 Character",
            buffType = BuffType.AddCharacter,
            mobValues = new float[] { 10f, 10f, 10f } // Setiap tambah karakter, HP musuh naik 10%
        });

        // Skill-skill berlevel (Lv 1 -> 2 -> 3)
        cardPool.Add(new UpgradeCard {
            cardName = "Sharpen Blade",
            buffType = BuffType.BoostAttack,
            playerValues = new float[] { 15f, 25f, 40f },  // Attack naik
            mobValues = new float[] { 10f, 20f, 35f }      // HP musuh naik
        });

        cardPool.Add(new UpgradeCard {
            cardName = "Rapid Fire",
            buffType = BuffType.BoostAttackSpeed,
            playerValues = new float[] { 15f, 30f, 50f },  // ASPD naik
            mobValues = new float[] { 8f, 15f, 25f }       // Spawn interval musuh lebih cepat
        });

        cardPool.Add(new UpgradeCard {
            cardName = "Frost Aura",
            buffType = BuffType.SlowMob,
            playerValues = new float[] { 15f, 25f, 35f },  // Mob jalan lambat
            mobValues = new float[] { 10f, 20f, 30f }      // HP musuh naik
        });

        cardPool.Add(new UpgradeCard {
            cardName = "Bounty Hunter",
            buffType = BuffType.ExpGain,
            playerValues = new float[] { 20f, 40f, 70f },  // EXP naik
            mobValues = new float[] { 15f, 30f, 50f }      // Damage musuh ke Base naik
        });
    }

    public void ShowUpgradeSelection()
    {
        // Kumpulkan kartu yang masih valid (belum max level dan slot belum penuh)
        List<UpgradeCard> available = new List<UpgradeCard>();
        bool slotMasihAda = SlotGridManager.Instance != null && SlotGridManager.Instance.GetRandomEmptySlot() != null;

        foreach (var card in cardPool)
        {
            if (card.buffType == BuffType.AddCharacter)
            {
                if (slotMasihAda) available.Add(card);
            }
            else if (!card.IsMaxLevel)
            {
                available.Add(card);
            }
        }

        if (available.Count == 0)
        {
            Debug.Log("Semua skill sudah MAX dan arena penuh!");
            return;
        }

        // Ambil kartu acak untuk ditampilkan
        currentOptions.Clear();
        int countToPick = Mathf.Min(cardButtons.Length, available.Count);
        for (int i = 0; i < countToPick; i++)
        {
            int randIdx = Random.Range(0, available.Count);
            currentOptions.Add(available[randIdx]);
            available.RemoveAt(randIdx);
        }

        // Jika Auto-Battle aktif: otomatis pilih yang pertama
        if (isAutoBattle)
        {
            ApplyUpgrade(currentOptions[0]);
            return;
        }

        Time.timeScale = 0f;
        cardChoicePanel.SetActive(true);

        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (i < currentOptions.Count)
            {
                cardButtons[i].gameObject.SetActive(true);
                UpgradeCard c = currentOptions[i];

                // --- PERAKITAN TEKS KARTU LENGKAP & JELAS ---
                string displayTitle;
                string displayDesc;

                if (c.buffType == BuffType.AddCharacter)
                {
                    displayTitle = "+1 Character";
                    displayDesc = "Spawn 1 karakter baru (*1).\n(3 karakter *1 otomatis melebur jadi *2)";
                }
                else
                {
                    int nextLv = c.currentLevel + 1;
                    float nextVal = c.GetNextPlayerValue();
                    float nextMob = c.GetNextMobValue();

                    displayTitle = $"{c.cardName} (Lv.{nextLv})";

                    // Tentukan label stat pemain dan buff musuh
                    string playerStatLabel = "";
                    string mobStatLabel = "";

                    switch (c.buffType)
                    {
                        case BuffType.BoostAttack:
                            playerStatLabel = $"Damage Karakter +{nextVal}%";
                            mobStatLabel = $"HP Musuh +{nextMob}%";
                            break;

                        case BuffType.BoostAttackSpeed:
                            playerStatLabel = $"Attack Speed +{nextVal}%";
                            mobStatLabel = $"Spawn Musuh +{nextMob}% lebih cepat";
                            break;

                        case BuffType.SlowMob:
                            playerStatLabel = $"Gerakan Musuh -{nextVal}% (Slow)";
                            mobStatLabel = $"HP Musuh +{nextMob}%";
                            break;

                        case BuffType.ExpGain:
                            playerStatLabel = $"Bonus EXP +{nextVal}%";
                            mobStatLabel = $"Damage Musuh ke Base +{nextMob}%";
                            break;
                    }

                    if (c.currentLevel == 0)
                    {
                        displayDesc = $"[Efek]: {playerStatLabel}\n[Musuh]: {mobStatLabel}";
                    }
                    else
                    {
                        float prevVal = c.playerValues[c.currentLevel - 1];
                        displayDesc = $"[Upgrade]: {prevVal}% -> {nextVal}%\n[Efek]: {playerStatLabel}\n[Musuh]: {mobStatLabel}";
                    }
                }

                cardButtons[i].Setup(displayTitle, displayDesc, i);
            }
            else
            {
                cardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void ApplyUpgradeByIndex(int index)
    {
        if (index < currentOptions.Count)
        {
            ApplyUpgrade(currentOptions[index]);
        }
    }

    public void ApplyUpgrade(UpgradeCard card)
    {
        if (card.buffType == BuffType.AddCharacter)
        {
            SpawnCharacterToGrid();
            Enemy.GlobalHpMultiplier += 0.10f;
        }
        else
        {
            card.currentLevel++;
            float playerVal = card.playerValues[card.currentLevel - 1];
            float mobVal = card.mobValues[card.currentLevel - 1];

            // Terapkan efek skill terstruktur (bukan flat stacking)
            switch (card.buffType)
            {
                case BuffType.BoostAttack:
                    Character.GlobalDamageBonusPercent = playerVal;
                    Enemy.GlobalHpMultiplier = 1f + (mobVal / 100f);
                    break;

                case BuffType.BoostAttackSpeed:
                    Character.GlobalAtkSpeedMultiplier = 1f + (playerVal / 100f);
                    if (WaveManager.Instance != null)
                        WaveManager.Instance.spawnInterval = Mathf.Max(0.4f, 1.5f * (1f - (mobVal / 100f)));
                    break;

                case BuffType.SlowMob:
                    Enemy.GlobalSpeedMultiplier = Mathf.Max(0.3f, 1f - (playerVal / 100f));
                    Enemy.GlobalHpMultiplier = 1f + (mobVal / 100f);
                    break;

                case BuffType.ExpGain:
                    Enemy.GlobalExpMultiplier = 1f + (playerVal / 100f);
                    Enemy.GlobalDamageMultiplier = 1f + (mobVal / 100f);
                    break;
            }
        }

        UpdateBuffListDisplay();

        cardChoicePanel.SetActive(false);
        if (!isAutoBattle && GameManager.Instance != null)
        {
            GameManager.Instance.RestoreSpeedAfterModal();
        }
    }

    void SpawnCharacterToGrid()
    {
        if (SlotGridManager.Instance == null) return;

        CharacterSlot availableSlot = SlotGridManager.Instance.GetRandomEmptySlot();
        if (availableSlot != null)
        {
            // Ambil prefab dari SlotGridManager langsung
            GameObject prefab = characterPrefab != null ? characterPrefab : SlotGridManager.Instance.characterPrefab;
            
            GameObject newChar = Instantiate(prefab, availableSlot.transform.position, Quaternion.identity);
            availableSlot.AssignCharacter(newChar);
            
            // Periksa auto merge
            SlotGridManager.Instance.CheckAndExecuteMerge();
        }
        else
        {
            Debug.LogWarning("Semua 15 slot arena sudah penuh!");
        }
    }

    public void ToggleAutoBattle()
    {
        isAutoBattle = !isAutoBattle;
        UpdateAutoBattleUI();
    }

    void UpdateAutoBattleUI()
    {
        if (autoBattleText != null)
        {
            autoBattleText.text = isAutoBattle ? "Auto: ON" : "Auto: OFF";
        }
    }

    public void ToggleBuffList()
    {
        buffListPanel.SetActive(!buffListPanel.activeSelf);
        if (buffListPanel.activeSelf) UpdateBuffListDisplay();
    }

    void UpdateBuffListDisplay()
    {
        if (buffListText == null) return;

        string summary = "=== ACTIVE BUFFS ===\n\n";
        bool hasActiveBuff = false;

        foreach (var c in cardPool)
        {
            if (c.buffType != BuffType.AddCharacter && c.currentLevel > 0)
            {
                hasActiveBuff = true;
                float val = c.playerValues[c.currentLevel - 1];
                string statDetail = "";

                switch (c.buffType)
                {
                    case BuffType.BoostAttack:
                        statDetail = $"Base Damage +{val}%";
                        break;
                    case BuffType.BoostAttackSpeed:
                        statDetail = $"Attack Speed +{val}%";
                        break;
                    case BuffType.SlowMob:
                        statDetail = $"Enemy Slow +{val}%";
                        break;
                    case BuffType.ExpGain:
                        statDetail = $"EXP Bonus +{val}%";
                        break;
                }

                summary += $"{c.cardName} (Lv.{c.currentLevel}/3)\n -> {statDetail}\n\n";
            }
        }

        if (!hasActiveBuff)
        {
            summary += "Belum ada buff skill aktif.";
        }

        buffListText.text = summary;
    }
}