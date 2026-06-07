using UnityEngine;

/// <summary>
/// Helper that dynamically assigns world targets to tutorial steps.
/// This is needed because rocks and shooters are created at runtime.
/// </summary>
public class TutorialDynamicTargets : MonoBehaviour
{
    private GameManager gameManager;
    
    void OnEnable()
    {
        if (TutorialSequenceManager.Instance != null)
        {
            TutorialSequenceManager.Instance.OnStepStart += OnTutorialStepStart;
        }
    }
    
    void OnDisable()
    {
        if (TutorialSequenceManager.Instance != null)
        {
            TutorialSequenceManager.Instance.OnStepStart -= OnTutorialStepStart;
        }
    }
    
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("[TutorialDynamicTargets] GameManager not found!");
        }
    }
    
    void OnTutorialStepStart(TutorialStep step, int stepIndex)
    {
        if (gameManager == null) return;
        
        // Check if this step needs dynamic world targets
        if (!step.useSpotlight) return;
        
        // Assign rock as spotlight target if step name contains "rock" or "grab"
        if (step.stepName.ToLower().Contains("rock") || step.stepName.ToLower().Contains("grab"))
        {
            AssignCurrentRock(step);
        }
        
        // Assign shooter as spotlight target if step name contains "shoot" or "sweeper"
        if (step.stepName.ToLower().Contains("shooter") || step.stepName.ToLower().Contains("sweeper"))
        {
            AssignCurrentShooter(step);
        }
        
        // Assign opponent rock as spotlight target if step name contains "target" or "identify"
        if (step.stepName.ToLower().Contains("target") || step.stepName.ToLower().Contains("identify"))
        {
            AssignOpponentRock(step);
        }
    }
    
    private void AssignCurrentRock(TutorialStep step)
    {
        if (gameManager.rockList == null || gameManager.rockCurrent >= gameManager.rockList.Count)
        {
            Debug.LogWarning($"[TutorialDynamicTargets] Cannot assign rock - rockList invalid or rockCurrent out of range");
            return;
        }
        
        GameObject rock = gameManager.rockList[gameManager.rockCurrent].rock;
        if (rock != null)
        {
            step.spotlightWorldTarget = rock.transform;
            Debug.Log($"[TutorialDynamicTargets] Assigned rock spotlight target: {rock.name}");
        }
    }
    
    private void AssignCurrentShooter(TutorialStep step)
    {
        if (gameManager.shooterGO != null)
        {
            step.spotlightWorldTarget = gameManager.shooterGO.transform;
            Debug.Log($"[TutorialDynamicTargets] Assigned shooter spotlight target: {gameManager.shooterGO.name}");
        }
        else
        {
            Debug.LogWarning($"[TutorialDynamicTargets] Cannot assign shooter - shooterGO is null");
        }
    }
    
    private void AssignOpponentRock(TutorialStep step)
    {
        // For tutorial game, highlight a pre-placed opponent rock
        // Rock index 2 is the center guard (opponent's 2nd rock at -0.13, 2.274)
        if (gameManager.rockList == null || gameManager.rockList.Count < 3)
        {
            Debug.LogWarning($"[TutorialDynamicTargets] Cannot assign opponent rock - rockList too small");
            return;
        }
        
        GameObject opponentRock = gameManager.rockList[2].rock; // Center guard
        if (opponentRock != null)
        {
            step.spotlightWorldTarget = opponentRock.transform;
            Debug.Log($"[TutorialDynamicTargets] Assigned opponent rock spotlight target: {opponentRock.name}");
        }
        else
        {
            Debug.LogWarning($"[TutorialDynamicTargets] Opponent rock at index 2 is null");
        }
    }
}
