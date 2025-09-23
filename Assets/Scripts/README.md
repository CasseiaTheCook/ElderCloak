# Unity2D Character Controller & Health System
*Hollow Knight-Inspired Modular Game Framework*

## Overview

This project provides a comprehensive, modular character controller and health system designed for 2D platformer games in Unity, inspired by Hollow Knight's mechanics. The system is built with future-proofing in mind, using Unity's latest Input System and following modular architecture principles.

## Features

### ✅ Core Movement System
- **Smooth 2D Movement**: Responsive horizontal movement with customizable speed
- **Enhanced Jump Physics**: Variable jump height based on input duration
- **Double Jump**: Air mobility with visual and audio feedback support
- **Dash Mechanics**: Fast directional movement with cooldown system
- **Ground Detection**: Reliable ground checking with customizable parameters

### ⚔️ Combat System
- **Modular Melee Attacks**: Flexible attack system with customizable damage and range
- **Hit Detection**: Box and circle-based hitbox options
- **Knockback System**: Physics-based impact reactions
- **Attack Cooldowns**: Balanced combat pacing

### 💚 Health System  
- **Comprehensive Health Management**: Current/max health tracking with events
- **Damage Processing**: Flexible damage system with invulnerability periods
- **Death Handling**: Extensible death logic with event callbacks
- **Healing Mechanics**: Full and partial healing with balance controls

### 🎮 Input System
- **New Input System Integration**: Future-proof input handling
- **Multiple Control Schemes**: Keyboard & Mouse, Gamepad, Touch support
- **Customizable Bindings**: Easily modifiable control mappings
- **Event-Driven Architecture**: Clean input event handling

### 🤖 Enemy AI Framework
- **Base Enemy Class**: Abstract foundation for all enemy types  
- **Behavior States**: Patrol, Chase, Attack, and Death states
- **Detection Systems**: Player detection and aggro mechanics
- **Modular Design**: Easy to extend for different enemy types

## Architecture

The system follows a modular, component-based architecture using interfaces and abstract classes:

```
ElderCloak/
├── Interfaces/
│   ├── IDamageable.cs      # Damage handling contract
│   ├── IHealth.cs          # Health system contract  
│   └── IAttackable.cs      # Attack behavior contract
├── Player/
│   ├── PlayerController.cs # Main movement controller
│   └── PlayerCombat.cs     # Combat input handler
├── Combat/
│   └── MeleeAttackSystem.cs # Attack mechanics
├── Enemy/
│   ├── BaseEnemy.cs        # Abstract enemy foundation
│   └── BasicMeleeEnemy.cs  # Example implementation
└── HealthComponent.cs      # Universal health system
```

## Quick Setup Guide

### 1. Player Setup
1. Create an empty GameObject for your player
2. Add these components:
   - `Rigidbody2D` (automatically added)
   - `SpriteRenderer` with your player sprite
   - `Collider2D` (BoxCollider2D or CapsuleCollider2D)
   - `PlayerController`
   - `PlayerCombat`  
   - `MeleeAttackSystem`
   - `HealthComponent`
3. Set the Player tag
4. Configure the Ground Layer in Project Settings
5. Assign the InputSystem_Actions.inputactions asset

### 2. Enemy Setup  
1. Create an empty GameObject for your enemy
2. Add these components:
   - `SpriteRenderer` with enemy sprite
   - `Collider2D`
   - `BasicMeleeEnemy` (or your custom enemy class)
   - `MeleeAttackSystem` (if the enemy attacks)
   - `HealthComponent`
3. Configure detection ranges and behavior parameters

### 3. Input System Configuration
The project includes a pre-configured InputSystem_Actions.inputactions with:
- **Movement**: WASD/Arrow Keys, Left Stick
- **Jump**: Space, South Button (A/X)  
- **Attack**: Left Mouse, Enter, West Button (X/□)
- **Dash**: Left Alt, Right Shoulder Button

## Component Reference

### PlayerController
**Key Properties:**
- `moveSpeed`: Horizontal movement speed
- `jumpForce`: Initial jump velocity  
- `doubleJumpForce`: Second jump velocity
- `dashSpeed`: Dash movement speed
- `dashDuration`: How long dash lasts
- `dashCooldown`: Time between dashes

**Key Methods:**
- `StopDash()`: Force stop current dash
- `ResetDashCooldown()`: Reset dash availability
- `SetCanDash(bool)`: Enable/disable dashing

### HealthComponent  
**Key Properties:**
- `MaxHealth`: Maximum health value
- `CurrentHealth`: Current health (readonly)
- `IsAlive`: Whether health > 0 (readonly)

**Key Methods:**
- `TakeDamage(float, GameObject)`: Apply damage
- `Heal(float)`: Restore health
- `RestoreFullHealth()`: Full heal
- `SetMaxHealth(float, bool)`: Update maximum health

**Events:**
- `OnHealthChanged(float current, float max)`: Health value changed
- `OnDeath()`: Health reached zero
- `OnDamageTaken(float damage, GameObject source)`: Damage received

### MeleeAttackSystem
**Key Properties:**
- `AttackDamage`: Damage per attack
- `AttackRange`: Attack reach distance
- `CanAttack`: Whether attack is available (readonly)
- `IsAttacking`: Whether currently attacking (readonly)

**Key Methods:**
- `PerformAttack()`: Execute an attack
- `CancelAttack()`: Stop current attack
- `SetAttackDamage(float)`: Modify damage
- `SetTargetLayerMask(LayerMask)`: Set what can be hit

**Events:**
- `OnAttackStart()`: Attack animation begins
- `OnAttackHit()`: Attack connects with target  
- `OnAttackEnd()`: Attack sequence completes
- `OnTargetHit(GameObject)`: Specific target was hit

### BaseEnemy
**Key Properties:**
- `IsAlive`: Whether enemy is alive (readonly)
- `IsPlayerInRange`: Player within detection range (readonly)
- `IsAggro`: Currently aggressive toward player (readonly)
- `Player`: Reference to player transform (readonly)

**Abstract Methods (Must Implement):**
- `OnEnemyStart()`: Initialization logic
- `UpdateBehavior()`: AI behavior logic
- `UpdateMovement()`: Movement logic  
- `OnEnemyDeath()`: Death handling
- `OnEnemyDamageTaken(float, GameObject)`: Damage reaction

## Extension Examples

### Custom Enemy Type
```csharp
public class RangedEnemy : BaseEnemy
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootRange = 5f;
    
    protected override void UpdateBehavior()
    {
        if (isAggro && IsPlayerInRange)
        {
            ShootAtPlayer();
        }
    }
    
    // Implement other abstract methods...
}
```

### Custom Player Ability
```csharp
public class PlayerWallJump : MonoBehaviour
{
    private PlayerController controller;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && IsOnWall())
        {
            PerformWallJump();
        }
    }
}
```

### Health Bar UI
```csharp  
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private HealthComponent playerHealth;
    
    private void Start()
    {
        playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
    }
    
    private void UpdateHealthBar(float current, float max)
    {
        healthSlider.value = current / max;
    }
}
```

## Performance Considerations

- **Object Pooling**: Consider pooling for frequently spawned objects (projectiles, effects)
- **Hit Detection**: Uses NonAlloc methods to reduce garbage collection
- **Event System**: UnityEvents provide flexibility but consider direct callbacks for high-frequency events
- **Ground Checking**: Uses efficient OverlapCircle for ground detection

## Future Compatibility

The system is designed with Unity's evolution in mind:

- **New Input System**: Uses Unity's recommended input solution
- **Component Architecture**: Follows Unity's ECS-friendly patterns
- **Interface-Based Design**: Easy to adapt to new Unity features
- **Event-Driven Communication**: Reduces tight coupling between systems
- **Scriptable Objects**: Ready for data-driven design patterns

## Troubleshooting

### Common Issues

**Player not moving:**
- Check if Rigidbody2D is present and not frozen
- Verify Input System package is installed
- Ensure InputSystem_Actions.inputactions is assigned

**Ground detection not working:**
- Confirm Ground LayerMask is set correctly
- Check if groundCheck transform is positioned properly
- Verify ground objects have correct layer assigned

**Attacks not hitting:**
- Check targetLayerMask on MeleeAttackSystem
- Ensure attackPoint is positioned correctly
- Verify target objects have appropriate colliders

**Health system not responding:**
- Confirm HealthComponent is added to GameObjects
- Check if objects implement IDamageable interface
- Verify UnityEvents are properly assigned

## Version Compatibility

- **Unity Version**: 6000.2.4f1 or higher
- **Input System Package**: 1.7.0 or higher
- **.NET Standard**: 2.1 compatible

## Contributing

When extending this system:

1. Follow the established interface patterns
2. Add comprehensive XML documentation
3. Include Gizmos for editor visualization
4. Provide configuration options via SerializeField
5. Use UnityEvents for extensible callback systems

This system provides a solid foundation for 2D platformer games while remaining flexible and extensible for your specific game requirements.