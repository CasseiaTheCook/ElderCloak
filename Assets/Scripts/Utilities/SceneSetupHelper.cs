using UnityEngine;
using ElderCloak.Systems;
using ElderCloak.Player;
using ElderCloak.Enemy;

namespace ElderCloak.Utilities
{
    /// <summary>
    /// Utility script to help set up the ElderCloak character controller system in a scene.
    /// This script can be used to quickly create a basic setup for testing the systems.
    /// </summary>
    public class SceneSetupHelper : MonoBehaviour
    {
        [Header("Setup Configuration")]
        [SerializeField] private bool setupOnStart = false;
        [SerializeField] private bool createFloor = true;
        [SerializeField] private bool createPlayer = true;
        [SerializeField] private bool createEnemy = true;
        [SerializeField] private bool setupCamera = true;
        [SerializeField] private bool setupInputManager = true;

        [Header("Player Setup")]
        [SerializeField] private Vector3 playerSpawnPosition = Vector3.zero;
        [SerializeField] private float playerHealth = 100f;

        [Header("Enemy Setup")]
        [SerializeField] private Vector3 enemySpawnPosition = new Vector3(5, 0, 0);
        [SerializeField] private float enemyHealth = 50f;
        [SerializeField] private BasicEnemyController.EnemyBehaviorType enemyBehavior = BasicEnemyController.EnemyBehaviorType.Patrol;

        [Header("Environment Setup")]
        [SerializeField] private Vector3 floorPosition = new Vector3(0, -2, 0);
        [SerializeField] private Vector2 floorSize = new Vector2(20, 1);

        private void Start()
        {
            if (setupOnStart)
            {
                SetupScene();
            }
        }

        /// <summary>
        /// Set up the entire scene with all systems.
        /// </summary>
        [ContextMenu("Setup Scene")]
        public void SetupScene()
        {
            Debug.Log("Setting up ElderCloak scene...");

            if (createFloor) CreateFloor();
            if (setupInputManager) SetupInputManager();
            if (setupCamera) SetupCamera();
            if (createPlayer) CreatePlayer();
            if (createEnemy) CreateEnemy();

            Debug.Log("Scene setup complete!");
        }

        /// <summary>
        /// Create a basic floor for the player to stand on.
        /// </summary>
        private void CreateFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.position = floorPosition;
            floor.transform.localScale = new Vector3(floorSize.x, floorSize.y, 1);
            
            // Set up layers
            floor.layer = LayerMask.NameToLayer("Ground") != -1 ? LayerMask.NameToLayer("Ground") : 0;

            Debug.Log("Floor created");
        }

        /// <summary>
        /// Create and configure the input manager.
        /// </summary>
        private void SetupInputManager()
        {
            if (FindObjectOfType<InputManager>() != null)
            {
                Debug.Log("InputManager already exists in scene");
                return;
            }

            GameObject inputManagerGO = new GameObject("InputManager");
            inputManagerGO.AddComponent<InputManager>();

            Debug.Log("InputManager created");
        }

        /// <summary>
        /// Set up the camera with CameraController.
        /// </summary>
        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("No main camera found in scene");
                return;
            }

            CameraController cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController == null)
            {
                cameraController = mainCamera.AddComponent<CameraController>();
            }

            Debug.Log("Camera setup complete");
        }

        /// <summary>
        /// Create a player with all necessary components.
        /// </summary>
        private void CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.transform.position = playerSpawnPosition;

            // Add physics components
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.8f, 1.8f);

            // Add controller components
            PlayerController playerController = player.AddComponent<PlayerController>();
            PlayerCombatController combatController = player.AddComponent<PlayerCombatController>();

            // Set up health
            var healthSystem = playerController.GetHealthSystem();
            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(playerHealth);
            }

            // Create ground check
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(player.transform);
            groundCheck.transform.localPosition = new Vector3(0, -collider.size.y * 0.5f, 0);

            // Create wall check
            GameObject wallCheck = new GameObject("WallCheck");
            wallCheck.transform.SetParent(player.transform);
            wallCheck.transform.localPosition = new Vector3(collider.size.x * 0.5f, 0, 0);

            // Set up layer
            player.layer = LayerMask.NameToLayer("Player") != -1 ? LayerMask.NameToLayer("Player") : 0;

            // Set camera target
            CameraController cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null)
            {
                cameraController.SetTarget(player.transform);
            }

            Debug.Log("Player created");
        }

        /// <summary>
        /// Create an enemy for testing combat.
        /// </summary>
        private void CreateEnemy()
        {
            GameObject enemy = new GameObject("Enemy");
            enemy.transform.position = enemySpawnPosition;

            // Add physics components
            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;

            BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.8f, 1.5f);

            // Add enemy controller
            BasicEnemyController enemyController = enemy.AddComponent<BasicEnemyController>();

            // Configure enemy behavior
            enemyController.SetBehaviorType(enemyBehavior);

            // Set up health
            var healthSystem = enemyController.GetHealthSystem();
            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(enemyHealth);
            }

            // Set up layer
            enemy.layer = LayerMask.NameToLayer("Enemy") != -1 ? LayerMask.NameToLayer("Enemy") : 0;

            Debug.Log("Enemy created");
        }

        /// <summary>
        /// Create just the player (useful for individual testing).
        /// </summary>
        [ContextMenu("Create Player Only")]
        public void CreatePlayerOnly()
        {
            CreatePlayer();
        }

        /// <summary>
        /// Create just the enemy (useful for individual testing).
        /// </summary>
        [ContextMenu("Create Enemy Only")]
        public void CreateEnemyOnly()
        {
            CreateEnemy();
        }

        /// <summary>
        /// Clean up the scene (remove created objects).
        /// </summary>
        [ContextMenu("Clean Up Scene")]
        public void CleanUpScene()
        {
            // Find and destroy created objects
            GameObject player = GameObject.Find("Player");
            if (player != null) DestroyImmediate(player);

            GameObject enemy = GameObject.Find("Enemy");
            if (enemy != null) DestroyImmediate(enemy);

            GameObject floor = GameObject.Find("Floor");
            if (floor != null) DestroyImmediate(floor);

            GameObject inputManager = GameObject.Find("InputManager");
            if (inputManager != null) DestroyImmediate(inputManager);

            Debug.Log("Scene cleaned up");
        }
    }
}