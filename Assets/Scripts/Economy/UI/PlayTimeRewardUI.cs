using UnityEngine;

public class PlayTimeRewardUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayTimeRewardController _controller;
    [SerializeField] private PlayTimeSlotUI[] _slots = new PlayTimeSlotUI[4];

    private void Awake()
    {
        if (_controller == null)
            _controller = PlayTimeRewardController.Instance ?? FindFirstObjectByType<PlayTimeRewardController>();
    }

    private void OnEnable()
    {
        if (_controller != null)
            _controller.OnSlotUpdated += HandleSlotUpdated;

        RefreshAllSlots();
    }

    private void OnDisable()
    {
        if (_controller != null)
            _controller.OnSlotUpdated -= HandleSlotUpdated;
    }

    private void Start()
    {
        RefreshAllSlots();
    }

    private void RefreshAllSlots()
    {
        if (_controller == null) return;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null)
            {
                _slots[i].Setup(_controller, i);
            }
        }
    }

    private void HandleSlotUpdated(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _slots.Length && _slots[slotIndex] != null)
        {
            _slots[slotIndex].RefreshDisplay();
        }
    }
}