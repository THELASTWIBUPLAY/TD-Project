using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string className;
    public CharacterClassType classType;
    public Color classColor = Color.cyan;
    public float baseDamage = 10f;
    public float baseAttackCooldown = 0.7f;
    public float attackRange = 7f;
    public GameObject classProjectilePrefab;
}