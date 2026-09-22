using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    private int _gold;
    private int _gem;
    private int _energy;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddGold(int gold)
    {
        _gold += gold;
    }

    public void AddGem(int gem)
    {
        _gem += gem;
    }

    public void AddEnergy(int energy)
    {
        _energy += energy;
    }
}
