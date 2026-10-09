using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VfxLink
{
    public string name;
    public GameObject prefab;
    public CharacterClassType classType;
    [Range(1,3)]
    public int starLevel;
    public bool useForAllStars;
}

public class VfxManager : MonoBehaviour
{
    public static VfxManager Instance;
    public List<VfxLink> projectileLinks = new List<VfxLink>();
    public List<VfxLink> auraLinks = new List<VfxLink>();

    private void Awake()
    {
        Instance = this;
    }

    public GameObject GetProjectilePrefab(CharacterClassType classType, int starLevel)
    {
        return GetLinkPrefab(projectileLinks, classType, starLevel, "projectile");
    }

    public GameObject GetAuraPrefab(CharacterClassType classType, int starLevel)
    {
        return GetLinkPrefab(auraLinks, classType, starLevel, "aura");
    }

    public GameObject GetLinkPrefab(List<VfxLink> links, CharacterClassType classType, int starLevel, string listName = "list")
    {
        if (links == null) return null;

        GameObject fallbackPrefab = null;

        foreach (var link in links)
        {
            if (link.classType == classType)
            {
                if (!link.useForAllStars && link.starLevel == starLevel)
                {
                    return link.prefab;
                }

                if (link.useForAllStars)
                {
                    fallbackPrefab = link.prefab;
                }
            }
        }

        if (fallbackPrefab != null)
        {
            return fallbackPrefab;
        }

        Debug.LogWarning($"No {listName} mapping found for {starLevel}-star {classType}");
        return null;
    }
}