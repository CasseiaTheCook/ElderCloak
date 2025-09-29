using UnityEngine;

/// <summary>
/// Simple script to automatically add PauseMenuManager to a scene.
/// Add this to any GameObject in your scene to enable the pause menu system.
/// This script will create and configure the PauseMenuManager automatically.
/// </summary>
public class PauseMenuInitializer : MonoBehaviour
{
    [Header("Auto-Setup")]
    [SerializeField] private bool autoCreatePauseManager = true;
    [SerializeField] private bool pauseOnApplicationFocusLoss = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;
    
    void Start()
    {
        if (autoCreatePauseManager && PauseMenuManager.Instance == null)
        {
            CreatePauseManager();
        }
        else if (showDebugMessages && PauseMenuManager.Instance != null)
        {
            Debug.Log("PauseMenuManager already exists in the scene.");
        }
    }
    
    private void CreatePauseManager()
    {
        GameObject pauseManagerObj = new GameObject("PauseMenuManager");
        PauseMenuManager pauseManager = pauseManagerObj.AddComponent<PauseMenuManager>();
        
        // The PauseMenuManager will handle DontDestroyOnLoad automatically
        
        if (showDebugMessages)
        {
            Debug.Log("PauseMenuManager created and initialized automatically.");
        }
    }
    
    void Update()
    {
        // For demonstration - show pause status in debug
        if (showDebugMessages && PauseMenuManager.Instance != null)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log($"Pause Status: {(PauseMenuManager.Instance.IsPaused ? "PAUSED" : "PLAYING")}");
                Debug.Log($"Time Scale: {Time.timeScale}");
            }
        }
    }
}