using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

/// <summary>
/// Edge case and performance tests for the pause menu system
/// </summary>
public class PauseMenuEdgeCaseTests
{
    private GameObject pauseManagerObject;
    private PauseMenuManager pauseManager;

    [SetUp]
    public void Setup()
    {
        // Create pause manager
        pauseManagerObject = new GameObject("PauseMenuManager");
        pauseManager = pauseManagerObject.AddComponent<PauseMenuManager>();
        
        // Reset time scale
        Time.timeScale = 1f;
    }

    [TearDown]
    public void Teardown()
    {
        if (pauseManagerObject != null)
            Object.DestroyImmediate(pauseManagerObject);
            
        // Clean up any test objects
        GameObject[] testObjects = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (var obj in testObjects)
        {
            if (obj.name.Contains("Test") || obj.name.Contains("Enemy"))
                Object.DestroyImmediate(obj);
        }
        
        Time.timeScale = 1f;
    }

    [Test]
    public void PauseSystem_WithManyEnemies_PerformsWell()
    {
        // Create many enemy objects to test performance
        var enemies = new GameObject[100];
        
        // Create 50 patrol enemies and 50 flying enemies
        for (int i = 0; i < 50; i++)
        {
            enemies[i] = new GameObject($"PatrolEnemy_{i}");
            enemies[i].AddComponent<PatrolScript>();
        }
        
        for (int i = 50; i < 100; i++)
        {
            enemies[i] = new GameObject($"FlyingEnemy_{i}");
            enemies[i].AddComponent<FlyingEnemyAI>();
        }

        // Measure pause time
        var startTime = System.DateTime.Now;
        pauseManager.PauseGame();
        var pauseTime = System.DateTime.Now - startTime;
        
        // Should complete within reasonable time (100ms)
        Assert.Less(pauseTime.TotalMilliseconds, 100, "Pause operation should complete quickly even with many enemies");
        Assert.IsTrue(pauseManager.IsPaused);
        
        // Measure resume time
        startTime = System.DateTime.Now;
        pauseManager.ResumeGame();
        var resumeTime = System.DateTime.Now - startTime;
        
        Assert.Less(resumeTime.TotalMilliseconds, 100, "Resume operation should complete quickly even with many enemies");
        Assert.IsFalse(pauseManager.IsPaused);
        
        // Cleanup
        foreach (var enemy in enemies)
        {
            if (enemy != null)
                Object.DestroyImmediate(enemy);
        }
    }

    [Test]
    public void PauseSystem_WithPausableComponents_WorksCorrectly()
    {
        // Create objects with pausable components
        GameObject testObject1 = new GameObject("TestPausable1");
        GameObject testObject2 = new GameObject("TestPausable2");
        
        var pausable1 = testObject1.AddComponent<PausableComponent>();
        var pausable2 = testObject2.AddComponent<PausableComponent>();
        
        // Manually register (since Start() might not be called in test)
        pauseManager.RegisterPausable(pausable1);
        pauseManager.RegisterPausable(pausable2);
        
        // Test pause
        pauseManager.PauseGame();
        Assert.IsTrue(pauseManager.IsPaused);
        
        // Test resume
        pauseManager.ResumeGame();
        Assert.IsFalse(pauseManager.IsPaused);
        
        // Test unregistration
        pauseManager.UnregisterPausable(pausable1);
        pauseManager.UnregisterPausable(pausable2);
        
        // Cleanup
        Object.DestroyImmediate(testObject1);
        Object.DestroyImmediate(testObject2);
    }

    [Test]
    public void PauseSystem_HandlesNullReferences()
    {
        // Register a null pausable component
        pauseManager.RegisterPausable(null);
        
        // Should not throw exception when pausing
        Assert.DoesNotThrow(() => pauseManager.PauseGame());
        Assert.DoesNotThrow(() => pauseManager.ResumeGame());
        
        // Test with destroyed components
        GameObject testObject = new GameObject("TestObject");
        var pausable = testObject.AddComponent<PausableComponent>();
        pauseManager.RegisterPausable(pausable);
        
        // Destroy the object
        Object.DestroyImmediate(testObject);
        
        // Should handle destroyed component gracefully
        Assert.DoesNotThrow(() => pauseManager.PauseGame());
        Assert.DoesNotThrow(() => pauseManager.ResumeGame());
    }

    [Test]
    public void PauseSystem_HandlesRapidToggling()
    {
        // Rapid pause/resume should not break the system
        for (int i = 0; i < 10; i++)
        {
            pauseManager.TogglePause();
            pauseManager.TogglePause();
        }
        
        // Should end up in resumed state
        Assert.IsFalse(pauseManager.IsPaused);
        Assert.AreEqual(1f, Time.timeScale);
    }

    [Test]
    public void PauseSystem_PreservesCustomTimeScale()
    {
        // Test with various time scales
        float[] testTimeScales = { 0.5f, 1.5f, 2.0f, 0.1f, 3.0f };
        
        foreach (float timeScale in testTimeScales)
        {
            Time.timeScale = timeScale;
            
            pauseManager.PauseGame();
            Assert.AreEqual(0f, Time.timeScale, "Time scale should be 0 when paused");
            
            pauseManager.ResumeGame();
            Assert.AreEqual(timeScale, Time.timeScale, 0.01f, $"Time scale should be restored to {timeScale}");
        }
    }

    [UnityTest]
    public IEnumerator PauseSystem_HandlesDuringHitstop()
    {
        // This test simulates pausing during a hitstop effect
        
        // Simulate hitstop by setting time scale to 0
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        yield return new WaitForSecondsRealtime(0.1f);
        
        // Try to pause during hitstop
        pauseManager.PauseGame();
        Assert.IsTrue(pauseManager.IsPaused);
        
        // Resume should restore the original time scale (before hitstop)
        pauseManager.ResumeGame();
        Assert.IsFalse(pauseManager.IsPaused);
        // Note: This test shows the limitation - we can't perfectly handle hitstop + pause
        // But the system should at least not crash
    }

    [Test]
    public void PauseSystem_SingletonBehavior_WorksCorrectly()
    {
        // Test singleton destruction and recreation
        Assert.AreEqual(pauseManager, PauseMenuManager.Instance);
        
        // Destroy current instance
        Object.DestroyImmediate(pauseManagerObject);
        
        // Instance should be null after destruction
        Assert.IsNull(PauseMenuManager.Instance);
        
        // Create new instance
        GameObject newManagerObject = new GameObject("NewPauseMenuManager");
        PauseMenuManager newManager = newManagerObject.AddComponent<PauseMenuManager>();
        
        // Should become the new instance
        Assert.AreEqual(newManager, PauseMenuManager.Instance);
        
        // Cleanup
        Object.DestroyImmediate(newManagerObject);
    }

    [Test]
    public void PauseSystem_MultipleInstances_HandlesProperly()
    {
        // Try to create a second instance
        GameObject secondManagerObject = new GameObject("SecondPauseMenuManager");
        PauseMenuManager secondManager = secondManagerObject.AddComponent<PauseMenuManager>();
        
        // Original should still be the instance
        Assert.AreEqual(pauseManager, PauseMenuManager.Instance);
        
        // Second manager should have destroyed itself
        // (This might need a frame to complete, so we test the behavior)
        Assert.IsTrue(secondManagerObject == null || !secondManagerObject.activeInHierarchy);
    }
}