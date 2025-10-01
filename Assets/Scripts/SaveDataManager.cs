using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public bool canDash = false;
    public bool canDoubleJump = false;
    public bool canRun = true;
    // Add more fields as needed.
}

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance { get; private set; }

    [HideInInspector] public SaveData currentSave;
    private string saveFilePath;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            saveFilePath = Path.Combine(Application.persistentDataPath, "SaveData.json");
            LoadSave();
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
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
            SaveGame();
        }
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(currentSave, true);
        File.WriteAllText(saveFilePath, json);
    }
}