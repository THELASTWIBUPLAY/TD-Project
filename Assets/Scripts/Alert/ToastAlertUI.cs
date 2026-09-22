using UnityEngine;
using TMPro;

public class ToastAlertUI : MonoBehaviour, IAlert
{
    [SerializeField] private TextMeshProUGUI _messageText;

    public string SourceName => gameObject.name;

    public void Show(string message)
    {
        gameObject.SetActive(true);
        _messageText.text = message;
    }
}