using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayTimeRewardController : MonoBehaviour, ISaveable
{
    public static PlayTimeRewardController Instance { get; private set; }

    public enum SlotState
    {
        Locked,         // Belum dibeli dengan Gem
        CountingDown,   // Terbuka & timer berjalan
        ReadyToClaim    // Waktu habis & siap diambil
    }

    [Serializable]
    public class SlotConfig
    {
        public string slotName = "Chest Slot";
        [Tooltip("Biaya Gem untuk membuka slot ini secara permanen (0 = gratis/terbuka awal)")]
        public int unlockGemCost = 0;
        [Tooltip("Durasi countdown dalam detik (default: 1800 detik / 30 menit)")]
        public float claimInterval = 1800f;
        [Tooltip("Jumlah Gold yang didapatkan")]
        public int goldReward = 100;
        [Tooltip("Jumlah Gem bonus jika ada")]
        public int gemReward = 0;
    }

    public class SlotRuntime
    {
        public bool isUnlocked;
        public float targetTime;
        public SlotState state;
    }

    [Header("Slot Configurations (4 Slots)")]
    [SerializeField]
    private SlotConfig[] _slotConfigs = new SlotConfig[4]
    {
        new SlotConfig { slotName = "Chest 1", unlockGemCost = 0,   claimInterval = 1800f, goldReward = 100 },
        new SlotConfig { slotName = "Chest 2", unlockGemCost = 50,  claimInterval = 1800f, goldReward = 150 },
        new SlotConfig { slotName = "Chest 3", unlockGemCost = 100, claimInterval = 1800f, goldReward = 200 },
        new SlotConfig { slotName = "Chest 4", unlockGemCost = 150, claimInterval = 1800f, goldReward = 250 }
    };

    private SlotRuntime[] _slotRuntimes;

    public event Action<int> OnSlotUpdated;
    public int TotalSlots => _slotConfigs.Length;

    private const string PrefKeyUnlockedPrefix = "PlayTime_SlotUnlocked_";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeSlots();
    }

    private void InitializeSlots()
    {
        if (_slotConfigs == null || _slotConfigs.Length == 0) return;

        // Alokasikan memori runtime persis sebesar jumlah slot di Inspector (misal 5)
        _slotRuntimes = new SlotRuntime[_slotConfigs.Length];

        for (int i = 0; i < _slotConfigs.Length; i++)
        {
            _slotRuntimes[i] = new SlotRuntime();

            // Slot 0 selalu terbuka default. Slot sisanya dibaca dari PlayerPrefs
            if (i == 0 || _slotConfigs[i].unlockGemCost <= 0)
            {
                _slotRuntimes[i].isUnlocked = true;
            }
            else
            {
                _slotRuntimes[i].isUnlocked = PlayerPrefs.GetInt(PrefKeyUnlockedPrefix + i, 0) == 1;
            }

            if (_slotRuntimes[i].isUnlocked)
            {
                StartSlotCycle(i);
            }
            else
            {
                _slotRuntimes[i].state = SlotState.Locked;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < _slotRuntimes.Length; i++)
        {
            var runtime = _slotRuntimes[i];
            if (runtime != null && runtime.isUnlocked && runtime.state == SlotState.CountingDown)
            {
                if (Time.time >= runtime.targetTime)
                {
                    runtime.state = SlotState.ReadyToClaim;
                    OnSlotUpdated?.Invoke(i);
                }
            }
        }
    }

    public void StartSlotCycle(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slotRuntimes.Length) return;

        var cfg = _slotConfigs[slotIndex];
        var runtime = _slotRuntimes[slotIndex];

        runtime.targetTime = Time.time + cfg.claimInterval;
        runtime.state = SlotState.CountingDown;
        OnSlotUpdated?.Invoke(slotIndex);
    }

    public SlotConfig GetConfig(int slotIndex) => _slotConfigs[slotIndex];
    public SlotState GetState(int slotIndex) => _slotRuntimes[slotIndex].state;

    public float GetRemainingSeconds(int slotIndex)
    {
        if (!_slotRuntimes[slotIndex].isUnlocked) return 0f;
        return Mathf.Max(0f, _slotRuntimes[slotIndex].targetTime - Time.time);
    }

    // =========================================================================
    // UNLOCK & CLAIM INTERACTION
    // =========================================================================

    public bool TryUnlockSlot(int slotIndex)
    {
        if (slotIndex <= 0 || slotIndex >= _slotConfigs.Length) return false;

        var runtime = _slotRuntimes[slotIndex];
        if (runtime.isUnlocked) return false;

        var cfg = _slotConfigs[slotIndex];

        if (EconomyManager.Instance == null || EconomyManager.Instance.CurrentData.gem < cfg.unlockGemCost)
        {
            AlertManager.Instance?.Show("Gem tidak mencukupi untuk membuka slot!");
            return false;
        }

        // Potong Gem & simpan status permanen
        EconomyManager.Instance.ModifyGem(-cfg.unlockGemCost);
        runtime.isUnlocked = true;
        PlayerPrefs.SetInt(PrefKeyUnlockedPrefix + slotIndex, 1);
        PlayerPrefs.Save();

        AlertManager.Instance?.Show($"{cfg.slotName} Terbuka!");
        StartSlotCycle(slotIndex);

        SaveManager.Instance?.SaveLocal();
        return true;
    }

    public void ClaimReward(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slotRuntimes.Length) return;

        var runtime = _slotRuntimes[slotIndex];
        if (runtime.state != SlotState.ReadyToClaim)
        {
            AlertManager.Instance?.Show("Peti belum siap diklaim!");
            return;
        }

        var cfg = _slotConfigs[slotIndex];

        if (EconomyManager.Instance != null)
        {
            if (cfg.goldReward > 0) EconomyManager.Instance.ModifyGold(cfg.goldReward);
            if (cfg.gemReward > 0) EconomyManager.Instance.ModifyGem(cfg.gemReward);
        }

        AlertManager.Instance?.Show($"Hadiah {cfg.slotName} berhasil diambil!");
        StartSlotCycle(slotIndex);
        SaveManager.Instance?.SaveLocal();
    }

    [ContextMenu("Debug: Lock All Slots Except First")]
    public void Debug_LockAllSlotsExceptFirst()
    {
        if (_slotRuntimes == null || _slotConfigs == null)
        {
            Debug.LogWarning("[Debug] Runtime belum diinisialisasi. Jalankan game terlebih dahulu!");
            return;
        }

        // 1. Hapus penyimpanan PlayerPrefs untuk slot indeks 1 ke atas
        for (int i = 1; i < _slotConfigs.Length; i++)
        {
            string key = PrefKeyUnlockedPrefix + i;
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
        PlayerPrefs.Save();

        // 2. Paksa status runtime kembali terkunci
        for (int i = 0; i < _slotRuntimes.Length; i++)
        {
            // Lewati slot pertama (indeks 0) atau slot yang berbiaya gratis
            if (i == 0 || _slotConfigs[i].unlockGemCost <= 0)
            {
                continue;
            }

            _slotRuntimes[i].isUnlocked = false;
            _slotRuntimes[i].targetTime = 0f;
            _slotRuntimes[i].state = SlotState.Locked;

            // Picu pembaruan visual ke UI
            OnSlotUpdated?.Invoke(i);
        }

        // 3. Sinkronisasi dengan sistem penyimpanan lokal jika ada
        SaveManager.Instance?.SaveLocal();

        Debug.Log("<color=cyan>[Debug]</color> Semua data penyimpanan dihapus & slot runtime berhasil dikunci kembali (kecuali slot pertama).");
        AlertManager.Instance?.Show("Debug: Slot berhasil dikunci kembali!");
    }

    // =========================================================================
    // ISAVEABLE IMPLEMENTATION
    // =========================================================================

    public void PopulateSaveData(GameSaveData saveData)
    {
        // Menyinkronkan status buka slot ke save file jika field sudah tersedia
    }

    public void LoadFromSaveData(GameSaveData saveData)
    {
        InitializeSlots();
    }
}