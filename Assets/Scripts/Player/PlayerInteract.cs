using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
public class PlayerInteract : MonoBehaviour
{
    [Header("Interact Settings")]
    public float interactDistance = 2f;
    public LayerMask interactLayerMask;



    private void Awake()
    {

    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {

    }

    public void TryInteract(InputAction.CallbackContext context)
    {
        if(context.performed)
        { // Oyuncunun pozisyonu ve y�n�
        Vector2 origin = transform.position;
        Vector2 direction = transform.right; // Sa� y�n, gerekirse de�i�tir

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, interactDistance, interactLayerMask);
        if (hit.collider != null)
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
        }

    }

    // Debug i�in ray g�sterimi
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * interactDistance);
    }
}
