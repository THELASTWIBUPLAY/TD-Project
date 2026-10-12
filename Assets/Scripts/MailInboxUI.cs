using UnityEngine;

public class MailInboxUI : MonoBehaviour
{
    [SerializeField] private GameObject listPanel;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject inboxRoot;

    [SerializeField] private Transform listContent;
    [SerializeField] private MailListItemUI itemPrefab;
    [SerializeField] private MailPreviewUI mailPreview;

    private void Awake()
    {
        CloseInbox();
    }

    public void ShowInbox(MailCatalog catalog)
    {
        if (listPanel == null ||
            detailPanel == null ||
            listContent == null ||
            itemPrefab == null ||
            mailPreview == null)
        {
            Debug.LogError("[MailInbox] Assign all Inspector references.");
            return;
        }

        foreach (Transform child in listContent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        if (catalog?.messages != null)
        {
            foreach (MailMessage mail in catalog.messages)
            {
                if (mail == null)
                    continue;

                MailListItemUI item = Instantiate(itemPrefab, listContent);
                item.Setup(mail, OpenMail);
            }
        }

        CloseMail();
    }

    private void OpenMail(MailMessage mail)
    {
        if (mail == null)
            return;

        detailPanel.SetActive(true);
        mailPreview.ShowMail(mail);
    }

    public void CloseMail()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);

        if (listPanel != null)
            listPanel.SetActive(true);
    }

    public void OpenInbox()
    {
        CloseMail();

        inboxRoot.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void CloseInbox()
    {
        CloseMail();

        inboxRoot.SetActive(false);
        menuPanel.SetActive(true);
    }
}