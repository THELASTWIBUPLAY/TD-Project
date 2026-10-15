using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckDraggableItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler, IInitializePotentialDragHandler
{
    public enum ItemType { Hero, Relic }

    [Header("Item Context")]
    public ItemType type;
    public string itemId;
    public bool isOwned;
    public bool isEquipped;

    [HideInInspector] public bool isFromEquippedSlot = false;
    [HideInInspector] public int sourceEquippedIndex = -1;

    [Header("UI Component")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private CanvasGroup _canvasGroup;

    // Referensi Navigasi & Drag
    private Transform _originalParent;
    private Canvas _rootCanvas;
    private ScrollRect _parentScrollRect;
    private Vector3 _originalPosition;

    // Status State Gestur
    private bool _isDraggingItem = false;
    private bool _isScrollingParent = false;
    private bool _wasDroppedSuccessfully = false;

    private void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>();
        // Cari ScrollRect terdekat di hierarki atas (ScrollView_Owned atau ScrollView_Locked)
        _parentScrollRect = GetComponentInParent<ScrollRect>();

        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(Sprite icon, string id, ItemType itemType, bool owned, bool equipped, bool fromEquipped = false, int slotIdx = -1)
    {
        itemId = id;
        type = itemType;
        isOwned = owned;
        isEquipped = equipped;
        isFromEquippedSlot = fromEquipped;
        sourceEquippedIndex = slotIdx;
        _wasDroppedSuccessfully = false;

        if (_iconImage != null)
        {
            _iconImage.sprite = icon;
            _iconImage.color = isOwned ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.6f);
        }
    }

    public void NotifyDropSuccess()
    {
        _wasDroppedSuccessfully = true;
    }

    // =========================================================================
    // INITIALIZE POTENTIAL DRAG
    // =========================================================================
    // Wajib ada agar ScrollRect siap menerima delegasi gesekan saat jari baru menempel
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (_parentScrollRect != null)
        {
            _parentScrollRect.OnInitializePotentialDrag(eventData);
        }
    }

    // =========================================================================
    // DRAG INTERACTION WITH GESTURE DETECTION
    // =========================================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        _wasDroppedSuccessfully = false;
        _isDraggingItem = false;
        _isScrollingParent = false;

        // 1. KASUS KARTU TERKUNCI (LOCKED):
        // Tidak bisa di-equip, alihkan 100% gestur ke ScrollRect agar tidak membeku
        if (!isOwned && !isFromEquippedSlot)
        {
            RouteToScrollRectBegin(eventData);
            return;
        }

        // 2. KASUS KARTU DIMILIKI (OWNED):
        // Deteksi arah tarikan awal dari posisi tekan pertama
        Vector2 dragVector = eventData.position - eventData.pressPosition;

        // Jika tarikan lebih dominan mendatar (Horizontal), arahkan ke ScrollRect
        if (Mathf.Abs(dragVector.x) > Mathf.Abs(dragVector.y))
        {
            RouteToScrollRectBegin(eventData);
        }
        else
        {
            // Jika tarikan lebih dominan tegak (Vertikal ke atas), angkat kartu untuk equip
            _isDraggingItem = true;
            _originalPosition = transform.position;
            _originalParent = transform.parent;

            transform.SetParent(_rootCanvas.transform, true);
            transform.SetAsLastSibling();

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0.75f;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isScrollingParent && _parentScrollRect != null)
        {
            // Teruskan geseran ke Scroll View
            _parentScrollRect.OnDrag(eventData);
        }
        else if (_isDraggingItem)
        {
            // Gerakkan kartu mengikuti ujung jari
            transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isScrollingParent && _parentScrollRect != null)
        {
            _parentScrollRect.OnEndDrag(eventData);
            _isScrollingParent = false;
            return;
        }

        if (!_isDraggingItem) return;

        _isDraggingItem = false;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;

        if (_wasDroppedSuccessfully)
        {
            Destroy(gameObject);
            return;
        }

        // Kembalikan ke posisi semula di dalam Scroll View jika batal drop
        transform.SetParent(_originalParent, true);
        transform.position = _originalPosition;
    }

    private void RouteToScrollRectBegin(PointerEventData eventData)
    {
        if (_parentScrollRect != null)
        {
            _isScrollingParent = true;
            _parentScrollRect.OnBeginDrag(eventData);
        }
    }

    // =========================================================================
    // TAP / CLICK INTERACTION
    // =========================================================================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging || _isScrollingParent || _isDraggingItem) return;

        if (string.IsNullOrEmpty(itemId)) return;

        if (type == ItemType.Hero)
        {
            DeckPanelUI.Instance?.OpenHeroUpgradeModal(itemId);
        }
        else if (type == ItemType.Relic)
        {
            // Panggil modal detail relik saat kartu di inventory diklik
            DeckPanelUI.Instance?.OpenRelicDetailModal(itemId);
        }
    }
}