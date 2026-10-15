using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonSlotUI : MonoBehaviour
{
    [Header("UI Dependencies")]
    [SerializeField] private TextMeshProUGUI _dungeonNameText;
    [SerializeField] private TextMeshProUGUI _energyCostText;
    [SerializeField] private Image _dungeonImage;
    [SerializeField] private Button _playBtn;

    private DungeonUI _ui;
    private DungeonData _dungeonData;

    private void OnEnable()
    {
        if (_playBtn != null) 
            _playBtn.onClick.AddListener(OnPlayButtonClicked);
    }

    private void OnDisable()
    {
        if (_playBtn != null) 
            _playBtn.onClick.RemoveListener(OnPlayButtonClicked);
    }

    public void Setup(DungeonData data, DungeonUI ui)
    {
        _ui = ui;
        _dungeonData = data;

        if (_dungeonNameText != null)
            _dungeonNameText.text = data.dungeonName;

        if (_energyCostText != null)
            _energyCostText.text = $"{data.energyCost}";

        if (_dungeonImage != null)
            _dungeonImage.sprite = data.dungeonSprite;
    }

    private void OnPlayButtonClicked()
    {
        if (_ui != null)
            _ui.OpenConfirmationPanel(_dungeonData);
    }
}
