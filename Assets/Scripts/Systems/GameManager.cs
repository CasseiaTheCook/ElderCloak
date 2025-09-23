using UnityEngine;

namespace ElderCloak.Systems
{
    /// <summary>
    /// Game manager that handles overall game state and coordination between systems.
    /// Provides a central point for game initialization, state management, and system coordination.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Configuration")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool pauseOnApplicationFocus = true;

        [Header("System References")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private CameraController cameraController;

        [Header("Player References")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;

        // Game state
        public bool IsGamePaused { get; private set; }
        public bool IsGameInitialized { get; private set; }

        // Singleton pattern
        public static GameManager Instance { get; private set; }

        // Events
        public System.Action OnGameInitialized;
        public System.Action<bool> OnGamePaused;

        #region Unity Lifecycle

        private void Awake()
        {
            // Implement singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeGame();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (pauseOnApplicationFocus)
            {
                PauseGame(!hasFocus);
            }
        }

        #endregion

        #region Game Initialization

        /// <summary>
        /// Initialize the game systems and spawn the player.
        /// </summary>
        public void InitializeGame()
        {
            if (IsGameInitialized) return;

            // Initialize input manager if not found
            if (inputManager == null)
                inputManager = FindObjectOfType<InputManager>();

            if (inputManager == null)
            {
                GameObject inputManagerGO = new GameObject("InputManager");
                inputManager = inputManagerGO.AddComponent<InputManager>();
            }

            // Initialize camera controller if not found
            if (cameraController == null)
                cameraController = FindObjectOfType<CameraController>();

            // Spawn player if needed
            SpawnPlayer();

            IsGameInitialized = true;
            OnGameInitialized?.Invoke();

            Debug.Log("Game initialized successfully!");
        }

        /// <summary>
        /// Spawn the player character.
        /// </summary>
        private void SpawnPlayer()
        {
            if (playerPrefab == null) return;

            Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            
            // Register with input manager
            if (inputManager != null)
            {
                var playerController = playerInstance.GetComponent<ElderCloak.Player.PlayerController>();
                var combatController = playerInstance.GetComponent<ElderCloak.Player.PlayerCombatController>();
                
                if (playerController != null)
                    inputManager.RegisterPlayerController(playerController);
                
                if (combatController != null)
                    inputManager.RegisterCombatController(combatController);
            }

            // Set camera target if camera controller exists
            if (cameraController != null)
            {
                cameraController.SetTarget(playerInstance.transform);
            }
        }

        #endregion

        #region Game State Management

        /// <summary>
        /// Pause or unpause the game.
        /// </summary>
        /// <param name="pause">True to pause, false to unpause</param>
        public void PauseGame(bool pause)
        {
            if (IsGamePaused == pause) return;

            IsGamePaused = pause;
            Time.timeScale = pause ? 0f : 1f;

            // Disable/enable input
            if (inputManager != null)
            {
                if (pause)
                    inputManager.DisableInput();
                else
                    inputManager.EnableInput();
            }

            OnGamePaused?.Invoke(pause);
        }

        /// <summary>
        /// Toggle game pause state.
        /// </summary>
        public void TogglePause()
        {
            PauseGame(!IsGamePaused);
        }

        /// <summary>
        /// Restart the current level/game.
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get the input manager instance.
        /// </summary>
        public InputManager GetInputManager() => inputManager;

        /// <summary>
        /// Get the camera controller instance.
        /// </summary>
        public CameraController GetCameraController() => cameraController;

        #endregion
    }
}