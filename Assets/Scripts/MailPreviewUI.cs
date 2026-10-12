using UnityEngine;

public class MailPreviewUI : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text titleText;
    [SerializeField] private TMPro.TMP_Text bodyText;

    private void Awake()
    {
        if (titleText != null)
            titleText.text = "Loading mail...";

        if (bodyText != null)
            bodyText.text = "";
    }

    public void ShowMail(MailMessage mail)
    {
        if (titleText == null || bodyText == null)
        {
            Debug.LogError("[MailPreview] Assign both text references.");
            return;
        }

        if (mail == null)
        {
            titleText.text = "No mail";
            bodyText.text = "Your inbox is empty.";
            return;
        }

        titleText.text = mail.title;
        bodyText.text = mail.body;
    }
}