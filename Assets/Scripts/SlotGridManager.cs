using System.Collections.Generic;
using UnityEngine;

public class SlotGridManager : MonoBehaviour
{
    public static SlotGridManager Instance { get; private set; }

    [Header("Grid Layout (5 Kolom x 3 Baris)")]
    public int columns = 5;
    public int rows = 3;
    public float spacingX = 1.0f;
    public float spacingY = 1.0f;
    public Vector2 gridCenterPosition = new Vector2(0f, -2.5f);

    [Header("Prefabs")]
    public GameObject slotPrefab;
    public GameObject characterPrefab;

    [HideInInspector]
    public List<CharacterSlot> allSlots = new List<CharacterSlot>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GenerateGrid();
    }

    void Start()
    {
        SpawnDefaultCenterCharacter();
    }

    void GenerateGrid()
    {
        allSlots.Clear();

        float startX = gridCenterPosition.x - ((columns - 1) * spacingX / 2f);
        float startY = gridCenterPosition.y - ((rows - 1) * spacingY / 2f);

        int index = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector2 pos = new Vector2(startX + (c * spacingX), startY + (r * spacingY));
                GameObject newSlot = Instantiate(slotPrefab, pos, Quaternion.identity, transform);
                newSlot.name = $"Slot_{index}";

                CharacterSlot slotComponent = newSlot.GetComponent<CharacterSlot>();
                allSlots.Add(slotComponent);
                index++;
            }
        }
    }

    void SpawnDefaultCenterCharacter()
    {
        int centerIndex = allSlots.Count / 2;

        if (allSlots.Count > centerIndex && allSlots[centerIndex] != null)
        {
            CharacterSlot centerSlot = allSlots[centerIndex];
            GameObject defaultChar = Instantiate(characterPrefab, centerSlot.transform.position, Quaternion.identity);

            Character charComp = defaultChar.GetComponent<Character>();
            if (charComp != null)
            {
                charComp.SetupClass(CharacterClassType.Ranger, 1);
            }

            centerSlot.AssignCharacter(defaultChar);
        }
    }

    public CharacterSlot GetRandomEmptySlot()
    {
        List<CharacterSlot> emptySlots = new List<CharacterSlot>();

        foreach (CharacterSlot slot in allSlots)
        {
            if (slot != null && !slot.isOccupied)
            {
                emptySlots.Add(slot);
            }
        }

        if (emptySlots.Count > 0)
        {
            int randIndex = Random.Range(0, emptySlots.Count);
            return emptySlots[randIndex];
        }

        return null;
    }

    public void CheckAndExecuteMerge()
    {

        for (int star = 1; star <= 2; star++)
        {
            foreach (CharacterClassType cls in System.Enum.GetValues(typeof(CharacterClassType)))
            {
                List<CharacterSlot> matchingSlots = new List<CharacterSlot>();

                foreach (var slot in allSlots)
                {
                    if (slot != null && slot.isOccupied && slot.currentCharacter != null)
                    {

                        if (slot.currentCharacter.starLevel == star && slot.currentCharacter.classType == cls)
                        {
                            matchingSlots.Add(slot);
                        }
                    }
                }

                if (matchingSlots.Count >= 3)
                {
                    ExecuteMerge(matchingSlots[0], matchingSlots[1], matchingSlots[2], star + 1, cls);

                    CheckAndExecuteMerge();
                    return;
                }
            }
        }
    }

    void ExecuteMerge(CharacterSlot targetSlot, CharacterSlot sacrificeA, CharacterSlot sacrificeB, int newStar, CharacterClassType cls)
    {

        sacrificeA.ClearSlot();
        sacrificeB.ClearSlot();

        if (targetSlot.currentCharacter != null)
        {
            targetSlot.currentCharacter.SetupClass(cls, newStar);
            targetSlot.currentCharacter.PlayMergeCelebration();
        }
    }
}