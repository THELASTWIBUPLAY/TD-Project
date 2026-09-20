using UnityEngine;
using TMPro;

public class CreditsPanelController : MonoBehaviour
{
    private CanvasGroup _myPanel;

    [Header("Scroll Settings")]
    [SerializeField] private RectTransform textRectTransform;
    [SerializeField] private float scrollSpeed = 50f;
    [Tooltip("Reduce if the Text is long")]
    [SerializeField] private float startPositionY = -1080f;
    [Tooltip("Increase if the Text is long")]
    [SerializeField] private float endPositionY = 2000f;

    [Header("Text Content")]
    [SerializeField] private TextMeshProUGUI creditTextComponent;

    private bool isScrolling = false;

    private void Awake()
    {
        _myPanel = GetComponent<CanvasGroup>();
    }

    public void StartScroll()
    {
        if (creditTextComponent != null)
        {
            SetCreditText();
        }

        if (textRectTransform != null)
        {
            textRectTransform.anchoredPosition = new Vector2(textRectTransform.anchoredPosition.x, startPositionY);
        }

        isScrolling = true;
    }

    private void StopScroll()
    {
        isScrolling = false;
    }

    private void Update()
    {
        // Hanya bergerak jika saklar isScrolling bernilai true
        if (!isScrolling || textRectTransform == null) return;

        textRectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

        if (textRectTransform.anchoredPosition.y >= endPositionY)
        {
            StopScroll();
            LoadMainMenu();
        }
    }

    private void SetCreditText()
    {
        creditTextComponent.text =
         "<b>Development Team</b>\n\n" +
         "<b>DIRECTOR</b>\n" +
         "Hendra Febri\n\n" +
         "<b>PRODUCER</b>\n" +
         "Ariiq Wicaksana\n\n" +
         "<b>PROGRAMMER 1</b>\n" +
         "Muhamad Masyhuri\n\n" +
         "<b>PROGRAMMER 2</b>\n" +
         "Rizky Fajar Maulana\n\n" +
         "<b>PROGRAMMER 3</b>\n" +
         "Gerrard Yazdan Arkinara\n\n" +
         "<b>PROGRAMMER 4</b>\n" +
         "Rafan Eka Dinata\n\n" +
         "<b>ARTIST 1</b>\n" +
         "Ramekkah Rona Jannah\n\n" +
         "<b>ARTIST 2</b>\n" +
         "Yolanda\n\n" +
         "<b>ARTIST 3</b>\n" +
         "Naysila Raisya Putri\n\n" +
         "<b>ARTIST 4</b>\n" +
         "Almaira Kusumawardhani";

    }

    private void LoadMainMenu()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_myPanel, false);
            MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, true);
        }
    }
}
