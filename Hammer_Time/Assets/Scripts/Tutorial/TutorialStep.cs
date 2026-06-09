using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Condition evaluated immediately after a step's end condition fires.
/// Determines which named step the sequence jumps to next.
/// </summary>
public enum TutorialBranchConditionType
{
    None,                    // No branching — always advance to next step in order
    AimPositionNearTarget,   // TrajectoryLine.aimCircle within branchThreshold of branchTargetPosition
    AimPositionFarFromTarget,// TrajectoryLine.aimCircle farther than branchThreshold from branchTargetPosition
    RockPositionNearTarget,  // Rock world position within branchThreshold (useful after RockStopped)
    FlickVelocityAbove,      // Release speed (rb.velocity.magnitude) above branchThreshold
    FlickVelocityBelow,      // Release speed below branchThreshold
}

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

    [Tooltip("If true, use spotlightWorldPosition as the cutout target instead of a GameObject reference. " +
             "The position is projected through the active camera (aim camera when live) every frame when dynamicSpotlight=true.")]
    public bool useSpotlightWorldPosition = false;

    [Tooltip("Fixed world-space position to spotlight (e.g. the house centre at (0, 6.5, 0)). " +
             "Enable useSpotlightWorldPosition and dynamicSpotlight so the cutout re-projects as the aim camera pans.")]
    public Vector3 spotlightWorldPosition;

    [Tooltip("Manual cutout position (if no target specified)")]
    public Vector2 manualCutoutPosition;
    
    [Tooltip("Manual cutout size (if no target specified)")]
    public Vector2 manualCutoutSize = new Vector2(200, 200);
    
    [Tooltip("Padding around spotlight target (makes cutout bigger)")]
    public float spotlightPadding = 10f;

    [Tooltip("If true, the spotlight cutout position updates every frame to track a moving target. " +
             "Required for $aimTarget (aim circle) which moves as the player drags.")]
    public bool dynamicSpotlight = false;
    
    [Header("Game Control")]
    [Tooltip("Should the game be paused during this step?")]
    public bool pauseGame = false;
    
    [Tooltip("Set time scale (1 = normal, 0.5 = half speed, etc)")]
    public float timeScale = 1f;
    
    [Header("Branching")]
    [Tooltip("Condition evaluated right after the end condition fires. Success/failure determines which step to jump to.")]
    public TutorialBranchConditionType branchCondition = TutorialBranchConditionType.None;

    [Tooltip("World position used as the reference point for position-based branch conditions.")]
    public Vector3 branchTargetPosition;

    [Tooltip("For position conditions: max distance (world units) that counts as success.\n" +
             "For velocity conditions: speed (units/sec) threshold.")]
    public float branchThreshold = 1f;

    [Tooltip("Name of the step to jump to when the branch condition is TRUE. Leave empty to continue to the next step in order.")]
    public string onSuccessStep;

    [Tooltip("Name of the step to jump to when the branch condition is FALSE. Leave empty to continue to the next step in order.")]
    public string onFailureStep;

    [Header("Events")]
    [Tooltip("Custom actions to invoke when entering this step")]
    public UnityEvent onStepStart;

    [Tooltip("Custom actions to invoke when exiting this step")]
    public UnityEvent onStepEnd;
}
