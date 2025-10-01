using UnityEngine;

public class FindPlayerPause : MonoBehaviour
{
    private PauseScript pauseScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        pauseScript = GameObject.FindFirstObjectByType<PauseScript>();
    }
    public void Resume()
    {
        pauseScript.TogglePause();
    }
}
