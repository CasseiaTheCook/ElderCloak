using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement playerMovement;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private HealthRegenSystem healthRegenSystem;
    private Animator animator; 
    private bool isAttacking = false;


    private void Awake()
    {
        if (healthRegenSystem == null)
            healthRegenSystem = GetComponent<HealthRegenSystem>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>(); // Get Animator from child if not assigned

            playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && !isAttacking)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is not assigned on PlayerAttack script!");
            return;
        }

        isAttacking = true;

        // Trigger attack animation
        animator.SetTrigger("Attack"); // Correctly using Trigger for Attack
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (context.performed && !isAttacking)
        {
            PerformHeavyAttack();
        }
    }

    private void PerformHeavyAttack()
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is not assigned on PlayerAttack script!");
            return;
        }

        isAttacking = true;


        animator.SetTrigger("HeavyAttack");

        playerMovement.DecreasedMovement();
        
    }


    // Called via Animation Event at the end of the normal attack animation
    public void EndAttack()
    {
        isAttacking = false;
        playerMovement.ResetMovement();
    }

    // Called via Animation Event at the end of the heavy attack animation
   /** public void EndHeavyAttack()
    {
        isHeavyAttacking = false;
    }**/

    public void OnSuccessfulHit()
    {
        healthRegenSystem?.AddFill();
    }

    

    // Called by Animation Event
    public void EnableAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.SetActive(true);
    }

    // Called by Animation Event
    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    public bool IsAttacking => isAttacking; // Expose the attacking state
}