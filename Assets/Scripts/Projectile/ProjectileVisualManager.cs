using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ProjectileLink
{
    public GameObject projectile;
    public CharacterClassType classType;
    [Range(1,3)]
    public int starLevel;
    public bool useForAllStars;
}

public class ProjectileVisualManager : MonoBehaviour
{
    public static ProjectileVisualManager Instance;
    public List<ProjectileLink> projectileLinks = new List<ProjectileLink>();

    private void Awake()
    {
        Instance = this;
    }

    public GameObject GetProjectilePrefab(CharacterClassType classType, int starLevel)
    {
        GameObject fallbackPrefab = null;

        foreach (var link in projectileLinks)
        {
            if (link.classType == classType)
            {
                if (!link.useForAllStars && link.starLevel == starLevel)
                {
                    return link.projectile;
                }

                if (link.useForAllStars && fallbackPrefab == null)
                {
                    fallbackPrefab = link.projectile;
                }
            }
        }

        if (fallbackPrefab != null)
        {
            return fallbackPrefab;
        }

        Debug.LogWarning($"No projectile prefab mapping found for {starLevel}-star {classType}");
        return null;
    }
}