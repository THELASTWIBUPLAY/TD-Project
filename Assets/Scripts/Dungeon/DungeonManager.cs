using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private DungeonSO _dungeonDB;

    public DungeonSO DungeonDB => _dungeonDB;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}