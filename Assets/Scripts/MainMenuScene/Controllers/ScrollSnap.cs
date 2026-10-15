using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollSnap : MonoBehaviour, IEndDragHandler, IDragHandler
{
    [Header("Dependencies")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _contentPanel;
    [SerializeField] private RectTransform[] _viewPages;
    [SerializeField] private Button _nextBtn;     // Tombol ke bawah / halaman berikutnya
    [SerializeField] private Button _previousBtn; // Tombol ke atas / halaman sebelumnya

    [Header("Settings")]
    [SerializeField] private float _snapSpeed = 10f;

    private float[] _pagePositions;
    private int _targetPage = 0;
    private bool _isDragging = false;

    private void Start()
    {
        if (_viewPages == null || _viewPages.Length == 0) return;

        int pageCount = _viewPages.Length;
        _pagePositions = new float[pageCount];

        // Pada vertical scroll: Index 0 (Page 1) ada di posisi atas (1.0)
        // bergerak turun ke halaman terakhir di posisi bawah (0.0)
        for (int i = 0; i < pageCount; i++)
        {
            _pagePositions[i] = (pageCount > 1) ? 1f - ((float)i / (pageCount - 1)) : 1f;
        }

        // Posisikan scroll langsung ke halaman awal saat start
        if (_scrollRect != null)
        {
            _scrollRect.verticalNormalizedPosition = _pagePositions[_targetPage];
        }
    }

    private void Update()
    {
        if (!_isDragging && _scrollRect != null && _pagePositions != null && _pagePositions.Length > 0)
        {
            float currentPos = _scrollRect.verticalNormalizedPosition;
            _scrollRect.verticalNormalizedPosition = Mathf.Lerp(currentPos, _pagePositions[_targetPage], Time.deltaTime * _snapSpeed);
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

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        if (_scrollRect == null || _pagePositions == null || _pagePositions.Length == 0) return;

        float currentPos = _scrollRect.verticalNormalizedPosition;

        // Cari halaman dengan jarak vertikal terdekat
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
        if (_targetPage + 1 < _viewPages.Length)
        {
            _targetPage++;
        }
    }

    public void GoToPreviousPage()
    {
        if (_targetPage - 1 >= 0)
        {
            _targetPage--;
        }
    }
}