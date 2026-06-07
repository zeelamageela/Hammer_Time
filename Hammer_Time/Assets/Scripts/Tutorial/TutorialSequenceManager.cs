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
            {
                // Tutorial was stopped/skipped
                yield break;
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
            
            // For action-based conditions, show dialogue but DON'T wait for acknowledgment
            // Let the player's action complete the step
            if (step.endCondition == TutorialConditionType.RockBeingDragged || 
                step.endCondition == TutorialConditionType.RockGrabbed)
            {
                Debug.Log($"[TutorialSequenceManager] Showing non-blocking dialogue for action-based step");
                dialogueController.Show(step.dialogue, null);
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
        
        // Set time scale
        Time.timeScale = step.timeScale;
        
        // Set pause state
        if (step.pauseGame)
            Time.timeScale = 0f;
        
        // Setup spotlight
        if (step.useSpotlight && spotlightOverlay != null && cutoutMask != null)
        {
            spotlightOverlay.SetActive(true);
            
            // Hide dialogue background when using spotlight (spotlight has its own background)
            if (DialogueController.Instance != null)
            {
                DialogueController.Instance.SetBackgroundVisible(false);
            }
            
            // Position and size the cutout based on target or manual settings
            if (step.spotlightTarget != null)
            {
                // Auto-position to match target UI element
                cutoutMask.position = step.spotlightTarget.position;
                cutoutMask.sizeDelta = step.spotlightTarget.sizeDelta + Vector2.one * step.spotlightPadding;
                Debug.Log($"[TutorialSequenceManager] Spotlight UI target: {step.spotlightTarget.name} at {cutoutMask.position}");
            }
            else if (step.spotlightWorldTarget != null)
            {
                // Auto-position to match world object (convert world to screen space)
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    Vector3 screenPos = mainCam.WorldToScreenPoint(step.spotlightWorldTarget.position);
                    cutoutMask.position = screenPos;
                    cutoutMask.sizeDelta = step.manualCutoutSize + Vector2.one * step.spotlightPadding;
                    Debug.Log($"[TutorialSequenceManager] Spotlight world target: {step.spotlightWorldTarget.name} at world {step.spotlightWorldTarget.position} -> screen {screenPos}, size: {cutoutMask.sizeDelta}");
                }
                else
                {
                    Debug.LogWarning($"[TutorialSequenceManager] Main camera not found for world spotlight!");
                }
            }
            else
            {
                // Use manual position/size
                cutoutMask.anchoredPosition = step.manualCutoutPosition;
                cutoutMask.sizeDelta = step.manualCutoutSize;
                Debug.Log($"[TutorialSequenceManager] Spotlight manual position: {step.manualCutoutPosition}, size: {step.manualCutoutSize}");
            }
        }
        else if (spotlightOverlay != null)
        {
            // No spotlight for this step
            spotlightOverlay.SetActive(false);
            
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
        // Hide spotlight
        if (spotlightOverlay != null)
        {
            spotlightOverlay.SetActive(false);
        }
        
        // Restore dialogue background when cleaning up
        if (DialogueController.Instance != null)
        {
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
    
    private IEnumerator WaitForCondition(TutorialStep step, TutorialConditionType conditionType)
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
                yield return new WaitForSeconds(step.waitDuration);
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
                
            case TutorialConditionType.GameStateChange:
                yield return WaitForGameState(step.targetGameState);
                break;
                
            case TutorialConditionType.CustomCondition:
                // Can be extended with custom condition functions
                Debug.LogWarning("Custom condition not implemented");
                break;
        }
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
    
    private IEnumerator WaitForGameState(GameState targetState)
    {
        if (gameManager == null)
            yield break;
        
        yield return new WaitUntil(() => gameManager.state == targetState);
    }
    
    private void CompleteTutorial(Action onComplete)
    {
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
