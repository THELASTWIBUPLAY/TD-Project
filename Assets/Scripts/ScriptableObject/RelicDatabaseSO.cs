using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RelicDB", menuName = "Deck/Relic Database")]
public class RelicDatabaseSO : ScriptableObject
{
    [SerializeField] private List<RelicSO> _allRelics = new List<RelicSO>();
    public IReadOnlyList<RelicSO> AllRelics => _allRelics;

    public RelicSO GetRelicById(string id)
    {
        return _allRelics.Find(r => r != null && r.relicId == id);
    }
}