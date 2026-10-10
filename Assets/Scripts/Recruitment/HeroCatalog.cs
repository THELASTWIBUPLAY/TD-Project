using System;
using UnityEngine;

[CreateAssetMenu(menuName = "VARMON/Hero Catalog")]
public class HeroCatalog : ScriptableObject
{
    [Serializable]
    public class Hero
    {
        public string id;
        public string displayName;
        public Sprite portrait;
        public CharacterClassType battleClass;
        public GameObject battlePrefab;
    }

    public Hero[] heroes = Array.Empty<Hero>();
    public Hero Find(string id) => Array.Find(heroes, hero => hero.id == id);
}
