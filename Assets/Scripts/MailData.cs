using System;

[Serializable]
public class MailCatalog
{
    public MailMessage[] messages;
}

[Serializable]
public class MailMessage
{
    public string id;
    public string title;
    public string body;
    public MailReward[] rewards;
}

[Serializable]
public class MailReward
{
    public string type;
    public int amount;
}