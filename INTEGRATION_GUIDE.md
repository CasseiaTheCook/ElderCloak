# ElderCloak Unity2D Character Controller System

## Overview
This comprehensive character controller system provides Hollow Knight-inspired 2D platformer mechanics with modular, extensible architecture designed for future Unity versions. The system includes movement, combat, health management, and AI enemies.

## Features
- **Double Jump** with coyote time and jump buffering
- **Dash Movement** with cooldown and air dash limitations
- **Melee Attack System** with combo support and multiple hitbox shapes
- **Health System** with damage immunity, regeneration, and events
- **AI Enemies** with patrol, chase, and guard behaviors
- **Input System Integration** with full Unity Input System support
- **Modular Architecture** using interfaces for easy extension

## Quick Setup Guide

### 1. Basic Player Setup

1. **Create a Player GameObject:**
   ```
   - Create empty GameObject named "Player"
   - Add Rigidbody2D component (set Freeze Rotation Z to true)
   - Add a Collider2D (BoxCollider2D or CapsuleCollider2D)
   - Add PlayerController script
   - Add PlayerCombatController script
   ```

2. **Configure PlayerController:**
   - Set movement parameters (moveSpeed: 8, jumpForce: 16)
   - Adjust double jump settings (enable: true, doubleJumpForce: 14)
   - Configure dash settings (enable: true, dashForce: 20, dashDuration: 0.2)
   - Set up ground detection (create child GameObject for GroundCheck)

3. **Set up Input System:**
   - Create InputManager GameObject in scene
   - The existing InputSystem_Actions.inputactions asset is already configured
   - InputManager will automatically connect to Player components

### 2. Enemy Setup

1. **Create Enemy GameObject:**
   ```
   - Create empty GameObject named "Enemy"
   - Add Rigidbody2D component
   - Add Collider2D component
   - Add BasicEnemyController script
   ```

2. **Configure Enemy Behavior:**
   - Set AI behavior type (Patrol, Chase, Guard, Idle)
   - Configure detection range and attack range
   - Set health values and attack damage
   - Assign player layer mask for detection

### 3. Camera Setup

1. **Configure Main Camera:**
   ```
   - Add CameraController script to Main Camera
   - Set target to Player GameObject
   - Configure follow settings and boundaries
   - Enable screen shake if desired
   ```

### 4. Game Manager Setup

1. **Create Game Manager:**
   ```
   - Create empty GameObject named "GameManager"
   - Add GameManager script
   - Assign player prefab and spawn point
   - Configure system references
   ```

## Architecture Overview

### Interface System
The system uses interfaces for modular design:

- **IDamageable**: Objects that can take damage
- **IHealth**: Health system management
- **IAttackable**: Objects that can attack
- **IMovementController**: Movement capabilities

### Component Hierarchy
```
Player GameObject
├── PlayerController (Movement + Health)
├── PlayerCombatController (Attack System)
└── Child GameObjects
    ├── GroundCheck (Ground detection)
    └── WallCheck (Wall detection)

Enemy GameObject
├── BasicEnemyController (AI + Health + Combat)
└── Child GameObjects (optional)

Systems
├── InputManager (Centralized input)
├── GameManager (Game state)
└── CameraController (Camera follow)
```

## Customization Guide

### Creating Custom Attacks

1. **Modify Attack Data:**
   ```csharp
   // In MeleeAttackSystem, create custom AttackData
   AttackData customAttack = new AttackData
   {
       attackName = "Heavy Strike",
       damage = 50f,
       range = 3f,
       duration = 0.5f,
       hitboxShape = HitboxShape.Box,
       knockbackForce = 15f
   };
   ```

### Adding New Enemy Types

1. **Inherit from BasicEnemyController:**
   ```csharp
   public class FlyingEnemyController : BasicEnemyController
   {
       // Override AI methods for custom behavior
       protected override void HandleMovement()
       {
           // Custom flying movement logic
       }
   }
   ```

### Custom Health Behaviors

1. **Use Health System Events:**
   ```csharp
   healthSystem.OnHealthChanged += (current, max) => {
       // Update UI health bar
   };
   
   healthSystem.OnDeath += () => {
       // Custom death behavior
   };
   ```

## Input Configuration

The system uses Unity's Input System with the following default bindings:

### Keyboard Controls
- **Movement**: WASD / Arrow Keys
- **Jump**: Space
- **Attack**: Left Mouse / Enter
- **Dash**: X / Left Shift (tap)
- **Sprint**: Left Shift (hold)

### Gamepad Controls
- **Movement**: Left Stick
- **Jump**: A Button (Xbox) / Cross (PlayStation)
- **Attack**: X Button (Xbox) / Square (PlayStation)
- **Dash**: Right Bumper
- **Sprint**: Left Stick Press

## Performance Considerations

### Optimization Tips
1. **Ground Detection**: Use small detection boxes, not raycasts
2. **Attack System**: Limit hit detection frequency during active frames
3. **AI Updates**: Consider using coroutines for complex AI logic
4. **Event Cleanup**: Always unsubscribe from events in OnDestroy

### Memory Management
- Health systems are designed to minimize allocations
- Attack systems reuse hit lists
- Input handlers use events to avoid polling

## Troubleshooting

### Common Issues

1. **Player Not Moving:**
   - Check if InputManager is in scene and enabled
   - Verify Rigidbody2D is not kinematic
   - Ensure ground detection is properly configured

2. **Attacks Not Working:**
   - Verify target layer masks are set correctly
   - Check if attack cooldowns are appropriate
   - Ensure combat controller is connected to input

3. **Health System Not Responding:**
   - Make sure health system is initialized
   - Check if object implements IDamageable correctly
   - Verify damage source layer masks

## Extension Examples

### Adding Wall Jumping
```csharp
// In PlayerController, add wall jump logic to HandleJumpInput()
if (isTouchingWall && !isGrounded && canWallJump)
{
    Vector2 wallJumpForce = new Vector2(-wallDirection * wallJumpPower, jumpForce);
    rb.velocity = wallJumpForce;
    airJumpsUsed = 0; // Reset air jumps
}
```

### Creating Special Attacks
```csharp
// Create new attack data with special properties
AttackData fireballAttack = new AttackData
{
    attackName = "Fireball",
    damage = 30f,
    range = 8f,
    hitboxShape = HitboxShape.Cone,
    coneAngle = 45f
};
```

## Future Compatibility

The system is designed to be future-proof:

- Uses Unity's new Input System (compatible with Unity 2022+)
- Modular architecture allows easy updates
- Interface-based design supports new features
- Event-driven systems reduce coupling
- Serializable systems preserve data across versions

## Support and Updates

For questions, improvements, or bug reports, please refer to the project repository. The system is designed to be extended and modified for your specific game needs.

## Dependencies

- Unity 2022.3+ (tested with Unity 6000.2.4f1)
- Unity Input System package
- Unity 2D packages (Sprite, Animation, etc.)
- Universal Render Pipeline (URP) recommended

This system provides a solid foundation for 2D platformer games and can be extended to support additional features like air dashing, wall climbing, ranged attacks, and more complex AI behaviors.