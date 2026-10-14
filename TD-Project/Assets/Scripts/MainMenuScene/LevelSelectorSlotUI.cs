using TMPro;
using UnityEngine;

public class LevelSelectorSlotUI : MonoBehaviour
{
    [Header("UI Dependencies")]
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _energyCostText;

    private LevelSelectorUI _ui;
    private StageConfig _data;
    private string _stageLevel;
    private string _stageEnergyCost;

    public void Setup(StageConfig data, LevelSelectorUI ui)
    {
        _ui = ui;
        _data = data;

        _stageLevel = data.stageLevel.ToString();
        _stageEnergyCost = data.energyCost.ToString();

        _levelText.text = _stageLevel;
        _energyCostText.text = _stageEnergyCost;
    }

    public void OpenConfirmation()
    {
        _ui.OpenConfirmationPanel(_data);
        Debug.Log("Opening Level Confirmation");
    }
}