using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RecruitmentService))]
public class RecruitmentServiceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.HelpBox("Test player only: set Protected Cloud Save recruitment_debug_enabled to true. Requires the updated RecruitHero script. Each button adds 10 keys on the server.", MessageType.Info);
        var service = (RecruitmentService)target;
        using (new EditorGUI.DisabledScope(!Application.isPlaying || service.Busy))
            foreach (RecruitmentChestType chest in System.Enum.GetValues(typeof(RecruitmentChestType)))
                if (GUILayout.Button("Debug: +10 " + chest + " keys (or retry)")) Grant(service, chest);
    }
    private async void Grant(RecruitmentService service, RecruitmentChestType chest)
    {
        try {
            var reply = await service.DebugAddKeysAsync(chest);
            if (!reply.ok) Debug.LogWarning("[Recruitment debug] " + reply.error);
            else Debug.Log("[Recruitment debug] " + chest + " keys: " + service.KeysFor(chest));
        } catch (System.Exception e) { Debug.LogWarning("[Recruitment debug] " + e.Message + " Retry the same debug button if the connection was interrupted."); }
    }
}
