using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ProfileManager : MonoBehaviour, ISaveable
{
    public static ProfileManager Instance { get; private set; }

    [Header("Profile Default")]
    [SerializeField] private string _defaultName = "The Hero";
    [SerializeField] private string _defaultBio = "I am ready";

    [Header("Level Progression Settings")]
    [SerializeField] private int _baseExpToNextLevel = 100;
    [SerializeField] private float _expGrowthMultiplier = 2.5f;

    //Runtime state
    private string _playerName;
    private string _playerBio;
    private int _accountLevel = 1;
    private int _accountExp = 0;

    public string PlayerId => AuthManager.Instance != null ? AuthManager.Instance.PlayerId : "Offline_ID";
    public string PlayerName => _playerName;
    public string PlayerBio => _playerBio;
    public int AccountLevel => _accountLevel;
    public int AccountExp => _accountExp;
    public int ExpToNextLevel => CalculateExpRequirment(_accountLevel);

    public event Action OnProfileChanged;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _playerName = _defaultName;
        _playerBio = _defaultBio;
    }

    private void Start()
    {
        var sm = SaveManager.Instance;
        if (sm != null && sm.IsLoaded)
        {
            LoadFromSaveData(sm.CurrentData);
        }
    }

    public int CalculateExpRequirment(int level)
    {
        return Mathf.RoundToInt(_baseExpToNextLevel * MathF.Pow(_expGrowthMultiplier, level - 1));
    }

    // Player Mutation
    public bool SetPlayerName(string newName)
    {
        string trimmed = newName?.Trim();

        if (string.IsNullOrEmpty(trimmed) || trimmed.Length < 3 || trimmed.Length > 16)
        {
            AlertManager.Instance?.Show("Name must between 3-16 characters");
            return false;
        }

        _playerName = trimmed;
        OnProfileChanged?.Invoke();
        SaveManager.Instance?.SaveLocal();
        return true;
    }

    public bool SetPlayerBio(string newBio)
    {
        string trimmed = newBio?.Trim() ?? string.Empty;
        if (trimmed.Length > 60)
        {
            AlertManager.Instance?.Show("Max 60 characters");
            return false;
        }

        _playerBio = trimmed;
        OnProfileChanged?.Invoke();
        SaveManager.Instance?.SaveLocal();
        return true;
    }

    public void AddAccountExp(int amount)
    {
        if (amount <= 0) return;

        _accountExp += amount;
        while (_accountExp >= ExpToNextLevel)
        {
            _accountExp -= ExpToNextLevel;
            _accountLevel++;
            AlertManager.Instance?.Show($"Level up to Lv. {_accountLevel}!");
        }

        OnProfileChanged?.Invoke();
        SaveManager.Instance?.SaveLocal();
    }

    // ISaveable
    public void PopulateSaveData(GameSaveData saveData)
    {
        if (saveData == null) return;
        if (saveData.profile == null) saveData.profile = new ProfileSaveData();

        saveData.profile.playerName = _playerName;
        saveData.profile.playerBio = _playerBio;
        saveData.profile.accountLevel = _accountLevel;
        saveData.profile.accountExp = _accountExp;
    }

    public void LoadFromSaveData(GameSaveData saveData)
    {
        if (saveData == null || saveData.profile == null) return;

        _playerName = string.IsNullOrEmpty(saveData.profile.playerName) ? _defaultName : saveData.profile.playerName;
        _playerBio = string.IsNullOrEmpty(saveData.profile.playerBio) ? _defaultBio : saveData.profile.playerBio;
        _accountLevel = Mathf.Max(1, saveData.profile.accountLevel);
        _accountExp = Mathf.Max(0, saveData.profile.accountExp);

        OnProfileChanged?.Invoke();
    }
}