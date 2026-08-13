using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages playing tutorial sequences.
/// Singleton accessible via TutorialSequenceManager.Instance
/// </summary>
public class TutorialSequenceManager : MonoBehaviour
{
    public static TutorialSequenceManager Instance { get; private set; }
    
    [Header("Available Tutorials")]
    [Tooltip("All tutorial sequences that can be played")]
    [SerializeField] private TutorialSequence[] availableTutorials;
    
    [Header("UI References")]
    [SerializeField] private GameObject skipButton;
    [SerializeField] private GameObject spotlightOverlay;
    [SerializeField] private RectTransform cutoutMask;
    
    [Header("Settings")]
    [SerializeField] private bool enableTutorials = true;
    [SerializeField] private bool enableAutoStartTutorials = false;
    
    // Current state
    private TutorialSequence currentSequence;
    private int currentStepIndex;
    private bool isPlaying;
    private Coroutine playCoroutine;
    private GameObject[] currentHighlights;
    private GameObject currentAimCircle;
    private Coroutine dynamicSpotlightCoroutine;

    // Captured game-state values used by branch condition evaluation.
    // Updated whenever the relevant end condition fires.
    private float lastCapturedSpeed;       // rb.velocity.magnitude at rock release
    private Vector3 lastCapturedAimPos;    // aimCircle world position at rock release

    private bool stepHouseViewActive;     // true while a step with useHouseCamera is running
    private bool _forceAdvanceStep;       // set by NotifyTurnComplete() to unstick throw-phase conditions
    
    // Completed tutorials tracking
    private HashSet<string> completedTutorials = new HashSet<string>();
    
    // References
    private GameManager gameManager;
    private CareerManager careerManager;
    
    // Events
    public event Action<TutorialSequence> OnTutorialStart;
    public event Action<TutorialSequence> OnTutorialComplete;
    public event Action<TutorialSequence> OnTutorialSkipped;
    public event Action<TutorialStep, int> OnStepStart;
    public event Action<TutorialStep, int> OnStepComplete;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        careerManager = UnityEngine.Object.FindAnyObjectByType<CareerManager>();
        
        // Load completed tutorials from save data
        LoadCompletedTutorials();
        
        // Check for auto-start tutorials
        if (enableAutoStartTutorials)
        {
            CheckAutoStartTutorials();
        }
    }
    
    private void Update()
    {
        // Skip tutorial on Escape key
        if (Input.GetKeyDown(KeyCode.Escape) && isPlaying && currentSequence != null && currentSequence.canSkip)
        {
            SkipTutorial();
        }
    }
    
    #region Public API
    
    /// <summary>
    /// Play a tutorial sequence by ID
    /// </summary>
    public void PlaySequence(string sequenceId, Action onComplete = null)
    {
        Debug.Log($"[TutorialSequenceManager] PlaySequence called for: {sequenceId}");
        
        if (!enableTutorials)
        {
            Debug.LogWarning($"[TutorialSequenceManager] Tutorials disabled, skipping: {sequenceId}");
            onComplete?.Invoke();
            return;
        }
        
        Debug.Log($"[TutorialSequenceManager] Tutorials enabled, looking up sequence...");
        TutorialSequence sequence = GetSequenceById(sequenceId);
        if (sequence == null)
        {
            Debug.LogWarning($"[TutorialSequenceManager] Tutorial sequence not found: {sequenceId} - Check availableTutorials array!");
            onComplete?.Invoke();
            return;
        }
        
        Debug.Log($"[TutorialSequenceManager] Sequence found: {sequence.name}, calling PlaySequence(sequence)...");
        PlaySequence(sequence, onComplete);
    }
    
    /// <summary>
    /// Play a tutorial sequence
    /// </summary>
    public void PlaySequence(TutorialSequence sequence, Action onComplete = null)
    {
        Debug.Log($"[TutorialSequenceManager] PlaySequence(sequence) called for: {sequence?.sequenceId ?? "NULL"}");
        
        if (!enableTutorials)
        {
            Debug.LogWarning($"[TutorialSequenceManager] Tutorials disabled in settings, skipping: {sequence?.sequenceId}");
            onComplete?.Invoke();
            return;
        }
        
        if (sequence == null || sequence.StepCount == 0)
        {
            Debug.LogWarning($"[TutorialSequenceManager] Null or empty sequence - StepCount: {sequence?.StepCount ?? 0}");
            onComplete?.Invoke();
            return;
        }
        
        Debug.Log($"[TutorialSequenceManager] Checking if tutorial already completed...");
        // Check if already completed
        if (HasCompletedTutorial(sequence.sequenceId))
        {
            Debug.LogWarning($"[TutorialSequenceManager] Tutorial already completed (stored in PlayerPrefs): {sequence.sequenceId}");
            onComplete?.Invoke();
            return;
        }
        
        Debug.Log($"[TutorialSequenceManager] ✓ Tutorial not yet completed, starting sequence: {sequence.sequenceId}");
        
        // Stop current tutorial if playing
        if (isPlaying)
        {
            Debug.Log($"[TutorialSequenceManager] Stopping current tutorial to start new one");
            StopTutorial();
        }
        
        currentSequence = sequence;
        currentStepIndex = 0;
        isPlaying = true;
        
        OnTutorialStart?.Invoke(sequence);
        
        if (skipButton != null)
            skipButton.SetActive(sequence.canSkip);
        
        playCoroutine = StartCoroutine(PlaySequenceCoroutine(onComplete));
    }
    
    /// <summary>
    /// Called at the end of each turn (rock stopped + score checked).
    /// Force-advances any step that is stuck waiting on a throw-phase condition
    /// (RockGrabbed, RockBeingDragged, RockPullbackThreshold, RockReleased).
    /// Steps that fire AFTER the throw (RockStopped, WaitForClick, etc.) are unaffected.
    /// </summary>
    public void NotifyTurnComplete()
    {
        if (!isPlaying) return;
        _forceAdvanceStep = true;
        Debug.Log("[TutorialSequenceManager] NotifyTurnComplete — forcing any stuck throw-phase step to advance");
    }

    /// <summary>
    /// Skip the current tutorial
    /// </summary>
    public void SkipTutorial()
    {
        if (!isPlaying || currentSequence == null)
            return;
        
        if (!currentSequence.canSkip)
        {
            Debug.Log("Current tutorial cannot be skipped");
            return;
        }
        
        Debug.Log($"[TutorialSequenceManager] Skipping tutorial: {currentSequence.sequenceId}");
        
        // Mark tutorial as complete when skipped (so it won't show again)
        MarkTutorialComplete(currentSequence.sequenceId);
        Debug.Log($"[TutorialSequenceManager] Tutorial marked complete after skip: {currentSequence.sequenceId}");
        
        OnTutorialSkipped?.Invoke(currentSequence);
        
        StopTutorial();
    }
    
    /// <summary>
    /// Stop the current tutorial without marking as complete
    /// </summary>
    public void StopTutorial()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
        
        CleanupStep();
        
        isPlaying = false;
        currentSequence = null;
        currentStepIndex = 0;
        
        if (skipButton != null)
            skipButton.SetActive(false);
        
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Skip to a specific step in the current tutorial
    /// </summary>
    public void SkipToStep(int stepIndex)
    {
        if (!isPlaying || currentSequence == null)
        {
            Debug.LogWarning("No tutorial is currently playing");
            return;
        }
        
        if (!currentSequence.IsValidStep(stepIndex))
        {
            Debug.LogWarning($"Invalid step index: {stepIndex}");
            return;
        }
        
        currentStepIndex = stepIndex;
    }
    
    /// <summary>
    /// Check if a tutorial has been completed
    /// </summary>
    public bool HasCompletedTutorial(string sequenceId)
    {
        return completedTutorials.Contains(sequenceId);
    }
    
    /// <summary>
    /// Mark a tutorial as completed (saves to persistent data)
    /// </summary>
    public void MarkTutorialComplete(string sequenceId)
    {
        if (!completedTutorials.Contains(sequenceId))
        {
            completedTutorials.Add(sequenceId);
            SaveCompletedTutorials();
        }
    }
    
    /// <summary>
    /// Enable or disable all tutorials
    /// </summary>
    public void SetTutorialsEnabled(bool enabled)
    {
        enableTutorials = enabled;
        
        if (!enabled && isPlaying)
        {
            StopTutorial();
        }
    }
    
    #endregion
    
    #region Private Methods
    
    private IEnumerator PlaySequenceCoroutine(Action onComplete)
    {
        for (currentStepIndex = 0; currentStepIndex < currentSequence.StepCount; currentStepIndex++)
        {
            TutorialStep step = currentSequence.GetStep(currentStepIndex);

            if (step == null)
            {
                Debug.LogWarning($"Null step at index {currentStepIndex}");
                continue;
            }

            yield return StartCoroutine(PlayStep(step));

            if (!isPlaying)
                yield break;

            // Branching: evaluate the condition and jump to the named success or failure step.
            if (step.branchCondition != TutorialBranchConditionType.None)
            {
                bool passed = EvaluateBranchCondition(step);
                string jumpTo = passed ? step.onSuccessStep : step.onFailureStep;
                if (!string.IsNullOrEmpty(jumpTo))
                {
                    int jumpIdx = FindStepByName(jumpTo);
                    if (jumpIdx >= 0)
                    {
                        // Subtract 1 because the for-loop will increment before the next iteration.
                        currentStepIndex = jumpIdx - 1;
                        Debug.Log($"[TutorialSequenceManager] Branch {(passed ? "SUCCESS" : "FAILURE")} → jumping to '{jumpTo}' (index {jumpIdx})");
                    }
                    else
                    {
                        Debug.LogWarning($"[TutorialSequenceManager] Branch target step '{jumpTo}' not found in sequence — continuing in order");
                    }
                }
            }
        }

        // Tutorial complete
        CompleteTutorial(onComplete);
    }
    
    private IEnumerator PlayStep(TutorialStep step)
    {
        OnStepStart?.Invoke(step, currentStepIndex);
        
        Debug.Log($"[TutorialSequenceManager] === Playing Step: {step.stepName} ===");
        Debug.Log($"[TutorialSequenceManager] Start Condition: {step.startCondition}, End Condition: {step.endCondition}");
        
        // STEP 1: Wait for START condition (if any)
        if (step.startCondition != TutorialConditionType.None)
        {
            Debug.Log($"[TutorialSequenceManager] Waiting for START condition: {step.startCondition}");
            yield return StartCoroutine(WaitForCondition(step, step.startCondition));
            Debug.Log($"[TutorialSequenceManager] START condition met!");
        }
        
        // SPECIAL CASE: For conditions that check dragging state on END condition, check BEFORE showing dialogue
        // If rock already released, skip entire step
        if (step.endCondition == TutorialConditionType.RockBeingDragged || 
            step.endCondition == TutorialConditionType.RockPullbackThreshold)
        {
            if (ShouldSkipDraggingStep(step.rockIndex))
            {
                Debug.Log($"[TutorialSequenceManager] Skipping dragging/pullback step - rock not in dragging state");
                yield break;
            }
        }
        
        // STEP 2: Setup step (spotlight, visuals)
        SetupStep(step);
        
        // STEP 3: Show dialogue if present
        // SPECIAL CASE: For RockPullbackThreshold, dialogue is shown INSIDE the wait function when threshold is reached
        if (step.dialogue != null && step.endCondition != TutorialConditionType.RockPullbackThreshold)
        {
            DialogueController dialogueController = DialogueController.Instance != null
                ? DialogueController.Instance
                : FindAnyObjectByType<DialogueController>();

            if (dialogueController == null)
            {
                Debug.LogError($"[TutorialSequenceManager] Cannot show tutorial step '{step.stepName}' - DialogueController is missing in scene.");
                yield break;
            }

            Debug.Log($"[TutorialSequenceManager] Showing dialogue for step end condition: {step.endCondition}");
            
            // For action-based conditions the player's physical action ends the step,
            // so dialogue must not intercept input or require a click to dismiss.
            if (step.endCondition == TutorialConditionType.RockBeingDragged ||
                step.endCondition == TutorialConditionType.RockGrabbed ||
                step.endCondition == TutorialConditionType.MouseReleased ||
                step.endCondition == TutorialConditionType.RockReleased)
            {
                Debug.Log($"[TutorialSequenceManager] Showing non-blocking dialogue for action-based step");
                dialogueController.Show(step.dialogue, null, nonBlocking: true);
            }
            else
            {
                // Normal dialogue - wait for acknowledgment
                bool dialogueComplete = false;
                dialogueController.Show(step.dialogue, () => dialogueComplete = true);
                yield return new WaitUntil(() => dialogueComplete);
            }
        }
        else if (step.dialogue != null && step.endCondition == TutorialConditionType.RockPullbackThreshold)
        {
            Debug.Log($"[TutorialSequenceManager] Skipping dialogue at setup for RockPullbackThreshold - will show when threshold is reached");
        }
        
        // STEP 4: Wait for END condition (for RockPullbackThreshold, this will show dialogue when threshold is reached)
        if (step.endCondition != TutorialConditionType.None)
        {
            Debug.Log($"[TutorialSequenceManager] Waiting for END condition: {step.endCondition}");
            yield return StartCoroutine(WaitForCondition(step, step.endCondition));
            Debug.Log($"[TutorialSequenceManager] END condition met!");
            
            // Hide dialogue when condition is met (for action-based steps)
            if (DialogueController.Instance != null && DialogueController.Instance.IsShowing)
            {
                DialogueController.Instance.Hide();
                Debug.Log($"[TutorialSequenceManager] Hiding dialogue after condition met");
            }
        }
        
        // STEP 5: Cleanup step
        CleanupStep();
        
        OnStepComplete?.Invoke(step, currentStepIndex);
    }
    
    private void SetupStep(TutorialStep step)
    {
        // Invoke unity events
        step.onStepStart?.Invoke();

        // House camera
        if (step.useHouseCamera)
        {
            CameraManager camMgr = FindAnyObjectByType<CameraManager>();
            if (camMgr != null)
            {
                camMgr.HouseViewOn();
                stepHouseViewActive = true;
            }
        }
        
        // Action-based end conditions require physics to keep running so the player
        // can drag, aim, and release. Rigidbody2D.position only syncs to the transform
        // during a physics step — if timeScale=0, FixedUpdate never runs and the
        // shooting knob/rock position freezes even though Update() still executes.
        bool requiresPlayerInput =
            step.endCondition == TutorialConditionType.RockBeingDragged  ||
            step.endCondition == TutorialConditionType.RockGrabbed        ||
            step.endCondition == TutorialConditionType.MouseReleased      ||
            step.endCondition == TutorialConditionType.RockReleased       ||
            step.endCondition == TutorialConditionType.RockReachedYPosition;

        if (requiresPlayerInput && (step.pauseGame || step.timeScale == 0f))
        {
            // Never freeze physics on interactive steps — the player can't drag/aim/release a frozen rock.
            Time.timeScale = 1f;
            Debug.LogWarning($"[TutorialSequenceManager] Step '{step.stepName}': " +
                             $"pauseGame={step.pauseGame}, timeScale={step.timeScale} would freeze physics " +
                             $"but endCondition={step.endCondition} requires player input — forcing timeScale=1.");
        }
        else
        {
            Time.timeScale = step.timeScale;
            if (step.pauseGame)
                Time.timeScale = 0f;
        }
        
        // Setup spotlight
        if (step.useSpotlight && spotlightOverlay != null && cutoutMask != null)
        {
            spotlightOverlay.SetActive(true);

            // Ensure the spotlight canvas is visible and correctly configured.
            Canvas spotlightCanvas = spotlightOverlay.GetComponent<Canvas>();
            if (spotlightCanvas == null)
                spotlightCanvas = spotlightOverlay.GetComponentInParent<Canvas>(true);
            if (spotlightCanvas != null)
            {
                // The spotlight must render over ALL game cameras (including the aim camera).
                // Screen Space - Camera mode with the UI camera is the only way to guarantee
                // the overlay appears on top regardless of which game camera is active.
                CameraManager camMgr = FindAnyObjectByType<CameraManager>();
                if (camMgr != null && camMgr.ui != null)
                {
                    if (spotlightCanvas.renderMode == RenderMode.WorldSpace ||
                        (spotlightCanvas.renderMode == RenderMode.ScreenSpaceCamera && spotlightCanvas.worldCamera == null))
                    {
                        spotlightCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                        spotlightCanvas.worldCamera = camMgr.ui;
                        spotlightCanvas.planeDistance = 1f;
                        Debug.Log($"[TutorialSequenceManager] Spotlight canvas set to Screen Space - Camera with '{camMgr.ui.name}'");
                    }
                }

                spotlightCanvas.overrideSorting = true;
                spotlightCanvas.sortingOrder = 490;

                // Spotlight must never intercept pointer events — the game needs mouse input during dragging steps.
                CanvasGroup spotlightCG = spotlightOverlay.GetComponent<CanvasGroup>();
                if (spotlightCG == null) spotlightCG = spotlightOverlay.AddComponent<CanvasGroup>();
                spotlightCG.blocksRaycasts = false;

                Debug.Log($"[TutorialSequenceManager] Spotlight canvas: " +
                          $"renderMode={spotlightCanvas.renderMode}, " +
                          $"worldCamera={(spotlightCanvas.worldCamera != null ? spotlightCanvas.worldCamera.name : "NULL")}, " +
                          $"sortingOrder={spotlightCanvas.sortingOrder}, " +
                          $"activeInHierarchy={spotlightOverlay.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("[TutorialSequenceManager] Spotlight overlay has no Canvas component — it will not render. " +
                                 "Add a Canvas component to the spotlightOverlay GameObject or one of its parents.");
            }

            // Hide dialogue background when using spotlight (spotlight has its own background)
            if (DialogueController.Instance != null)
            {
                DialogueController.Instance.SetBackgroundVisible(false);
            }
            
            // Position and size the cutout based on target or manual settings.
            // Call ResolveWorldTarget once so we don't search the scene twice.
            Transform resolvedWorldTarget = ResolveWorldTarget(step);
            bool hasWorldTarget = step.useSpotlightWorldPosition || resolvedWorldTarget != null;

            Vector2? spotlightScreenPos = null;
            if (step.spotlightTarget != null)
            {
                // Auto-position to match target UI element
                cutoutMask.position = step.spotlightTarget.position;
                cutoutMask.sizeDelta = step.spotlightTarget.sizeDelta + Vector2.one * step.spotlightPadding;
                spotlightScreenPos = step.spotlightTarget.position; // world pos == screen pos on Overlay canvas
                Debug.Log($"[TutorialSequenceManager] Spotlight UI target: {step.spotlightTarget.name} at {cutoutMask.position}");
            }
            else if (hasWorldTarget)
            {
                // Project a world-space point through the active camera.
                // useSpotlightWorldPosition uses a fixed Vector3 (e.g. house centre at (0, 6.5, 0)).
                // Otherwise we use the resolved Transform (keyword or GameObject reference).
                // The aim camera (depth > 0) shows a completely different area than the main camera,
                // so we must project through whichever camera is currently rendering that world region.
                Vector3 targetWorldPos = (step.useSpotlightWorldPosition
                    ? step.spotlightWorldPosition
                    : resolvedWorldTarget.position) + step.spotlightWorldOffset;

                CameraManager camMgr2 = FindAnyObjectByType<CameraManager>();
                Camera renderCam = (camMgr2?.aim != null && camMgr2.aim.depth > 0) ? camMgr2.aim : Camera.main;

                if (renderCam != null)
                {
                    Vector3 screenPos = renderCam.WorldToScreenPoint(targetWorldPos);
                    spotlightScreenPos = new Vector2(screenPos.x, screenPos.y);
                    RectTransform parentRect = cutoutMask.parent as RectTransform;
                    Canvas parentCanvas = cutoutMask.GetComponentInParent<Canvas>();
                    Camera canvasCam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                        ? parentCanvas.worldCamera : null;
                    if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        parentRect, new Vector2(screenPos.x, screenPos.y), canvasCam, out Vector2 localPoint))
                    {
                        cutoutMask.anchoredPosition = localPoint;
                    }
                    cutoutMask.sizeDelta = step.manualCutoutSize + Vector2.one * step.spotlightPadding;
                    Debug.Log($"[TutorialSequenceManager] Spotlight world pos {targetWorldPos} -> screen {screenPos} -> anchoredPos {cutoutMask.anchoredPosition}, size: {cutoutMask.sizeDelta}");
                }
                else
                {
                    Debug.LogWarning($"[TutorialSequenceManager] No active camera found for world spotlight!");
                }

                // dynamicSpotlight keeps the cutout re-projecting every frame.
                // Required whenever the camera can pan (e.g. aim camera tracking the house).
                if (step.dynamicSpotlight)
                {
                    if (dynamicSpotlightCoroutine != null)
                        StopCoroutine(dynamicSpotlightCoroutine);
                    dynamicSpotlightCoroutine = StartCoroutine(DynamicSpotlightUpdate(step));
                }
            }
            else
            {
                // Use manual position/size — no screen pos available, dialogue stays at default
                cutoutMask.anchoredPosition = step.manualCutoutPosition;
                cutoutMask.sizeDelta = step.manualCutoutSize;
                Debug.Log($"[TutorialSequenceManager] Spotlight manual position: {step.manualCutoutPosition}, size: {step.manualCutoutSize}");
            }

            // Reposition dialogue to avoid covering the spotlight
            if (spotlightScreenPos.HasValue && DialogueController.Instance != null)
            {
                Vector2 normalized = new Vector2(
                    spotlightScreenPos.Value.x / Screen.width,
                    spotlightScreenPos.Value.y / Screen.height);
                DialogueController.Instance.PositionAroundSpotlight(normalized);
            }
        }
        else if (spotlightOverlay != null)
        {
            // No spotlight for this step
            spotlightOverlay.SetActive(false);
            DialogueController.Instance?.ResetDialoguePosition();

            // Restore dialogue background when not using spotlight
            if (DialogueController.Instance != null)
            {
                DialogueController.Instance.SetBackgroundVisible(true);
            }
        }
        else if (step.useSpotlight)
        {
            Debug.LogWarning($"[TutorialSequenceManager] Step {step.stepName} wants spotlight but spotlightOverlay or cutoutMask is null!");
        }
        
        // Highlight objects
        if (step.objectsToHighlight != null && step.objectsToHighlight.Length > 0)
        {
            currentHighlights = step.objectsToHighlight;
            foreach (var obj in currentHighlights)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
        
        // Show aim circle
        if (step.aimCirclePosition.HasValue && step.aimCirclePrefab != null)
        {
            currentAimCircle = Instantiate(step.aimCirclePrefab);
            currentAimCircle.transform.position = step.aimCirclePosition.Value;
        }
    }
    
    private void CleanupStep()
    {
        // Restore house camera if it was activated for this step
        if (stepHouseViewActive)
        {
            CameraManager camMgr = FindAnyObjectByType<CameraManager>();
            if (camMgr != null)
                camMgr.HouseViewOff();
            stepHouseViewActive = false;
        }

        // Stop dynamic spotlight tracking (no-op if not running)
        if (dynamicSpotlightCoroutine != null)
        {
            StopCoroutine(dynamicSpotlightCoroutine);
            dynamicSpotlightCoroutine = null;
        }

        // Hide spotlight
        if (spotlightOverlay != null)
        {
            spotlightOverlay.SetActive(false);
        }

        // Restore dialogue position and background
        if (DialogueController.Instance != null)
        {
            DialogueController.Instance.ResetDialoguePosition();
            DialogueController.Instance.SetBackgroundVisible(true);
        }
        
        // Hide highlights
        if (currentHighlights != null)
        {
            foreach (var obj in currentHighlights)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
            currentHighlights = null;
        }
        
        // Destroy aim circle
        if (currentAimCircle != null)
        {
            Destroy(currentAimCircle);
            currentAimCircle = null;
        }
        
        // Invoke step end events
        if (currentSequence != null && currentSequence.IsValidStep(currentStepIndex))
        {
            var step = currentSequence.GetStep(currentStepIndex);
            step?.onStepEnd?.Invoke();
        }
    }
    
    /// <summary>
    /// Runs every frame while the spotlight is active, repositioning the cutout to
    /// track a moving world-space target through whichever camera is currently live.
    /// Handles the aim camera (depth > 0) switching in automatically.
    /// </summary>
    private IEnumerator DynamicSpotlightUpdate(TutorialStep step)
    {
        CameraManager camMgr = FindAnyObjectByType<CameraManager>();
        // Fixed world position takes priority over a Transform reference.
        bool useFixedPos = step.useSpotlightWorldPosition;
        Transform target = useFixedPos ? null : ResolveWorldTarget(step);
        RectTransform parentRect = cutoutMask.parent as RectTransform;
        Canvas parentCanvas = cutoutMask.GetComponentInParent<Canvas>();

        while (spotlightOverlay != null && spotlightOverlay.activeInHierarchy)
        {
            // Resolve world position this frame
            Vector3 worldPos;
            if (useFixedPos)
            {
                worldPos = step.spotlightWorldPosition + step.spotlightWorldOffset;
            }
            else if (target != null)
            {
                worldPos = target.position + step.spotlightWorldOffset;
            }
            else
            {
                yield return null;
                continue;
            }

            if (cutoutMask != null)
            {
                // Project through whichever camera is currently rendering that world region.
                // The aim camera (depth > 0) shows the house area; main camera shows the hack.
                Camera renderCam = (camMgr?.aim != null && camMgr.aim.depth > 0) ? camMgr.aim : Camera.main;

                if (renderCam != null)
                {
                    Vector3 screenPos = renderCam.WorldToScreenPoint(worldPos);
                    if (screenPos.z > 0f)
                    {
                        Camera canvasCam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                            ? parentCanvas.worldCamera : null;
                        if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            parentRect, new Vector2(screenPos.x, screenPos.y), canvasCam, out Vector2 localPoint))
                        {
                            cutoutMask.anchoredPosition = localPoint;
                        }
                    }
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Evaluates the step's branch condition synchronously using captured and current game state.
    /// Returns true on success, false on failure.
    /// </summary>
    private bool EvaluateBranchCondition(TutorialStep step)
    {
        switch (step.branchCondition)
        {
            case TutorialBranchConditionType.AimPositionNearTarget:
            case TutorialBranchConditionType.AimPositionFarFromTarget:
            {
                // Use the position captured at the moment of release (aimCircle retains its last
                // position after DrawTrajectory stops). Fall back to current aimCircle if fresh.
                Vector3 aimPos = lastCapturedAimPos;
                TrajectoryLine tLine = FindAnyObjectByType<TrajectoryLine>();
                if (tLine != null && tLine.aimCircle != null)
                    aimPos = tLine.aimCircle.transform.position;

                float dist = Vector3.Distance(aimPos, step.branchTargetPosition);
                bool near = dist <= step.branchThreshold;
                bool pass = step.branchCondition == TutorialBranchConditionType.AimPositionNearTarget ? near : !near;
                Debug.Log($"[TutorialSequenceManager] Branch {step.branchCondition}: aimPos={aimPos}, target={step.branchTargetPosition}, dist={dist:F2}, threshold={step.branchThreshold:F2}, pass={pass}");
                return pass;
            }

            case TutorialBranchConditionType.RockPositionNearTarget:
            {
                if (gameManager?.rockList == null) return false;
                int idx = gameManager.rockCurrent;
                if (idx >= gameManager.rockList.Count) return false;
                GameObject rock = gameManager.rockList[idx].rock;
                if (rock == null) return false;
                float dist = Vector3.Distance(rock.transform.position, step.branchTargetPosition);
                bool pass = dist <= step.branchThreshold;
                Debug.Log($"[TutorialSequenceManager] Branch RockPositionNearTarget: dist={dist:F2}, threshold={step.branchThreshold:F2}, pass={pass}");
                return pass;
            }

            case TutorialBranchConditionType.FlickVelocityAbove:
            {
                bool pass = lastCapturedSpeed > step.branchThreshold;
                Debug.Log($"[TutorialSequenceManager] Branch FlickVelocityAbove: speed={lastCapturedSpeed:F2}, threshold={step.branchThreshold:F2}, pass={pass}");
                return pass;
            }

            case TutorialBranchConditionType.FlickVelocityBelow:
            {
                bool pass = lastCapturedSpeed < step.branchThreshold;
                Debug.Log($"[TutorialSequenceManager] Branch FlickVelocityBelow: speed={lastCapturedSpeed:F2}, threshold={step.branchThreshold:F2}, pass={pass}");
                return pass;
            }

            default:
                return true;
        }
    }

    /// <summary>
    /// Returns the index of the step with the given stepName in the current sequence, or -1 if not found.
    /// </summary>
    private int FindStepByName(string name)
    {
        if (currentSequence == null || string.IsNullOrEmpty(name)) return -1;
        for (int i = 0; i < currentSequence.StepCount; i++)
        {
            TutorialStep s = currentSequence.GetStep(i);
            if (s != null && s.stepName == name) return i;
        }
        return -1;
    }

    private static bool IsThrowPhaseCondition(TutorialConditionType t) =>
        t == TutorialConditionType.RockGrabbed       ||
        t == TutorialConditionType.RockBeingDragged  ||
        t == TutorialConditionType.RockPullbackThreshold ||
        t == TutorialConditionType.RockReleased;

    private IEnumerator WaitForCondition(TutorialStep step, TutorialConditionType conditionType)
    {
        // Run through the interruptible wrapper when:
        //   (a) a fixed timeout is set, OR
        //   (b) this is a throw-phase condition that should clear when NotifyTurnComplete() fires.
        // WaitForSeconds / None manage themselves and are never interrupted.
        bool useWrapper = conditionType == step.endCondition
            && conditionType != TutorialConditionType.WaitForSeconds
            && conditionType != TutorialConditionType.None
            && (step.stepTimeout > 0f || IsThrowPhaseCondition(conditionType));

        if (useWrapper)
        {
            yield return StartCoroutine(WaitForConditionWithTimeout(step, conditionType));
            yield break;
        }

        yield return StartCoroutine(WaitForConditionCore(step, conditionType));
    }

    private IEnumerator WaitForConditionWithTimeout(TutorialStep step, TutorialConditionType conditionType)
    {
        bool conditionMet = false;
        _forceAdvanceStep = false; // clear any stale flag from last turn

        IEnumerator conditionRoutine = WaitForConditionCore(step, conditionType, onComplete: () => conditionMet = true);
        Coroutine running = StartCoroutine(conditionRoutine);

        float timeout = step.stepTimeout > 0f ? step.stepTimeout : float.MaxValue;
        float elapsed = 0f;

        while (!conditionMet && elapsed < timeout)
        {
            // NotifyTurnComplete() was called — only bail on throw-phase conditions
            if (_forceAdvanceStep && IsThrowPhaseCondition(conditionType))
            {
                Debug.Log($"[TutorialSequenceManager] Step '{step.stepName}' force-advanced by NotifyTurnComplete (condition: {conditionType})");
                break;
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!conditionMet)
        {
            StopCoroutine(running);
            if (elapsed >= timeout && step.stepTimeout > 0f)
                Debug.Log($"[TutorialSequenceManager] Step '{step.stepName}' timed out after {step.stepTimeout}s — auto-advancing");
        }

        _forceAdvanceStep = false; // consume the flag
    }

    private IEnumerator WaitForConditionCore(TutorialStep step, TutorialConditionType conditionType, Action onComplete = null)
    {
        switch (conditionType)
        {
            case TutorialConditionType.None:
                break;

            case TutorialConditionType.WaitForInput:
                yield return new WaitUntil(() => Input.anyKeyDown);
                break;

            case TutorialConditionType.WaitForClick:
                yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
                break;

            case TutorialConditionType.WaitForSeconds:
                // WaitForSecondsRealtime is unaffected by Time.timeScale, so this works even when pauseGame=true
                yield return new WaitForSecondsRealtime(step.waitDuration);
                break;

            case TutorialConditionType.MouseReleased:
                yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
                break;

            case TutorialConditionType.CameraStoppedMoving:
                yield return WaitForCameraStop();
                break;

            case TutorialConditionType.RockGrabbed:
                yield return WaitForRockGrabbed(step.rockIndex);
                break;

            case TutorialConditionType.RockBeingDragged:
                yield return WaitForRockBeingDragged(step.rockIndex);
                break;

            case TutorialConditionType.RockPullbackThreshold:
                // Only pass dialogue if this is the END condition (dialogue shows when threshold reached)
                DialogueData dialogueToShow = (conditionType == step.endCondition) ? step.dialogue : null;
                yield return WaitForRockPullbackThreshold(step.rockIndex, step.pullbackThreshold, dialogueToShow);
                break;

            case TutorialConditionType.RockReleased:
                yield return WaitForRockReleased(step.rockIndex);
                break;

            case TutorialConditionType.RockStopped:
                yield return WaitForRockStopped(step.rockIndex);
                break;

            case TutorialConditionType.RockReachedYPosition:
                yield return WaitForRockReachedY(step.rockIndex, step.targetYPosition, step.targetYAbove);
                break;

            case TutorialConditionType.GameStateChange:
                yield return WaitForGameState(step.targetGameState);
                break;

            case TutorialConditionType.CustomCondition:
                // Can be extended with custom condition functions
                Debug.LogWarning("Custom condition not implemented");
                break;
        }

        onComplete?.Invoke();
    }
    
    private IEnumerator WaitForRockReleased(int rockIndex)
    {
        if (gameManager == null)
            yield break;

        int index = rockIndex >= 0 ? rockIndex : gameManager.rockCurrent;

        yield return new WaitUntil(() =>
            gameManager.rockList != null &&
            index < gameManager.rockList.Count &&
            gameManager.rockList[index].rockInfo.released);

        // Capture branch-condition data at the moment of release.
        // Aim position: aimCircle retains its last DrawTrajectory position after the mouse is released.
        TrajectoryLine tLine = FindAnyObjectByType<TrajectoryLine>();
        if (tLine != null && tLine.aimCircle != null)
            lastCapturedAimPos = tLine.aimCircle.transform.position;

        // Release speed: yield one physics frame so the rigidbody has its launch velocity applied.
        yield return new WaitForFixedUpdate();
        if (gameManager.rockList != null && index < gameManager.rockList.Count)
        {
            GameObject rock = gameManager.rockList[index].rock;
            if (rock != null)
            {
                Rigidbody2D rb = rock.GetComponent<Rigidbody2D>();
                lastCapturedSpeed = rb != null ? rb.linearVelocity.magnitude : 0f;
                Debug.Log($"[TutorialSequenceManager] Captured release speed={lastCapturedSpeed:F2}, aimPos={lastCapturedAimPos}");
            }
        }
    }
    
    private IEnumerator WaitForRockGrabbed(int rockIndex)
    {
        if (gameManager == null)
            yield break;
        
        int index = rockIndex >= 0 ? rockIndex : gameManager.rockCurrent;
        
        // Wait for player to actually press/grab the rock (isPressed on Rock_Flick)
        // SpringJoint connectedBody is set at startup so it can't be used as grab signal
        yield return new WaitUntil(() => 
        {
            if (gameManager.rockList == null || index >= gameManager.rockList.Count)
                return false;
            
            GameObject rock = gameManager.rockList[index].rock;
            if (rock == null || !rock.activeInHierarchy)
                return false;
            
            // Primary check: Rock_Flick.isPressed is set true exactly on OnMouseDown
            Rock_Flick flick = rock.GetComponent<Rock_Flick>();
            if (flick != null && flick.enabled && flick.isPressed)
            {
                Debug.Log($"[TutorialSequenceManager] Rock grabbed! Rock_Flick.isPressed=true");
                return true;
            }
            
            // Fallback: mouse button is held and mouse is over the rock collider
            if (Input.GetMouseButton(0))
            {
                Rock_Info ri = gameManager.rockList[index].rockInfo;
                if (ri != null && ri.shotTaken && !ri.released)
                {
                    Debug.Log($"[TutorialSequenceManager] Rock grabbed via shotTaken fallback");
                    return true;
                }
            }
            
            return false;
        });
    }
    
    private IEnumerator WaitForCameraStop()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("[TutorialSequenceManager] Main camera not found, skipping camera stop wait");
            yield break;
        }
        
        const float STOP_THRESHOLD = 0.005f; // Smaller threshold = more strict (was 0.01)
        const int STABLE_FRAMES = 10; // More frames = more strict (was 5)
        
        Vector3 lastPos = mainCam.transform.position;
        int stableFrameCount = 0;
        
        Debug.Log($"[TutorialSequenceManager] Waiting for camera to stop moving... Current pos: {mainCam.transform.position}");
        
        // Wait a minimum of 10 frames to allow camera to start moving if it's going to
        for (int i = 0; i < 10; i++)
        {
            yield return null;
        }
        
        while (stableFrameCount < STABLE_FRAMES)
        {
            Vector3 currentPos = mainCam.transform.position;
            float movement = Vector3.Distance(currentPos, lastPos);
            
            if (movement < STOP_THRESHOLD)
            {
                stableFrameCount++;
                if (stableFrameCount % 3 == 0)
                {
                    Debug.Log($"[TutorialSequenceManager] Camera stable for {stableFrameCount} frames (movement: {movement:F4})");
                }
            }
            else
            {
                if (stableFrameCount > 0)
                {
                    Debug.Log($"[TutorialSequenceManager] Camera moved {movement:F4}, resetting stable count from {stableFrameCount}");
                }
                stableFrameCount = 0; // Reset if camera moved
            }
            
            lastPos = currentPos;
            yield return null;
        }
        
        Debug.Log($"[TutorialSequenceManager] Camera stopped moving! Final pos: {mainCam.transform.position}");
    }
    
    private IEnumerator WaitForRockBeingDragged(int rockIndex)
    {
        if (gameManager == null)
            yield break;
        
        int index = rockIndex >= 0 ? rockIndex : gameManager.rockCurrent;
        
        // Check if rock is valid
        if (gameManager.rockList == null || index >= gameManager.rockList.Count)
        {
            Debug.Log($"[TutorialSequenceManager] Invalid rock index for 'being dragged' condition");
            yield break;
        }
        
        GameObject rock = gameManager.rockList[index].rock;
        if (rock == null)
        {
            Debug.Log($"[TutorialSequenceManager] Rock object null for index {index}");
            yield break;
        }
        
        Debug.Log($"[TutorialSequenceManager] Waiting for rock to be grabbed/dragged...");
        yield return new WaitUntil(() =>
        {
            if (rock == null || !rock.activeInHierarchy)
                return false;

            Rock_Flick flick = rock.GetComponent<Rock_Flick>();
            if (flick != null && flick.isPressed)
            {
                Debug.Log($"[TutorialSequenceManager] Detected rock being dragged via Rock_Flick.isPressed");
                return true;
            }

            SpringJoint2D spring = rock.GetComponent<SpringJoint2D>();
            if (spring != null && spring.enabled && spring.connectedBody != null)
            {
                Debug.Log($"[TutorialSequenceManager] Detected rock being dragged via active spring joint");
                return true;
            }

            return false;
        });

        Debug.Log($"[TutorialSequenceManager] Rock drag started!");
    }
    
    private IEnumerator WaitForRockPullbackThreshold(int rockIndex, float threshold, DialogueData dialogue)
    {
        if (gameManager == null)
            yield break;
        
        int index = rockIndex >= 0 ? rockIndex : gameManager.rockCurrent;
        
        // Check if rock is valid
        if (gameManager.rockList == null || index >= gameManager.rockList.Count)
        {
            Debug.Log($"[TutorialSequenceManager] Invalid rock index for pullback threshold condition");
            yield break;
        }
        
        var rockGO = gameManager.rockList[index].rock;
        var rockInfo = gameManager.rockList[index].rockInfo;
        Transform launcherTransform = gameManager.launcher;
        
        if (launcherTransform == null || rockGO == null)
        {
            Debug.LogWarning($"[TutorialSequenceManager] Launcher or rock missing - can't check pullback threshold");
            yield break;
        }
        
        // CRITICAL: Wait for rock to be grabbed (clicked/pressed) FIRST
        // The pullback threshold only matters if the player is actually dragging
        Debug.Log($"[TutorialSequenceManager] Waiting for rock to be grabbed before checking pullback...");
        yield return new WaitUntil(() => 
        {
            if (rockGO == null || !rockGO.activeInHierarchy)
                return false;
            
            Rock_Flick flick = rockGO.GetComponent<Rock_Flick>();
            return flick != null && flick.isPressed;
        });
        
        Debug.Log($"[TutorialSequenceManager] Rock grabbed! Now measuring pullback distance...");
        
        // Now rock is being grabbed - measure the pullback distance from launcher
        Vector2 launcherPos = new Vector2(launcherTransform.position.x, launcherTransform.position.y);
        Vector2 baselineRockPos = new Vector2(rockGO.transform.position.x, rockGO.transform.position.y);
        float baselineDistance = Vector2.Distance(launcherPos, baselineRockPos);
        float maxDistance = baselineDistance; // Track max distance reached
        
        Debug.Log($"[TutorialSequenceManager] Baseline distance from launcher: {baselineDistance:F3}. Need to pull {threshold:F3} units further to reach threshold.");
        
        bool thresholdReached = false;
        int frameCount = 0;
        
        // Measure pullback while rock is still being held
        while (rockInfo != null && !rockInfo.released)
        {
            if (rockGO == null || !rockGO.activeInHierarchy)
            {
                Debug.LogWarning($"[TutorialSequenceManager] Rock became inactive during pullback measurement");
                yield break;
            }
            
            // Recalculate current distance
            Vector2 currentRockPos = new Vector2(rockGO.transform.position.x, rockGO.transform.position.y);
            float currentDistance = Vector2.Distance(launcherPos, currentRockPos);
            float distancePulledBack = currentDistance - baselineDistance;
            
            // Track maximum distance for logging
            if (currentDistance > maxDistance)
                maxDistance = currentDistance;
            
            // Log every 15 frames
            if (frameCount % 15 == 0)
            {
                Debug.Log($"[TutorialSequenceManager] Pullback: {distancePulledBack:F3} / {threshold:F3} (current distance: {currentDistance:F3}, baseline: {baselineDistance:F3})");
            }
            
            // Check if pulled back far enough
            if (distancePulledBack >= threshold && !thresholdReached)
            {
                thresholdReached = true;
                Debug.Log($"[TutorialSequenceManager] ✓ Pullback threshold reached! Pulled {distancePulledBack:F3} >= {threshold:F3}");
                
                // Show dialogue when threshold is reached
                if (dialogue != null && DialogueController.Instance != null)
                {
                    Debug.Log($"[TutorialSequenceManager] Showing dialogue: {dialogue.dialogueLines[0]}");
                    DialogueController.Instance.Show(dialogue, null);
                }
            }
            
            frameCount++;
            yield return null;
        }
        
        // Rock was released
        if (!thresholdReached)
        {
            Debug.Log($"[TutorialSequenceManager] ✗ Released before threshold. Max pullback was: {maxDistance - baselineDistance:F3}, needed: {threshold:F3}");
            yield break;
        }
        
        Debug.Log($"[TutorialSequenceManager] Rock released after threshold met! Max distance reached: {maxDistance:F3}");
    }
    
    /// <summary>
    /// Check if we should skip the "being dragged" step
    /// Returns true if rock is not currently being dragged (either not grabbed yet or already released)
    /// </summary>
    private bool ShouldSkipDraggingStep(int rockIndex)
    {
        if (gameManager == null)
        {
            Debug.Log($"[TutorialSequenceManager] GameManager null, skipping dragging step");
            return true;
        }
        
        int index = rockIndex >= 0 ? rockIndex : gameManager.rockCurrent;
        
        // Check if rock is valid
        if (gameManager.rockList == null || index >= gameManager.rockList.Count)
        {
            Debug.Log($"[TutorialSequenceManager] Invalid rock index {index}, skipping dragging step");
            return true;
        }
        
        var rockInfo = gameManager.rockList[index].rockInfo;
        GameObject rock = gameManager.rockList[index].rock;
        bool isDragging = false;

        if (rock != null)
        {
            Rock_Flick flick = rock.GetComponent<Rock_Flick>();
            if (flick != null)
            {
                isDragging = flick.isPressed;
            }
            else
            {
                SpringJoint2D spring = rock.GetComponent<SpringJoint2D>();
                isDragging = spring != null && spring.enabled && spring.connectedBody != null;
            }
        }

        Debug.Log($"[TutorialSequenceManager] ShouldSkipDraggingStep check: shotTaken={rockInfo.shotTaken}, released={rockInfo.released}, isDragging={isDragging}");
        
        // Skip if already released
        if (rockInfo.released)
        {
            Debug.Log($"[TutorialSequenceManager] Rock already released - SKIP");
            return true;
        }
        
        // Don't skip simply because the rock isn't being dragged yet.
        // Drag steps should wait for the player to start dragging, not abort immediately.
        if (!isDragging)
        {
            Debug.Log($"[TutorialSequenceManager] Rock is not currently being dragged - will wait for drag instead of skipping");
            return false;
        }
        
        Debug.Log($"[TutorialSequenceManager] Rock IS being dragged - DON'T SKIP");
        return false;
    }
    
    private IEnumerator WaitForRockStopped(int rockIndex)
    {
        if (gameManager == null)
            yield break;
        
        int index = rockIndex >= 0 ? rockIndex : gameManager.rockCurrent;
        
        yield return new WaitUntil(() => 
            gameManager.rockList != null && 
            index < gameManager.rockList.Count &&
            gameManager.rockList[index].rockInfo.rest);
    }
    
    /// <summary>
    /// Resolves the world-space Transform to spotlight for a step.
    /// Priority: spotlightWorldTarget (direct ref) > spotlightTargetName (by name) > spotlightTargetTag (by tag).
    /// Returns null if none resolve — SetupStep falls through to manual positioning.
    /// </summary>
    private Transform ResolveWorldTarget(TutorialStep step)
    {
        if (step.spotlightWorldTarget != null)
            return step.spotlightWorldTarget;

        if (!string.IsNullOrEmpty(step.spotlightTargetName))
        {
            // Reserved keywords for runtime objects that can't be found by static name
            switch (step.spotlightTargetName)
            {
                case "$currentRock":
                    if (gameManager?.rockList != null && gameManager.rockCurrent < gameManager.rockList.Count)
                        return gameManager.rockList[gameManager.rockCurrent].rock?.transform;
                    Debug.LogWarning("[TutorialSequenceManager] $currentRock: rock not available");
                    return null;
                case "$shooter":
                    if (gameManager?.shooterGO != null)
                        return gameManager.shooterGO.transform;
                    Debug.LogWarning("[TutorialSequenceManager] $shooter: shooterGO is null");
                    return null;
                case "$launcher":
                    GameObject launcher = GameObject.FindWithTag("Launcher");
                    if (launcher != null) return launcher.transform;
                    Debug.LogWarning("[TutorialSequenceManager] $launcher: no object tagged 'Launcher'");
                    return null;
                case "$aimTarget":
                    TrajectoryLine tLine = FindAnyObjectByType<TrajectoryLine>();
                    if (tLine != null && tLine.aimCircle != null) return tLine.aimCircle.transform;
                    Debug.LogWarning("[TutorialSequenceManager] $aimTarget: TrajectoryLine or aimCircle not found");
                    return null;
            }

            GameObject found = GameObject.Find(step.spotlightTargetName);
            if (found != null)
            {
                Debug.Log($"[TutorialSequenceManager] Resolved spotlight by name: '{step.spotlightTargetName}'");
                return found.transform;
            }
            Debug.LogWarning($"[TutorialSequenceManager] spotlightTargetName '{step.spotlightTargetName}' not found in scene");
        }

        if (!string.IsNullOrEmpty(step.spotlightTargetTag))
        {
            GameObject found = GameObject.FindWithTag(step.spotlightTargetTag);
            if (found != null)
            {
                Debug.Log($"[TutorialSequenceManager] Resolved spotlight by tag: '{step.spotlightTargetTag}'");
                return found.transform;
            }
            Debug.LogWarning($"[TutorialSequenceManager] spotlightTargetTag '{step.spotlightTargetTag}' not found in scene");
        }

        return null;
    }

    private IEnumerator WaitForRockReachedY(int rockIndex, float targetY, bool waitUntilAbove)
    {
        if (gameManager == null)
            yield break;

        int index = rockIndex >= 0 ? rockIndex : gameManager.rockCurrent;

        Debug.Log($"[TutorialSequenceManager] Waiting for rock {index} to reach Y {(waitUntilAbove ? ">=" : "<=")} {targetY}");

        yield return new WaitUntil(() =>
        {
            if (gameManager.rockList == null || index >= gameManager.rockList.Count)
                return false;
            GameObject rock = gameManager.rockList[index].rock;
            if (rock == null || !rock.activeInHierarchy)
                return false;
            float y = rock.transform.position.y;
            return waitUntilAbove ? y >= targetY : y <= targetY;
        });

        Debug.Log($"[TutorialSequenceManager] Rock {index} reached Y target {targetY}");
    }

    private IEnumerator WaitForGameState(GameState targetState)
    {
        if (gameManager == null)
            yield break;

        yield return new WaitUntil(() => gameManager.state == targetState);
    }
    
    private void CompleteTutorial(Action onComplete)
    {
        TutorialSequence completedSequence = currentSequence;

        if (currentSequence != null)
        {
            MarkTutorialComplete(currentSequence.sequenceId);
            OnTutorialComplete?.Invoke(currentSequence);
        }

        if (skipButton != null)
            skipButton.SetActive(false);

        Time.timeScale = 1f;
        isPlaying = false;

        onComplete?.Invoke();

        currentSequence = null;
        currentStepIndex = 0;

        if (completedSequence != null && !string.IsNullOrEmpty(completedSequence.chainSequenceId))
        {
            Debug.Log($"[TutorialSequenceManager] Chaining to '{completedSequence.chainSequenceId}' after '{completedSequence.sequenceId}'");
            PlaySequence(completedSequence.chainSequenceId);
        }
    }
    
    private TutorialSequence GetSequenceById(string sequenceId)
    {
        if (availableTutorials == null)
            return null;
        
        foreach (var tutorial in availableTutorials)
        {
            if (tutorial != null && tutorial.sequenceId == sequenceId)
                return tutorial;
        }
        
        return null;
    }
    
    private void CheckAutoStartTutorials()
    {
        if (!enableTutorials || availableTutorials == null)
            return;

        // Don't auto-fire tutorials when loading/continuing a career game.
        // gsp.tutorial=true means this is explicitly tutorial/practice mode;
        // gsp.loadGame=true means we're resuming a saved career game.
        GameSettingsPersist gsp = UnityEngine.Object.FindAnyObjectByType<GameSettingsPersist>();
        if (gsp != null && !gsp.tutorial && gsp.loadGame)
        {
            Debug.Log("[TutorialSequenceManager] Skipping auto-start tutorials: career game load in progress (loadGame=true)");
            return;
        }
        
        foreach (var tutorial in availableTutorials)
        {
            if (tutorial == null || !tutorial.autoStart)
                continue;
            
            if (HasCompletedTutorial(tutorial.sequenceId))
                continue;
            
            if (tutorial.autoStartCondition != null && tutorial.autoStartCondition.Evaluate(careerManager))
            {
                PlaySequence(tutorial);
                break; // Only play one auto-start tutorial at a time
            }
        }
    }
    
    private void LoadCompletedTutorials()
    {
        // Load completed tutorials from PlayerPrefs (standalone tutorial mode supported)
        string saved = PlayerPrefs.GetString("CompletedTutorials", "");
        if (!string.IsNullOrEmpty(saved))
        {
            string[] tutorials = saved.Split(',');
            completedTutorials = new HashSet<string>(tutorials);
        }
    }
    
    private void SaveCompletedTutorials()
    {
        string[] tutorials = new string[completedTutorials.Count];
        completedTutorials.CopyTo(tutorials);
        string saved = string.Join(",", tutorials);
        PlayerPrefs.SetString("CompletedTutorials", saved);
        PlayerPrefs.Save();
    }
    
    #endregion
}
