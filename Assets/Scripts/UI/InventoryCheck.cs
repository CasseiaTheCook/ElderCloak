using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInputActions = PlayerInput;

public class InventoryCheck : MonoBehaviour
{
    public GameObject InventoryMenu;

    
    private PlayerInputActions playerInputActions;
    private bool inventoryOpen = false;

    private void Awake()
    {
       
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        playerInputActions.PlayerControls.Enable();
        playerInputActions.PlayerControls.Inventory.performed += ToggleInventory;
    }

    private void OnDisable()
    {
        playerInputActions.PlayerControls.Disable();
        playerInputActions.PlayerControls.Inventory.performed -= ToggleInventory;
    }

    private void ToggleInventory(InputAction.CallbackContext context)
    {
        inventoryOpen = !inventoryOpen;
        InventoryMenu.SetActive(inventoryOpen);
    }
}