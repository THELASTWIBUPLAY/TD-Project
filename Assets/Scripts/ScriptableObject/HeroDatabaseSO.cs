using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroDB", menuName = "Deck/Hero Database")]
public class HeroDatabaseSO : ScriptableObject
{
    [SerializeField] private List<HeroSO> _allHeroes = new List<HeroSO>();
    public IReadOnlyList<HeroSO> AllHeroes => _allHeroes;

    public HeroSO GetHeroById(string id)
    {
        return _allHeroes.Find(h => h != null && h.heroId == id);
    }
}