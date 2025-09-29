using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PauseScript : MonoBehaviour
{
    public static PauseScript Instance { get; private set; }
    public static bool IsPaused = false;

    [Header("Unity Events")]
    public UnityEvent onPause;
    public UnityEvent onResume;

    private GameObject pauseMenuUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Tag ile UI elemanýný bul
        pauseMenuUI = GameObject.FindGameObjectWithTag("PauseMenu");
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        else
        {
            Debug.LogError("PauseMenu tag'ine sahip bir GameObject bulunamadý!");
        }
    }

    // Yeni Input Sistemi ile baðlanacak fonksiyon
    public void OnPauseAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    private void Pause()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        Time.timeScale = 0f;
        IsPaused = true;

        // Unity Event tetiklenir
        onPause?.Invoke();
    }

    private void Resume()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        Time.timeScale = 1f;
        IsPaused = false;

        // Unity Event tetiklenir
        onResume?.Invoke();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}