using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollSnap : MonoBehaviour, IEndDragHandler, IDragHandler
{
    [Header("Dependencies")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _contentPanel;
    [SerializeField] private RectTransform[] _viewPages;
    [SerializeField] private Button _nextBtn;
    [SerializeField] private Button _previousBtn;

    [Header("Settings")]
    [SerializeField] private float _snapSpeed = 10f;

    private float[] _pagePositions;
    private int _targetPage;
    private bool _isDragging;

    private void Start()
    {
        int pageCount = _viewPages.Length;
        if (pageCount == 0) return;

        _pagePositions = new float[pageCount];

        // Menghitung titik koordinat normalisasi (0 sampai 1) untuk setiap stage
        for (int i = 0; i < pageCount; i++)
        {
            _pagePositions[i] = (float)i / (pageCount - 1);
        }
    }

    private void Update()
    {
        if (!_isDragging)
        {
            float currentPos = _scrollRect.horizontalNormalizedPosition;
            _scrollRect.horizontalNormalizedPosition = Mathf.Lerp(currentPos, _pagePositions[_targetPage], Time.deltaTime * _snapSpeed);
        }

        if (_viewPages != null && _viewPages.Length > 0)
        {
            if (_previousBtn != null) _previousBtn.interactable = (_targetPage > 0);
            if (_nextBtn != null) _nextBtn.interactable = (_targetPage < _viewPages.Length - 1);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        _isDragging = true;
    }

    // Mendeteksi ketika pemain melepas jari/klik mouse
    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        float currentPos = _scrollRect.horizontalNormalizedPosition;

        // Cari stage mana yang posisinya paling dekat dengan posisi scroll saat ini
        float closestDistance = float.MaxValue;
        for (int i = 0; i < _pagePositions.Length; i++)
        {
            float distance = Mathf.Abs(currentPos - _pagePositions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                _targetPage = i;
            }
        }
    }

    public void GoToNextPage()
    {
        // Cek apakah halaman berikutnya masih ada di dalam batas array
        if (_targetPage + 1 < _viewPages.Length)
        {
            _targetPage++;
        }
    }

    public void GoToPreviousPage()
    {
        // Cek agar tidak minus dari halaman pertama (indeks 0)
        if (_targetPage - 1 >= 0)
        {
            _targetPage--;
        }
    }
}
