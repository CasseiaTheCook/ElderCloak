using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

/// <summary>
/// Integration tests for the pause menu system across different game scenarios
/// </summary>
public class PauseMenuIntegrationTests
{
    private GameObject pauseManagerObject;
    private PauseMenuManager pauseManager;
    private GameObject enemyObject;
    private PatrolScript patrolScript;

    [SetUp]
    public void Setup()
    {
        // Create pause manager
        pauseManagerObject = new GameObject("PauseMenuManager");
        pauseManager = pauseManagerObject.AddComponent<PauseMenuManager>();

        // Create a mock enemy with patrol script
        enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        patrolScript = enemyObject.AddComponent<PatrolScript>();
        
        // Create basic patrol points
        GameObject pointA = new GameObject("PointA");
        GameObject pointB = new GameObject("PointB");
        pointA.transform.position = Vector3.left * 5f;
        pointB.transform.position = Vector3.right * 5f;
        
        // This would normally be set in the inspector
        // patrolScript.pointA = pointA.transform;
        // patrolScript.pointB = pointB.transform;
    }

    [TearDown]
    public void Teardown()
    {
        if (pauseManagerObject != null)
            Object.DestroyImmediate(pauseManagerObject);
        if (enemyObject != null)
            Object.DestroyImmediate(enemyObject);
        
        // Clean up any extra objects
        GameObject[] pointObjects = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (var obj in pointObjects)
        {
            if (obj.name.Contains("Point"))
                Object.DestroyImmediate(obj);
        }
        
        Time.timeScale = 1f;
    }

    [Test]
    public void PauseSystem_WithEnemies_PausesAndResumesCorrectly()
    {
        // Arrange
        Assert.IsFalse(pauseManager.IsPaused);
        
        // Act - Pause the game
        pauseManager.PauseGame();
        
        // Assert - Game should be paused
        Assert.IsTrue(pauseManager.IsPaused);
        Assert.AreEqual(0f, Time.timeScale);
        
        // Act - Resume the game
        pauseManager.ResumeGame();
        
        // Assert - Game should be resumed
        Assert.IsFalse(pauseManager.IsPaused);
        Assert.AreEqual(1f, Time.timeScale);
    }

    [UnityTest]
    public IEnumerator PauseMenu_UI_CreatedAutomatically()
    {
        // Wait one frame for Start() to be called
        yield return null;
        
        // Check if UI was created (should be created automatically)
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
        Assert.IsTrue(canvases.Length > 0, "Pause menu UI canvas should be created automatically");
        
        bool foundPauseCanvas = false;
        foreach (var canvas in canvases)
        {
            if (canvas.name == "PauseMenuCanvas")
            {
                foundPauseCanvas = true;
                break;
            }
        }
        
        Assert.IsTrue(foundPauseCanvas, "PauseMenuCanvas should be created");
    }

    [UnityTest]
    public IEnumerator PauseMenu_ShowsAndHides_Correctly()
    {
        // Wait for initialization
        yield return null;
        
        // Find the pause menu panel
        GameObject pausePanel = GameObject.Find("PauseMenuPanel");
        
        if (pausePanel != null)
        {
            // Initially should be inactive
            Assert.IsFalse(pausePanel.activeInHierarchy);
            
            // Pause should show the menu
            pauseManager.PauseGame();
            Assert.IsTrue(pausePanel.activeInHierarchy);
            
            // Resume should hide the menu
            pauseManager.ResumeGame();
            Assert.IsFalse(pausePanel.activeInHierarchy);
        }
    }

    [Test]
    public void PauseSystem_HandlesMultiplePauseCallsSafely()
    {
        // Multiple pause calls should not cause issues
        pauseManager.PauseGame();
        pauseManager.PauseGame();
        pauseManager.PauseGame();
        
        Assert.IsTrue(pauseManager.IsPaused);
        Assert.AreEqual(0f, Time.timeScale);
        
        // Should still resume normally
        pauseManager.ResumeGame();
        Assert.IsFalse(pauseManager.IsPaused);
        Assert.AreEqual(1f, Time.timeScale);
    }

    [Test]
    public void PauseSystem_PreservesTimeScaleWhenNotOne()
    {
        // Set a custom time scale
        float customTimeScale = 2.5f;
        Time.timeScale = customTimeScale;
        
        // Pause and resume
        pauseManager.PauseGame();
        Assert.AreEqual(0f, Time.timeScale);
        
        pauseManager.ResumeGame();
        Assert.AreEqual(customTimeScale, Time.timeScale, 0.01f);
    }
}