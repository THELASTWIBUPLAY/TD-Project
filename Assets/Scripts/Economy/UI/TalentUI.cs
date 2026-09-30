using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentUI : MonoBehaviour
{
    [Header("UI Dependencies")]
    [SerializeField] private Button _atkBuffBtn;
    [SerializeField] private Button _hpBuffBtn;
    [SerializeField] private Button _coinBuffBtn;

    // Price
    [SerializeField] private TextMeshProUGUI _atkBuffPrice;
    [SerializeField] private TextMeshProUGUI _hpBuffPrice;
    [SerializeField] private TextMeshProUGUI _coinBuffPrice;

    // Level
    [SerializeField] private TextMeshProUGUI _atkLevelText;
    [SerializeField] private TextMeshProUGUI _hpLevelText;
    [SerializeField] private TextMeshProUGUI _coinLevelText;

    // Buff
    [SerializeField] private TextMeshProUGUI _atkBuffText;
    [SerializeField] private TextMeshProUGUI _hpBuffText;
    [SerializeField] private TextMeshProUGUI _coinBuffText;

    [SerializeField] private Slider _progressBar;

    private void OnEnable()
    {
        TalentManager.Instance.OnTalentUpdated += HandleTalentUpdate;
        RefreshAllUI();
    }

    private void OnDisable()
    {
        TalentManager.Instance.OnTalentUpdated -= HandleTalentUpdate;
    }

    private void Start()
    {
        _atkBuffBtn.onClick.AddListener(() => TalentManager.Instance.ModifyAttackBuff());
        _hpBuffBtn.onClick.AddListener(() => TalentManager.Instance.ModifyHpBuff());
        _coinBuffBtn.onClick.AddListener(() => TalentManager.Instance.ModifyCoinBuff());

        _progressBar.maxValue = TalentManager.Instance.MaxTalentLevel * 3;
        RefreshAllUI();
    }

    public void RefreshAllUI()
    {
        if (TalentManager.Instance == null) return;

        var tm = TalentManager.Instance;
        HandleTalentUpdate(TalentManager.TalentType.Attack, tm.AttackLevel, tm.CurrentTalentAttackBuff, tm.GetUpgradeCost(tm.AttackLevel));
        HandleTalentUpdate(TalentManager.TalentType.Hp, tm.HpLevel, tm.CurrentTalentHpBuff, tm.GetUpgradeCost(tm.HpLevel));
        HandleTalentUpdate(TalentManager.TalentType.Coin, tm.CoinLevel, tm.CurrentTalentCoinBuff, tm.GetUpgradeCost(tm.CoinLevel));
    }

    private void HandleTalentUpdate(TalentManager.TalentType type, int level, float buff, int cost)
    {
        bool isMax = level >= TalentManager.Instance.MaxTalentLevel;

        switch (type)
        {
            case TalentManager.TalentType.Attack:
                _atkLevelText.text = $"LV. {level}";
                _atkBuffText.text = $"+{buff:F1}% ATK";
                _atkBuffPrice.text = isMax ? "MAX" : cost.ToString("N0");
                _atkBuffBtn.interactable = !isMax;
                break;

            case TalentManager.TalentType.Hp:
                _hpLevelText.text = $"LV. {level}";
                _hpBuffText.text = $"+{buff:F1}% HP";
                _hpBuffPrice.text = isMax ? "MAX" : cost.ToString("N0");
                _hpBuffBtn.interactable = !isMax;
                break;

            case TalentManager.TalentType.Coin:
                _coinLevelText.text = $"LV. {level}";
                _coinBuffText.text = $"+{buff:F1}% Coins";
                _coinBuffPrice.text = isMax ? "MAX" : cost.ToString("N0");
                _coinBuffBtn.interactable = !isMax;
                break;
        }

        _progressBar.value = TalentManager.Instance.TotalTalentLevel;
    }
}