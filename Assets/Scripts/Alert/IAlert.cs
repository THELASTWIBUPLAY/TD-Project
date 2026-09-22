using UnityEngine;

public interface IAlert
{
    string SourceName { get; }
    void Show(string messages);
}