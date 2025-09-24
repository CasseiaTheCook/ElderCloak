using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInputActions = PlayerInput;

public class InventoryCheck : MonoBehaviour
{
    private GameObject InventoryMenu;

    

    private bool inventoryOpen = false;

    private void Awake()
    {
       InventoryMenu=GameObject.Find("Inventory");
       if(InventoryMenu==null)
       {
        Debug.LogError("InventoryMenu GameObject not found in the scene.");
       }
       else
       {
        InventoryMenu.SetActive(false);
       }
    }
    public void ToggleInventory(InputAction.CallbackContext context)
    {
        inventoryOpen = !inventoryOpen;
        InventoryMenu.SetActive(inventoryOpen);

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