using System.IO;
using UnityEngine;

[System.Serializable]
[HideInInspector]public class SaveData
{
    public bool canDash = false;
    public bool canDoubleJump = false;
    public bool canRun = true;
    // Gerekirse ileride yeni �zellikler ekleyebilirsin.
}

public class SaveDataManager : MonoBehaviour
{
    [HideInInspector] public SaveData currentSave;
    private string saveFilePath;

    void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "SaveData.json");
        LoadSave();
    }

    public void LoadSave()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentSave = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            currentSave = new SaveData();
            SaveGame(); // Varsay�lan dosyay� yaz
        }
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(currentSave, true); // Pretty print i�in true
        File.WriteAllText(saveFilePath, json);
    }
}