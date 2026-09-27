using Assets.Scripts.Economy.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AchievementSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descText;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private TextMeshProUGUI _rewardText;

    [Header("Claim Elements")]
    [SerializeField] private Button _claimButton;
    [SerializeField] private GameObject _claimedBadge; // Badge/Teks bertuliskan "CLAIMED"

    private AchievementData _currentData;

    private void Awake()
    {
        if (_claimButton != null)
        {
            _claimButton.onClick.AddListener(OnClaimClicked);
        }
    }

    private void OnDestroy()
    {
        if (_claimButton != null)
        {
            _claimButton.onClick.RemoveListener(OnClaimClicked);
        }
    }

    public void Setup(AchievementData data, int currentProgress, AchievementState state)
    {
        _currentData = data;

        if (_titleText != null) _titleText.text = data.title;
        if (_descText != null) _descText.text = data.description;
        if (_iconImage != null && data.icon != null) _iconImage.sprite = data.icon;

        // Teks Hadiah
        if (_rewardText != null)
        {
            if (data.rewardGold > 0 && data.rewardGem > 0)
                _rewardText.text = $"{data.rewardGold}G + {data.rewardGem}D";
            else if (data.rewardGold > 0)
                _rewardText.text = $"{data.rewardGold} Gold";
            else
                _rewardText.text = $"{data.rewardGem} Gem";
        }

        // Teks dan Nilai Progress Bar
        if (_progressText != null)
            _progressText.text = $"{currentProgress} / {data.targetGoal}";

        if (_progressSlider != null)
            _progressSlider.value = Mathf.Clamp01((float)currentProgress / data.targetGoal);

        // Aturan Visibilitas Tombol & Badge
        ApplyVisualState(state);
    }
    private void ApplyVisualState(AchievementState state)
    {
        switch (state)
        {
            case AchievementState.InProgress:
                // Masih berprogres: Sembunyikan tombol klaim dan badge
                if (_claimButton != null) _claimButton.gameObject.SetActive(false);
                if (_claimedBadge != null) _claimedBadge.SetActive(false);
                break;

            case AchievementState.ReadyToClaim:
                // Target tercapai: Munculkan tombol klaim dan sembunyikan badge
                if (_claimButton != null)
                {
                    _claimButton.gameObject.SetActive(true);
                    _claimButton.interactable = true;
                }
                if (_claimedBadge != null) _claimedBadge.SetActive(false);
                break;

            case AchievementState.Claimed:
                // Sudah diklaim: Sembunyikan tombol klaim dan munculkan badge
                if (_claimButton != null) _claimButton.gameObject.SetActive(false);
                if (_claimedBadge != null) _claimedBadge.SetActive(true);
                break;
        }
    }

    private void OnClaimClicked()
    {
        if (AchievementManager.Instance == null) return;

        // 1. Kirim perintah klaim ke manager
        AchievementManager.Instance.ClaimAchievement(_currentData);

        // 2. Lepas fokus EventSystem agar tidak tersangkut di UI
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // 3. Langsung ubah visual kartu detik itu juga ke state Claimed
        ApplyVisualState(AchievementState.Claimed);
    }
}