using UnityEngine;

public class CharacterSlot : MonoBehaviour
{
    [Header("Status")]
    public bool isOccupied = false;
    public Character currentCharacter;
    public GameObject characterPrefab;

    public void AssignCharacter(GameObject characterGO)
    {
        isOccupied = true;
        currentCharacter = characterGO.GetComponent<Character>();

        characterGO.transform.position = transform.position;
        characterGO.transform.SetParent(transform);
    }

    public void ClearSlot()
    {
        isOccupied = false;

        if (currentCharacter != null)
        {
            Destroy(currentCharacter.gameObject);
        }
        else
        {

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        currentCharacter = null;
    }

    public Character CreateCharacter(CharacterClassType classType = CharacterClassType.Ranger, int starLevel = 1)
    {
        if (isOccupied)
        {
            ClearSlot();
        }

        GameObject newCharGO = Instantiate(characterPrefab, transform.position, Quaternion.identity);
        currentCharacter = newCharGO.GetComponent<Character>();

        if (currentCharacter != null)
        {
            currentCharacter.SetupClass(classType, starLevel);
        }

        newCharGO.transform.position = transform.position;
        newCharGO.transform.SetParent(transform);
        isOccupied = true;

        return currentCharacter;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.8f, 0.8f, 0f));
    }
}