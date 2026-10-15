using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DebugRaycast : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                // Ini akan memunculkan nama objek terdepan yang memblokir klik Anda
                Debug.Log("Objek yang terkena klik: " + results[0].gameObject.name, results[0].gameObject);
            }
        }
    }
}
