# Trajectory & Collision Visualization Toggle Setup Guide

## Overview
You now have a complete system to control trajectory dots and collision arrows visibility via UI toggles. The system persists settings across sessions using PlayerPrefs.

## Files Created/Modified

### New Files:
1. **`Assets/Scripts/Settings/GameVisualizationSettings.cs`**
   - Singleton settings manager
   - Persists settings to PlayerPrefs
   - Notifies listeners when settings change

2. **`Assets/Scripts/UI/VisualizationToggleUI.cs`**
   - UI controller for toggle components
   - Wires UI toggles to settings

### Modified Files:
1. **`Assets/Scripts/UI/TrajectoryLine.cs`**
   - Subscribes to visibility change events
   - Respects visibility settings when drawing dots
   - Respects visibility settings when showing collision arrows

## Unity Inspector Setup (3 Easy Steps)

### Step 1: Add VisualizationToggleUI Component
1. In your **Options/Settings menu scene**, find or create a GameObject for settings
2. Add Component ? `VisualizationToggleUI`
3. This component will wire up your toggles

### Step 2: Create UI Toggles
If you don't have toggles yet:
1. Right-click in Hierarchy ? UI ? Toggle (creates "Trajectory Dots Toggle")
2. Right-click in Hierarchy ? UI ? Toggle (creates "Collision Lines Toggle")
3. Position them in your settings menu
4. Rename them for clarity (e.g., "Toggle - Trajectory Dots", "Toggle - Collision Lines")

### Step 3: Wire Up the Toggles
1. Select the GameObject with `VisualizationToggleUI` component
2. In Inspector, drag your toggles into the fields:
   - **Trajectory Dots Toggle** ? drag "Toggle - Trajectory Dots"
   - **Collision Lines Toggle** ? drag "Toggle - Collision Lines"
3. Done! The system is now connected.

## How It Works

### Trajectory Dots Toggle
When **ON (default)**:
- Dots appear along the predicted path during aiming
- Variable size dots (bigger = faster speed, smaller = slower speed)

When **OFF**:
- No dots shown (cleaner aim view)
- Trajectory line still shows
- Aim circle still shows

### Collision Lines Toggle
When **ON (default)**:
- Orange arrow shows where thrown rock will deflect after collision
- Yellow arrow shows where hit rock will exit
- Collision marker shows impact point

When **OFF**:
- No collision visualization (less cluttered screen)
- Trajectory line still shows path to collision point

## Settings Persistence
- Settings are saved to PlayerPrefs automatically
- Persist across game sessions
- Default: Both toggles ON

## Advanced: Accessing Settings in Code

```csharp
// Get current settings anywhere in your code
GameVisualizationSettings settings = GameVisualizationSettings.Instance;

// Check current values
bool dotsVisible = settings.TrajectoryVisible;
bool collisionVisible = settings.CollisionLinesVisible;

// Change settings programmatically
settings.ToggleTrajectoryVisibility(false);
settings.ToggleCollisionLinesVisibility(true);

// Subscribe to changes
settings.OnTrajectoryVisibilityChanged += (visible) => {
    Debug.Log($"Trajectory dots are now: {visible}");
};

// Reset to defaults
settings.ResetToDefaults();
```

## Testing

1. **Start game** and go to settings menu
2. **Toggle trajectory dots OFF**
   - Start a new shot
   - Pull back rock
   - Should see trajectory line but NO dots
3. **Toggle trajectory dots ON**
   - Release and start another shot
   - Pull back rock
   - Should see dots along trajectory
4. **Toggle collision lines OFF**
   - Aim at a rock
   - Should see trajectory to collision point but NO arrows
5. **Toggle collision lines ON**
   - Aim at same rock
   - Should see orange/yellow arrows showing deflection

## Troubleshooting

### Toggles don't do anything
- Check Inspector: Are toggles assigned to `VisualizationToggleUI`?
- Check Console: Should see `[VisualizationToggleUI] Initialized` on start
- Check Console: Should see `[TrajectoryLine] Visualization settings initialized` on start

### Settings don't persist
- Settings are saved to PlayerPrefs automatically
- Check: `PlayerPrefs.GetInt("TrajectoryVisible")` in Console
- If still broken, call `settings.ResetToDefaults()` to reinitialize

### Dots still show when toggle is OFF
- Check `TrajectoryLine` Inspector: Is component on correct GameObject?
- Check Console for `[TrajectoryLine] Trajectory dots visibility changed to: false`
- Verify GameVisualizationSettings singleton is initialized

## Optional Enhancements

Want to add more options? Easy template:

```csharp
// In GameVisualizationSettings.cs, add new setting:
private bool aimCircleVisible = true;
public bool AimCircleVisible { 
    get => aimCircleVisible; 
    set { /* same pattern as other properties */ } 
}

// In TrajectoryLine.cs, subscribe:
visualSettings.OnAimCircleVisibilityChanged += (visible) => {
    aimCircle.GetComponent<SpriteRenderer>().enabled = visible;
};
```

## Summary
? Trajectory dots: Controlled by toggle (ON/OFF)
? Collision arrows: Controlled by toggle (ON/OFF)
? Settings persist across sessions
? Clean, event-driven architecture
? Easy to extend for more options

Enjoy your customizable curling visualization! ??
