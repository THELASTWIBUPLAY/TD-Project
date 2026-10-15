using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DungeonData
{
    public string dungeonName;
    public string dungeonScene;
    public int energyCost;
}

[CreateAssetMenu(fileName = "DungeonDB", menuName = "Dungeon/Dungeon Database")]
public class DungeonSO : ScriptableObject
{
    [SerializeField] private List<DungeonData> dungeons = new List<DungeonData>();

    public IReadOnlyList<DungeonData> Dungeons => dungeons;
}
