using TMPro;
using UnityEngine;

public class CreditsPanelController : MonoBehaviour
{
    [Header("UI Dependencies")]
    [SerializeField] private TextMeshProUGUI _creditsText;

    private string _credits;

    private void Start()
    {
        _credits = "Lead, Programmer, Artist";

        Debug.Log(_credits);
    }
}
