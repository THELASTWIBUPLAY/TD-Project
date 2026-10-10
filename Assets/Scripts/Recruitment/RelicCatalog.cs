using System;
using UnityEngine;

[CreateAssetMenu(menuName = "VARMON/Relic Catalog")]
public class RelicCatalog : ScriptableObject
{
    [Serializable]
    public class Relic
    {
        public string id;
        public string displayName;
        public Sprite icon;
        [TextArea] public string description;
    }

    public Relic[] relics = Array.Empty<Relic>();
    public Relic Find(string id) => Array.Find(relics, relic => relic.id == id);
}
