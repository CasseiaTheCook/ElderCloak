# ElderCloak Pause Menu System Documentation

## Overview

The ElderCloak pause menu system provides cross-scene pause functionality with the following features:

- **Universal Pause**: Works across all scenes without requiring individual scene setup
- **Enemy AI Integration**: Automatically pauses/resumes all PatrolScript and FlyingEnemyAI enemies
- **Time Management**: Safely handles Time.timeScale with existing hitstop effects
- **Auto-UI Generation**: Creates pause menu UI automatically if not provided
- **Input Handling**: ESC key or gamepad Start button to pause/resume

## Quick Setup

### Method 1: Automatic Setup (Recommended)
1. Add the `PauseMenuInitializer` script to any GameObject in your first scene
2. The pause system will be automatically created and persist across scenes
3. Press ESC to test the pause functionality

### Method 2: Manual Setup
1. Create an empty GameObject called "PauseMenuManager"
2. Add the `PauseMenuManager` component to it
3. The system will create UI automatically or you can assign custom UI elements

## Features

### Cross-Scene Persistence
- The PauseMenuManager uses `DontDestroyOnLoad` to persist across scenes
- Automatically refreshes enemy references when new scenes load
- Auto-resumes if paused during scene transitions

### Enemy AI Integration
The system automatically finds and manages:
- `PatrolScript` enemies (calls PauseAI/ResumeAI)
- `FlyingEnemyAI` enemies (calls PauseAI/ResumeAI)

### Input Controls
- **Keyboard**: ESC key
- **Gamepad**: Start button (configured in PlayerInput.inputactions)

### UI Elements
When auto-created, the pause menu includes:
- Semi-transparent dark background
- "PAUSED" title
- Resume button
- Settings button (placeholder)
- Quit to Menu button

## Integration with Existing Systems

### Time.timeScale Handling
The pause system works safely with the existing hitstop effect in `AttackHitbox.cs`:
- Saves the current timescale before pausing
- Restores the original timescale when resuming
- Hitstop effects check if the game is paused before applying

### Player Input System
The pause action has been added to `PlayerInput.inputactions`:
```json
{
    "name": "Pause",
    "type": "Button",
    "bindings": [
        {"path": "<Keyboard>/escape"},
        {"path": "<Gamepad>/start"}
    ]
}
```

## Testing

### Unit Tests
Run the `PauseMenuTests` class to verify:
- Singleton pattern functionality
- Pause/resume state management
- Time scale preservation
- Error handling

### Integration Tests
Run the `PauseMenuIntegrationTests` class to verify:
- Enemy AI integration
- UI creation and visibility
- Multi-scene functionality
- Edge case handling

### Manual Testing
1. Start the game and press ESC - pause menu should appear
2. Click Resume or press ESC again - game should resume
3. Pause during combat - enemies should stop moving
4. Change scenes while paused - should auto-resume in new scene

## Customization

### Custom UI
To use your own pause menu UI:
1. Set `createUIAutomatically = false` on PauseMenuManager
2. Assign your UI elements to the public fields:
   - `pauseMenuPanel`: Main container GameObject
   - `resumeButton`: Button to resume game
   - `settingsButton`: Button for settings (optional)
   - `quitButton`: Button to quit to main menu

### Adding New Enemy Types
To add support for new enemy AI types:
1. Ensure your AI script has `PauseAI()` and `ResumeAI()` methods
2. Add the AI type to the `PauseMenuManager.RefreshEnemyReferences()` method
3. Add the AI type to the pause/resume methods

### Settings Integration
The Settings button is currently a placeholder. To integrate:
1. Create your settings menu system
2. Replace the `OpenSettings()` method in PauseMenuManager
3. Ensure settings menu also pauses the game if needed

## Known Limitations

1. **Main Menu Scene**: Quit button currently quits the application since no main menu scene exists
2. **Settings Menu**: Placeholder implementation - needs custom settings system
3. **Audio**: No audio pause/resume handling implemented yet
4. **Particles**: Particle systems may need separate pause handling

## Best Practices

1. **Single Instance**: Only use one PauseMenuManager per game session
2. **Scene Independence**: Don't rely on pause state persisting across scene loads
3. **Testing**: Always test pause functionality with enemies and during combat
4. **Performance**: The system uses FindObjectsOfType for enemy discovery - consider caching for large scenes

## API Reference

### PauseMenuManager Public Methods

```csharp
// Toggle between pause and resume
public void TogglePause()

// Explicitly pause the game
public void PauseGame()

// Explicitly resume the game  
public void ResumeGame()

// Check if game is currently paused
public bool IsPaused { get; }

// Singleton instance access
public static PauseMenuManager Instance { get; }
```

### Events and Callbacks

The system automatically handles:
- `OnApplicationFocus()` - Auto-pause on focus loss (configurable)
- `OnApplicationPause()` - Auto-pause on application pause
- `OnSceneLoaded()` - Refresh enemy references and auto-resume

## Troubleshooting

### Common Issues

1. **Pause not working**: Check if PauseMenuManager exists and ESC key is not blocked by other UI
2. **Enemies not pausing**: Verify enemy scripts have PauseAI/ResumeAI methods implemented
3. **UI not showing**: Check if `createUIAutomatically` is enabled or manual UI is properly assigned
4. **Time scale issues**: Ensure no other scripts are modifying Time.timeScale during pause

### Debug Information

Enable debug messages in `PauseMenuInitializer` and check console for:
- PauseMenuManager creation confirmation
- Enemy count discovery messages
- Pause/resume state changes