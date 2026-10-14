using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartController : MonoBehaviour
{
    [Header("UI Dependencies")]
    [SerializeField] private TextMeshProUGUI _levelName;
    [SerializeField] private TextMeshProUGUI _levelDiff;

    private CanvasGroup _myCanvas;

    private void Awake()
    {
        _myCanvas = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
    }

    private void Start()
    {
        _levelName.text = SceneManager.GetActiveScene().name;
    }

    public void OnStartClicked()
    {
        _myCanvas.alpha = 0f;
        _myCanvas.interactable = false; 
        _myCanvas.blocksRaycasts = false;

        Time.timeScale = 1f;
    }

    public void OnBackToMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
