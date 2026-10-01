using UnityEngine;

public class EconomyTester : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            EconomyManager.Instance.ModifyGem(50);
            Debug.Log("[TEST] Add 50 Gem!");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            EconomyManager.Instance.SimulateMemoryHack(9999);
            Debug.Log("[TEST] Simulate Hack by adding 9999 Gem!");
        }
    }
}
