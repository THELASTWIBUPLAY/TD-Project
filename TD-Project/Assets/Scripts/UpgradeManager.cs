using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private static readonly CharacterClassType[] AllClassTypes = (CharacterClassType[])Enum.GetValues(typeof(CharacterClassType));

    public static bool HasThornsPlating = false;
    public static bool HasArcaneOvercharge = false;
    public static bool HasDesperateGambit = false;
    public static bool HasHeavyCaliber = false;

    [Header("UI Panels")]
    [SerializeField] private GameObject cardChoicePanel;
    [SerializeField] private CardUI[] cardButtons;
    [SerializeField] private GameObject buffListPanel;
    [SerializeField] private TextMeshProUGUI buffListText;
    [SerializeField] private TextMeshProUGUI autoBattleText;

    [Header("Buff List UI (Dynamic)")]
    [SerializeField] private Transform buffListContainer;
    [SerializeField] private GameObject buffItemPrefab;    

    [Header("Dynamic Card Spawn")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardSpawnParent;

    [Header("Evolution Prefab Setup")]
    [SerializeField] public GameObject evolutionChoicePanel;
    [SerializeField] private Transform evolutionCardContainer;
    [SerializeField] private GameObject evolutionCardPrefab;

    [Header("Black Market UI")]
    [SerializeField] public GameObject classPickModalPanel;
    [SerializeField] private Transform topRowContainer;
    [SerializeField] private Transform bottomRowContainer;
    [SerializeField] private GameObject classPickPrefab;
    [SerializeField] private TextMeshProUGUI blackMarketTitleText;

    [Header("Character Setup")]
    [SerializeField] private GameObject characterPrefab;

    [Header("Auto Battle & Ban System")]
    public bool isAutoBattle = false;
    public List<CharacterClassType> bannedClasses = new List<CharacterClassType>();

    [Header("Reroll System Setup")]
    [SerializeField] private Button rerollButton; 
    [SerializeField] private TextMeshProUGUI rerollText; 
    public int maxRerollCount = 3; 
    private int currentRerollCount;

    private readonly List<UpgradeCard> cardPool = new List<UpgradeCard>();
    private readonly List<UpgradeCard> currentOptions = new List<UpgradeCard>();
    private readonly List<GameObject> spawnedCards = new List<GameObject>();
    private readonly List<GameObject> spawnedEvolutionCards = new List<GameObject>();
    private readonly List<GameObject> spawnedClassCards = new List<GameObject>();
    
    private Character targetEvolutionChar;
    private int remainingPicks = 0;
    private bool isPickingClass = false;

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitCardPool();
    }

    private void Start()
    {
        if (cardChoicePanel != null) cardChoicePanel.SetActive(false);
        if (buffListPanel != null) buffListPanel.SetActive(false);
        if (evolutionChoicePanel != null) evolutionChoicePanel.SetActive(false);
        InitRerollSystem();
        UpdateAutoBattleUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) OpenBlackMarketCheat();
        if (Input.GetKeyDown(KeyCode.F2)) AddCharacterCheat();
        if (Input.GetKeyDown(KeyCode.F3)) InstantEvolutionCheat();
    }
    #endregion

    #region Initialization & Card Pool
    private void InitCardPool()
    {
        cardPool.Clear();

        cardPool.Add(new UpgradeCard
        {
            cardName = "Recruit: Random Agent",
            buffType = BuffType.AddRandomCharacter,
            mobValues = new float[] { 10f }
        });

        foreach (CharacterClassType cls in AllClassTypes)
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

        AddBaseCardPools();
    }

    private void AddBaseCardPools()
    {
        cardPool.Add(new UpgradeCard { cardName = "Sharpen Blade", buffType = BuffType.BoostAttack, playerValues = new float[] { 15f, 25f, 40f }, mobValues = new float[] { 10f, 20f, 35f } });
        cardPool.Add(new UpgradeCard { cardName = "Rapid Fire", buffType = BuffType.BoostAttackSpeed, playerValues = new float[] { 15f, 30f, 50f }, mobValues = new float[] { 8f, 15f, 25f } });
        cardPool.Add(new UpgradeCard { cardName = "Frost Aura", buffType = BuffType.SlowMob, playerValues = new float[] { 15f, 25f, 35f }, mobValues = new float[] { 10f, 20f, 30f } });
        cardPool.Add(new UpgradeCard { cardName = "Bounty Hunter", buffType = BuffType.ExpGain, playerValues = new float[] { 20f, 40f, 70f }, mobValues = new float[] { 15f, 30f, 50f } });
        cardPool.Add(new UpgradeCard { cardName = "Field Repair", buffType = BuffType.EmergencyRepair, playerValues = new float[] { 35f, 35f, 35f }, mobValues = new float[] { 5f, 5f, 5f } });
        cardPool.Add(new UpgradeCard { cardName = "Fortified Bastion", buffType = BuffType.FortifiedBastion, playerValues = new float[] { 50f }, mobValues = new float[] { 20f } });
        cardPool.Add(new UpgradeCard { cardName = "Trickshot Ricochet", buffType = BuffType.TrickshotFighter, playerValues = new float[] { 1f }, mobValues = new float[] { 15f } });
        cardPool.Add(new UpgradeCard { cardName = "Black Market Deal", buffType = BuffType.BlackMarketDeal, playerValues = new float[] { 2f }, mobValues = new float[] { 1f } });
        cardPool.Add(new UpgradeCard { cardName = "Desperate Gambit", buffType = BuffType.DesperateGambit, playerValues = new float[] { 60f }, mobValues = new float[] { 2f } });
    }

    private void InitRerollSystem()
    {
        currentRerollCount = maxRerollCount;
        UpdateRerollUI();

        if (rerollButton != null)
        {
            rerollButton.onClick.RemoveAllListeners();
            rerollButton.onClick.AddListener(RerollUpgradeCards);
        }
    }

    public void RerollUpgradeCards()
    {
        if (currentRerollCount <= 0)
        {
            Debug.Log("[UpgradeManager] Kesempatan Reroll sudah habis!");
            return;
        }

        List<UpgradeCard> available = GetAvailableCards();
        if (available.Count == 0) return;

        currentRerollCount--;
        UpdateRerollUI();

        PopulateOptionsFromAvailable(available);

        ClearSpawnedCards();
        SpawnCardUIElements();
        Debug.Log($"[UpgradeManager] Card di-reroll! Sisa Reroll: {currentRerollCount}");
    }

    private void UpdateRerollUI()
    {
        if (rerollButton != null) rerollButton.interactable = currentRerollCount > 0;
        if (rerollText != null) rerollText.text = $"Reroll ({currentRerollCount})";
    }
    #endregion

    #region Selection & UI Spawning (Upgrade Cards)
    public void ShowUpgradeSelection()
    {
        List<UpgradeCard> available = GetAvailableCards();
        if (available.Count == 0)
        {
            Debug.Log("[UpgradeManager] Semua skill sudah MAX dan arena penuh!");
            return;
        }

        PopulateOptionsFromAvailable(available);

        if (isAutoBattle)
        {
            ApplyUpgrade(currentOptions[0]);
            return;
        }

        Time.timeScale = 0f;
        if (cardChoicePanel != null) cardChoicePanel.SetActive(true);

        ClearSpawnedCards();
        SpawnCardUIElements();
    }

    private void PopulateOptionsFromAvailable(List<UpgradeCard> available)
    {
        currentOptions.Clear();
        int countToPick = Mathf.Min(cardButtons != null && cardButtons.Length > 0 ? cardButtons.Length : 3, available.Count);

        for (int i = 0; i < countToPick; i++)
        {
            float totalWeight = 0f;
            foreach (var card in available) totalWeight += GetCardWeight(card);

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
    }

    private List<UpgradeCard> GetAvailableCards()
    {
        List<UpgradeCard> available = new List<UpgradeCard>();
        bool slotMasihAda = SlotGridManager.Instance != null && SlotGridManager.Instance.GetRandomEmptySlot() != null;
        bool hasFighterOnGrid = CheckClassOnGrid(CharacterClassType.Fighter);

        foreach (var card in cardPool)
        {
            if (card.buffType == BuffType.TrickshotFighter && (!hasFighterOnGrid || IsClassBanned(CharacterClassType.Fighter)))
                continue;

            if (card.buffType == BuffType.AddRandomCharacter || 
                card.buffType == BuffType.AddSpecificCharacter || 
                card.buffType == BuffType.BlackMarketDeal)
            {
                if (slotMasihAda) available.Add(card);
            }
            else if (card.buffType == BuffType.EmergencyRepair || !card.IsMaxLevel)
            {
                available.Add(card);
            }
        }

        return available;
    }

    private void SpawnCardUIElements()
    {
        Transform parentToUse = cardSpawnParent != null ? cardSpawnParent : cardChoicePanel.transform;

        for (int i = 0; i < currentOptions.Count; i++)
        {
            if (cardPrefab == null) break;

            GameObject cardObj = Instantiate(cardPrefab, parentToUse);
            spawnedCards.Add(cardObj);

            UpgradeCard c = currentOptions[i];
            GetCardDisplayInfo(c, out string displayTitle, out string displayDesc);

            CardUI cardUI = cardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                int cardIndex = i;
                int requiredStars = c.currentLevel + 1;
                cardUI.SetupCard(displayTitle, displayDesc, null, requiredStars, () => ApplyUpgradeByIndex(cardIndex));
            }
        }
    }

    private void ClearSpawnedCards()
    {
        foreach (var obj in spawnedCards) if (obj != null) Destroy(obj);
        spawnedCards.Clear();
    }

    private void GetCardDisplayInfo(UpgradeCard card, out string title, out string description)
    {
        title = card.cardName;

        if (card.buffType == BuffType.AddRandomCharacter)
        {
            title = "Recruit: Random Agent";
            description = "Spawn 1 agen acak (*1).\n(3 agen sejenis *1 melebur jadi *2)";
            return;
        }

        if (card.buffType == BuffType.AddSpecificCharacter)
        {
            title = $"Deploy: {card.targetClassType}";
            description = $"Spawn langsung 1 {card.targetClassType} (*1).\nBagus untuk melengkapi merge!";
            return;
        }

        float nextVal = card.GetNextPlayerValue();
        float nextMob = card.GetNextMobValue();
        string playerLabel, mobLabel;

        switch (card.buffType)
        {
            case BuffType.BoostAttack: playerLabel = $"DMG Karakter +{nextVal}%"; mobLabel = $"HP Musuh +{nextMob}%"; break;
            case BuffType.BoostAttackSpeed: playerLabel = $"ASPD +{nextVal}%"; mobLabel = $"Spawn Musuh +{nextMob}% lebih cepat"; break;
            case BuffType.SlowMob: playerLabel = $"Slow Musuh -{nextVal}%"; mobLabel = $"HP Musuh +{nextMob}%"; break;
            case BuffType.ExpGain: playerLabel = $"Bonus EXP +{nextVal}%"; mobLabel = $"DMG Musuh ke Base +{nextMob}%"; break;
            case BuffType.EmergencyRepair: playerLabel = $"Heal Base +{nextVal}"; mobLabel = $"DMG Musuh ke Base +{nextMob}%"; break;
            case BuffType.FortifiedBastion: playerLabel = $"Max HP Base +{nextVal} & Heal"; mobLabel = $"HP Musuh +{nextMob}%"; break;
            case BuffType.TrickshotFighter: playerLabel = "Ricochet Fighter (Hit ke-2 -40%)"; mobLabel = $"Kecepatan Musuh +{nextMob}%"; break;
            case BuffType.GlassCannonCore: playerLabel = $"Fighter/Mage ATK +{nextVal}%"; mobLabel = "Base HP -25%"; break;
            case BuffType.DeepFreeze: playerLabel = $"Slow Support +{nextVal}% (2x Durasi)"; mobLabel = "Boss kebal slow pelan"; break;
            case BuffType.HeavyCaliber: playerLabel = $"Ranged Tembus Armor +{nextVal}%"; mobLabel = "Jeda tembak Ranged +1.0s"; break;
            case BuffType.ThornsPlating: playerLabel = "Shockwave Base Damager"; mobLabel = "Repair -25%"; break;
            case BuffType.ArcaneOvercharge: playerLabel = $"AoE Damage Mage +{nextVal}%"; mobLabel = "Jeda tembak Mage +1.0s"; break;
            case BuffType.BlackMarketDeal: title = "Black Market Deal"; playerLabel = "2 Tiket Deploy Instan"; mobLabel = "Ekstra peluang Mini-Tank"; break;
            case BuffType.DesperateGambit: playerLabel = $"HP <30%: ASPD +{nextVal}%, ATK +15%"; mobLabel = "Musuh tabrak base DMG x2"; break;
            default: playerLabel = $"Efek +{nextVal}"; mobLabel = $"Mob +{nextMob}"; break;
        }

        description = $"{playerLabel} | {mobLabel}";
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

        if (isSingleUse) return 15f;
        if (card.buffType == BuffType.AddRandomCharacter || card.buffType == BuffType.AddSpecificCharacter || 
            card.buffType == BuffType.EmergencyRepair || card.buffType == BuffType.BlackMarketDeal) return 45f;
        if (card.buffType == BuffType.DesperateGambit) return 3f;

        return 70f;
    }
    #endregion

    #region Upgrade Logic & Application
    public void ApplyUpgradeByIndex(int index)
    {
        if (index >= 0 && index < currentOptions.Count) ApplyUpgrade(currentOptions[index]);
    }

    public void ApplyUpgrade(UpgradeCard card)
    {
        bool isBlackMarket = (card.buffType == BuffType.BlackMarketDeal);

        if (card.buffType == BuffType.AddRandomCharacter)
        {
            SpawnCharacterToGrid(GetRandomAllowedClass());
            Enemy.GlobalHpMultiplier += 0.10f;
        }
        else if (card.buffType == BuffType.AddSpecificCharacter)
        {
            SpawnCharacterToGrid(card.targetClassType);
            Enemy.GlobalHpMultiplier += 0.08f;
        }
        else if (card.buffType == BuffType.EvolvePathA || card.buffType == BuffType.EvolvePathB)
        {
            ApplyEvolvePath(card);
        }
        else
        {
            ApplyStatBuffCard(card);
        }

        UpdateBuffListDisplay();
        if (cardChoicePanel != null) cardChoicePanel.SetActive(false);

        if (!isBlackMarket && !isAutoBattle && GameManager.Instance != null)
        {
            GameManager.Instance.RestoreSpeedAfterModal();
        }
    }

    private void ApplyEvolvePath(UpgradeCard card)
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

    private void ApplyStatBuffCard(UpgradeCard card)
    {
        float playerVal = card.GetNextPlayerValue();
        float mobVal = card.GetNextMobValue();
        card.currentLevel++;

        BaseHealth baseHp = FindFirstObjectByType<BaseHealth>();

        switch (card.buffType)
        {
            case BuffType.BoostAttack:
                Character.GlobalDamageBonusPercent += playerVal;
                Enemy.GlobalHpMultiplier += (mobVal / 100f);
                break;
            case BuffType.BoostAttackSpeed:
                Character.GlobalAtkSpeedMultiplier += (playerVal / 100f);
                if (WaveManager.Instance != null)
                    WaveManager.Instance.spawnInterval = Mathf.Max(0.4f, 1.5f * (1f - (mobVal / 100f)));
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
                if (baseHp != null) baseHp.HealBase(playerVal);
                Enemy.GlobalDamageMultiplier += (mobVal / 100f);
                break;
            case BuffType.FortifiedBastion:
                if (baseHp != null)
                {
                    baseHp.maxHealth += playerVal;
                    baseHp.HealBase(playerVal);
                }
                Enemy.GlobalHpMultiplier += (mobVal / 100f);
                break;
            case BuffType.TrickshotFighter:
                Projectile.GlobalRicochetUnlocked = true;
                Enemy.GlobalSpeedMultiplier += (mobVal / 100f);
                break;
            case BuffType.GlassCannonCore:
                Character.GlobalDamageBonusPercent += playerVal;
                if (baseHp != null)
                {
                    baseHp.ReduceMaxHpPermanently(25f);
                    baseHp.repairEffectivenessMultiplier *= 0.75f;
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
                if (baseHp != null) baseHp.repairEffectivenessMultiplier *= 0.75f;
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

    public void SkipUpgradeSelection()
    {
        currentOptions.Clear();
        if (cardChoicePanel != null) cardChoicePanel.SetActive(false);

        bool isEvolutionOpen = (evolutionChoicePanel != null && evolutionChoicePanel.activeSelf);
        bool isBlackMarketOpen = (classPickModalPanel != null && classPickModalPanel.activeSelf);

        if (!isEvolutionOpen && !isBlackMarketOpen)
        {
            if (!isAutoBattle && GameManager.Instance != null)
                GameManager.Instance.RestoreSpeedAfterModal();
            else
                Time.timeScale = 1f;
        }
    }
    #endregion

    #region Evolution System (Dynamic Card Spawning)
    public void UnlockEvolutionCards(CharacterClassType cls)
    {
        foreach (var c in cardPool)
        {
            if (c.targetClassType == cls && (c.buffType == BuffType.EvolvePathA || c.buffType == BuffType.EvolvePathB))
                return;
        }

        GetEvolutionCardNames(cls, out string nameA, out string nameB);

        cardPool.Add(new UpgradeCard { cardName = nameA, buffType = BuffType.EvolvePathA, targetClassType = cls, playerValues = new float[] { 1f } });
        cardPool.Add(new UpgradeCard { cardName = nameB, buffType = BuffType.EvolvePathB, targetClassType = cls, playerValues = new float[] { 1f } });

        Debug.Log($"[UpgradeManager] Kartu evolusi untuk {cls} berhasil dibuka!");
    }

    private void GetEvolutionCardNames(CharacterClassType cls, out string nameA, out string nameB)
    {
        switch (cls)
        {
            case CharacterClassType.Fighter: nameA = "Evolve: Piercing Claw"; nameB = "Evolve: Triple Rend"; break;
            case CharacterClassType.Mage: nameA = "Evolve: Ignis Puddle"; nameB = "Evolve: Heavy Concussion"; break;
            case CharacterClassType.Support: nameA = "Evolve: Polar Vortex"; nameB = "Evolve: Roulette Fortune"; break;
            case CharacterClassType.Tank: nameA = "Evolve: Ironclad Fortress"; nameB = "Evolve: Scatter Blast"; break;
            case CharacterClassType.Ranged: nameA = "Evolve: Armor Piercer"; nameB = "Evolve: Headhunter"; break;
            default: nameA = "Evolve Path A"; nameB = "Evolve Path B"; break;
        }
    }

    public void TriggerInstantEvolutionChoice(Character character)
    {
        if (character == null) return;

        targetEvolutionChar = character;
        ClearSpawnedEvolutionCards();

        if (evolutionChoicePanel != null) evolutionChoicePanel.SetActive(true);

        GetEvolutionDisplayInfo(character.classType, out string titleA, out string descA, out string titleB, out string descB);

        SpawnEvolutionCard(0, titleA, descA, null);
        SpawnEvolutionCard(1, titleB, descB, null);

        Time.timeScale = 0f;
    }

    private void SpawnEvolutionCard(int index, string title, string desc, Sprite icon)
    {
        if (evolutionCardPrefab == null)
        {
            Debug.LogError("PERHATIAN: Slot 'Evolution Card Prefab' di Inspector UpgradeManager MASIH KOSONG!");
            return;
        }

        Transform containerToUse = evolutionCardContainer != null ? evolutionCardContainer : evolutionChoicePanel.transform;

        GameObject cardObj = Instantiate(evolutionCardPrefab, containerToUse);
        spawnedEvolutionCards.Add(cardObj);

        CardUI cardUI = cardObj.GetComponent<CardUI>();
        if (cardUI != null)
        {
            int pathIndex = index;
            cardUI.SetupCard(title, desc, icon, 3, () => SelectEvolutionByIndex(pathIndex));
        }
    }

    public void ClearSpawnedEvolutionCards()
    {
        foreach (var obj in spawnedEvolutionCards) if (obj != null) Destroy(obj);
        spawnedEvolutionCards.Clear();
    }

    private void GetEvolutionDisplayInfo(CharacterClassType classType, out string titleA, out string descA, out string titleB, out string descB)
    {
        titleA = descA = titleB = descB = "";

        switch (classType)
        {
            case CharacterClassType.Fighter:
                titleA = "Path A: Piercing Claw"; descA = "Serangan cakar menembus hingga 4 musuh sekaligus secara lurus (-10% DMG/hit).";
                titleB = "Path B: Triple Rend"; descB = "Setiap serangan ke-4 meluncurkan 3 cakaran beruntun secara instan.";
                break;
            case CharacterClassType.Mage:
                titleA = "Path A: Ignis Puddle"; descA = "Ledakan meninggalkan kubangan api selama 4 detik yang membakar musuh (35% DPS).";
                titleB = "Path B: Heavy Concussion"; descB = "Jeda tembak lebih lambat, radius ledakan masif + knockback kuat (3.5f).";
                break;
            case CharacterClassType.Support:
                titleA = "Path A: Polar Vortex"; descA = "Ledakan es memperlambat musuh 45% (2.5s) dan memberi status Chilled (+15% DMG tim).";
                titleB = "Path B: Roulette Fortune"; descB = "Tiap 60s memutar 1 buff tim acak selama 20s (ASPD, Crit Rate, Crit DMG, ATK, atau Range x1.5).";
                break;
            case CharacterClassType.Tank:
                titleA = "Path A: Ironclad Fortress"; descA = "Berhenti menyerang untuk memanggil perisai base (Aegis) penahan 1 serangan (cooldown 60s).";
                titleB = "Path B: Scatter Blast"; descB = "Tembakan shotgun cone 3 arah jarak dekat dengan efek knockback memukul mundur musuh.";
                break;
            case CharacterClassType.Ranged:
                titleA = "Path A: Armor Piercer"; descA = "Serangan terhadap musuh tipe Tank dan Boss selalu menghasilkan Critical Damage ekstra tinggi.";
                titleB = "Path B: Headhunter"; descB = "Tidak bisa crit, namun langsung mengeksekusi musuh non-Boss saat HP berada di bawah 10%.";
                break;
        }
    }

    public void SelectEvolutionByIndex(int pathIndex)
    {
        EvolutionPath chosen = (pathIndex == 0) ? EvolutionPath.PathA : EvolutionPath.PathB;

        if (targetEvolutionChar != null)
        {
            targetEvolutionChar.ApplyEvolution(chosen);
            UnlockStar3SpecialCard(targetEvolutionChar.classType);
        }

        ClearSpawnedEvolutionCards();
        if (evolutionChoicePanel != null) evolutionChoicePanel.SetActive(false);

        if (GameManager.Instance != null) GameManager.Instance.RestoreSpeedAfterModal();
        else Time.timeScale = 1f;
    }

    public void UnlockStar3SpecialCard(CharacterClassType cls)
    {
        switch (cls)
        {
            case CharacterClassType.Fighter:
                cardPool.Add(new UpgradeCard { cardName = "Glass Cannon Core", buffType = BuffType.GlassCannonCore, playerValues = new float[] { 35f }, mobValues = new float[] { 25f } });
                break;
            case CharacterClassType.Support:
                cardPool.Add(new UpgradeCard { cardName = "Deep Freeze", buffType = BuffType.DeepFreeze, playerValues = new float[] { 10f }, mobValues = new float[] { 2f } });
                break;
            case CharacterClassType.Ranged:
                cardPool.Add(new UpgradeCard { cardName = "Heavy Caliber", buffType = BuffType.HeavyCaliber, playerValues = new float[] { 25f }, mobValues = new float[] { 1.0f } });
                break;
            case CharacterClassType.Tank:
                cardPool.Add(new UpgradeCard { cardName = "Thorns Plating", buffType = BuffType.ThornsPlating, playerValues = new float[] { 1f }, mobValues = new float[] { 25f } });
                break;
            case CharacterClassType.Mage:
                cardPool.Add(new UpgradeCard { cardName = "Arcane Overcharge", buffType = BuffType.ArcaneOvercharge, playerValues = new float[] { 15f }, mobValues = new float[] { 1.0f } });
                break;
        }

        Debug.Log($"[UpgradeManager] Kartu eksklusif B3 untuk {cls} resmi dibuka ke pool upgrade!");
    }
    #endregion

    #region Black Market System
    public void StartBlackMarketPicks(int tickets)
    {
        remainingPicks = tickets;
        isPickingClass = true;

        Time.timeScale = 0f;
        if (classPickModalPanel != null) classPickModalPanel.SetActive(true);

        UpdateBlackMarketTitle();
        SpawnClassPickCards();

        Debug.Log($"[Black Market] Dimulai! Sisa tiket: {remainingPicks}");
    }

    private void SpawnClassPickCards()
    {
        ClearSpawnedClassCards();
        if (classPickPrefab == null) return;

        int count = 0;

        foreach (CharacterClassType cls in AllClassTypes)
        {
            if (IsClassBanned(cls)) continue;

            Transform targetContainer = (count < 3) ? topRowContainer : bottomRowContainer;
            if (targetContainer == null) targetContainer = classPickModalPanel.transform;

            GameObject cardObj = Instantiate(classPickPrefab, targetContainer);
            spawnedClassCards.Add(cardObj);

            CardUI cardUI = cardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                CharacterClassType currentClass = cls;
                GetClassDisplayInfo(currentClass, out string title, out string desc, out Sprite icon);
                cardUI.SetupCard(title, desc, icon, 1, () => OnClassPickSelected(currentClass));
            }

            count++;
        }
    }

    private void ClearSpawnedClassCards()
    {
        foreach (var obj in spawnedClassCards) if (obj != null) Destroy(obj);
        spawnedClassCards.Clear();
    }

    public void OnClassPickSelected(CharacterClassType selectedClass)
    {
        if (!isPickingClass || remainingPicks <= 0) return;

        if (IsClassBanned(selectedClass))
        {
            Debug.LogWarning($"[Black Market] Kelas {selectedClass} sedang di-banned dan tidak bisa dipilih!");
            return;
        }

        Debug.Log($"[Black Market] Memilih Class: {selectedClass}");
        SpawnCharacterToGrid(selectedClass);

        remainingPicks--;
        UpdateBlackMarketTitle();
        Time.timeScale = 0f;

        if (remainingPicks <= 0)
        {
            isPickingClass = false;
            ClearSpawnedClassCards();

            if (classPickModalPanel != null) classPickModalPanel.SetActive(false);

            if (GameManager.Instance != null) GameManager.Instance.RestoreSpeedAfterModal();
            else Time.timeScale = 1f;

            Debug.Log("[Black Market] Selesai! Panel ditutup dan waktu dilanjutkan.");
        }
    }

    private void GetClassDisplayInfo(CharacterClassType cls, out string title, out string desc, out Sprite icon)
    {
        title = $"Deploy {cls}";
        icon = null;

        switch (cls)
        {
            case CharacterClassType.Fighter: desc = "Petarung jarak dekat dengan kecepatan serang tinggi dan damage stabil."; break;
            case CharacterClassType.Mage: desc = "Penyerang area (AoE) yang memberikan damage ledakan ke banyak musuh."; break;
            case CharacterClassType.Support: desc = "Memberikan efek pembekuan/slow untuk memperlambat pergerakan musuh."; break;
            case CharacterClassType.Tank: desc = "Garda depan tangguh yang mampu menahan dan memukul mundur musuh."; break;
            case CharacterClassType.Ranged: desc = "Penembak jitu jarak jauh dengan damage kritikal tinggi ke target utama."; break;
            default: desc = "Unit agen tempur siap dideploy."; break;
        }
    }

    private void UpdateBlackMarketTitle()
    {
        if (blackMarketTitleText != null)
        {
            blackMarketTitleText.text = $"PILIH AGEN ({remainingPicks} TIKET TERSISA)";
        }
    }
    #endregion

    #region Helper & Utility Methods
    private void SpawnCharacterToGrid(CharacterClassType targetClass)
    {
        if (SlotGridManager.Instance == null) return;

        CharacterSlot availableSlot = SlotGridManager.Instance.GetRandomEmptySlot();
        if (availableSlot != null)
        {
            GameObject prefab = characterPrefab != null ? characterPrefab : SlotGridManager.Instance.characterPrefab;
            GameObject newChar = Instantiate(prefab, availableSlot.transform.position, Quaternion.identity);

            Character charComp = newChar.GetComponent<Character>();
            if (charComp != null) charComp.SetupClass(targetClass, 1);

            availableSlot.AssignCharacter(newChar);
            SlotGridManager.Instance.CheckAndExecuteMerge();
        }
        else
        {
            Debug.LogWarning("Semua slot grid sudah penuh!");
        }
    }

    private bool CheckClassOnGrid(CharacterClassType targetClass)
    {
        if (SlotGridManager.Instance == null) return false;

        foreach (var slot in SlotGridManager.Instance.allSlots)
        {
            if (slot != null && slot.isOccupied && slot.currentCharacter != null && slot.currentCharacter.classType == targetClass)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsClassBanned(CharacterClassType cls)
    {
        return bannedClasses != null && bannedClasses.Contains(cls);
    }

    public CharacterClassType GetRandomAllowedClass()
    {
        List<CharacterClassType> allowed = new List<CharacterClassType>();

        foreach (CharacterClassType cls in AllClassTypes)
        {
            if (!IsClassBanned(cls)) allowed.Add(cls);
        }

        if (allowed.Count == 0) return CharacterClassType.Fighter;
        return allowed[Random.Range(0, allowed.Count)];
    }

    public void ToggleAutoBattle()
    {
        isAutoBattle = !isAutoBattle;
        UpdateAutoBattleUI();
    }

    private void UpdateAutoBattleUI()
    {
        if (autoBattleText != null) autoBattleText.text = isAutoBattle ? "ON" : "OFF";
    }

    public void ToggleBuffList()
    {
        if (buffListPanel == null) return;
        buffListPanel.SetActive(!buffListPanel.activeSelf);
        if (buffListPanel.activeSelf) UpdateBuffListDisplay();
    }

    private void UpdateBuffListDisplay()
    {
        if (buffListContainer != null && buffItemPrefab != null)
        {
            foreach (Transform child in buffListContainer)
            {
                if (child.GetComponent<TMPro.TextMeshProUGUI>() != null) continue;  
                Destroy(child.gameObject);
            }

            foreach (var c in cardPool)
            {
                if (c.buffType != BuffType.AddRandomCharacter && 
                    c.buffType != BuffType.AddSpecificCharacter && 
                    c.buffType != BuffType.BlackMarketDeal && 
                    c.currentLevel > 0)
                {
                    float currentVal = c.playerValues != null && c.playerValues.Length > 0 
                        ? c.playerValues[Mathf.Min(c.currentLevel - 1, c.playerValues.Length - 1)] 
                        : 0f;

                    string effectDesc = GetActiveBuffDescription(c.buffType, currentVal);

                    GameObject obj = Instantiate(buffItemPrefab, buffListContainer);
                    BuffItemUI itemUI = obj.GetComponent<BuffItemUI>();

                    if (itemUI != null)
                    {
                        itemUI.SetupBuff(null, c.cardName, effectDesc, c.currentLevel);
                    }
                }
            }
            return; 
        }

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

                string effectDesc = GetActiveBuffDescription(c.buffType, currentVal);
                summary += $"• <b>{c.cardName} (Lv.{c.currentLevel})</b>\n   <color=#B0BEC5>{effectDesc}</color>\n\n";
            }
        }

        if (!hasActiveBuff) summary += "Belum ada buff skill aktif.";
        buffListText.text = summary;
    }

    private string GetActiveBuffDescription(BuffType buffType, float currentVal)
    {
        switch (buffType)
        {
            case BuffType.BoostAttack: return $"ATK Karakter +{currentVal}%";
            case BuffType.BoostAttackSpeed: return $"Attack Speed +{currentVal}%";
            case BuffType.SlowMob: return $"Slow Musuh {currentVal}%";
            case BuffType.ExpGain: return $"Bonus EXP +{currentVal}%";
            case BuffType.FortifiedBastion: return $"Max Base HP +{currentVal}";
            case BuffType.TrickshotFighter: return "Proyektil Fighter Ricochet";
            case BuffType.GlassCannonCore: return "Fighter & Mage ATK +35%, Base HP -25%";
            case BuffType.DeepFreeze: return "Slow Support +10% (Durasi 2x)";
            case BuffType.HeavyCaliber: return "Ranged Pierce Armor, Boss DMG +25%";
            case BuffType.ThornsPlating: return "Base Shockwave Damage Aktif";
            case BuffType.ArcaneOvercharge: return "Mage AoE Damage +15%";
            case BuffType.DesperateGambit: return "HP < 30%: ASPD +60%, ATK +15%";
            default: return "Aktif";
        }
    }
    #endregion

    #region Debug & Cheats
    [Header("Debug & Cheat Settings")]
    public CharacterClassType debugTargetClass = CharacterClassType.Fighter;
    public int debugBlackMarketTickets = 2;

    [ContextMenu("Cheat: F1 - Open Black Market")]
    public void OpenBlackMarketCheat()
    {
        Debug.Log("<color=yellow>[CHEAT F1] Open Black Market</color>");
        StartBlackMarketPicks(debugBlackMarketTickets);
    }

    [ContextMenu("Cheat: F2 - Spawn 3x Character (Auto Merge B3)")]
    public void AddCharacterCheat()
    {
        Debug.Log($"<color=yellow>[CHEAT F2] Spawn 3x {debugTargetClass} for Auto B3</color>");
        for (int i = 0; i < 3; i++)
        {
            SpawnCharacterToGrid(debugTargetClass);
        }
    }

    [ContextMenu("Cheat: F3 - Instant Evolution Choice")]
    public void InstantEvolutionCheat()
    {
        Debug.Log($"<color=yellow>[CHEAT F3] Trigger Instant Evolution for {debugTargetClass}</color>");

        Character targetChar = null;

        if (SlotGridManager.Instance != null)
        {
            foreach (var slot in SlotGridManager.Instance.allSlots)
            {
                if (slot != null && slot.isOccupied && slot.currentCharacter != null)
                {
                    if (slot.currentCharacter.classType == debugTargetClass)
                    {
                        targetChar = slot.currentCharacter;
                        break;
                    }
                }
            }

            if (targetChar == null)
            {
                foreach (var slot in SlotGridManager.Instance.allSlots)
                {
                    if (slot != null && slot.isOccupied && slot.currentCharacter != null)
                    {
                        targetChar = slot.currentCharacter;
                        break;
                    }
                }
            }
        }

        if (targetChar != null)
        {
            TriggerInstantEvolutionChoice(targetChar);
        }
        else
        {
            Debug.LogWarning("[CHEAT F3] Tidak ada karakter di grid untuk di-evolusi!");
        }
    }
    #endregion
}