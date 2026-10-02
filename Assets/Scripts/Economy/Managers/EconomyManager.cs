using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour, ISaveable
{
    public static EconomyManager Instance { get; private set; }

    [Serializable]
    public struct EconomyData
    {
        public int gold { get; internal set; }
        public int gem { get; internal set; }
        public int currentEnergy { get; internal set; }
        public int maxEnergy { get; internal set; }
    }
        
    // Binding: UI will sub to this
    public event Action<EconomyData> OnEconomyChanged;

    private EconomyData _currentData;
    public EconomyData CurrentData => _currentData;

    // Security Key XOR Encryption
    private int _secureGemKey;
    private int _encryptedGem;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _secureGemKey = UnityEngine.Random.Range(1000, 9999);
        SetEncryptedGem(0);

        _currentData.maxEnergy = 50;
    }

    // Helper method for encryption
    private void SetEncryptedGem(int actualValue)
    {
        _encryptedGem = actualValue ^ _secureGemKey;
        _currentData.gem = actualValue;
    }

    // Helper method for decryption
    private int GetDecryptedGem()
    {
        return _encryptedGem ^ _secureGemKey;
    }

    public void ModifyGold(int gold)
    {
        _currentData.gold += gold;
        NotifyUpdate();
        Debug.Log($"[Economy Manager] Modify gold by {gold}. Current gold is {_currentData.gold}");
    }

    public void ModifyGem(int gem)
    {
        int realGemValue = GetDecryptedGem();

        if (realGemValue != _currentData.gem)
        {
            Debug.LogWarning("[ANTI-CHEAT] Gem cheat detected. Exiting game...");
            
            _currentData.gem = realGemValue;
            NotifyUpdate();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.ExitPlaymode();
            #endif
            return;
        }

        int newGemTotal = _currentData.gem + gem;

        SetEncryptedGem(newGemTotal);
        NotifyUpdate();
        Debug.Log($"[Economy Manager] Modify gem by {gem}. Current gem is {_currentData.gem}");
    }

    public void ModifyEnergy(int energy)
    {
        _currentData.currentEnergy += energy;
        NotifyUpdate();
        Debug.Log($"[Economy Manager] Modify energy by {energy}. Current energy is {_currentData.currentEnergy}");
    }

    private void NotifyUpdate()
    {
        OnEconomyChanged?.Invoke(_currentData);
    }

    public void PopulateSaveData(GameSaveData saveData)
    {
        saveData.economy.gold = _currentData.gold;
        saveData.economy.gem = _currentData.gem;
        saveData.economy.currentEnergy = _currentData.currentEnergy;
    }

    public void LoadFromSaveData(GameSaveData saveData)
    {
        _currentData.gold = saveData.economy.gold;
        SetEncryptedGem(saveData.economy.gem); // Set via enkripsi XOR
        _currentData.currentEnergy = saveData.economy.currentEnergy;

        NotifyUpdate(); // Pemicu agar EconomyBarUI langsung menampilkan angka hasil load
    }


    // Debug
    [ContextMenu("Reset All Energy")]
    public void ResetAllEnergy()
    {
        _currentData.currentEnergy = 0;
    }

#if UNITY_EDITOR
    public void SimulateMemoryHack(int fakeGemValue)
        {
            // Simulate What GameGuardian on RAM:
            // Changing gem value without going through ModifyGem() Method
            _currentData.gem = fakeGemValue;
            Debug.LogWarning($"[HACK SIMULATOR] Change gem value forcefully by {fakeGemValue}");
        }
    #endif
}