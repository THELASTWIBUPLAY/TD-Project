#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterSlot))]
public class CharacterSlotEditor : Editor
{
    [MenuItem("CONTEXT/CharacterSlot/Create Character...")]
    private static void CreateCharacterContextMenu(MenuCommand command)
    {
        CharacterSlot slot = (CharacterSlot)command.context;
        CreateCharacterWindow.ShowWindow(slot);
    }
}

public class CreateCharacterWindow : EditorWindow
{
    private CharacterSlot targetSlot;
    private CharacterClassType selectedClass = CharacterClassType.Melee;
    private int starLevel = 1;

    public static void ShowWindow(CharacterSlot slot)
    {
        CreateCharacterWindow window = GetWindow<CreateCharacterWindow>("Create Character");
        window.targetSlot = slot;
        window.minSize = new Vector2(300, 140);
        window.maxSize = new Vector2(300, 140);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        if (targetSlot == null)
        {
            EditorGUILayout.HelpBox("No CharacterSlot targeted.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField($"Target Slot: {targetSlot.gameObject.name}", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        selectedClass = (CharacterClassType)EditorGUILayout.EnumPopup("Class Type", selectedClass);
        starLevel = EditorGUILayout.IntSlider("Star Level", starLevel, 1, 3);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create & Assign Character", GUILayout.Height(30)))
        {
            Undo.RegisterFullObjectHierarchyUndo(targetSlot.gameObject, "Create Character");

            targetSlot.CreateCharacter(selectedClass, starLevel);
            SlotGridManager.Instance.CheckAndExecuteMerge();

            EditorUtility.SetDirty(targetSlot);
            Close();
        }
    }
}
#endif