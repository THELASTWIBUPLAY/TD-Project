using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePanelUI : MonoBehaviour
{
    [Header("UI Prefab & Containers")]
    public Transform statsContainer;  
    public GameObject statItemPrefab; 

    [System.Serializable]
    public struct ClassIconData
    {
        public CharacterClassType classType;
        public Sprite icon;
    }

    [Header("Class Icons Setup")]
    public List<ClassIconData> classIcons;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (statsContainer == null || statItemPrefab == null) return;

        foreach (Transform child in statsContainer)
        {
            Destroy(child.gameObject);
        }

        if (SlotGridManager.Instance == null) return;

        Dictionary<CharacterClassType, List<Character>> groupedCharacters = new Dictionary<CharacterClassType, List<Character>>();

        foreach (var slot in SlotGridManager.Instance.allSlots)
        {
            if (slot != null && slot.isOccupied && slot.currentCharacter != null)
            {
                Character c = slot.currentCharacter;
                if (!groupedCharacters.ContainsKey(c.classType))
                {
                    groupedCharacters[c.classType] = new List<Character>();
                }
                groupedCharacters[c.classType].Add(c);
            }
        }

        float totalDamageGlobal = GameManager.Instance != null ? GameManager.Instance.GetGlobalTotalDamage() : 1f;

        foreach (var kvp in groupedCharacters)
        {
            CharacterClassType classType = kvp.Key;
            List<Character> charList = kvp.Value;

            int b1 = 0, b2 = 0, b3 = 0;
            foreach (Character c in charList)
            {
                if (c.starLevel == 1) b1++;
                else if (c.starLevel == 2) b2++;
                else if (c.starLevel >= 3) b3++;
            }

            float classDmg = 0f;
            if (GameManager.Instance != null && GameManager.Instance.classTotalDamage.ContainsKey(classType))
            {
                classDmg = GameManager.Instance.classTotalDamage[classType];
            }

            float fillPercent = Mathf.Clamp01(classDmg / totalDamageGlobal);
            string detailText = $"x{charList.Count} (B1:{b1} B2:{b2} B3:{b3})";

            GameObject itemObj = Instantiate(statItemPrefab, statsContainer);
            StatItemUI itemUI = itemObj.GetComponent<StatItemUI>();

            if (itemUI != null)
            {
                Sprite iconSprite = GetIconForClass(classType);
                itemUI.Setup(iconSprite, classType.ToString(), detailText, classDmg, fillPercent);
            }
        }
    }

    private Sprite GetIconForClass(CharacterClassType type)
    {
        foreach (var data in classIcons)
        {
            if (data.classType == type) return data.icon;
        }
        return null;
    }
}