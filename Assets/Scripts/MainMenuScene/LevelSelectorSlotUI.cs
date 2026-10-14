using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectorSlotUI : MonoBehaviour
{
    [Header("UI Dependencies")]
    [SerializeField] private Sprite _normalStageImage;
    [SerializeField] private Sprite _bossStageImage;
    [SerializeField] private TextMeshProUGUI _levelText;

    private LevelSelectorUI _ui;
    private StageConfig _data;
    private int _stageLevel;
    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void Setup(StageConfig data, LevelSelectorUI ui)
    {
        _ui = ui;
        _data = data;

        _stageLevel = data.stageLevel;
        _levelText.text = _stageLevel.ToString();

        // Reset text color defaults at the start of setup
        _levelText.color = Color.white;

        if (_stageLevel % 10 == 0)
        {
            if (_bossStageImage != null)
            {
                _image.sprite = _bossStageImage;
            }
            _levelText.color = new Color(1f, 0.85f, 0f);
        }
        else if (_stageLevel % 5 == 0)
        {
            if (_bossStageImage != null)
            {
                _image.sprite = _bossStageImage;
            }
        }
        else
        {
            if (_normalStageImage != null)
            {
                _image.sprite = _normalStageImage;
            }
        }
    }

    public void OpenConfirmation()
    {
        _ui.OpenConfirmationPanel(_data);
        Debug.Log("Opening Level Confirmation");
    }
}
