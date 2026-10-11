using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private TalentRewardSlotUI[] _rewardSlots;

    private void Awake()
    {
        _atkBuffBtn.onClick.AddListener(() => TalentManager.Instance?.ModifyAttackBuff());
        _hpBuffBtn.onClick.AddListener(() => TalentManager.Instance?.ModifyHpBuff());
        _coinBuffBtn.onClick.AddListener(() => TalentManager.Instance?.ModifyCoinBuff());
    }

    private void OnEnable()
    {
        if (TalentManager.Instance != null)
        {
            TalentManager.Instance.OnTalentUpdated += HandleTalentUpdate;
            TalentManager.Instance.OnTalentRewardUpdated += HandleRewardUpdate;
        }
        PositionRewardSlotOnSlider();
        RefreshAllUI();
    }

    private void OnDisable()
    {
        if (TalentManager.Instance != null)
        {
            TalentManager.Instance.OnTalentUpdated -= HandleTalentUpdate;
            TalentManager.Instance.OnTalentRewardUpdated -= HandleRewardUpdate;
        }
    }

    private void Start()
    {
        if (TalentManager.Instance != null)
        {
            _progressBar.maxValue = TalentManager.Instance.MaxTalentLevel * 3;

            TalentManager.Instance.OnTalentUpdated += HandleTalentUpdate;
            TalentManager.Instance.OnTalentRewardUpdated += HandleRewardUpdate;
        }

        PositionRewardSlotOnSlider();
        RefreshAllUI();
    }

    private void OnDestroy()
    {
        if (TalentManager.Instance != null)
        {
            TalentManager.Instance.OnTalentUpdated -= HandleTalentUpdate;
            TalentManager.Instance.OnTalentRewardUpdated -= HandleRewardUpdate;
        }
    }

    // Helper method to position reward slots on the slider based on their req levelr
    private void PositionRewardSlotOnSlider()
    {
        if (TalentManager.Instance == null || _progressBar == null) return;

        float maxLevel = _progressBar.maxValue;
        if (maxLevel <= 0) return;

        var tm = TalentManager.Instance;

        for (int i = 0; i < _rewardSlots.Length; i++)
        {
            if (_rewardSlots[i] == null) continue;

            int reqLevel = tm.GetRequiredLevelForTier(i);

            float normalizedPosition = Mathf.Clamp01((float)reqLevel / maxLevel);

            RectTransform rect = _rewardSlots[i].GetComponent<RectTransform>();
            if (rect != null)
            {
                // Set the anchored position based on the normalized position
                rect.pivot = new Vector2(normalizedPosition, rect.pivot.y);

                // Adjust the anchor to position the reward slot correctly on the slider
                rect.anchorMin = new Vector2(normalizedPosition, rect.anchorMin.y);
                rect.anchorMax = new Vector2(normalizedPosition, rect.anchorMax.y);

                // Reset the local position to ensure it aligns with the anchor
                rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
            }
        }
    }

    public void RefreshAllUI()
    {
        if (TalentManager.Instance == null) return;

        var tm = TalentManager.Instance;
        HandleTalentUpdate(TalentManager.TalentType.Attack, tm.AttackLevel, tm.CurrentTalentAttackBuff, tm.GetUpgradeCost(tm.AttackLevel));
        HandleTalentUpdate(TalentManager.TalentType.Hp, tm.HpLevel, tm.CurrentTalentHpBuff, tm.GetUpgradeCost(tm.HpLevel));
        HandleTalentUpdate(TalentManager.TalentType.Coin, tm.CoinLevel, tm.CurrentTalentCoinBuff, tm.GetUpgradeCost(tm.CoinLevel));
    
        for (int i = 0; i < _rewardSlots.Length; i++)
        {
            if (_rewardSlots[i] == null) continue;

            if (i < tm.Rewards.Count)
            {
                _rewardSlots[i].gameObject.SetActive(true);
                int reqLevel = tm.GetRequiredLevelForTier(i);
                var state = tm.GetRewardState(i);
                _rewardSlots[i].Setup(i, reqLevel, state);
            }
            else
            {
                _rewardSlots[i].gameObject.SetActive(false);
            }
        }
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

    private void HandleRewardUpdate(int tierIndex, TalentManager.TalentReward reward, TalentManager.TalentRewardState state)
    {
        if (tierIndex >= 0 && tierIndex < _rewardSlots.Length && _rewardSlots[tierIndex] != null)
        {
            _rewardSlots[tierIndex].ApplyVisualState(state);
        }
    }
}