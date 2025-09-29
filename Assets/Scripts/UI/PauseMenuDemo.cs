using UnityEngine;

/// <summary>
/// Demo script for testing the pause menu system.
/// Add this to a GameObject in your test scene to see the pause menu in action.
/// </summary>
public class PauseMenuDemo : MonoBehaviour
{
    [Header("Demo Settings")]
    [SerializeField] private bool createDemoEnemies = true;
    [SerializeField] private int numberOfEnemies = 5;
    [SerializeField] private bool showInstructions = true;
    
    [Header("Demo Info")]
    [SerializeField] private bool pauseMenuActive = false;
    [SerializeField] private int enemiesFound = 0;
    [SerializeField] private float currentTimeScale = 1f;
    
    void Start()
    {
        // Create pause menu system
        if (PauseMenuManager.Instance == null)
        {
            GameObject pauseManagerObj = new GameObject("PauseMenuManager");
            pauseManagerObj.AddComponent<PauseMenuManager>();
            Debug.Log("Created PauseMenuManager for demo");
        }
        
        if (createDemoEnemies)
        {
            CreateDemoEnemies();
        }
        
        if (showInstructions)
        {
            ShowInstructions();
        }
    }
    
    void Update()
    {
        // Update demo info
        if (PauseMenuManager.Instance != null)
        {
            pauseMenuActive = PauseMenuManager.Instance.IsPaused;
        }
        
        currentTimeScale = Time.timeScale;
        
        // Count enemies for demo purposes
        PatrolScript[] patrolEnemies = FindObjectsOfType<PatrolScript>();
        FlyingEnemyAI[] flyingEnemies = FindObjectsOfType<FlyingEnemyAI>();
        enemiesFound = patrolEnemies.Length + flyingEnemies.Length;
    }
    
    private void CreateDemoEnemies()
    {
        // Create some demo patrol enemies
        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject enemy = new GameObject($"DemoPatrolEnemy_{i}");
            enemy.tag = "Enemy";
            
            // Add basic components for a patrol enemy
            var patrolScript = enemy.AddComponent<PatrolScript>();
            enemy.AddComponent<BoxCollider2D>();
            enemy.AddComponent<Rigidbody2D>();
            
            // Create patrol points
            GameObject pointA = new GameObject($"PointA_{i}");
            GameObject pointB = new GameObject($"PointB_{i}");
            
            pointA.transform.position = new Vector3(i * 3f - 10f, 0, 0);
            pointB.transform.position = new Vector3(i * 3f - 5f, 0, 0);
            
            // Position enemy
            enemy.transform.position = new Vector3(i * 3f - 7.5f, 0, 0);
            
            // Setup patrol points (would normally be done in inspector)
            // patrolScript.pointA = pointA.transform;
            // patrolScript.pointB = pointB.transform;
            
            Debug.Log($"Created demo enemy: {enemy.name}");
        }
    }
    
    private void ShowInstructions()
    {
        Debug.Log("=== PAUSE MENU DEMO INSTRUCTIONS ===");
        Debug.Log("Press ESC to open/close the pause menu");
        Debug.Log("Use mouse or keyboard/gamepad to navigate menu");
        Debug.Log("Watch the Time Scale in the inspector - it should go to 0 when paused");
        Debug.Log("Enemies will stop moving when paused");
        Debug.Log("Try pausing and changing scenes - the menu will persist!");
        Debug.Log("=====================================");
    }
    
    void OnGUI()
    {
        if (!showInstructions) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        var boldStyle = new GUIStyle(GUI.skin.label);
        boldStyle.fontStyle = FontStyle.Bold;
        
        GUILayout.Label("Pause Menu Demo", boldStyle);
        GUILayout.Space(10);
        
        GUILayout.Label($"Pause Status: {(pauseMenuActive ? "PAUSED" : "PLAYING")}");
        GUILayout.Label($"Time Scale: {currentTimeScale:F2}");
        GUILayout.Label($"Enemies Found: {enemiesFound}");
        
        GUILayout.Space(10);
        
        GUILayout.Label("Controls:");
        GUILayout.Label("• ESC - Toggle Pause");
        GUILayout.Label("• Mouse - Click UI");
        GUILayout.Label("• Arrow Keys - Navigate");
        GUILayout.Label("• Enter - Select Button");
        
        if (GUILayout.Button("Toggle Pause (Manual)"))
        {
            if (PauseMenuManager.Instance != null)
            {
                PauseMenuManager.Instance.TogglePause();
            }
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}