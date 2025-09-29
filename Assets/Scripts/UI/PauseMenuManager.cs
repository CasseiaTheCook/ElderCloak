using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private bool pauseOnFocusLoss = true;
    [SerializeField] private bool createUIAutomatically = true;

    // Singleton pattern for cross-scene persistence
    public static PauseMenuManager Instance { get; private set; }

    // State management
    private bool isPaused = false;
    private float timeScaleBeforePause = 1f;
    
    // Enemy AI references for pausing
    private List<PatrolScript> patrolScripts = new List<PatrolScript>();
    private List<FlyingEnemyAI> flyingEnemyAIs = new List<FlyingEnemyAI>();
    
    // General pausable components
    private List<IPausable> pausableComponents = new List<IPausable>();

    // UI Canvas reference
    private Canvas pauseCanvas;

    private void Awake()
    {
        // Implement singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from scene loading events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (createUIAutomatically && pauseMenuPanel == null)
        {
            CreatePauseMenuUI();
        }
        InitializePauseMenu();
        RefreshEnemyReferences();
    }

    private void Update()
    {
        // Handle pause input (ESC key)
        // Only allow pause toggle when no UI element has focus or when the game is paused
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Check if any input field is focused (prevent pausing during text input)
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
            {
                var inputField = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>();
                if (inputField != null)
                {
                    // Don't pause if an input field is active
                    return;
                }
            }
            
            TogglePause();
        }
    }

    private void CreatePauseMenuUI()
    {
        // Create Canvas for pause menu
        GameObject canvasObj = new GameObject("PauseMenuCanvas");
        canvasObj.transform.SetParent(transform);
        
        pauseCanvas = canvasObj.AddComponent<Canvas>();
        pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseCanvas.sortingOrder = 1000; // High sorting order to appear on top
        
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Ensure we have an EventSystem for UI interaction
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.transform.SetParent(transform);
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Create main panel
        GameObject panelObj = new GameObject("PauseMenuPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        
        pauseMenuPanel = panelObj;
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black background
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Create title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "PAUSED";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 48;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(300, 60);
        titleRect.anchoredPosition = Vector2.zero;

        // Create buttons container
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(panelObj.transform);
        
        VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childAlignment = TextAnchor.MiddleCenter;
        
        ContentSizeFitter sizeFitter = buttonContainer.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        RectTransform containerRect = buttonContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(200, 0);
        containerRect.anchoredPosition = Vector2.zero;

        // Create buttons
        resumeButton = CreateButton(buttonContainer, "Resume", ResumeGame);
        settingsButton = CreateButton(buttonContainer, "Settings", OpenSettings);
        quitButton = CreateButton(buttonContainer, "Quit to Menu", QuitToMainMenu);
    }

    private Button CreateButton(GameObject parent, string text, System.Action onClick)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent.transform);
        
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200, 50);
        
        button.onClick.AddListener(() => onClick());
        
        // Setup navigation for gamepad/keyboard support
        Navigation nav = button.navigation;
        nav.mode = Navigation.Mode.Automatic;
        button.navigation = nav;
        
        return button;
    }

    private void InitializePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Setup button listeners (if buttons were manually assigned)
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitToMainMenu);
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;

        // Show pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            
            // Select the first button for gamepad/keyboard navigation
            if (resumeButton != null && UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
            }
        }

        // Pause all enemy AIs
        PauseEnemyAI();

        // Set cursor state
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = timeScaleBeforePause;

        // Hide pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Resume all enemy AIs
        ResumeEnemyAI();

        // Reset cursor state
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Game Resumed");
    }

    private void PauseEnemyAI()
    {
        foreach (var patrolScript in patrolScripts)
        {
            if (patrolScript != null)
                patrolScript.PauseAI();
        }

        foreach (var flyingAI in flyingEnemyAIs)
        {
            if (flyingAI != null)
                flyingAI.PauseAI();
        }
        
        // Pause all registered pausable components
        foreach (var pausable in pausableComponents)
        {
            if (pausable != null)
                pausable.OnPause();
        }
    }

    private void ResumeEnemyAI()
    {
        foreach (var patrolScript in patrolScripts)
        {
            if (patrolScript != null)
                patrolScript.ResumeAI();
        }

        foreach (var flyingAI in flyingEnemyAIs)
        {
            if (flyingAI != null)
                flyingAI.ResumeAI();
        }
        
        // Resume all registered pausable components
        foreach (var pausable in pausableComponents)
        {
            if (pausable != null)
                pausable.OnResume();
        }
    }

    private void RefreshEnemyReferences()
    {
        // Clear existing references
        patrolScripts.Clear();
        flyingEnemyAIs.Clear();
        pausableComponents.Clear();

        // Find all enemy AIs in the current scene
        PatrolScript[] foundPatrolScripts = FindObjectsOfType<PatrolScript>();
        FlyingEnemyAI[] foundFlyingAIs = FindObjectsOfType<FlyingEnemyAI>();
        
        // Find all pausable components
        PausableComponent[] foundPausableComponents = FindObjectsOfType<PausableComponent>();

        patrolScripts.AddRange(foundPatrolScripts);
        flyingEnemyAIs.AddRange(foundFlyingAIs);
        
        // Add pausable components to the list
        foreach (var pausableComponent in foundPausableComponents)
        {
            pausableComponents.Add(pausableComponent);
        }

        Debug.Log($"Found {patrolScripts.Count} PatrolScripts, {flyingEnemyAIs.Count} FlyingEnemyAIs, and {pausableComponents.Count} PausableComponents");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Refresh enemy references when a new scene loads
        RefreshEnemyReferences();

        // Auto-resume game when loading new scene if it was paused
        if (isPaused)
        {
            ResumeGame();
        }
    }

    private void OpenSettings()
    {
        // Placeholder for settings menu
        Debug.Log("Settings menu not implemented yet");
    }

    private void QuitToMainMenu()
    {
        // Resume game before quitting to avoid issues
        if (isPaused)
        {
            ResumeGame();
        }

        // For now, just quit the application since we don't have a main menu scene
        // In a full game, this would load the main menu scene
        Debug.Log("Quit to Main Menu requested");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Handle application focus loss
    private void OnApplicationFocus(bool hasFocus)
    {
        if (pauseOnFocusLoss && !hasFocus && !isPaused)
        {
            PauseGame();
        }
    }

    // Handle application pause (for mobile/console)
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseOnFocusLoss && pauseStatus && !isPaused)
        {
            PauseGame();
        }
    }

    // Public properties for external access
    public bool IsPaused => isPaused;

    // Public methods for registering pausable components
    public void RegisterPausable(IPausable pausable)
    {
        if (pausable != null && !pausableComponents.Contains(pausable))
        {
            pausableComponents.Add(pausable);
            Debug.Log($"Registered pausable component: {pausable}");
        }
    }

    public void UnregisterPausable(IPausable pausable)
    {
        if (pausable != null && pausableComponents.Contains(pausable))
        {
            pausableComponents.Remove(pausable);
            Debug.Log($"Unregistered pausable component: {pausable}");
        }
    }

    // Cleanup on destroy
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}