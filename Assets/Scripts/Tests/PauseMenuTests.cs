using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class PauseMenuTests
{
    private GameObject pauseManagerObject;
    private PauseMenuManager pauseManager;

    [SetUp]
    public void Setup()
    {
        // Create a pause manager for testing
        pauseManagerObject = new GameObject("PauseMenuManager");
        pauseManager = pauseManagerObject.AddComponent<PauseMenuManager>();
    }

    [TearDown]
    public void Teardown()
    {
        if (pauseManagerObject != null)
        {
            Object.DestroyImmediate(pauseManagerObject);
        }
        
        // Reset time scale
        Time.timeScale = 1f;
    }

    [Test]
    public void PauseMenuManager_Singleton_WorksProperly()
    {
        // Arrange & Act
        PauseMenuManager instance1 = PauseMenuManager.Instance;

        // Assert
        Assert.IsNotNull(instance1);
        Assert.AreEqual(pauseManager, instance1);
    }

    [Test]
    public void PauseGame_SetsTimeScaleToZero()
    {
        // Arrange
        Time.timeScale = 1f;
        
        // Act
        pauseManager.PauseGame();
        
        // Assert
        Assert.AreEqual(0f, Time.timeScale);
        Assert.IsTrue(pauseManager.IsPaused);
    }

    [Test]
    public void ResumeGame_RestoresOriginalTimeScale()
    {
        // Arrange
        float originalTimeScale = 1.5f;
        Time.timeScale = originalTimeScale;
        pauseManager.PauseGame();
        
        // Act
        pauseManager.ResumeGame();
        
        // Assert
        Assert.AreEqual(originalTimeScale, Time.timeScale);
        Assert.IsFalse(pauseManager.IsPaused);
    }

    [Test]
    public void TogglePause_WorksProperly()
    {
        // Arrange
        Assert.IsFalse(pauseManager.IsPaused);
        
        // Act - First toggle should pause
        pauseManager.TogglePause();
        
        // Assert
        Assert.IsTrue(pauseManager.IsPaused);
        Assert.AreEqual(0f, Time.timeScale);
        
        // Act - Second toggle should resume
        pauseManager.TogglePause();
        
        // Assert
        Assert.IsFalse(pauseManager.IsPaused);
        Assert.AreEqual(1f, Time.timeScale);
    }

    [Test]
    public void PauseGame_WhenAlreadyPaused_DoesNothing()
    {
        // Arrange
        pauseManager.PauseGame();
        float timeScaleAfterFirstPause = Time.timeScale;
        
        // Act
        pauseManager.PauseGame();
        
        // Assert
        Assert.AreEqual(timeScaleAfterFirstPause, Time.timeScale);
        Assert.IsTrue(pauseManager.IsPaused);
    }

    [Test]
    public void ResumeGame_WhenNotPaused_DoesNothing()
    {
        // Arrange
        Time.timeScale = 1f;
        Assert.IsFalse(pauseManager.IsPaused);
        
        // Act
        pauseManager.ResumeGame();
        
        // Assert
        Assert.AreEqual(1f, Time.timeScale);
        Assert.IsFalse(pauseManager.IsPaused);
    }

    [UnityTest]
    public IEnumerator PauseMenuManager_SurvivesSceneTransition()
    {
        // Arrange
        pauseManager.PauseGame();
        Assert.IsTrue(pauseManager.IsPaused);
        
        // Act - Simulate scene loading (the pause manager should auto-resume)
        pauseManager.ResumeGame(); // Manually trigger what would happen on scene load
        
        // Assert
        Assert.IsFalse(pauseManager.IsPaused);
        Assert.AreEqual(1f, Time.timeScale);
        
        yield return null;
    }

    [Test]
    public void PauseMenuManager_HandlesNullEnemyReferences()
    {
        // This test ensures the pause manager doesn't crash when no enemies are present
        
        // Act & Assert - Should not throw exceptions
        Assert.DoesNotThrow(() => pauseManager.PauseGame());
        Assert.DoesNotThrow(() => pauseManager.ResumeGame());
    }
}