using UnityEngine;

namespace Assets.Scripts.Economy
{
    public class DailyQuestTester : MonoBehaviour
    {
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                DailyQuestManager.Instance?.AddProgress(DailyQuestType.Kill, 1);
                Debug.Log("[TEST] Kill progress +1");
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                DailyQuestManager.Instance?.AddProgress(DailyQuestType.Gather, 1);
                Debug.Log("[TEST] Gather Progress +1");
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                DailyQuestManager.Instance?.AddProgress(DailyQuestType.ClearDungeon, 1);
                Debug.Log("[TEST] ClearDungeon Progress +1");
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                DailyQuestManager.Instance?.DebugCompleteAllQuests();
                Debug.Log("[TEST] All Quest Cleared");
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                DailyQuestManager.Instance?.DebugResetAllQuests();
            }
        }
       #endif
    }
}