using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Defines when a tutorial condition should be checked
/// </summary>
public enum TutorialConditionType
{
    None,                   // No condition, proceeds immediately
    WaitForInput,           // Wait for any input
    WaitForClick,           // Wait for mouse button down
    MouseReleased,          // Wait for mouse button up
    WaitForSeconds,         // Wait for real-time delay (unaffected by timeScale/pause)
    CameraStoppedMoving,    // Wait until camera stops moving (velocity near zero)
    RockGrabbed,            // Wait until player grabs/clicks the rock (shotTaken)
    RockBeingDragged,       // Check if rock is being dragged (grabbed but not released) - skips if already released
    RockPullbackThreshold,  // Wait until rock is pulled back past a distance threshold (skips if already released)
    RockReleased,           // Wait until rock is released
    RockStopped,            // Wait until rock stops moving
    RockReachedYPosition,   // Wait until rock Y crosses a target Y value
    GameStateChange,        // Wait for specific game state
    CustomCondition         // Use custom condition function
}

/// <summary>
/// A single step in a tutorial sequence.
/// Create via: Assets > Create > Tutorial > Tutorial Step
/// </summary>
[CreateAssetMenu(fileName = "NewTutorialStep", menuName = "Tutorial/Tutorial Step", order = 1)]
public class TutorialStep : ScriptableObject
{
    [Header("Step Info")]
    [Tooltip("Descriptive name for this step (for organization)")]
    public string stepName = "New Tutorial Step";
    
    [Tooltip("Optional dialogue to show during this step")]
    public DialogueData dialogue;
    
    [Header("Conditions")]
    [Tooltip("START condition: Must be met BEFORE showing dialogue/setup (None = no wait, show immediately)")]
    public TutorialConditionType startCondition = TutorialConditionType.None;
    
    [Tooltip("END condition: What condition must be met to advance to next step")]
    public TutorialConditionType endCondition = TutorialConditionType.WaitForClick;
    
    [Tooltip("For WaitForSeconds condition: how long to wait")]
    public float waitDuration = 1f;
    
    [Tooltip("For GameStateChange condition: target game state")]
    public GameState targetGameState;
    
    [Tooltip("For RockReleased/RockStopped/RockPullbackThreshold/RockReachedYPosition: which rock index to watch (-1 for current rock)")]
    public int rockIndex = -1;

    [Tooltip("For RockPullbackThreshold: minimum pullback distance to trigger (e.g., 0.5 = pulled back 50% of max)")]
    public float pullbackThreshold = 0.5f;

    [Tooltip("For RockReachedYPosition: the Y world position to watch for")]
    public float targetYPosition = 0f;

    [Tooltip("For RockReachedYPosition: true = wait until rock Y >= target (rock moving up-ice), false = wait until rock Y <= target")]
    public bool targetYAbove = true;
    
    [Header("Visual Highlights")]
    [Tooltip("UI elements or game objects to highlight during this step")]
    public GameObject[] objectsToHighlight;
    
    [Tooltip("Show aim circle at this position (leave null to skip)")]
    public Vector2? aimCirclePosition;
    
    [Tooltip("Reference to aim circle prefab")]
    public GameObject aimCirclePrefab;
    
    [Header("Spotlight/Cutout")]
    [Tooltip("Enable spotlight overlay with cutout")]
    public bool useSpotlight = false;
    
    [Tooltip("UI element to spotlight (auto-sizes cutout to match)")]
    public RectTransform spotlightTarget;
    
    [Tooltip("World object to spotlight (for game objects, not UI)")]
    public Transform spotlightWorldTarget;

    [Tooltip("Find spotlight target by name at runtime. Use exact GameObject name (e.g. 'ScorePanel') OR a reserved keyword: $currentRock, $shooter, $launcher")]
    public string spotlightTargetName;

    [Tooltip("Find spotlight target by Tag at runtime (e.g. 'GameController'). Used if spotlightTargetName is empty.")]
    public string spotlightTargetTag;

    [Tooltip("Manual cutout position (if no target specified)")]
    public Vector2 manualCutoutPosition;
    
    [Tooltip("Manual cutout size (if no target specified)")]
    public Vector2 manualCutoutSize = new Vector2(200, 200);
    
    [Tooltip("Padding around spotlight target (makes cutout bigger)")]
    public float spotlightPadding = 10f;
    
    [Header("Game Control")]
    [Tooltip("Should the game be paused during this step?")]
    public bool pauseGame = false;
    
    [Tooltip("Set time scale (1 = normal, 0.5 = half speed, etc)")]
    public float timeScale = 1f;
    
    [Header("Events")]
    [Tooltip("Custom actions to invoke when entering this step")]
    public UnityEvent onStepStart;
    
    [Tooltip("Custom actions to invoke when exiting this step")]
    public UnityEvent onStepEnd;
}
