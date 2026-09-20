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

    [Header("Evolution Modal UI")]
    public GameObject evolutionChoicePanel;
    public CardUI cardPathA;
    public CardUI cardPathB;

    [Header("Black Market UI")]
    public GameObject classPickModalPanel;
    public UnityEngine.UI.Button[] classPickButtons;
    public TextMeshProUGUI blackMarketTitleText;
    private int remainingPicks = 0;
    private bool isPickingClass = false;

    [Header("Debug & Ban System")]
    public List<CharacterClassType> bannedClasses = new List<CharacterClassType>();

    public static bool HasThornsPlating = false;
    public static bool HasArcaneOvercharge = false;
    public static bool HasDesperateGambit = false;
    public static bool HasHeavyCaliber = false;

    private List<UpgradeCard> cardPool = new List<UpgradeCard>();
    private List<UpgradeCard> currentOptions = new List<UpgradeCard>();
    private Character targetEvolutionChar;

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
            if (IsClassBanned(cls)) continue;

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
            cardName = "Field Repair",
            buffType = BuffType.EmergencyRepair,
            playerValues = new float[] { 35f, 35f, 35f },
            mobValues = new float[] { 5f, 5f, 5f }
        });

        cardPool.Add(new UpgradeCard
        {
            cardName = "Fortified Bastion",
            buffType = BuffType.FortifiedBastion,
            playerValues = new float[] { 50f },
            mobValues = new float[] { 20f }
        });

        cardPool.Add(new UpgradeCard
        {
            cardName = "Trickshot Ricochet",
            buffType = BuffType.TrickshotFighter,
            playerValues = new float[] { 1f },
            mobValues = new float[] { 15f }
        });

        // cardPool.Add(new UpgradeCard
        // {
        //     cardName = "Glass Cannon Core",
        //     buffType = BuffType.GlassCannonCore,
        //     playerValues = new float[] { 35f },
        //     mobValues = new float[] { 25f }
        // });

        // cardPool.Add(new UpgradeCard
        // {
        //     cardName = "Deep Freeze",
        //     buffType = BuffType.DeepFreeze,
        //     playerValues = new float[] { 10f },
        //     mobValues = new float[] { 2f }
        // });

        // cardPool.Add(new UpgradeCard
        // {
        //     cardName = "Heavy Caliber",
        //     buffType = BuffType.HeavyCaliber,
        //     playerValues = new float[] { 25f },
        //     mobValues = new float[] { 1.0f }
        // });

        // cardPool.Add(new UpgradeCard
        // {
        //     cardName = "Thorns Plating",
        //     buffType = BuffType.ThornsPlating,
        //     playerValues = new float[] { 1f },
        //     mobValues = new float[] { 25f }
        // });

        // cardPool.Add(new UpgradeCard
        // {
        //     cardName = "Arcane Overcharge",
        //     buffType = BuffType.ArcaneOvercharge,
        //     playerValues = new float[] { 15f },
        //     mobValues = new float[] { 1.0f }
        // });

        cardPool.Add(new UpgradeCard
        {
            cardName = "Black Market Deal",
            buffType = BuffType.BlackMarketDeal,
            playerValues = new float[] { 2f },
            mobValues = new float[] { 1f }
        });

        cardPool.Add(new UpgradeCard
        {
            cardName = "Desperate Gambit",
            buffType = BuffType.DesperateGambit,
            playerValues = new float[] { 60f },
            mobValues = new float[] { 2f }
        });
    }

    public void ShowUpgradeSelection()
    {
        List<UpgradeCard> available = new List<UpgradeCard>();
        bool slotMasihAda = SlotGridManager.Instance != null && SlotGridManager.Instance.GetRandomEmptySlot() != null;

        bool hasFighterOnGrid = false;
        if (SlotGridManager.Instance != null)
        {
            foreach (var slot in SlotGridManager.Instance.allSlots)
            {
                if (slot != null && slot.isOccupied && slot.currentCharacter != null)
                {
                    if (slot.currentCharacter.classType == CharacterClassType.Fighter)
                    {
                        hasFighterOnGrid = true;
                        break;
                    }
                }
            }
        }

        foreach (var card in cardPool)
        {
            if (card.buffType == BuffType.TrickshotFighter)
            {
                if (!hasFighterOnGrid || IsClassBanned(CharacterClassType.Fighter)) continue;
            }

            if (card.buffType == BuffType.AddRandomCharacter || card.buffType == BuffType.AddSpecificCharacter || card.buffType == BuffType.BlackMarketDeal)
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

            float totalWeight = 0f;
            foreach (var card in available)
            {
                totalWeight += GetCardWeight(card);
            }

            float randomRoll = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;
            UpgradeCard selected = available[0];

            for (int j = 0; j < available.Count; j++)
            {
                cumulativeWeight += GetCardWeight(available[j]);
                if (randomRoll <= cumulativeWeight)
                {
                    selected = available[j];
                    break;
                }
            }

            currentOptions.Add(selected);
            available.Remove(selected);
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

                        case BuffType.EmergencyRepair:
                            playerStatLabel = $"Pulihkan HP Base Sebanyak +{nextVal}";
                            mobStatLabel = $"Damage Musuh ke Base +{nextMob}%";
                            break;

                        case BuffType.FortifiedBastion:
                            playerStatLabel = $"Max HP Base +{nextVal} & Heal Seketika";
                            mobStatLabel = $"HP Musuh +{nextMob}%, Spawn +10% saat base kena hit";
                            break;

                        case BuffType.TrickshotFighter:
                            playerStatLabel = "Proyektil Fighter Memantul (Hit ke-2 -40%)";
                            mobStatLabel = $"Kecepatan Musuh +{nextMob}%, 15% Miss Chance";
                            break;

                        case BuffType.GlassCannonCore:
                            playerStatLabel = $"Fighter & Mage ATK +{nextVal}%, Ranged ATK +15%";
                            mobStatLabel = "Base HP -25%, Efisiensi Repair -25%";
                            break;

                        case BuffType.DeepFreeze:
                            playerStatLabel = $"Slow Support +{nextVal}%, Durasi 2x Lipat";
                            mobStatLabel = "Boss punya peluang kebal efek Slow";
                            break;

                        case BuffType.HeavyCaliber:
                            playerStatLabel = $"Ranged tembus armor, DMG Boss/Tank +{nextVal}%";
                            mobStatLabel = "Jeda tembak Ranged +1.0s (berkurang per Ranged di grid)";
                            break;

                        case BuffType.ThornsPlating:
                            playerStatLabel = "Shockwave Damage saat musuh nabrak base (5-50%)";
                            mobStatLabel = "Repair -25%, Setiap shockwave kurangi 1% Max Base HP";
                            break;

                        case BuffType.ArcaneOvercharge:
                            playerStatLabel = $"AoE Damage Mage +{nextVal}% (Sinergi Evolusi)";
                            mobStatLabel = "Jeda tembak Mage +1.0s";
                            break;

                        case BuffType.BlackMarketDeal:
                            displayTitle = "Black Market Deal";
                            playerStatLabel = "Dapatkan 2 Tiket Deploy Instan";
                            mobStatLabel = "Peluang spawn Mini-Tank ekstra tiap wave";
                            break;

                        case BuffType.DesperateGambit:
                            playerStatLabel = $"Jika HP Base < 30%: ASPD +{nextVal}%, ATK +15%";
                            mobStatLabel = "Jika musuh nabrak base, damage diterima x2 lipat";
                            break;
                    }

                    displayDesc = $"[Efek]: {playerStatLabel}\n[Musuh]: {mobStatLabel}";
                }

                cardButtons[i].Setup(displayTitle, displayDesc, i);
            }
            else
            {
                cardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private float GetCardWeight(UpgradeCard card)
    {

        bool isSingleUse = (card.playerValues != null && card.playerValues.Length == 1) ||
                           card.buffType == BuffType.Ricochet ||
                           card.buffType == BuffType.Overdrive ||
                           card.buffType == BuffType.GlassCannonCore ||
                           card.buffType == BuffType.FortifiedBastion ||
                           card.buffType == BuffType.TrickshotFighter ||
                           card.buffType == BuffType.DeepFreeze ||
                           card.buffType == BuffType.HeavyCaliber ||
                           card.buffType == BuffType.ThornsPlating ||
                           card.buffType == BuffType.ArcaneOvercharge ||
                           card.buffType == BuffType.DesperateGambit;

        if (isSingleUse)

        {

            return 15f;
        }

        if (card.buffType == BuffType.AddRandomCharacter || 
            card.buffType == BuffType.AddSpecificCharacter || 
            card.buffType == BuffType.EmergencyRepair ||
            card.buffType == BuffType.BlackMarketDeal)
        {
            return 45f;
        }

        if (card.buffType == BuffType.DesperateGambit)
        {
            return 3f; 
        }

        return 70f;
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
        bool isBlackMarket = (card.buffType == BuffType.BlackMarketDeal);

        if (card.buffType == BuffType.AddRandomCharacter)
        {
            CharacterClassType randomClass = GetRandomAllowedClass();
            SpawnCharacterToGrid(randomClass);
            Enemy.GlobalHpMultiplier += 0.10f;
        }
        else if (card.buffType == BuffType.AddSpecificCharacter)
        {
            SpawnCharacterToGrid(card.targetClassType);
            Enemy.GlobalHpMultiplier += 0.08f;
        }
        else if (card.buffType == BuffType.EvolvePathA || card.buffType == BuffType.EvolvePathB)
        {
            EvolutionPath chosen = (card.buffType == BuffType.EvolvePathA) ? EvolutionPath.PathA : EvolutionPath.PathB;
            if (SlotGridManager.Instance != null)
            {
                foreach (var slot in SlotGridManager.Instance.allSlots)
                {
                    if (slot != null && slot.isOccupied && slot.currentCharacter != null)
                    {
                        if (slot.currentCharacter.classType == card.targetClassType && slot.currentCharacter.starLevel >= 3)
                        {
                            slot.currentCharacter.ApplyEvolution(chosen);
                        }
                    }
                }
            }

            cardPool.RemoveAll(c => c.targetClassType == card.targetClassType && 
                                   (c.buffType == BuffType.EvolvePathA || c.buffType == BuffType.EvolvePathB));
        }
        else
        {
            float playerVal = card.GetNextPlayerValue();
            float mobVal = card.GetNextMobValue();

            card.currentLevel++;

            switch (card.buffType)
            {
                case BuffType.BoostAttack:
                    Character.GlobalDamageBonusPercent += playerVal;
                    Enemy.GlobalHpMultiplier += (mobVal / 100f);
                    break;

                case BuffType.BoostAttackSpeed:
                    Character.GlobalAtkSpeedMultiplier += (playerVal / 100f);
                    if (WaveManager.Instance != null)
                    {
                        WaveManager.Instance.spawnInterval = Mathf.Max(0.4f, 1.5f * (1f - (mobVal / 100f)));
                    }
                    break;

                case BuffType.SlowMob:
                    Enemy.GlobalSpeedMultiplier = Mathf.Max(0.3f, 1f - (playerVal / 100f));
                    Enemy.GlobalHpMultiplier += (mobVal / 100f);
                    break;

                case BuffType.ExpGain:
                    Enemy.GlobalExpMultiplier += (playerVal / 100f);
                    Enemy.GlobalDamageMultiplier += (mobVal / 100f);
                    break;

                case BuffType.EmergencyRepair:
                    BaseHealth baseHp = FindFirstObjectByType<BaseHealth>();
                    if (baseHp != null) baseHp.HealBase(playerVal);
                    Enemy.GlobalDamageMultiplier += (mobVal / 100f);
                    break;

                case BuffType.FortifiedBastion:
                    BaseHealth bHealth = FindFirstObjectByType<BaseHealth>();
                    if (bHealth != null)
                    {
                        bHealth.maxHealth += playerVal;
                        bHealth.HealBase(playerVal);
                    }
                    Enemy.GlobalHpMultiplier += (mobVal / 100f);
                    break;

                case BuffType.TrickshotFighter:
                    Projectile.GlobalRicochetUnlocked = true;
                    Enemy.GlobalSpeedMultiplier += (mobVal / 100f);
                    break;

                case BuffType.GlassCannonCore:
                    Character.GlobalDamageBonusPercent += playerVal;
                    BaseHealth gHealth = FindFirstObjectByType<BaseHealth>();
                    if (gHealth != null)
                    {
                        gHealth.ReduceMaxHpPermanently(25f);
                        gHealth.repairEffectivenessMultiplier *= 0.75f;
                    }
                    cardPool.Remove(card);
                    break;

                case BuffType.DeepFreeze:
                    Enemy.GlobalSpeedMultiplier = Mathf.Max(0.2f, Enemy.GlobalSpeedMultiplier - 0.1f);
                    cardPool.Remove(card);
                    break;

                case BuffType.HeavyCaliber:
                    HasHeavyCaliber = true;
                    cardPool.Remove(card);
                    break;

                case BuffType.ThornsPlating:
                    HasThornsPlating = true;
                    BaseHealth tpHealth = FindFirstObjectByType<BaseHealth>();
                    if (tpHealth != null) tpHealth.repairEffectivenessMultiplier *= 0.75f;
                    cardPool.Remove(card);
                    break;

                case BuffType.ArcaneOvercharge:
                    HasArcaneOvercharge = true;
                    cardPool.Remove(card);
                    break;

                case BuffType.DesperateGambit:
                    HasDesperateGambit = true;
                    cardPool.Remove(card);
                    break;
                
                case BuffType.BlackMarketDeal:
                    Enemy.GlobalExpMultiplier = Mathf.Max(0.1f, Enemy.GlobalExpMultiplier - 0.30f);
                    cardPool.Remove(card); 
                    StartBlackMarketPicks(2);
                    break;
            }
        }

        UpdateBuffListDisplay();

        cardChoicePanel.SetActive(false);

        if (!isBlackMarket && !isAutoBattle && GameManager.Instance != null)
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

        string summary = "<b><size=120%>=== ACTIVE BUFFS ===</size></b>\n\n";
        bool hasActiveBuff = false;

        foreach (var c in cardPool)
        {
            if (c.buffType != BuffType.AddRandomCharacter && 
                c.buffType != BuffType.AddSpecificCharacter && 
                c.buffType != BuffType.BlackMarketDeal && 
                c.currentLevel > 0)
            {
                hasActiveBuff = true;

                float currentVal = c.playerValues != null && c.playerValues.Length > 0 
                    ? c.playerValues[Mathf.Min(c.currentLevel - 1, c.playerValues.Length - 1)] 
                    : 0f;

                string effectDesc = "";

                switch (c.buffType)
                {
                    case BuffType.BoostAttack:
                        effectDesc = $"ATK Karakter +{currentVal}%";
                        break;
                    case BuffType.BoostAttackSpeed:
                        effectDesc = $"Attack Speed +{currentVal}%";
                        break;
                    case BuffType.SlowMob:
                        effectDesc = $"Slow Musuh {currentVal}%";
                        break;
                    case BuffType.ExpGain:
                        effectDesc = $"Bonus EXP +{currentVal}%";
                        break;
                    case BuffType.FortifiedBastion:
                        effectDesc = $"Max Base HP +{currentVal}";
                        break;
                    case BuffType.TrickshotFighter:
                        effectDesc = "Proyektil Fighter Ricochet";
                        break;
                    case BuffType.GlassCannonCore:
                        effectDesc = "Fighter & Mage ATK +35%, Base HP -25%";
                        break;
                    case BuffType.DeepFreeze:
                        effectDesc = "Slow Support +10% (Durasi 2x)";
                        break;
                    case BuffType.HeavyCaliber:
                        effectDesc = "Ranged Pierce Armor, Boss DMG +25%";
                        break;
                    case BuffType.ThornsPlating:
                        effectDesc = "Base Shockwave Damage Aktif";
                        break;
                    case BuffType.ArcaneOvercharge:
                        effectDesc = "Mage AoE Damage +15%";
                        break;
                    case BuffType.DesperateGambit:
                        effectDesc = "HP < 30%: ASPD +60%, ATK +15%";
                        break;
                    default:
                        effectDesc = "Aktif";
                        break;
                }

                summary += $"• <b>{c.cardName} (Lv.{c.currentLevel})</b>\n   <color=#B0BEC5>{effectDesc}</color>\n\n";
            }
        }

        if (!hasActiveBuff)
        {
            summary += "Belum ada buff skill aktif.";
        }

        buffListText.text = summary;
    }

    public void UnlockEvolutionCards(CharacterClassType cls)
    {

        foreach (var c in cardPool)
        {
            if (c.targetClassType == cls && (c.buffType == BuffType.EvolvePathA || c.buffType == BuffType.EvolvePathB))
            {
                return;
            }
        }

        string nameA = "";
        string nameB = "";

        switch (cls)
        {
            case CharacterClassType.Fighter:
                nameA = "Evolve: Clawslash (Piercing)";
                nameB = "Evolve: Claw Claw Claw (Triple)";
                break;
            case CharacterClassType.Mage:
                nameA = "Evolve: Ignis Alchemist (Burn DoT)";
                nameB = "Evolve: Cataclysm Cannon (Nuke AoE)";
                break;
            case CharacterClassType.Support:
                nameA = "Evolve: Absolute Zero (Freeze)";
                nameB = "Evolve: Permafrost Conduit (Spread)";
                break;
            case CharacterClassType.Tank:
                nameA = "Evolve: Ironclad Fortress (Aura Slow)";
                nameB = "Evolve: Riot Punisher (Shotgun Cone)";
                break;
            case CharacterClassType.Ranged:
                nameA = "Evolve: Headhunter (Crit Boss)";
                nameB = "Evolve: Execute Protocol (Execute <5%)";
                break;
        }

        cardPool.Add(new UpgradeCard
        {
            cardName = nameA,
            buffType = BuffType.EvolvePathA,
            targetClassType = cls,
            playerValues = new float[] { 1f }
        });

        cardPool.Add(new UpgradeCard
        {
            cardName = nameB,
            buffType = BuffType.EvolvePathB,
            targetClassType = cls,
            playerValues = new float[] { 1f }
        });

        Debug.Log($"[UpgradeManager] Kartu evolusi untuk {cls} berhasil dibuka!");
    }

    public void TriggerInstantEvolutionChoice(Character character)
    {
        if (character == null) return;

        targetEvolutionChar = character;

        if (evolutionChoicePanel != null)
        {
            evolutionChoicePanel.SetActive(true);
        }

        string titleA = "", descA = "";
        string titleB = "", descB = "";

        switch (character.classType)
        {
            case CharacterClassType.Fighter:
                titleA = "Path A: Clawslash";
                descA = "Jangkauan cakar tak terbatas, menembus musuh (-10% DMG/hit).";
                titleB = "Path B: Claw Claw Claw";
                descB = "Setiap serangan ke-4 meluncurkan 3 cakaran beruntun.";
                break;

            case CharacterClassType.Mage:
                titleA = "Path A: Ignis Alchemist";
                descA = "Ledakan meninggalkan kubangan api DoT yang membakar musuh.";
                titleB = "Path B: Cataclysm Cannon";
                descB = "ASPD lambat, radius ledakan masif + knockback kuat.";
                break;

            case CharacterClassType.Support:
                titleA = "Path A: Absolute Zero";
                descA = "Slow menumpuk hingga 8x; tumpukan penuh membekukan musuh 0,8 detik.";
                titleB = "Path B: Permafrost Conduit";
                descB = "Musuh slow yang gugur menyebarkan efek slow ke musuh terdekat.";
                break;

            case CharacterClassType.Tank:
                titleA = "Path A: Ironclad Fortress";
                descA = "Area depan base menjadi zona AoE yang melambatkan & mendamage musuh.";
                titleB = "Path B: Riot Punisher";
                descB = "Tembakan shotgun cone jarak dekat dengan knockback masif.";
                break;

            case CharacterClassType.Ranged:
                titleA = "Path A: Headhunter";
                descA = "Bonus Critical Damage masif khusus ke musuh Tank dan Boss.";
                titleB = "Path B: Execute Protocol";
                descB = "Langsung melenyapkan musuh non-Boss berdarah sekarat (<5%).";
                break;
        }

        if (cardPathA != null) cardPathA.Setup(titleA, descA, 0);
        if (cardPathB != null) cardPathB.Setup(titleB, descB, 1);

        Time.timeScale = 0f;
    }

    public void SelectEvolutionByIndex(int pathIndex)
    {
        EvolutionPath chosen = (pathIndex == 0) ? EvolutionPath.PathA : EvolutionPath.PathB;

        if (targetEvolutionChar != null)
        {
            targetEvolutionChar.ApplyEvolution(chosen);
            UnlockStar3SpecialCard(targetEvolutionChar.classType);
        }

        if (evolutionChoicePanel != null)
        {
            evolutionChoicePanel.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestoreSpeedAfterModal();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void UnlockStar3SpecialCard(CharacterClassType cls)
    {
        switch (cls)
        {
            case CharacterClassType.Fighter:
                cardPool.Add(new UpgradeCard
                {
                    cardName = "Glass Cannon Core",
                    buffType = BuffType.GlassCannonCore,
                    playerValues = new float[] { 35f },
                    mobValues = new float[] { 25f }
                });
                break;

            case CharacterClassType.Support:
                cardPool.Add(new UpgradeCard
                {
                    cardName = "Deep Freeze",
                    buffType = BuffType.DeepFreeze,
                    playerValues = new float[] { 10f },
                    mobValues = new float[] { 2f }
                });
                break;

            case CharacterClassType.Ranged:
                cardPool.Add(new UpgradeCard
                {
                    cardName = "Heavy Caliber",
                    buffType = BuffType.HeavyCaliber,
                    playerValues = new float[] { 25f },
                    mobValues = new float[] { 1.0f }
                });
                break;

            case CharacterClassType.Tank:
                cardPool.Add(new UpgradeCard
                {
                    cardName = "Thorns Plating",
                    buffType = BuffType.ThornsPlating,
                    playerValues = new float[] { 1f },
                    mobValues = new float[] { 25f }
                });
                break;

            case CharacterClassType.Mage:
                cardPool.Add(new UpgradeCard
                {
                    cardName = "Arcane Overcharge",
                    buffType = BuffType.ArcaneOvercharge,
                    playerValues = new float[] { 15f },
                    mobValues = new float[] { 1.0f }
                });
                break;
        }

        Debug.Log($"[UpgradeManager] Kartu eksklusif B3 untuk {cls} resmi dibuka ke pool upgrade!");
    }

    public void StartBlackMarketPicks(int tickets)
    {
        remainingPicks = tickets;
        isPickingClass = true;

        Time.timeScale = 0f;

        if (classPickButtons != null)
        {
            for (int i = 0; i < classPickButtons.Length; i++)
            {
                if (classPickButtons[i] != null)
                {
                    CharacterClassType cls = (CharacterClassType)i;
                    classPickButtons[i].interactable = !IsClassBanned(cls);
                }
            }
        }

        if (classPickModalPanel != null)
        {
            classPickModalPanel.SetActive(true);
        }

        UpdateBlackMarketTitle();
        Debug.Log($"[Black Market] Dimulai! Sisa tiket: {remainingPicks}");
    }

   public void OnClassPickSelected(int classIndex)
    {
        if (!isPickingClass || remainingPicks <= 0) return;

        classIndex = Mathf.Clamp(classIndex, 0, 4);
        CharacterClassType selectedClass = (CharacterClassType)classIndex;

        if (IsClassBanned(selectedClass))
        {
            Debug.LogWarning($"[Black Market] Kelas {selectedClass} sedang di-banned dan tidak bisa dipilih!");
            return;
        }

        Debug.Log($"[Black Market] Memilih Class: {selectedClass} (Index: {classIndex})");
        SpawnCharacterToGrid(selectedClass);

        remainingPicks--;
        UpdateBlackMarketTitle();

        Time.timeScale = 0f;

        if (remainingPicks <= 0)
        {
            isPickingClass = false;

            if (classPickModalPanel != null)
            {
                classPickModalPanel.SetActive(false);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestoreSpeedAfterModal();
            }
            else
            {
                Time.timeScale = 1f;
            }

            Debug.Log("[Black Market] Selesai! Panel ditutup dan waktu dilanjutkan.");
        }
    }

    void UpdateBlackMarketTitle()
    {
        if (blackMarketTitleText != null)
        {
            blackMarketTitleText.text = $"PILIH AGEN ({remainingPicks} TIKET TERSISA)";
        }
    }

    public bool IsClassBanned(CharacterClassType cls)
    {
        return bannedClasses != null && bannedClasses.Contains(cls);
    }

    public CharacterClassType GetRandomAllowedClass()
    {
        List<CharacterClassType> allowed = new List<CharacterClassType>();

        foreach (CharacterClassType cls in System.Enum.GetValues(typeof(CharacterClassType)))
        {
            if (!IsClassBanned(cls))
            {
                allowed.Add(cls);
            }
        }

        if (allowed.Count == 0) return CharacterClassType.Fighter;

        return allowed[Random.Range(0, allowed.Count)];
    }

    public void SkipUpgradeSelection()
    {
        Debug.Log("[UpgradeManager] Pemain memilih Skip pada pemilihan kartu buff.");

        currentOptions.Clear();

        if (cardChoicePanel != null)
        {
            cardChoicePanel.SetActive(false);
        }

        bool isEvolutionOpen = (evolutionChoicePanel != null && evolutionChoicePanel.activeSelf);
        bool isBlackMarketOpen = (classPickModalPanel != null && classPickModalPanel.activeSelf);

        if (!isEvolutionOpen && !isBlackMarketOpen && !isAutoBattle && GameManager.Instance != null)
        {
            GameManager.Instance.RestoreSpeedAfterModal();
        }
        else if (!isEvolutionOpen && !isBlackMarketOpen)
        {
            Time.timeScale = 1f;
        }
    }
}