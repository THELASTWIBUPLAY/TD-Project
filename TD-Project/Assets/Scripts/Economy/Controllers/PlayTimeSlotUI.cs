using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayTimeSlotUI : MonoBehaviour
{
    [Header("Slot Configuration")]
    [SerializeField] private int _slotIndex;

    [Header("Kandang / Cage (Tombol Klaim)")]
    [SerializeField] private Image _cageImage;
    [SerializeField] private Button _cageButton;

    [Header("Progress Elements (Menggantikan Timer Teks)")]
    [Tooltip("Slider bar yang berjalan selama masa cooldown")]
    [SerializeField] private Slider _progressSlider;
    [Tooltip("Teks yang muncul menggantikan slider ketika waktu sudah penuh")]
    [SerializeField] private TextMeshProUGUI _claimText;

    [Header("Warna Kandang")]
    [SerializeField] private Color _lockedCageColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color _unlockedCageColor = Color.white;

    [Header("Gembok / Lock (Tombol Unlock Gem)")]
    [SerializeField] private Button _lockButton;
    [SerializeField] private TextMeshProUGUI _gemCostText;

    private PlayTimeRewardController _controller;

    private void Awake()
    {
        if (_cageButton != null)
            _cageButton.onClick.AddListener(OnCageClicked);

        if (_lockButton != null)
            _lockButton.onClick.AddListener(OnLockClicked);

        if (_progressSlider != null)
        {
            _progressSlider.minValue = 0f;
            _progressSlider.maxValue = 1f;
        }
    }

    private void OnDestroy()
    {
        if (_cageButton != null)
            _cageButton.onClick.RemoveListener(OnCageClicked);

        if (_lockButton != null)
            _lockButton.onClick.RemoveListener(OnLockClicked);
    }

    public void Setup(PlayTimeRewardController controller, int slotIndex)
    {
        _controller = controller;
        _slotIndex = slotIndex;
        RefreshDisplay();
    }

    private void Update()
    {
        if (_controller == null) return;

        // Update fill bar slider secara mulus setiap frame selama countdown
        if (_controller.GetState(_slotIndex) == PlayTimeRewardController.SlotState.CountingDown)
        {
            UpdateProgressDisplay();
        }
    }

    private void UpdateProgressDisplay()
    {
        if (_progressSlider == null) return;

        var cfg = _controller.GetConfig(_slotIndex);
        if (cfg.claimInterval <= 0f) return;

        float remainingSeconds = _controller.GetRemainingSeconds(_slotIndex);
        // Nilai 0.0 (awal mulai) bergerak ke 1.0 (penuh)
        float progress = Mathf.Clamp01(1f - (remainingSeconds / cfg.claimInterval));
        _progressSlider.value = progress;
    }

    public void RefreshDisplay()
    {
        if (_controller == null) return;

        var cfg = _controller.GetConfig(_slotIndex);
        var state = _controller.GetState(_slotIndex);

        switch (state)
        {
            case PlayTimeRewardController.SlotState.Locked:
                // Warna redup & tombol klaim mati
                if (_cageImage != null) _cageImage.color = _lockedCageColor;
                if (_cageButton != null) _cageButton.interactable = false;

                // Slider dan Teks Klaim disembunyikan
                if (_progressSlider != null) _progressSlider.gameObject.SetActive(false);
                if (_claimText != null) _claimText.gameObject.SetActive(false);

                // Tampilkan gembok
                if (_lockButton != null) _lockButton.gameObject.SetActive(true);
                if (_gemCostText != null) _gemCostText.text = $"{cfg.unlockGemCost} Gem";
                break;

            case PlayTimeRewardController.SlotState.CountingDown:
                // Warna normal, tombol klaim belum aktif
                if (_cageImage != null) _cageImage.color = _unlockedCageColor;
                if (_cageButton != null) _cageButton.interactable = false;

                // TAMPILKAN SLIDER, SEMBUNYIKAN TEKS
                if (_progressSlider != null) _progressSlider.gameObject.SetActive(true);
                if (_claimText != null) _claimText.gameObject.SetActive(false);

                UpdateProgressDisplay();

                // Gembok disembunyikan
                if (_lockButton != null) _lockButton.gameObject.SetActive(false);
                break;

            case PlayTimeRewardController.SlotState.ReadyToClaim:
                // Warna normal & kandang siap diklik
                if (_cageImage != null) _cageImage.color = _unlockedCageColor;
                if (_cageButton != null) _cageButton.interactable = true;

                // SEMBUNYIKAN SLIDER, TAMPILKAN TEKS KLAIM
                if (_progressSlider != null) _progressSlider.gameObject.SetActive(false);
                if (_claimText != null)
                {
                    _claimText.gameObject.SetActive(true);
                    _claimText.text = "CLAIM!";
                }

                // Gembok disembunyikan
                if (_lockButton != null) _lockButton.gameObject.SetActive(false);
                break;
        }
    }

    private void OnCageClicked()
    {
        _controller?.ClaimReward(_slotIndex);
    }

    private void OnLockClicked()
    {
        _controller?.TryUnlockSlot(_slotIndex);
    }
}