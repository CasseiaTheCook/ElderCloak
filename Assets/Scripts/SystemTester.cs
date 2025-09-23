using UnityEngine;
using UnityEngine.InputSystem;
using ElderCloak.Player;
using ElderCloak.Player.Health;
using ElderCloak.Player.Movement;
using ElderCloak.Player.Combat;
using ElderCloak.Enemy;

namespace ElderCloak
{
    /// <summary>
    /// Test script to verify the character controller system is working
    /// Add this to any GameObject in your scene to test the system
    /// </summary>
    public class SystemTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool createTestPlayer = true;
        [SerializeField] private bool createTestEnemy = true;
        [SerializeField] private bool createTestGround = true;
        
        [Header("Test Objects")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;
        
        private void Start()
        {
            if (createTestPlayer && playerPrefab == null)
            {
                CreateTestPlayer();
            }
            
            if (createTestEnemy && enemyPrefab == null)
            {
                CreateTestEnemy();
            }
            
            if (createTestGround)
            {
                CreateTestGround();
            }
        }
        
        private void CreateTestPlayer()
        {
            GameObject player = new GameObject("TestPlayer");
            player.transform.position = new Vector3(0, 2, 0);
            
            // Add required components
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            
            var collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.8f, 1.6f);
            
            var spriteRenderer = player.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.blue;
            
            // Create a simple sprite (white square)
            Texture2D texture = new Texture2D(32, 32);
            for (int x = 0; x < 32; x++)
                for (int y = 0; y < 32; y++)
                    texture.SetPixel(x, y, Color.white);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            spriteRenderer.sprite = sprite;
            
            // Add player systems
            var playerInput = player.AddComponent<PlayerInput>();
            // Note: You need to assign the InputSystem_Actions asset in the inspector
            
            player.AddComponent<PlayerManager>();
            player.AddComponent<HealthSystem>();
            player.AddComponent<PlayerController2D>();
            player.AddComponent<MeleeAttack>();
            
            // Set layer
            player.layer = LayerMask.NameToLayer("Default");
            
            Debug.Log("Test Player created! Don't forget to assign InputSystem_Actions in the PlayerInput component.");
        }
        
        private void CreateTestEnemy()
        {
            GameObject enemy = new GameObject("TestEnemy");
            enemy.transform.position = new Vector3(3, 1, 0);
            
            // Add required components
            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            
            var collider = enemy.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.8f, 0.8f);
            collider.isTrigger = true;
            
            var spriteRenderer = enemy.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.red;
            
            // Create a simple sprite (red square)
            Texture2D texture = new Texture2D(32, 32);
            for (int x = 0; x < 32; x++)
                for (int y = 0; y < 32; y++)
                    texture.SetPixel(x, y, Color.white);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            spriteRenderer.sprite = sprite;
            
            // Add enemy systems
            enemy.AddComponent<BasicEnemy>();
            enemy.AddComponent<EnemyDamager>();
            
            Debug.Log("Test Enemy created!");
        }
        
        private void CreateTestGround()
        {
            GameObject ground = new GameObject("TestGround");
            ground.transform.position = new Vector3(0, -1, 0);
            
            var collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(20, 1);
            
            var spriteRenderer = ground.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.gray;
            
            // Create a simple ground sprite
            Texture2D texture = new Texture2D(640, 32);
            for (int x = 0; x < 640; x++)
                for (int y = 0; y < 32; y++)
                    texture.SetPixel(x, y, Color.white);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 640, 32), new Vector2(0.5f, 0.5f));
            spriteRenderer.sprite = sprite;
            
            Debug.Log("Test Ground created!");
        }
        
        private void Update()
        {
            // Test keys for manual testing
            if (Input.GetKeyDown(KeyCode.T))
            {
                TestHealthSystem();
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartTest();
            }
        }
        
        private void TestHealthSystem()
        {
            GameObject player = GameObject.Find("TestPlayer");
            if (player != null)
            {
                HealthSystem health = player.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(25);
                    Debug.Log($"Player health: {health.CurrentHealth}/{health.MaxHealth}");
                }
            }
        }
        
        private void RestartTest()
        {
            // Destroy existing test objects
            GameObject player = GameObject.Find("TestPlayer");
            GameObject enemy = GameObject.Find("TestEnemy");
            GameObject ground = GameObject.Find("TestGround");
            
            if (player != null) DestroyImmediate(player);
            if (enemy != null) DestroyImmediate(enemy);
            if (ground != null) DestroyImmediate(ground);
            
            // Recreate them
            Start();
        }
    }
}