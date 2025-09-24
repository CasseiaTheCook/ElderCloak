using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement playerMovement;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private SkillRegenSystem SkillRegenSystem;
    private Animator animator; 
    private bool isAttacking = false;


    private void Awake()
    {
        if (SkillRegenSystem == null)
            SkillRegenSystem = GetComponent<SkillRegenSystem>();

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
        // Heavy attack can only be performed if not already attacking AND the skill bar is full.
        if (context.performed && !isAttacking && SkillRegenSystem.IsFull())
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

        // Consume the skill bar
        SkillRegenSystem.Use();


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
        SkillRegenSystem?.AddFill();
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