using UnityEngine;
using UnityEngine.EventSystems;

public class DeckInventoryDropZone : MonoBehaviour, IDropHandler
{
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