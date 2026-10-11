using Assets.Scripts.Economy.Managers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string LocalSaveFilePath => Path.Combine(Application.persistentDataPath, "game_save.json");
    private const string CloudSaveKey = "PLAYER_SAVE_DATA";

    private GameSaveData _currentData = new GameSaveData();
    public GameSaveData CurrentData => _currentData;

    public bool IsLoaded { get; private set; }
    private bool _isDistributing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadLocal();
        IsLoaded = true;
    }

    /// <summary>
    /// Mengumpulkan seluruh data dari manager gameplay dan menyimpannya ke disk lokal.
    /// </summary>
    public void SaveLocal()
    {
        if (!IsLoaded || _isDistributing)
        {
            Debug.LogWarning($"[SaveManager] SaveLocal diblokir sebelum load selesai!\n{StackTraceUtility.ExtractStackTrace()}");
            return;
        }

        _currentData.lastSavedTimestampUtc = DateTime.UtcNow.ToString("o");

        // 1. Minta masing-masing manager mengisi datanya
        CollectDataFromManagers(_currentData);

        // 2. Serialisasi ke format JSON
        string json = JsonUtility.ToJson(_currentData, true);

        // 3. Tulis ke direktori aman perangkat
        try
        {
            File.WriteAllText(LocalSaveFilePath, json, Encoding.UTF8);
            Debug.Log($"[SaveManager] Data lokal berhasil disimpan ke: {LocalSaveFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Gagal menulis file save lokal: {ex.Message}");
        }
    }

    /// <summary>
    /// Memuat data dari disk lokal ke seluruh manager.
    /// </summary>
    public bool LoadLocal()
    {
        if (!File.Exists(LocalSaveFilePath))
        {
            Debug.Log("[SaveManager] File save lokal belum ditemukan (Pemain Baru).");
            return false;
        }

        try
        {
            string json = File.ReadAllText(LocalSaveFilePath, Encoding.UTF8);
            _currentData = JsonUtility.FromJson<GameSaveData>(json);

            DistributeDataToManagers(_currentData);
            Debug.Log("[SaveManager] Data lokal berhasil dimuat ke seluruh sistem!");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Gagal membaca save lokal: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Sinkronisasi payload data ke Unity Gaming Services (UGS) Cloud Save.
    /// </summary>
    public async Task SyncToCloudAsync()
    {
        try
        {
            SaveLocal(); // Pastikan data lokal sudah yang paling mutakhir

            string json = JsonUtility.ToJson(_currentData);
            var payload = new System.Collections.Generic.Dictionary<string, object>
            {
                { CloudSaveKey, json }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(payload);
            Debug.Log("[SaveManager] Berhasil mencadangkan save ke UGS Cloud!");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveManager] Sinkronisasi Cloud tertunda: {ex.Message}");
        }
    }

    /// <summary>
    /// Mengambil data dari cloud saat pemain berganti HP atau login akun Google.
    /// </summary>
    public async Task<bool> LoadFromCloudAsync()
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new System.Collections.Generic.HashSet<string> { CloudSaveKey });

            if (savedData.TryGetValue(CloudSaveKey, out var item))
            {
                string json = item.Value.GetAs<string>();
                _currentData = JsonUtility.FromJson<GameSaveData>(json);

                // Perbarui penyimpanan lokal dengan data cloud terbaru
                File.WriteAllText(LocalSaveFilePath, json, Encoding.UTF8);

                DistributeDataToManagers(_currentData);
                Debug.Log("[SaveManager] Data cloud berhasil diunduh dan diterapkan!");
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveManager] Gagal mengambil data dari Cloud: {ex.Message}");
        }

        return false;
    }

    private void CollectDataFromManagers(GameSaveData target)
    {
        // Hubungkan ke modul yang ada di Scene
        if (EconomyManager.Instance is ISaveable eco) eco.PopulateSaveData(target);
        if (DailyQuestManager.Instance is ISaveable quest) quest.PopulateSaveData(target);
        if (AchievementManager.Instance is ISaveable ach) ach.PopulateSaveData(target);
        if (DailyRewardViewModel.Instance is ISaveable reward) reward.PopulateSaveData(target);
        if (TalentManager.Instance is ISaveable talent) talent.PopulateSaveData(target);
    }

    private void DistributeDataToManagers(GameSaveData source)
    {
        _isDistributing = true;
        try
        {
            if (EconomyManager.Instance is ISaveable eco) eco.LoadFromSaveData(source);
            if (DailyQuestManager.Instance is ISaveable quest) quest.LoadFromSaveData(source);
            if (AchievementManager.Instance is ISaveable ach) ach.LoadFromSaveData(source);
            if (DailyRewardViewModel.Instance is ISaveable reward) reward.LoadFromSaveData(source);
            if (TalentManager.Instance is ISaveable talent) talent.LoadFromSaveData(source);
        }
        finally
        {
            _isDistributing = false;
        }
    }

    // Auto-save saat pemain meminimalkan atau keluar dari game
    private void OnApplicationPause(bool pauseStatus)
    {
        #if !UNITY_EDITOR
            if (pauseStatus) SaveLocal();
        #endif
    }

    private void OnApplicationQuit()
    {
        #if !UNITY_EDITOR
            SaveLocal();
        #endif
    }
}