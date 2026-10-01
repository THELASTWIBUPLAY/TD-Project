using System;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceItemType
{
    Gold,
    Gem,
}

[Serializable]
public struct ResourceItemData
{
    [HideInInspector] public ResourceItemType type;
    public string name;
    [Min(1)] public int amount;
    [Min(0)] public float price;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "ResourcesItems", menuName = "ResourcesShop/Resources")]
public class ResourcesItemSO : ScriptableObject
{
    [Header("Gold Settings. Price in Gems")]
    [SerializeField] private List<ResourceItemData> _goldItem = new List<ResourceItemData>();

    [Header("Gem Settings. Price in US Dollars")]
    [SerializeField] private List<ResourceItemData> _gemItem = new List<ResourceItemData>();

    public IReadOnlyList<ResourceItemData> GoldItem => _goldItem;
    public IReadOnlyList<ResourceItemData> GemItem => _gemItem;

    private void OnValidate()
    {
        for (int i = 0; i < _goldItem.Count; i++)
        {
            var item = _goldItem[i];
            item.type = ResourceItemType.Gold;
            _goldItem[i] = item;
        }

        for (int i = 0; i < _gemItem.Count; i++)
        {
            var item = _gemItem[i];
            item.type = ResourceItemType.Gem;
            _gemItem[i] = item;
        }
    }
}