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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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

        cardPool.Add(new UpgradeCard
        {
            cardName = "Recruit: Random Agent",
            buffType = BuffType.AddRandomCharacter,
            mobValues = new float[] { 10f }
        });

        foreach (CharacterClassType cls in System.Enum.GetValues(typeof(CharacterClassType)))
        {
            cardPool.Add(new UpgradeCard
            {
                cardName = $"Deploy: {cls}",
                buffType = BuffType.AddSpecificCharacter,
                targetClassType = cls,
                mobValues = new float[] { 8f }
            });
        }

        cardPool.Add(new UpgradeCard
        {
            cardName = "Sharpen Blade",
            buffType = BuffType.BoostAttack,
            playerValues = new float[] { 15f, 25f, 40f },
            mobValues = new float[] { 10f, 20f, 35f }
        });

        cardPool.Add(new UpgradeCard
        {
            cardName = "Rapid Fire",
            buffType = BuffType.BoostAttackSpeed,
            playerValues = new float[] { 15f, 30f, 50f },
            mobValues = new float[] { 8f, 15f, 25f }
        });

        cardPool.Add(new UpgradeCard
        {
            cardName = "Frost Aura",
            buffType = BuffType.SlowMob,
            playerValues = new float[] { 15f, 25f, 35f },
            mobValues = new float[] { 10f, 20f, 30f }
        });

        cardPool.Add(new UpgradeCard
        {
            cardName = "Bounty Hunter",
            buffType = BuffType.ExpGain,
            playerValues = new float[] { 20f, 40f, 70f },
            mobValues = new float[] { 15f, 30f, 50f }
        });

        cardPool.Add(new UpgradeCard
        {
            cardName = "Ricochet Round",
            buffType = BuffType.Ricochet,
            playerValues = new float[] { 1f },
            mobValues = new float[] { 15f }
        });

        cardPool.Add(new UpgradeCard
        {
            cardName = "Overdrive Protocol",
            buffType = BuffType.Overdrive,
            playerValues = new float[] { 45f },
            mobValues = new float[] { 10f }
        });

        cardPool.Add(new UpgradeCard
        {
            cardName = "Field Repair",
            buffType = BuffType.EmergencyRepair,
            playerValues = new float[] { 35f, 35f, 35f },
            mobValues = new float[] { 5f, 5f, 5f }
        });
    }

    public void ShowUpgradeSelection()
    {
        List<UpgradeCard> available = new List<UpgradeCard>();
        bool slotMasihAda = SlotGridManager.Instance != null && SlotGridManager.Instance.GetRandomEmptySlot() != null;

        foreach (var card in cardPool)
        {

            if (card.buffType == BuffType.AddRandomCharacter || card.buffType == BuffType.AddSpecificCharacter)
            {
                if (slotMasihAda) available.Add(card);
            }
            else if (card.buffType == BuffType.EmergencyRepair)
            {
                available.Add(card);
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

        currentOptions.Clear();
        int countToPick = Mathf.Min(cardButtons.Length, available.Count);
        for (int i = 0; i < countToPick; i++)
        {
            int randIdx = Random.Range(0, available.Count);
            currentOptions.Add(available[randIdx]);
            available.RemoveAt(randIdx);
        }

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

                string displayTitle = "";
                string displayDesc = "";

                if (c.buffType == BuffType.AddRandomCharacter)
                {
                    displayTitle = "Recruit: Random Agent";
                    displayDesc = "Spawn 1 agen acak (*1).\n(3 agen sejenis *1 melebur jadi *2)";
                }
                else if (c.buffType == BuffType.AddSpecificCharacter)
                {
                    displayTitle = $"Deploy: {c.targetClassType}";
                    displayDesc = $"Spawn langsung 1 {c.targetClassType} (*1).\nBagus untuk melengkapi merge!";
                }
                else
                {
                    int nextLv = c.currentLevel + 1;
                    float nextVal = c.GetNextPlayerValue();
                    float nextMob = c.GetNextMobValue();

                    displayTitle = $"{c.cardName} (Lv.{nextLv})";

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

                        case BuffType.Ricochet:
                            playerStatLabel = "Peluru Memantul ke 1 Musuh Ekstra";
                            mobStatLabel = $"HP Musuh +{nextMob}%";
                            break;

                        case BuffType.Overdrive:
                            playerStatLabel = "Attack Speed +45%, tapi Jarak Tembak -20%";
                            mobStatLabel = $"Kecepatan Gerak Musuh +{nextMob}%";
                            break;

                        case BuffType.EmergencyRepair:
                            playerStatLabel = $"Pulihkan HP Base Sebanyak +{nextVal}";
                            mobStatLabel = $"Damage Musuh ke Base +{nextMob}%";
                            break;
                    }

                    if (c.currentLevel == 0)
                    {
                        displayDesc = $"[Efek]: {playerStatLabel}\n[Musuh]: {mobStatLabel}";
                    }
                    else
                    {
                        int prevIdx = Mathf.Clamp(c.currentLevel - 1, 0, c.playerValues.Length - 1);
                        float prevVal = c.playerValues.Length > 0 ? c.playerValues[prevIdx] : 0f;
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
        if (card.buffType == BuffType.AddRandomCharacter)
        {
            var values = System.Enum.GetValues(typeof(CharacterClassType));
            CharacterClassType randomClass = (CharacterClassType)values.GetValue(Random.Range(0, values.Length));
            SpawnCharacterToGrid(randomClass);
            Enemy.GlobalHpMultiplier += 0.10f;
        }
        else if (card.buffType == BuffType.AddSpecificCharacter)
        {
            SpawnCharacterToGrid(card.targetClassType);
            Enemy.GlobalHpMultiplier += 0.08f;
        }
        else
        {
            float playerVal = card.GetNextPlayerValue();
            float mobVal = card.GetNextMobValue();

            card.currentLevel++;

            switch (card.buffType)
            {
                case BuffType.BoostAttack:
                    Character.GlobalDamageBonusPercent = playerVal;
                    Enemy.GlobalHpMultiplier = 1f + (mobVal / 100f);
                    break;

                case BuffType.BoostAttackSpeed:
                    Character.GlobalAtkSpeedMultiplier = 1f + (playerVal / 100f);
                    if (WaveManager.Instance != null)
                    {
                        WaveManager.Instance.spawnInterval = Mathf.Max(0.4f, 1.5f * (1f - (mobVal / 100f)));
                    }
                    break;

                case BuffType.SlowMob:
                    Enemy.GlobalSpeedMultiplier = Mathf.Max(0.3f, 1f - (playerVal / 100f));
                    Enemy.GlobalHpMultiplier = 1f + (mobVal / 100f);
                    break;

                case BuffType.ExpGain:
                    Enemy.GlobalExpMultiplier = 1f + (playerVal / 100f);
                    Enemy.GlobalDamageMultiplier = 1f + (mobVal / 100f);
                    break;

                case BuffType.Ricochet:
                    Projectile.GlobalRicochetUnlocked = true;
                    Enemy.GlobalHpMultiplier += (mobVal / 100f);
                    break;

                case BuffType.Overdrive:
                    Character.GlobalAtkSpeedMultiplier += 0.45f;
                    if (SlotGridManager.Instance != null)
                    {
                        foreach (var slot in SlotGridManager.Instance.allSlots)
                        {
                            if (slot.isOccupied && slot.currentCharacter != null)
                            {
                                slot.currentCharacter.attackRange = Mathf.Max(3.5f, slot.currentCharacter.attackRange * 0.8f);
                            }
                        }
                    }
                    Enemy.GlobalSpeedMultiplier += (mobVal / 100f);
                    break;

                case BuffType.EmergencyRepair:
                    BaseHealth baseHp = FindFirstObjectByType<BaseHealth>();
                    if (baseHp != null)
                    {
                        baseHp.HealBase(playerVal);
                    }
                    Enemy.GlobalDamageMultiplier += (mobVal / 100f);
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

    void SpawnCharacterToGrid(CharacterClassType targetClass)
    {
        if (SlotGridManager.Instance == null) return;

        CharacterSlot availableSlot = SlotGridManager.Instance.GetRandomEmptySlot();
        if (availableSlot != null)
        {
            GameObject prefab = characterPrefab != null ? characterPrefab : SlotGridManager.Instance.characterPrefab;
            GameObject newChar = Instantiate(prefab, availableSlot.transform.position, Quaternion.identity);

            Character charComp = newChar.GetComponent<Character>();
            if (charComp != null)
            {
                charComp.SetupClass(targetClass, 1);
            }

            availableSlot.AssignCharacter(newChar);
            SlotGridManager.Instance.CheckAndExecuteMerge();
        }
        else
        {
            Debug.LogWarning("Semua slot grid sudah penuh!");
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
            if (c.buffType != BuffType.AddRandomCharacter && c.buffType != BuffType.AddSpecificCharacter && c.currentLevel > 0)
            {
                hasActiveBuff = true;
                int safeIdx = Mathf.Clamp(c.currentLevel - 1, 0, c.playerValues.Length - 1);
                float val = c.playerValues.Length > 0 ? c.playerValues[safeIdx] : 0f;
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
                    case BuffType.Ricochet:
                        statDetail = "Ricochet Active (1x Bounce)";
                        break;
                    case BuffType.Overdrive:
                        statDetail = "Overdrive (ASPD +45%, Range -20%)";
                        break;
                    case BuffType.EmergencyRepair:
                        statDetail = $"Base Repaired ({c.currentLevel}x)";
                        break;
                }

                summary += $"{c.cardName} (Lv.{c.currentLevel})\n -> {statDetail}\n\n";
            }
        }

        if (!hasActiveBuff)
        {
            summary += "Belum ada buff skill aktif.";
        }

        buffListText.text = summary;
    }
}