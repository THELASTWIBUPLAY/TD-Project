using UnityEngine;

public class DungeonUI : MonoBehaviour
{
    [SerializeField] private DungeonSlotUI[] _slots;
    [SerializeField] private PlayConfirmationUI _confirmPanel;

    private void Start()
    {
        if (_confirmPanel != null)
        {
            _confirmPanel.gameObject.SetActive(false);
        }
        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (DungeonManager.Instance == null || DungeonManager.Instance.DungeonDB == null) return;

        var dungeonList = DungeonManager.Instance.DungeonDB.Dungeons;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null) continue;

            if (i < dungeonList.Count)
            {
                _slots[i].Setup(dungeonList[i], this);
                _slots[i].gameObject.SetActive(true);
            }
            else
            {
                _slots[i].gameObject.SetActive(false);
            }
        }
    }

    public void OpenConfirmationPanel(DungeonData data)
    {
        if (_confirmPanel == null) return;

        _confirmPanel.Setup(data);
        _confirmPanel.gameObject.SetActive(true);
    }
}
