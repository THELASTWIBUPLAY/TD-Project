using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LevelSelectorUI : MonoBehaviour
{
    [SerializeField] private List<StageConfig> _stageConfig;
    [SerializeField] private TextMeshProUGUI _stageNameText;
    [SerializeField] private LevelSelectorSlotUI[] _slotUI;
    [SerializeField] private PlayConfirmationUI _confirmPanel;

    private CanvasGroup _myPanel;

    private void Awake()
    {
        _myPanel = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (_confirmPanel != null)
        {
            _confirmPanel.gameObject.SetActive(false);
        }
        RefreshUI();
    }

    private void RefreshUI()
    {
        var config = _stageConfig.Where(stage => stage != null).ToList();

        if (config == null || config.Count == 0)
        {
            Debug.LogWarning("Stage Config is empty");
            return;
        }

        _stageNameText.text = config[0].stageName;

        for (int i = 0; i < _slotUI.Length; i++)
        {
            if (_slotUI[i] == null) continue;

            if (i < config.Count)
            {
                StageConfig data = config[i];

                _slotUI[i].Setup(data, this);
                _slotUI[i].gameObject.SetActive(true);
            }
            else
            {
                _slotUI[i].gameObject.SetActive(false);
            }
        }
    }

    public void OpenConfirmationPanel(StageConfig data)
    {
        if (_confirmPanel == null) return;

        _confirmPanel.SetupLevel(data);
        _confirmPanel.gameObject.SetActive(true);
    }

    public void OnBackButtonClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.BottomMenu, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.LeftMenu, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.ProfileMenu, true);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.EconomyBar, true);
        }
    }
}