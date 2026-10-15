using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckDropSlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Slot Configuration")]
    [SerializeField] private DeckDraggableItem.ItemType _allowedType;
    [SerializeField] private int _slotIndex;

    public DeckDraggableItem.ItemType AllowedType => _allowedType;
    public int SlotIndex => _slotIndex;

    [Header("Visual Components")]
    [SerializeField] private Image _slotIcon;
    [SerializeField] private Sprite _emptyFrameSprite;
    [SerializeField] private CanvasGroup _canvasGroup;

    private string _currentEquippedId = string.Empty;
    public string CurrentEquippedId => _currentEquippedId;
    public bool IsOccupied => !string.IsNullOrEmpty(_currentEquippedId);

    private Canvas _rootCanvas;
    private bool _isDragging = false;

    // Objek visual tiruan yang melayang mengikuti kursor
    private GameObject _dragProxy;

    private void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Configure(DeckDraggableItem.ItemType allowedType, int index)
    {
        _allowedType = allowedType;
        _slotIndex = index;
    }

    public void SetupSlot(Sprite icon, string equippedId)
    {
        _currentEquippedId = equippedId;

        if (string.IsNullOrEmpty(equippedId))
        {
            _slotIcon.sprite = _emptyFrameSprite;
            _slotIcon.color = new Color(1f, 1f, 1f, 0.25f);
        }
        else
        {
            _slotIcon.sprite = icon;
            _slotIcon.color = Color.white;
        }
    }

    // =========================================================================
    // DROP HANDLER (Tetap sama seperti sebelumnya)
    // =========================================================================

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        // 1. Kasus dari Inventory
        var draggedItem = eventData.pointerDrag.GetComponent<DeckDraggableItem>();
        if (draggedItem != null)
        {
            if (draggedItem.type != _allowedType)
            {
                AlertManager.Instance?.Show($"Slot ini hanya untuk {_allowedType}!");
                return;
            }

            draggedItem.NotifyDropSuccess();

            DeckPanelUI.Instance?.HandleItemDroppedOnSlot(
                _allowedType,
                _slotIndex,
                draggedItem.itemId,
                draggedItem.isFromEquippedSlot,
                draggedItem.sourceEquippedIndex
            );
            return;
        }

        // 2. Kasus Swap antar Slot Equipped
        var draggedSlot = eventData.pointerDrag.GetComponent<DeckDropSlot>();
        if (draggedSlot != null && draggedSlot != this)
        {
            if (draggedSlot._allowedType != _allowedType) return;

            DeckPanelUI.Instance?.HandleSlotToSlotSwap(
                _allowedType,
                draggedSlot._slotIndex,
                _slotIndex
            );
        }
    }

    // =========================================================================
    // DRAG HANDLER (DIPERBARUI: Menggunakan Drag Proxy)
    // =========================================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsOccupied) return;

        _isDragging = true;

        // 1. Buat visual tiruan melayang di Root Canvas
        CreateDragProxy();

        // 2. Redupkan icon pada slot asli sebagai feedback bahwa item sedang diangkat
        if (_slotIcon != null)
        {
            Color c = _slotIcon.color;
            c.a = 0.35f;
            _slotIcon.color = c;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || _dragProxy == null) return;

        // Gerakkan hanya objek tiruan mengikuti sentuhan jari
        _dragProxy.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        _isDragging = false;

        // 1. Hancurkan visual tiruan setelah selesai
        if (_dragProxy != null)
        {
            Destroy(_dragProxy);
            _dragProxy = null;
        }

        // 2. Kembalikan alpha sesuai status slot saat ini
        if (_slotIcon != null)
        {
            Color c = _slotIcon.color;
            // Jika slot masih terisi gunakan 1f, jika sudah kosong (akibat dipindah) kembalikan ke 0.25f
            c.a = IsOccupied ? 1f : 0.25f;
            _slotIcon.color = c;
        }
    }

    private void CreateDragProxy()
    {
        if (_rootCanvas == null || _slotIcon == null) return;

        // Buat GameObject bayangan sementara secara dinamis
        _dragProxy = new GameObject("Slot_DragProxy", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        _dragProxy.transform.SetParent(_rootCanvas.transform, false);
        _dragProxy.transform.SetAsLastSibling(); // Tampilkan di lapisan paling atas layar

        // Samakan ukuran fisik dengan icon aslinya
        RectTransform proxyRect = _dragProxy.GetComponent<RectTransform>();
        RectTransform sourceRect = _slotIcon.rectTransform;
        proxyRect.sizeDelta = new Vector2(sourceRect.rect.width, sourceRect.rect.height);
        proxyRect.position = sourceRect.position;

        // Tampilkan gambar hero yang sama
        Image img = _dragProxy.GetComponent<Image>();
        img.sprite = _slotIcon.sprite;
        img.raycastTarget = false; // Hindari pemblokiran raycast

        // Berikan sedikit transparansi
        CanvasGroup cg = _dragProxy.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.85f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging || !IsOccupied) return;

        if (_allowedType == DeckDraggableItem.ItemType.Hero)
        {
            DeckPanelUI.Instance?.OpenHeroUpgradeModal(_currentEquippedId);
        }
        else if (_allowedType == DeckDraggableItem.ItemType.Relic)
        {
            // Panggil modal detail relik saat slot equipped diklik
            DeckPanelUI.Instance?.OpenRelicDetailModal(_currentEquippedId);
        }
    }
}