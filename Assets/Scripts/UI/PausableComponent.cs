using UnityEngine;

/// <summary>
/// Helper component that can be added to objects that should be paused/resumed
/// when the game is paused. Useful for custom animations, particle systems, etc.
/// </summary>
public class PausableComponent : MonoBehaviour, IPausable
{
    [Header("Pausable Settings")]
    [SerializeField] private bool pauseAnimator = true;
    [SerializeField] private bool pauseParticleSystem = true;
    [SerializeField] private bool pauseAudioSource = true;
    [SerializeField] private bool pauseRigidbody = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = false;
    
    // Component references
    private Animator animator;
    private ParticleSystem particles;
    private AudioSource audioSource;
    private Rigidbody2D rb2D;
    private Rigidbody rb3D;
    
    // State tracking
    private bool wasAnimatorEnabled;
    private bool wasParticleSystemPlaying;
    private bool wasAudioSourcePlaying;
    private bool wasRigidbodyKinematic2D;
    private bool wasRigidbodyKinematic3D;
    
    private void Awake()
    {
        // Cache component references
        animator = GetComponent<Animator>();
        particles = GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        rb2D = GetComponent<Rigidbody2D>();
        rb3D = GetComponent<Rigidbody>();
    }
    
    private void Start()
    {
        // Register with pause manager
        RegisterWithPauseSystem();
    }
    
    private void OnDestroy()
    {
        // Unregister from pause manager
        UnregisterFromPauseSystem();
    }
    
    private void RegisterWithPauseSystem()
    {
        if (PauseMenuManager.Instance != null)
        {
            PauseMenuManager.Instance.RegisterPausable(this);
            if (showDebugMessages)
                Debug.Log($"Registered {gameObject.name} with pause system");
        }
    }
    
    private void UnregisterFromPauseSystem()
    {
        if (PauseMenuManager.Instance != null)
        {
            PauseMenuManager.Instance.UnregisterPausable(this);
            if (showDebugMessages)
                Debug.Log($"Unregistered {gameObject.name} from pause system");
        }
    }
    
    public void OnPause()
    {
        if (showDebugMessages)
            Debug.Log($"Pausing {gameObject.name}");
            
        // Pause Animator
        if (pauseAnimator && animator != null)
        {
            wasAnimatorEnabled = animator.enabled;
            animator.speed = 0f;
        }
        
        // Pause Particle System
        if (pauseParticleSystem && particles != null)
        {
            wasParticleSystemPlaying = particles.isPlaying;
            if (wasParticleSystemPlaying)
                particles.Pause();
        }
        
        // Pause Audio Source
        if (pauseAudioSource && audioSource != null)
        {
            wasAudioSourcePlaying = audioSource.isPlaying;
            if (wasAudioSourcePlaying)
                audioSource.Pause();
        }
        
        // Pause Rigidbody2D
        if (pauseRigidbody && rb2D != null)
        {
            wasRigidbodyKinematic2D = rb2D.isKinematic;
            rb2D.isKinematic = true;
        }
        
        // Pause Rigidbody (3D)
        if (pauseRigidbody && rb3D != null)
        {
            wasRigidbodyKinematic3D = rb3D.isKinematic;
            rb3D.isKinematic = true;
        }
    }
    
    public void OnResume()
    {
        if (showDebugMessages)
            Debug.Log($"Resuming {gameObject.name}");
            
        // Resume Animator
        if (pauseAnimator && animator != null)
        {
            animator.speed = 1f;
        }
        
        // Resume Particle System
        if (pauseParticleSystem && particles != null)
        {
            if (wasParticleSystemPlaying)
                particles.Play();
        }
        
        // Resume Audio Source
        if (pauseAudioSource && audioSource != null)
        {
            if (wasAudioSourcePlaying)
                audioSource.UnPause();
        }
        
        // Resume Rigidbody2D
        if (pauseRigidbody && rb2D != null)
        {
            rb2D.isKinematic = wasRigidbodyKinematic2D;
        }
        
        // Resume Rigidbody (3D)
        if (pauseRigidbody && rb3D != null)
        {
            rb3D.isKinematic = wasRigidbodyKinematic3D;
        }
    }
}

/// <summary>
/// Interface for objects that can be paused and resumed
/// </summary>
public interface IPausable
{
    void OnPause();
    void OnResume();
}