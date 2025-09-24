using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InventoryCheck : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryMenu;

    // List to hold all other UI panels that are not the inventory.
    private List<GameObject> otherUIPanels = new List<GameObject>();

    private Canvas rootCanvas;
    private bool inventoryOpen = false;

    private void Awake()
    {
       if(inventoryMenu == null)
       {
            Debug.LogError("InventoryMenu GameObject is not assigned in the Inspector.", this);
            return;
       }
       else
       {
            inventoryMenu.SetActive(false);
       }

       // Find the root canvas automatically.
       rootCanvas = GetComponentInParent<Canvas>();
       if (rootCanvas == null)
       {
           Debug.LogError("InventoryCheck must be a child of a Canvas.", this);
           return;
       }

       // Find all other UI panels to hide them when inventory is open.
       foreach (Transform child in rootCanvas.transform)
       {
           if (child.gameObject != inventoryMenu)
           {
               otherUIPanels.Add(child.gameObject);
           }
       }
    }

    public void ToggleInventory(InputAction.CallbackContext context)
    {
        if (context.performed == false) return;

        inventoryOpen = !inventoryOpen;
        inventoryMenu.SetActive(inventoryOpen);

        // Hide or show other UI panels
        foreach (var panel in otherUIPanels)
        {
            if (panel != null)
            {
                panel.SetActive(!inventoryOpen);
            }
        }

        // Slow down or reset player movement
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            if (inventoryOpen)
            {
                playerMovement.DecreasedMovement();
            }
            else
            {
                playerMovement.ResetMovement();
            }
        }
    }
}