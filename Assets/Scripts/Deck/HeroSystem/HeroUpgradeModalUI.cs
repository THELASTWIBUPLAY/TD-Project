using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class HeroUpgradeModalUI : MonoBehaviour
{
    [Header("Hero Identity")]
    [SerializeField] private Image _heroIcon;
    [SerializeField] private TextMeshProUGUI _heroNameText;
    [SerializeField] private TextMeshProUGUI _levelStarText;

    [Header("Stats Display (5 Pilar)")]
    [SerializeField] private TextMeshProUGUI _atkText;
    [SerializeField] private TextMeshProUGUI _cooldownText;
    [SerializeField] private TextMeshProUGUI _rangeText;
    [SerializeField] private TextMeshProUGUI _critRateText;
    [SerializeField] private TextMeshProUGUI _critDmgText;

    [Header("Dynamic Action Button")]
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _actionButtonText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Image _costCurrencyIcon;
    [SerializeField] private Sprite _goldIconSprite;
    [SerializeField] private Sprite _gemIconSprite;

    [Header("Ascend Skills Panel Integration")]
    [SerializeField] private Button _viewSkillsButton;
    [SerializeField] private GameObject _ascendSkillsPanel;
    [SerializeField] private Button _closeSkillsPanelButton;
    [SerializeField] private Transform _skillsContainer;
    [SerializeField] private GameObject _skillItemPrefab;

    [Header("Debug Controls")]
    [SerializeField] private Button _debugToggleLockBtn;
    [SerializeField] private TextMeshProUGUI _debugLockBtnText;

    [Header("Modal Controls")]
    [SerializeField] private Button _closeButton;

    private string _currentHeroId;
    private CanvasGroup _myCanvasGroup;

    private void Awake()
    {
        _myCanvasGroup = GetComponent<CanvasGroup>();

        if (_closeButton != null)
            _closeButton.onClick.AddListener(CloseModal);

        if (_viewSkillsButton != null)
            _viewSkillsButton.onClick.AddListener(() => ToggleSkillsPanel(true));

        if (_closeSkillsPanelButton != null)
            _closeSkillsPanelButton.onClick.AddListener(() => ToggleSkillsPanel(false));

        if (_debugToggleLockBtn != null)
            _debugToggleLockBtn.onClick.AddListener(OnDebugToggleLockClicked);
    }

    public void SetupModal(string heroId)
    {
        _currentHeroId = heroId;

        if (_myCanvasGroup == null) _myCanvasGroup = GetComponent<CanvasGroup>();

        if (MainMenuManager.Instance != null && _myCanvasGroup != null)
        {
            MainMenuManager.Instance.OpenPopup(_myCanvasGroup);
        }
        else
        {
            // Fallback pengaman mandiri jika tidak lewat MainMenuManager
            gameObject.SetActive(true);
            if (_myCanvasGroup != null)
            {
                _myCanvasGroup.alpha = 1f;
                _myCanvasGroup.interactable = true;
                _myCanvasGroup.blocksRaycasts = true;
            }
            transform.SetAsLastSibling();
        }

        if (_ascendSkillsPanel != null) _ascendSkillsPanel.SetActive(false);
        RefreshModal();
    }

    public void CloseModal()
    {
        if (_ascendSkillsPanel != null) _ascendSkillsPanel.SetActive(false);

        if (MainMenuManager.Instance != null && _myCanvasGroup != null)
        {
            MainMenuManager.Instance.ClosePopup(_myCanvasGroup);
        }
        else
        {
            if (_myCanvasGroup != null)
            {
                _myCanvasGroup.alpha = 0f;
                _myCanvasGroup.interactable = false;
                _myCanvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }
    }

    private void ToggleSkillsPanel(bool show)
    {
        if (_ascendSkillsPanel != null)
        {
            _ascendSkillsPanel.SetActive(show);
            if (show) RenderAscendSkills();
        }
    }

    private void RenderAscendSkills()
    {
        if (HeroManager.Instance == null || _skillsContainer == null || _skillItemPrefab == null) return;

        var hero = HeroManager.Instance.Database?.GetHeroById(_currentHeroId);
        if (hero == null) return;

        var entry = HeroManager.Instance.GetOrCreateEntry(_currentHeroId);

        // Bersihkan list kartu sebelumnya
        foreach (Transform child in _skillsContainer)
        {
            bool isHeroUnlocked = HeroManager.Instance.IsHeroUnlocked(_currentHeroId);
            Destroy(child.gameObject);
        }

        // Tampilkan seluruh pasif bintang
        foreach (var passive in hero.AscendPassives)
        {
            GameObject obj = Instantiate(_skillItemPrefab, _skillsContainer);
            var card = obj.GetComponent<HeroAscendSkillItemUI>();
            if (card != null)
            {
                bool isPassiveActive = entry.isUnlocked && entry.star >= passive.unlockAtStar;
                card.Setup(passive, isPassiveActive);
            }
        }
    }

    private void OnDebugToggleLockClicked()
    {
        if (HeroManager.Instance == null || string.IsNullOrEmpty(_currentHeroId)) return;

        HeroManager.Instance.DebugToggleHeroUnlock(_currentHeroId);
        RefreshModal();
    }

    public void RefreshModal()
    {
        bool isHeroUnlocked = HeroManager.Instance.IsHeroUnlocked(_currentHeroId);

        if (HeroManager.Instance == null) return;

        var hero = HeroManager.Instance.Database?.GetHeroById(_currentHeroId);
        if (hero == null) return;

        var entry = HeroManager.Instance.GetOrCreateEntry(_currentHeroId);
        var config = HeroManager.Instance.UpgradeConfig;
        var buttonState = HeroManager.Instance.GetUpgradeActionState(_currentHeroId);
        var stats = HeroManager.Instance.GetCalculatedStats(_currentHeroId);

        // 1. Identitas Visual
        if (_heroIcon != null) _heroIcon.sprite = hero.heroIcon;
        if (_heroNameText != null) _heroNameText.text = hero.heroName;

        int maxLevel = config != null ? config.GetMaxLevelForStar(entry.star) : 30;
        if (_levelStarText != null)
            _levelStarText.text = $"★{entry.star}  Lv. {entry.level}/{maxLevel} - {hero.classType}";

        // 2. Rincian 5 Stat
        if (_atkText != null) _atkText.text = $"{stats.totalDamage:F0}";
        if (_cooldownText != null) _cooldownText.text = $"{stats.attackCooldown:F2}s";
        if (_rangeText != null) _rangeText.text = $"{stats.attackRange:F1}";
        if (_critRateText != null) _critRateText.text = $"{stats.totalCritRate:F0}%";
        if (_critDmgText != null) _critDmgText.text = $"x{stats.totalCritDamage:F1}";

        // 3. Status Tombol Debug
        if (_debugLockBtnText != null)
        {
            _debugLockBtnText.text = isHeroUnlocked ? "DEBUG: LOCK HERO" : "DEBUG: UNLOCK HERO";
        }

        // 4. Tombol Aksi Dinamis
        _actionButton.onClick.RemoveAllListeners();

        if (!isHeroUnlocked)
        {
            _actionButton.interactable = false;
            _actionButtonText.text = "LOCKED";
            _costText.text = $"{entry.currentFragments}/{hero.requiredFragments} Frag";
            if (_costCurrencyIcon != null) _costCurrencyIcon.gameObject.SetActive(false);
            return;
        }

        switch (buttonState)
        {
            case HeroUpgradeButtonState.LevelUp:
                int goldCost = config != null ? config.GetGoldCostForLevelUp(entry.level) : 0;
                _actionButton.interactable = true;
                _actionButtonText.text = "LEVEL UP";
                _costText.text = goldCost.ToString("N0");
                if (_costCurrencyIcon != null)
                {
                    _costCurrencyIcon.gameObject.SetActive(true);
                    _costCurrencyIcon.sprite = _goldIconSprite;
                }

                _actionButton.onClick.AddListener(() =>
                {
                    if (HeroManager.Instance.TryLevelUp(_currentHeroId))
                        RefreshModal();
                });
                break;

            case HeroUpgradeButtonState.Ascend:
                var nextReq = config != null ? config.GetRequirementForStar(entry.star + 1) : default;
                _actionButton.interactable = true;
                _actionButtonText.text = "ASCEND";
                _costText.text = $"{nextReq.gemCost} Gem | {entry.currentFragments}/{nextReq.requiredHeroFragments} Frag";
                if (_costCurrencyIcon != null)
                {
                    _costCurrencyIcon.gameObject.SetActive(true);
                    _costCurrencyIcon.sprite = _gemIconSprite;
                }

                _actionButton.onClick.AddListener(() =>
                {
                    if (HeroManager.Instance.TryAscend(_currentHeroId))
                    {
                        RefreshModal();
                        if (_ascendSkillsPanel != null && _ascendSkillsPanel.activeSelf)
                            RenderAscendSkills();
                    }
                });
                break;

            case HeroUpgradeButtonState.MaxedOut:
                _actionButton.interactable = false;
                _actionButtonText.text = "MAX LEVEL";
                _costText.text = "-";
                if (_costCurrencyIcon != null)
                    _costCurrencyIcon.gameObject.SetActive(false);
                break;
        }
    }
}