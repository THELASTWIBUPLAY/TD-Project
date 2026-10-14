using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckInventoryDropZone : MonoBehaviour, IDropHandler
{
    private void Awake()
    {
        // OTOMATISASI SENSOR HITBOX:
        // Memastikan GameObject ini selalu memiliki penangkap raycast transparan
        // sehingga OnDrop tetap aktif meski container di dalamnya kosong melompong.
        var graphic = GetComponent<Graphic>();
        if (graphic == null)
        {
            var img = gameObject.AddComponent<Image>();
            img.color = Color.clear; // Transparan 100%
            img.raycastTarget = true;
        }
        else
        {
            graphic.raycastTarget = true;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        // 1. Deteksi jika yang ditarik adalah DeckDropSlot (Equipped Slot atas)
        var droppedSlot = eventData.pointerDrag.GetComponent<DeckDropSlot>();
        if (droppedSlot != null && droppedSlot.IsOccupied)
        {
            DeckPanelUI.Instance?.UnequipItemFromSlot(droppedSlot.AllowedType, droppedSlot.SlotIndex);
            return;
        }

        // 2. Deteksi cadangan jika yang ditarik adalah komponen Draggable
        var draggedItem = eventData.pointerDrag.GetComponent<DeckDraggableItem>();
        if (draggedItem != null && draggedItem.isFromEquippedSlot)
        {
            DeckPanelUI.Instance?.UnequipItemFromSlot(draggedItem.type, draggedItem.sourceEquippedIndex);
        }
    }
}