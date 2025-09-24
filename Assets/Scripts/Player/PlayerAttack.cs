using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private HealthRegenSystem healthRegenSystem;

    [Header("Attack Settings")]
    [SerializeField] private float attackDuration = 0.2f;

    private Collider2D attackCollider;
    private bool isAttacking = false;

    private void Awake()
    {
        if (healthRegenSystem == null)
            healthRegenSystem = GetComponent<HealthRegenSystem>();

        if (attackHitbox != null)
            attackCollider = attackHitbox.GetComponent<Collider2D>();
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
        if (attackCollider == null)
        {
            Debug.LogWarning("AttackHitbox is not assigned on PlayerAttack script!");
            return;
        }

      
        isAttacking = true;
        attackCollider.enabled = true;

       
        Invoke(nameof(ResetAttack), attackDuration);
    }

    private void ResetAttack()
    {
        attackCollider.enabled = false;
        isAttacking = false;
    }

    
    public void OnSuccessfulHit()
    {
        healthRegenSystem?.AddFill();
    }
}