using UnityEngine;

public class CharacterSlot : MonoBehaviour
{
    [Header("Status")]
    public bool isOccupied = false;
    public Character currentCharacter;

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
    private void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.8f, 0.8f, 0f));
    }
}