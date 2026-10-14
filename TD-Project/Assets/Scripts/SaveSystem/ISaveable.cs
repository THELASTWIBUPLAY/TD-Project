using UnityEngine;

public interface ISaveable
{
    void PopulateSaveData(GameSaveData saveData);
    void LoadFromSaveData(GameSaveData saveData);
}