using System;
using TMPro;
using UnityEngine;

public class MailListItemUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button button;
    [SerializeField] private TMP_Text titleText;

    public void Setup(MailMessage mail, Action<MailMessage> onSelected)
    {
        titleText.text = mail.title;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelected?.Invoke(mail));
    }
}