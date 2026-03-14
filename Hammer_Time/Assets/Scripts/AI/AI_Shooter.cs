using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Shooter : MonoBehaviour
{
    public GameManager gm;
    public TutorialManager tm;
    public RockManager rm;

    public AIManager aim;
    public AI_Target aiTarg;
    public AI_Strategy aiStrat;
    public AI_Sweeper aiSweep;

    Rock_Info rockInfo;
    Rock_Flick rockFlick;
    Rigidbody2D rockRB;
    
    int currentRockNumber;

    public Vector2 centreGuard;
    public Vector2 tightCentreGuard;
    public Vector2 highCentreGuard;

    public Vector2 leftHighCornerGuard;
    public Vector2 leftTightCornerGuard;
    public Vector2 leftCornerGuard;
    public Vector2 rightHighCornerGuard;
    public Vector2 rightTightCornerGuard;
    public Vector2 rightCornerGuard;

    public Vector2 topTwelveFoot;
    public Vector2 backTwelveFoot;
    public Vector2 leftTwelveFoot;
    public Vector2 rightTwelveFoot;

    public Vector2 backFourFoot;
    public Vector2 topFourFoot;
    public Vector2 leftFourFoot;
    public Vector2 rightFourFoot;
    public Vector2 button;

    public Vector2 peel;
    public Vector2 takeOut;
    public Vector2 raise;
    public Vector2 tick;


    public Vector2 guardAccu;
    public Vector2 drawAccu;
    public Vector2 toAccu;
    public Vector2 tickAccu;

    public float takeOutOffset;
    public float peelOffset;
    public float raiseOffset;
    public float tickOffset;

    float targetX;
    float targetY;
    public float takeOutX;
    public float takeOutY;
    float raiseY;
    GameSettingsPersist gsp;

    public void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
    }


    public void OnShot(string aiShotType, int rockCurrent)
    {
        rockInfo = gm.rockList[rockCurrent].rockInfo;
        rockFlick = gm.rockList[rockCurrent].rock.GetComponent<Rock_Flick>();
        rockRB = gm.rockList[rockCurrent].rock.GetComponent<Rigidbody2D>();
        currentRockNumber = rockCurrent;

        // CRITICAL FIX: Lock flipAxis IMMEDIATELY to prevent RockManager from overriding!
        // AI_Target has already set rm.inturn, so use that value NOW
        GameObject rock = gm.rockList[currentRockNumber].rock;
        Rock_Force rockForce = rock.GetComponent<Rock_Force>();
        if (rockForce != null)
        {
            rockForce.flipAxis = rm.inturn;
            Debug.Log($"[AI_Shooter.OnShot] LOCKED flipAxis = {rm.inturn} immediately for {aiShotType}");
        }

        StartCoroutine(Shot(aiShotType, rm.inturn));
    }

    IEnumerator Shot(string aiShotType, bool inturn)
    {
        Debug.Log("AI Shot " + aiShotType);
        gm.dbText.text = aiShotType;
        rockFlick.isPressedAI = true;
        takeOutX = aiTarg.takeOutX;
        takeOutY = aiTarg.takeOutY;

        // REMOVED: aiSweep.OnSweep() - Legacy sweeping system is disabled
        // Physics-based sweeping is started AFTER rock is released (see below)
        
        // CRITICAL FIX: Set BOTH flipAxis AND rm.inturn to keep everything synchronized!
        GameObject rock = gm.rockList[currentRockNumber].rock;
        Rock_Force rockForce = rock.GetComponent<Rock_Force>();
        if (rockForce != null)
        {
            rockForce.flipAxis = inturn;
            rm.inturn = inturn;
            Debug.Log($"[AI_Shooter] Set flipAxis AND rm.inturn = {inturn} for {aiShotType}");
        }

        yield return new WaitForSeconds(0.5f);

        
        
        // UNIFIED SHOT HANDLING: All physics-based shots use the same logic
        // AI_Target has already calculated the exact pullback position WITH accuracy error applied
        // Just use that position directly - no need for shot-specific handling!
        
        float shotX;
        float shotY;
        
        // Check if physics calculation succeeded (non-zero position)
        if (takeOutX != 0f || takeOutY != 0f)
        {
            // SUCCESS: Use physics-calculated position (accuracy error already applied by AI_Target)
            shotX = takeOutX;
            shotY = takeOutY;
            
            Debug.Log($"[AI_Shooter] {aiShotType} - Using physics position: ({shotX:F3}, {shotY:F3})");
        }
        else
        {
            // FALLBACK: Physics failed, weight to button with accuracy error
            // This should rarely happen - indicates a problem with AI_Target
            Debug.LogWarning($"[AI_Shooter] {aiShotType} FALLBACK - No physics position available!");
            
            CharacterStats stats = GetShooterStats();
            float accuracy = stats != null ? stats.weightAccuracy.GetValue() : 70f;
            Vector2 error = GetAccuracyError(accuracy, 0.15f);
            
            shotX = button.x + error.x;
            shotY = button.y + error.y;
        }
        
        // Execute shot: Set position and trigger release
        rockFlick.rb.isKinematic = true;
        rockRB.position = new Vector2(shotX, shotY);
        
        Debug.Log($"[AI_Shooter] {aiShotType} final position: ({rockRB.position.x:F3}, {rockRB.position.y:F3})");
        
        yield return new WaitForFixedUpdate();
        rockFlick.mouseUp = true;

        // Wait for rock to actually be released and have velocity
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // Start AI sweeping via AI_Sweeper's new physics-based system
        if (gm != null && rm != null && aiSweep.sm != null)
        {
            // Get ACTUAL velocity (includes accuracy errors from pullback offset)
            Vector2 actualVelocity = rockRB.linearVelocity;
            
            // Get PERFECT velocity (before accuracy errors) from AI_Target
            Vector2 perfectVelocity = aiTarg.lastPerfectVelocity;
            
            // SAFETY: If no perfect velocity stored, use actual velocity (degraded mode)
            if (perfectVelocity == Vector2.zero)
            {
                Debug.LogWarning("[AI_Shooter] No perfect velocity stored - using actual velocity (sweepers won't correct errors!)");
                perfectVelocity = actualVelocity;
            }
            
            Vector2 targetPosition = aiTarg.targetPos;
            bool isInTurn = inturn;
            
            // Calculate launch error for diagnostics
            float launchError = (actualVelocity - perfectVelocity).magnitude;
            float angleError = Vector2.Angle(actualVelocity, perfectVelocity);

            Debug.Log($"[AI_Shooter] Starting physics-based sweeping:");
            Debug.Log($"  Perfect velocity: {perfectVelocity.magnitude:F2} m/s @ {Mathf.Atan2(perfectVelocity.y, perfectVelocity.x) * Mathf.Rad2Deg:F1}° (ideal target)");
            Debug.Log($"  Actual velocity: {actualVelocity.magnitude:F2} m/s @ {Mathf.Atan2(actualVelocity.y, actualVelocity.x) * Mathf.Rad2Deg:F1}° (includes errors)");
            Debug.Log($"  Launch error: {launchError:F3} m/s ({angleError:F2}° off-angle)");
            Debug.Log($"  Target: {targetPosition}, Turn: {(isInTurn ? "IN" : "OUT")}");

            aiSweep.StartPhysicsBasedSweeping(rockRB, actualVelocity, perfectVelocity, isInTurn, targetPosition, aiShotType, currentRockNumber);
        }
    }


    /// <summary>
    /// Get character stats for the current shooter
    /// </summary>
    private CharacterStats GetShooterStats()
    {
        TeamManager tm = FindObjectOfType<TeamManager>();
        if (tm == null) return null;
        
        int memberIndex = currentRockNumber / 4;
        memberIndex = Mathf.Clamp(memberIndex, 0, 3);
        
        bool isRedTeam = (currentRockNumber % 2 == 0) ? gm.redHammer : !gm.redHammer;
        
        if (isRedTeam && tm.teamRed != null && memberIndex < tm.teamRed.Length)
            return tm.teamRed[memberIndex].charStats;
        else if (!isRedTeam && tm.teamYellow != null && memberIndex < tm.teamYellow.Length)
            return tm.teamYellow[memberIndex].charStats;
        
        return null;
    }
    
    /// <summary>
    /// Apply character-based accuracy using realistic distribution
    /// Returns error offset to add to target position
    /// </summary>
    private Vector2 GetAccuracyError(float accuracy, float baseMaxError)
    {
        // Convert accuracy from 0-100 to 0-1 scale
        float accuracyRatio = Mathf.Clamp01(accuracy / 100f);
        
        // Calculate max error based on accuracy (better accuracy = less error)
        float maxError = baseMaxError * (1f - accuracyRatio);
        
        // Use circular distribution for natural shot spread
        return Random.insideUnitCircle * maxError;
    }
}
