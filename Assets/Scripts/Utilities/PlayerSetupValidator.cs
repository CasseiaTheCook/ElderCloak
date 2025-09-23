using UnityEngine;
using ElderCloak.Player.Controller;
using ElderCloak.Player.Input;
using ElderCloak.Player.Health;

namespace ElderCloak.Utilities
{
    /// <summary>
    /// Utility script to help validate and set up the player character correctly
    /// </summary>
    public class PlayerSetupValidator : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private LayerMask groundLayerMask = 1 << 8; // Default to layer 8
        [SerializeField] private string enemyTag = "Enemy";
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                ValidateAndSetupPlayer();
            }
        }
        
        [ContextMenu("Validate Player Setup")]
        public void ValidateAndSetupPlayer()
        {
            Debug.Log("=== Player Setup Validation Started ===");
            
            ValidateComponents();
            ValidateInputActions();
            ValidatePhysicsSettings();
            ValidateTags();
            
            Debug.Log("=== Player Setup Validation Complete ===");
        }
        
        private void ValidateComponents()
        {
            Debug.Log("Checking required components...");
            
            // Check Rigidbody2D
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                Debug.Log("✓ Added Rigidbody2D component");
            }
            
            // Configure Rigidbody2D
            rb.freezeRotation = true;
            rb.gravityScale = 3f;
            
            // Check Collider2D
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<CapsuleCollider2D>();
                Debug.Log("✓ Added CapsuleCollider2D component");
            }
            
            // Check PlayerController
            PlayerController controller = GetComponent<PlayerController>();
            if (controller == null)
            {
                gameObject.AddComponent<PlayerController>();
                Debug.Log("✓ Added PlayerController component");
            }
            
            // Check PlayerInputHandler
            PlayerInputHandler inputHandler = GetComponent<PlayerInputHandler>();
            if (inputHandler == null)
            {
                gameObject.AddComponent<PlayerInputHandler>();
                Debug.Log("✓ Added PlayerInputHandler component");
            }
            
            // Check HealthSystem
            HealthSystem health = GetComponent<HealthSystem>();
            if (health == null)
            {
                gameObject.AddComponent<HealthSystem>();
                Debug.Log("✓ Added HealthSystem component");
            }
            
            Debug.Log("Component validation complete!");
        }
        
        private void ValidateInputActions()
        {
            Debug.Log("Checking Input Actions...");
            
            // Try to find InputActions asset
            var inputActions = Resources.LoadAll<UnityEngine.InputSystem.InputActionAsset>("");
            
            if (inputActions.Length == 0)
            {
                Debug.LogWarning("⚠ No InputActionAsset found! Make sure InputSystem_Actions.inputactions exists");
            }
            else
            {
                Debug.Log($"✓ Found {inputActions.Length} InputActionAsset(s)");
                
                var playerInputHandler = GetComponent<PlayerInputHandler>();
                if (playerInputHandler != null)
                {
                    // You would set the input actions here via reflection or public field
                    Debug.Log("✓ PlayerInputHandler ready for InputActions assignment");
                }
            }
        }
        
        private void ValidatePhysicsSettings()
        {
            Debug.Log("Checking Physics Settings...");
            
            // Check if Ground layer exists
            string groundLayerName = LayerMask.LayerToName(8);
            if (string.IsNullOrEmpty(groundLayerName))
            {
                Debug.LogWarning("⚠ Ground layer (8) not found! Please create a 'Ground' layer at index 8");
            }
            else
            {
                Debug.Log($"✓ Ground layer found: {groundLayerName}");
            }
            
            // Check if Enemy layer exists  
            string enemyLayerName = LayerMask.LayerToName(6);
            if (string.IsNullOrEmpty(enemyLayerName))
            {
                Debug.LogWarning("⚠ Enemy layer (6) not found! Please create an 'Enemy' layer at index 6");
            }
            else
            {
                Debug.Log($"✓ Enemy layer found: {enemyLayerName}");
            }
        }
        
        private void ValidateTags()
        {
            Debug.Log("Checking Tags...");
            
            try
            {
                GameObject.FindWithTag("Player");
                Debug.Log("✓ Player tag exists");
            }
            catch
            {
                Debug.LogWarning("⚠ Player tag not found! Please create a 'Player' tag");
            }
            
            try
            {
                GameObject.FindWithTag(enemyTag);
                Debug.Log($"✓ {enemyTag} tag exists");
            }
            catch
            {
                Debug.LogWarning($"⚠ {enemyTag} tag not found! Please create an '{enemyTag}' tag");
            }
        }
        
        [ContextMenu("Create Test Enemy")]
        public void CreateTestEnemy()
        {
            // Create a simple test enemy
            GameObject enemy = new GameObject("TestEnemy");
            enemy.transform.position = transform.position + Vector3.right * 3f;
            
            // Add components
            enemy.AddComponent<BoxCollider2D>().isTrigger = true;
            enemy.AddComponent<Rigidbody2D>().isKinematic = true;
            enemy.AddComponent<ElderCloak.Enemies.EnemyDamager>();
            
            // Set tag and layer
            enemy.tag = enemyTag;
            enemy.layer = 6; // Enemy layer
            
            // Visual representation
            var renderer = enemy.AddComponent<SpriteRenderer>();
            renderer.color = Color.red;
            
            // Create a simple sprite (1x1 white texture)
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            renderer.sprite = sprite;
            
            Debug.Log("✓ Test enemy created!");
        }
        
        [ContextMenu("Setup Example Scene")]
        public void SetupExampleScene()
        {
            Debug.Log("Setting up example scene...");
            
            // Create ground platform
            GameObject ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0, -2, 0);
            ground.transform.localScale = new Vector3(10, 1, 1);
            
            var groundRenderer = ground.AddComponent<SpriteRenderer>();
            groundRenderer.color = Color.green;
            
            // Create ground sprite
            Texture2D groundTexture = new Texture2D(1, 1);
            groundTexture.SetPixel(0, 0, Color.white);
            groundTexture.Apply();
            
            Sprite groundSprite = Sprite.Create(groundTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            groundRenderer.sprite = groundSprite;
            
            // Add collider and set layer
            ground.AddComponent<BoxCollider2D>();
            ground.layer = 8; // Ground layer
            
            // Create test enemy
            CreateTestEnemy();
            
            Debug.Log("✓ Example scene setup complete!");
        }
    }
}