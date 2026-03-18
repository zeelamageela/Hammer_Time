using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Sweeper : MonoBehaviour
{
    public AIManager aim;
    public GameManager gm;
    public SweeperManager sm;

    private void Start()
    {
        // Initialize AI trajectory visualization system
        InitializeTrajectoryVisualization();
    }
    
    /// <summary>
    /// Entry point for sweeping decisions
    /// LEGACY SYSTEM DISABLED - All AI sweeping now uses physics-based trajectory monitoring
    /// </summary>
    public void OnSweep(bool aiTurn, string shotType, Vector2 target, bool inturn)
    {
        if (aiTurn)
        {
            // LEGACY SYSTEM DISABLED
            // Physics-based sweeping is now handled by StartPhysicsBasedSweeping()
            // Called directly from AI_Shooter after rock is released
            Debug.Log("[AI_Sweeper] OnSweep called - legacy system DISABLED, using physics-based sweeping");
        }
        else
        {
            // Player callouts still use legacy system
            StartCoroutine(PlayerSpeed(shotType, target, inturn));
        }
    }
    
    /// <summary>
    /// Start physics-based sweeping monitor
    /// Predicts trajectory and makes intelligent sweeping decisions in real-time
    /// 
    /// CRITICAL: Uses IDEAL (pre-error) velocity to determine where rock SHOULD go
    /// Sweepers correct deviations from ideal path caused by accuracy errors
    /// </summary>
    /// <param name="rockRB">Rigidbody of the rock being swept</param>
    /// <param name="actualVelocity">ACTUAL velocity rock was launched with (includes accuracy errors)</param>
    /// <param name="idealVelocity">IDEAL velocity from physics calculation (NO accuracy errors) - sweepers aim for this trajectory</param>
    /// <param name="isInTurn">Turn direction</param>
    /// <param name="targetPosition">Final target position</param>
    /// <param name="shotType">Type of shot being played</param>
    /// <param name="currentRockNumber">Index of current rock</param>
    public void StartPhysicsBasedSweeping(Rigidbody2D rockRB, Vector2 actualVelocity, Vector2 idealVelocity, bool isInTurn, Vector2 targetPosition, string shotType, int currentRockNumber)
    {
        StartCoroutine(MonitorAndSweepCoroutine(rockRB, actualVelocity, idealVelocity, isInTurn, targetPosition, shotType, currentRockNumber));
    }
    /// <summary>
    /// LEGACY HARD-CODED SWEEPING SYSTEM - DISABLED
    /// This method contained shot-specific velocity checkpoints that don't adapt to actual trajectory
    /// Replaced by physics-based MonitorAndSweepCoroutine
    /// </summary>
    IEnumerator TargetShot(string aiShotType, Vector2 target, bool inturn)
    {
        // LEGACY SYSTEM DISABLED - DO NOT USE
        Debug.LogWarning("[AI_Sweeper] TargetShot called but DISABLED - use StartPhysicsBasedSweeping instead");
        yield break;
        
        // OLD CODE BELOW - KEPT FOR REFERENCE ONLY
        /*
        Rigidbody2D rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();
        GameObject rock = gm.rockList[gm.rockCurrent].rock;

        Debug.Log("Auto Sweep - " + aiShotType);

        switch (aiShotType)
        {
            #region Centre Guards
            case "Centre Guard":
            case "Tight Centre Guard":
            case "High Centre Guard":
                // PHYSICS-BASED GUARDS: Sweeping handled by Guard To Target callback
                // These legacy names are intercepted and redirected, so this case is never actually reached
                // But kept for safety - just pass through to Guard To Target logic
                Debug.Log("[AI_Sweeper] Legacy finesse name detected, should have been redirected. Passing through.");
                goto case "Guard To Target";
            #endregion

            #region Corner Guards
            case "Left Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.75f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.4f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                break;

            case "Left Tight Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 4.25f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.75f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.25f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                break;

            case "Left High Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.3f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1.95f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                break;

            case "Right Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.75f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.4f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                break;

            case "Right Tight Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 4.25f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.75f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.25f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                break;

            case "Right High Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.3f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1.85f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                break;
            #endregion

            #region Twelve Foot Draws
            case "Top Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 4.58f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.04f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1.85f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.48f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.46f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1.25f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.44f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.42f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.34f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.32f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                sm.SweepWhoa(true);
                break;

            case "Left Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 4.9f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.3f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -1.62f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= -0.47f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -1.63f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= -0.74f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -1.55f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= -0.96f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                //yield return new WaitUntil(() => Mathf.Abs(rock.transform.position.x) >= 0.05f);
                //sm.SweepWhoa(true);

                break;

            case "Back Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 5.5f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 4f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.8f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.71f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.7f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1.55f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.65f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.63f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 1f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.57f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.55f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 6.5f);
                Debug.Log("y = 6.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 6.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.44f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.41f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 7.75f);
                sm.SweepWhoa(true);
                break;

            case "Right Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 4.9f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.3f)
                    sm.SweepWeight(true);
                else if (!inturn && rock.transform.position.x <= 1.62f)
                    sm.SweepRight(true);
                else if (inturn && rock.transform.position.x >= 0.47f)
                    sm.SweepLeft(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (!inturn && rock.transform.position.x <= 1.63f)
                    sm.SweepRight(true);
                else if (inturn && rock.transform.position.x >= 0.74f)
                    sm.SweepLeft(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (!inturn && rock.transform.position.x <= 1.55f)
                    sm.SweepRight(true);
                else if (inturn && rock.transform.position.x >= 0.96f)
                    sm.SweepLeft(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 6.5f);
                sm.SweepWhoa(true);

                break;
            #endregion

            #region Four Foot Draws
            case "Button":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 4.9f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.3f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.65f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.65f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.55f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.55f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.4f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.4f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 6.5f);
                sm.SweepWhoa(true);

                break;

            case "Left Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 4.9f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.3f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -1f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.33f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.95f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.18f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.83f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => Mathf.Abs(0.37f - rock.transform.position.x) >= 0.1f);
                sm.SweepWhoa(true);

                break;

            case "Right Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 4.9f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.3f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.36f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 1f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.2f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.92f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.3f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.8f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);

                break;

            case "Top Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 4.75f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.3f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.2f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.65f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.65f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.85f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.5f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.5f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.4f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.35f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.35f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                break;

            case "Back Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 5f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 3.6f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 2.4f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.69f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.66f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.59f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.57f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.linearVelocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.48f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.45f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                //yield return new WaitUntil(() => Mathf.Abs(rock.transform.position.x) >= 0.05f);
                //sm.SweepWhoa(true);

                break;
            #endregion

            #region Take Outs
            case "Peel":
                // DISABLED FOR PHYSICS-BASED SHOTS
                Debug.Log("[AI_Sweeper] Sweeping disabled for physics-based Peel shot");
                yield return new WaitUntil(() => rock.transform.position.y >= 7.75f);
                sm.SweepWhoa(true);
                break;

            case "Take Out":
                // DISABLED FOR PHYSICS-BASED SHOTS
                // Physics-based targeting calculates exact velocity needed
                // Sweeping interferes with the predicted trajectory
                Debug.Log("[AI_Sweeper] Sweeping disabled for physics-based Take Out shot");
                yield return new WaitUntil(() => rock.transform.position.y >= 7.75f);
                sm.SweepWhoa(true);
                break;

            case "Tick":
                // DISABLED FOR PHYSICS-BASED SHOTS
                Debug.Log("[AI_Sweeper] Sweeping disabled for physics-based Tick shot");
                yield return new WaitUntil(() => rock.transform.position.y >= 7.75f);
                sm.SweepWhoa(true);
                break;

            case "Raise":
                // DISABLED FOR PHYSICS-BASED SHOTS
                Debug.Log("[AI_Sweeper] Sweeping disabled for physics-based Raise shot");
                yield return new WaitUntil(() => rock.transform.position.y >= 7.75f);
                sm.SweepWhoa(true);
                break;
            #endregion

            case "Draw To Target":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                float velLimit = ((5.5f - 4.58f) * ((target.y - 5.225f) / 2.55f)) + 4.58f;
                Debug.Log("y = -7 velLimit is " + velLimit);
                if (rockRB.linearVelocity.y <= velLimit)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                velLimit = ((4f - 3f) * ((target.y - 5.225f) / 2.55f)) + 3f;
                if (rockRB.linearVelocity.y <= velLimit)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                velLimit = ((2.8f - 1.85f) * ((target.y - 5.225f) / 2.55f)) + 1.85f;
                if (rockRB.linearVelocity.y <= velLimit)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= target.x - 0.65f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= target.x + 0.65f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                velLimit = ((1.55f - 1.25f) * ((target.y - 5.225f) / 2.55f)) + 1.25f;
                if (rockRB.linearVelocity.y <= velLimit)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= target.x - 0.55f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= target.x + 0.55f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                velLimit = ((1f - 0.5f) * ((target.y - 5.225f) / 2.55f)) + 0.5f;
                if (rockRB.linearVelocity.y <= velLimit)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= target.x - 0.4f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= target.x + 0.4f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= target.x);
                velLimit = 0.5f * ((target.y - 5.225f) / 2.55f);
                if (rockRB.linearVelocity.y >= velLimit)
                    sm.SweepWhoa(true);

                break;

            case "Guard To Target":
                // PHYSICS-BASED GUARD: Light sweeping to help it reach finesse zone (Y=2.5-4.5)
                // Target velocity should be ~50-70% of button velocity
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                if (rockRB.linearVelocity.y <= 3.5f)  // Guard needs less speed than draws
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                if (rockRB.linearVelocity.y <= 2.2f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                if (rockRB.linearVelocity.y <= 1.2f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                if (rockRB.linearVelocity.y <= 0.6f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                
                // Guards should stop in finesse zone (Y=2.5-4.5), so whoa at Y=2.5
                yield return new WaitUntil(() => rock.transform.position.y >= 2.5f);
                Debug.Log("y = 2.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                sm.SweepWhoa(true);  // STOP! We're in finesse zone
                break;

            default:
                break;
        }
        */
    }

    IEnumerator PlayerSpeed(string playerShotType, Vector2 target, bool inturn)
    {
        Rigidbody2D rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();
        GameObject rock = gm.rockList[gm.rockCurrent].rock;

        Debug.Log("sweeperL is " + sm.sweeperL.gameObject.activeSelf);
        //fltText.TargetTransform = rock.transform;

        Debug.Log("Player Speed Callouts - " + playerShotType);

        switch (playerShotType)
        {
            case "Draw To Target":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                float velLimit = ((5.5f - 4.58f) * ((target.y - 5.225f) / 2.55f)) + 4.58f;
                Debug.Log("y = -7 velLimit is " + velLimit);
                if (rockRB.linearVelocity.y <= velLimit)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "Sweep!!");
                else
                    TextCalloutManager.Instance.ShowRockCallout(rock, "Leave it!!");
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                velLimit = ((4f - 3f) * ((target.y - 5.225f) / 2.55f)) + 3f;
                if (rockRB.linearVelocity.y <= velLimit)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "SWEEP!!");
                else
                    TextCalloutManager.Instance.ShowRockCallout(rock, "Nope!!");
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                velLimit = ((2.8f - 1.85f) * ((target.y - 5.225f) / 2.55f)) + 1.85f;
                if (rockRB.linearVelocity.y <= velLimit)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "SWEEEEEP!!");
                else if (inturn && rock.transform.position.x <= target.x - 0.65f)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "Sweep the Curl!!");
                else if (!inturn && rock.transform.position.x >= target.x + 0.65f)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "Sweep the Curl!!");
                else
                    TextCalloutManager.Instance.ShowRockCallout(rock, "Leave it!!");
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                velLimit = ((1.55f - 1.25f) * ((target.y - 5.225f) / 2.55f)) + 1.25f;
                if (rockRB.linearVelocity.y <= velLimit)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "SWEEEEEEEEEP HARD!!");
                else if (inturn && rock.transform.position.x <= target.x - 0.55f)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "Sweep the Curl!!");
                else if (!inturn && rock.transform.position.x >= target.x + 0.55f)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "Sweep the Curl!!");
                else
                    TextCalloutManager.Instance.ShowRockCallout(rock, "It's good!!");
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                velLimit = ((1f - 0.5f) * ((target.y - 5.225f) / 2.55f)) + 0.5f;
                if (rockRB.linearVelocity.y <= velLimit)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "HARRRRRD!! GO GO GO!!!");
                else if (inturn && rock.transform.position.x <= target.x - 0.4f)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "Sweep the Curl!!");
                else if (!inturn && rock.transform.position.x >= target.x + 0.4f)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "Sweep the Curl!!");
                else
                    TextCalloutManager.Instance.ShowRockCallout(rock, "It's good!!");
                yield return new WaitUntil(() => rock.transform.position.y >= target.x);
                velLimit = 0.5f * ((target.y - 5.225f) / 2.55f);
                if (rockRB.linearVelocity.y >= velLimit)
                    TextCalloutManager.Instance.ShowRockCallout(rock, "We're there!!");
                break;

            case "Guard To Target":

                break;

            default:
                break;
        }

        //fltText.Play(rockRB.position);
    }
    
    // ========== AI TRAJECTORY VISUALIZATION (DEBUG) ==========
    // Visual debugging system to see ideal vs actual trajectories
    
    private LineRenderer idealTrajectoryLine;     // Green line: IDEAL path (perfect physics, what sweepers aim for)
    private LineRenderer actualTrajectoryLine;    // Red line: ACTUAL path (error-contaminated, what rock got)
    private LineRenderer currentPositionMarker;   // Yellow crosshair: Current rock position on ideal path
    
    private GameObject idealTrajectoryLineObj;
    private GameObject actualTrajectoryLineObj;
    private GameObject currentPositionMarkerObj;
    
    public bool showAITrajectoryDebug = false;     // Toggle in inspector to enable/disable visualization (DISABLED - causing performance issues with limited trajectory points)
    
    /// <summary>
    /// Initialize visual debugging system for AI trajectories
    /// </summary>
    private void InitializeTrajectoryVisualization()
    {
        // GREEN LINE: IDEAL trajectory (perfect physics - what sweepers want)
        idealTrajectoryLineObj = new GameObject("AI_IdealTrajectory");
        idealTrajectoryLineObj.transform.parent = transform;
        idealTrajectoryLine = idealTrajectoryLineObj.AddComponent<LineRenderer>();
        idealTrajectoryLine.startWidth = 0.08f;
        idealTrajectoryLine.endWidth = 0.08f;
        idealTrajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        idealTrajectoryLine.startColor = new Color(0f, 1f, 0f, 0.7f); // Bright green, semi-transparent
        idealTrajectoryLine.endColor = new Color(0f, 1f, 0f, 0.7f);
        
        // ✅ CRITICAL FIX: Set sorting layer to ensure visibility above background!
        idealTrajectoryLine.sortingLayerName = "UI";  // Use UI layer (highest priority)
        idealTrajectoryLine.sortingOrder = 100;  // Very high order to render on top
        idealTrajectoryLine.enabled = false;
        
        // RED LINE: ACTUAL trajectory (error-contaminated - what rock actually got)
        actualTrajectoryLineObj = new GameObject("AI_ActualTrajectory");
        actualTrajectoryLineObj.transform.parent = transform;
        actualTrajectoryLine = actualTrajectoryLineObj.AddComponent<LineRenderer>();
        actualTrajectoryLine.startWidth = 0.08f;
        actualTrajectoryLine.endWidth = 0.08f;
        actualTrajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        actualTrajectoryLine.startColor = new Color(1f, 0f, 0f, 0.7f); // Bright red, semi-transparent
        actualTrajectoryLine.endColor = new Color(1f, 0f, 0f, 0.7f);
        
        // ✅ CRITICAL FIX: Set sorting layer to ensure visibility above background!
        actualTrajectoryLine.sortingLayerName = "UI";  // Use UI layer (highest priority)
        actualTrajectoryLine.sortingOrder = 100;  // Very high order to render on top
        actualTrajectoryLine.enabled = false;
        
        // YELLOW CROSSHAIR: Current rock position on ideal path
        currentPositionMarkerObj = new GameObject("AI_CurrentPositionMarker");
        currentPositionMarkerObj.transform.parent = transform;
        currentPositionMarker = currentPositionMarkerObj.AddComponent<LineRenderer>();
        currentPositionMarker.startWidth = 0.12f;
        currentPositionMarker.endWidth = 0.12f;
        currentPositionMarker.material = new Material(Shader.Find("Sprites/Default"));
        currentPositionMarker.startColor = new Color(1f, 1f, 0f, 1f); // Bright yellow, full opacity
        currentPositionMarker.endColor = new Color(1f, 1f, 0f, 1f);
        
        // ✅ CRITICAL FIX: Set sorting layer to ensure visibility above background!
        currentPositionMarker.sortingLayerName = "UI";  // Use UI layer (highest priority)
        currentPositionMarker.sortingOrder = 101;  // Even higher to render on top of trajectories
        currentPositionMarker.enabled = false;
        
        Debug.Log("[AI_Sweeper] Trajectory visualization initialized - Green=IDEAL, Red=ACTUAL, Yellow=CURRENT (UI layer, order 100+)");
    }
    
    /// <summary>
    /// Update visual debugging: show ideal trajectory, actual trajectory, and current position
    /// </summary>
    private void UpdateTrajectoryVisualization(List<Vector2> idealPath, List<Vector2> actualPath, Vector2 currentPosition, Vector2 idealPosAtCurrentY)
    {
        if (!showAITrajectoryDebug || idealPath == null || actualPath == null)
        {
            // Hide all visualization
            if (idealTrajectoryLine != null) idealTrajectoryLine.enabled = false;
            if (actualTrajectoryLine != null) actualTrajectoryLine.enabled = false;
            if (currentPositionMarker != null) currentPositionMarker.enabled = false;
            return;
        }
        
        Debug.Log($"[Trajectory Viz] Updating visualization - Ideal: {idealPath.Count} points, Actual: {actualPath.Count} points");
        
        // GREEN LINE: IDEAL trajectory (perfect physics)
        if (idealTrajectoryLine != null && idealPath.Count > 0)
        {
            idealTrajectoryLine.enabled = true;
            idealTrajectoryLine.positionCount = idealPath.Count;
            for (int i = 0; i < idealPath.Count; i++)
            {
                idealTrajectoryLine.SetPosition(i, new Vector3(idealPath[i].x, idealPath[i].y, 0f));
            }
            Debug.Log($"[Trajectory Viz] GREEN line rendered with {idealPath.Count} points");
        }
        
        // RED LINE: ACTUAL trajectory (error-contaminated)
        if (actualTrajectoryLine != null && actualPath.Count > 0)
        {
            actualTrajectoryLine.enabled = true;
            actualTrajectoryLine.positionCount = actualPath.Count;
            for (int i = 0; i < actualPath.Count; i++)
            {
                actualTrajectoryLine.SetPosition(i, new Vector3(actualPath[i].x, actualPath[i].y, 0f));
            }
            Debug.Log($"[Trajectory Viz] RED line rendered with {actualPath.Count} points");
        }
        
        // YELLOW CROSSHAIR: Show current position and ideal position (small crosshair)
        if (currentPositionMarker != null && idealPosAtCurrentY != Vector2.zero)
        {
            currentPositionMarker.enabled = true;
            
            // Draw a small crosshair: vertical and horizontal lines intersecting at ideal position
            float crosshairSize = 0.3f; // 30cm crosshair
            
            // 4 points: left, center, right, center, up, center, down
            // This creates a + shape
            currentPositionMarker.positionCount = 5;
            
            // Horizontal line (left to right through ideal position)
            currentPositionMarker.SetPosition(0, new Vector3(idealPosAtCurrentY.x - crosshairSize, idealPosAtCurrentY.y, 0f)); // Left
            currentPositionMarker.SetPosition(1, new Vector3(idealPosAtCurrentY.x, idealPosAtCurrentY.y, 0f));                   // Center
            currentPositionMarker.SetPosition(2, new Vector3(idealPosAtCurrentY.x + crosshairSize, idealPosAtCurrentY.y, 0f)); // Right
            currentPositionMarker.SetPosition(3, new Vector3(idealPosAtCurrentY.x, idealPosAtCurrentY.y, 0f));                   // Back to center
            currentPositionMarker.SetPosition(4, new Vector3(idealPosAtCurrentY.x, idealPosAtCurrentY.y + crosshairSize, 0f)); // Up
            // Note: This creates a connected line - for a proper crosshair you'd need 2 separate lines
            // But this is simpler and still shows the position clearly
            
            Debug.Log($"[Trajectory Viz] YELLOW crosshair at ({idealPosAtCurrentY.x:F2}, {idealPosAtCurrentY.y:F2})");
        }
        else if (currentPositionMarker != null)
        {
            currentPositionMarker.enabled = false;
        }
    }
    
    /// <summary>
    /// Clear trajectory visualization when sweeping ends
    /// </summary>
    private void ClearTrajectoryVisualization()
    {
        if (idealTrajectoryLine != null) idealTrajectoryLine.enabled = false;
        if (actualTrajectoryLine != null) actualTrajectoryLine.enabled = false;
        if (currentPositionMarker != null) currentPositionMarker.enabled = false;
    }
    
    // ========== PHYSICS-BASED SWEEPING SYSTEM ==========
    // New intelligent sweeping that predicts trajectory and corrects deviations in real-time
    
    /// <summary>
    /// Monitor rock position vs predicted trajectory and make sweeping decisions
    /// PHILOSOPHY:
    /// 1. Predict IDEAL trajectory from perfect velocity (no accuracy errors)
    /// 2. Compare actual rock position to ideal path
    /// 3. Priority: Collision avoidance > Distance > Line accuracy
    /// 4. Post-collision: Scoring position > Cover behind rocks
    /// 5. Sculpt rock back toward IDEAL trajectory to correct accuracy errors
    /// 
    /// CRITICAL DISTINCTION:
    /// - actualVelocity: Rock's REAL launch velocity (includes shooter accuracy errors)
    /// - idealVelocity: PERFECT physics-calculated velocity (what sweepers aim for)
    /// - Sweepers CORRECT the difference between actual and ideal paths!
    /// </summary>
    private IEnumerator MonitorAndSweepCoroutine(Rigidbody2D rockRB, Vector2 actualVelocity, Vector2 idealVelocity, bool isInTurn, Vector2 targetPosition, string shotType, int currentRockNumber)
    {
        GameObject rock = gm.rockList[currentRockNumber].rock;
        if (rock == null)
        {
            Debug.LogWarning("[AI_Sweeper] No active rock found!");
            yield break;
        }

        Rock_Info rockInfo = rock.GetComponent<Rock_Info>();
        bool isOpponentRock = false; // AI rocks are never opponent rocks
        bool hasCollided = false; // Track if rock has hit another rock
        sm.sweepSel.gameObject.SetActive(true);

        // Get trajectory simulator
        TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
        TrajectorySimulator trajectorySimulator = null;

        if (playerTrajectory != null)
        {
            trajectorySimulator = new TrajectorySimulator(
                playerTrajectory.iceFriction,
                playerTrajectory.curlStrength
            );
        }
        else
        {
            Debug.LogWarning("[AI_Sweeper] TrajectoryLine not found!");
            yield break;
        }

        // ========================================
        // CONTEXT-AWARE SWEEPING PARAMETERS
        // ========================================
        // Different shot types need different sweeping strategies!
        
        bool isTakeoutShot = (shotType == "Take Out" || shotType == "Peel" || shotType == "Runback" || shotType == "Tick");
        bool isDrawShot = (shotType == "Draw To Target" || shotType == "Guard To Target");
        bool isRaiseShot = (shotType == "Raise" || shotType == "Tap Back");

        // ========================================
        // GENERATE TWO TRAJECTORIES:
        // 1. IDEAL trajectory (from perfect physics calculation - what we WANT)
        // 2. ACTUAL trajectory (from error-contaminated launch - what we GOT)
        // ========================================
        Vector2 launcherPos = new Vector2(0f, -25f);
        List<GameObject> rocksInPlay = new List<GameObject>();
        
        // CRITICAL FIX: For takeout shots, EXCLUDE the target rock from collision detection!
        // We WANT to hit it, so it shouldn't be treated as an obstacle to avoid
        Vector2 targetRockPos = targetPosition; // Approximate target rock position
        float targetRockRadius = 0.5f; // Collision detection radius
        
        foreach (var rockEntry in gm.rockList)
        {
            if (rockEntry.rock == null || !rockEntry.rock.activeInHierarchy || !rockEntry.rockInfo.inPlay)
                continue;
            
            if (rockEntry.rock == rock)
                continue; // Skip self
            
            // For TAKEOUT shots: Skip rocks near the target position (they're what we're trying to hit!)
            if (isTakeoutShot)
            {
                Vector2 otherRockPos = rockEntry.rock.transform.position;
                float distToTarget = Vector2.Distance(otherRockPos, targetRockPos);
                
                if (distToTarget < targetRockRadius)
                {
                    Debug.Log($"[AI_Sweeper] TAKEOUT: Excluding target rock {rockEntry.rock.name} from collision detection (we want to hit it!)");
                    continue; // Skip target rock - we WANT to collide with it!
                }
            }
            
            rocksInPlay.Add(rockEntry.rock);
        }
        
        Debug.Log($"[AI_Sweeper] Rocks in play for collision detection: {rocksInPlay.Count} (excluding self{(isTakeoutShot ? " and target" : "")})");



        // IDEAL TRAJECTORY: Perfect physics calculation (NO accuracy errors)
        // This is what sweepers will try to ACHIEVE by correcting errors
        List<Vector2> idealTrajectory = trajectorySimulator.SimulateTrajectory(
            launcherPos,
            idealVelocity,  // ? PERFECT velocity from physics (before errors)
            isInTurn,
            250,
            rocksInPlay,
            forPlayerPreview: false
        );
        
        // ACTUAL TRAJECTORY: Error-contaminated launch (what rock actually got)
        // Used for collision detection and real-time prediction
        List<Vector2> actualTrajectory = trajectorySimulator.SimulateTrajectory(
            launcherPos,
            actualVelocity,  // ? ACTUAL velocity (includes accuracy errors)
            isInTurn,
            250,
            rocksInPlay,
            forPlayerPreview: false
        );

        Debug.Log($"[AI_Sweeper] Monitoring started:");
        Debug.Log($"  Shot type: {shotType}");
        Debug.Log($"  IDEAL trajectory (sweeping target): {idealTrajectory.Count} points from perfect velocity {idealVelocity}");
        Debug.Log($"  ACTUAL trajectory (error-contaminated): {actualTrajectory.Count} points from actual velocity {actualVelocity}");
        Debug.Log($"  Launch error: {(actualVelocity - idealVelocity).magnitude:F3} m/s ({Vector2.Angle(actualVelocity, idealVelocity):F2}°)");
        
        // TRAJECTORY VERIFICATION: Log first/last points to verify it's not a straight line
        if (idealTrajectory.Count > 10)
        {
            Debug.Log($"[Trajectory Verification] IDEAL path sample:");
            Debug.Log($"  Start: ({idealTrajectory[0].x:F2}, {idealTrajectory[0].y:F2})");
            Debug.Log($"  25%: ({idealTrajectory[idealTrajectory.Count / 4].x:F2}, {idealTrajectory[idealTrajectory.Count / 4].y:F2})");
            Debug.Log($"  50%: ({idealTrajectory[idealTrajectory.Count / 2].x:F2}, {idealTrajectory[idealTrajectory.Count / 2].y:F2})");
            Debug.Log($"  75%: ({idealTrajectory[idealTrajectory.Count * 3 / 4].x:F2}, {idealTrajectory[idealTrajectory.Count * 3 / 4].y:F2})");
            Debug.Log($"  End: ({idealTrajectory[idealTrajectory.Count - 1].x:F2}, {idealTrajectory[idealTrajectory.Count - 1].y:F2})");
        }
        if (actualTrajectory.Count > 10)
        {
            Debug.Log($"[Trajectory Verification] ACTUAL path sample:");
            Debug.Log($"  Start: ({actualTrajectory[0].x:F2}, {actualTrajectory[0].y:F2})");
            Debug.Log($"  25%: ({actualTrajectory[actualTrajectory.Count / 4].x:F2}, {actualTrajectory[actualTrajectory.Count / 4].y:F2})");
            Debug.Log($"  50%: ({actualTrajectory[actualTrajectory.Count / 2].x:F2}, {actualTrajectory[actualTrajectory.Count / 2].y:F2})");
            Debug.Log($"  75%: ({actualTrajectory[actualTrajectory.Count * 3 / 4].x:F2}, {actualTrajectory[actualTrajectory.Count * 3 / 4].y:F2})");
            Debug.Log($"  End: ({actualTrajectory[actualTrajectory.Count - 1].x:F2}, {actualTrajectory[actualTrajectory.Count - 1].y:F2})");
        }
        
        // CRITICAL: Check collision info to see if simulation stopped early
        TrajectorySimulator.CollisionInfo idealCollisionInfo = trajectorySimulator.GetCollisionInfo();
        if (idealCollisionInfo.hasCollision)
        {
            Debug.LogWarning($"[Trajectory Verification] IDEAL trajectory has collision at {idealCollisionInfo.collisionPoint} (index {idealCollisionInfo.collisionIndex})");
        }
        
        // Check actual trajectory collision (this might be WHY it's short!)
        List<Vector2> actualTrajectoryCheck = trajectorySimulator.SimulateTrajectory(
            launcherPos,
            actualVelocity,
            isInTurn,
            250,
            rocksInPlay,
            forPlayerPreview: false
        );
        TrajectorySimulator.CollisionInfo actualCollisionInfo = trajectorySimulator.GetCollisionInfo();
        if (actualCollisionInfo.hasCollision)
        {
            Debug.LogWarning($"[Trajectory Verification] ACTUAL trajectory has collision at {actualCollisionInfo.collisionPoint} (index {actualCollisionInfo.collisionIndex}) - THIS IS WHY IT'S SHORT!");
        }
        else if (actualTrajectory.Count < 100)
        {
            Debug.LogError($"[Trajectory Verification] ACTUAL trajectory only has {actualTrajectory.Count} points but NO COLLISION! Velocity might have hit zero prematurely!");
        }

        // Wait until rock crosses hog line (Y > -16.15)
        while (rock.transform.position.y < -16.15f)
        {
            yield return new WaitForFixedUpdate();
        }

        Debug.Log($"[AI_Sweeper] Rock crossed hog line - sweeping enabled!");

        // ========================================
        // ENABLE AI TRAJECTORY VISUALIZATION
        // ========================================
        // Show IDEAL (green) and ACTUAL (red) trajectories for debugging
        UpdateTrajectoryVisualization(idealTrajectory, actualTrajectory, Vector2.zero, Vector2.zero);

        // ========================================
        // CALCULATE ACTUAL SWEEPING GOAL
        // ========================================
        // CRITICAL: For TAKEOUT shots, the goal is to reach the COLLISION POINT,
        // not the target rock's center position!
        // Collision happens when rock centers are ~0.58 units apart (2 × rock radius)
        
        Vector2 sweepingGoal;
        
        if (isTakeoutShot)
        {
            // TAKEOUT: Goal is the collision point (before reaching target center)
            // Approach from behind, collision happens when centers are 0.58 units apart
            float rockRadius = 0.145f; // Half of rock diameter (~0.29m)
            float twoRockRadii = rockRadius * 2.0f; // Two radii (0.29m)
            
            // Calculate collision point: target position minus collision distance
            Vector2 approachDirection = (targetPosition - launcherPos).normalized;
            sweepingGoal = targetPosition - (approachDirection * twoRockRadii);
            
            Debug.Log($"[AI_Sweeper] TAKEOUT sweeping goal: COLLISION POINT at ({sweepingGoal.x:F2}, {sweepingGoal.y:F2})");
            Debug.Log($"  Target rock center: ({targetPosition.x:F2}, {targetPosition.y:F2})");
            Debug.Log($"  Collision distance: {twoRockRadii:F3}m (2 × rock radius)");
            Debug.Log($"  Goal is {(targetPosition.y - sweepingGoal.y):F3}m BEFORE target center");
        }
        else
        {
            // DRAW/GUARD/RAISE: Goal is the exact target position (final resting spot)
            sweepingGoal = targetPosition;
            
            Debug.Log($"[AI_Sweeper] {shotType} sweeping goal: TARGET POSITION at ({sweepingGoal.x:F2}, {sweepingGoal.y:F2})");
        }

        // Shot type flags already declared above before trajectory generation
        // (isTakeoutShot, isDrawShot, isRaiseShot)
        
        // TAKEOUT SHOTS: Need VELOCITY! Sweep aggressively for weight
        // DRAW SHOTS: Need PRECISION! Sweep carefully for line and distance
        // RAISE SHOTS: Need CONTROL! Gentle sweeping only
        
        float lateralErrorThreshold;
        float distanceErrorThreshold;
        float predictionLookahead;
        
        if (isTakeoutShot)
        {
            // TAKEOUTS: ULTRA-AGGRESSIVE parameters with ULTRA-TIGHT line control
            // - MASSIVE lookahead (8.0 units!) to detect velocity drops SUPER early
            // - VERY sensitive to distance errors (0.10 = 10cm) - even tiny shortfalls trigger sweeping
            // - ULTRA-TIGHT lateral tolerance (0.01 = 1cm) - HYPER-SENSITIVE line correction! (was 6cm)
            lateralErrorThreshold = 0.01f;  // 1cm lateral (ULTRA-TIGHT - 33% more sensitive!)
            distanceErrorThreshold = 0.10f;  // 10cm distance (ULTRA sensitive - sweep early!)
            predictionLookahead = 15.0f;      // Look 15 units ahead (MASSIVE - detect problems WAY ahead!)
            
            Debug.Log($"[AI_Sweeper] TAKEOUT MODE: ULTRA-AGGRESSIVE with ULTRA-TIGHT line control!");
            Debug.Log($"  Lookahead: {predictionLookahead}m (MASSIVE - detect velocity drops SUPER early!)");
            Debug.Log($"  Distance threshold: {distanceErrorThreshold}m (ULTRA sensitive - must reach!)");
            Debug.Log($"  Lateral threshold: {lateralErrorThreshold}m (ULTRA-TIGHT - 33% more sensitive!)");
        }
        else if (isDrawShot)
        {
            // DRAWS: PRECISION parameters with ULTRA-TIGHT line control
            // - Medium lookahead (4.0 units) to balance correction time
            // - Moderate distance sensitivity (0.20 = 20cm) - stopping is critical
            // - ULTRA-TIGHT lateral tolerance (0.035 = 3.5cm) - HYPER-PRECISE line accuracy! (was 5cm)
            lateralErrorThreshold = 0.035f;   // 3.5cm lateral (ULTRA-TIGHT - 30% more sensitive!)
            distanceErrorThreshold = 0.20f;  // 20cm distance (important but less critical than takeouts)
            predictionLookahead = 4.0f;      // Look 4 units ahead (balanced)
            
            Debug.Log($"[AI_Sweeper] DRAW MODE: Precision line/distance control with ULTRA-TIGHT thresholds!");
            Debug.Log($"  Lookahead: {predictionLookahead}m (balanced prediction)");
            Debug.Log($"  Distance threshold: {distanceErrorThreshold}m (stopping control)");
            Debug.Log($"  Lateral threshold: {lateralErrorThreshold}m (HYPER-PRECISE!)");
        }
        else if (isRaiseShot)
        {
            // RAISES: GENTLE parameters
            // - Short lookahead (3.0 units) to avoid over-correction
            // - Relaxed thresholds (raises are light contact, less precision needed)
            lateralErrorThreshold = 0.15f;   // 15cm lateral (relaxed - light contact)
            distanceErrorThreshold = 0.30f;  // 30cm distance (relaxed - just need contact)
            predictionLookahead = 3.0f;      // Look 3 units ahead (short - avoid over-sweeping)
            
            Debug.Log($"[AI_Sweeper] RAISE MODE: Gentle sweeping for light contact");
        }
        else
        {
            // DEFAULT: Balanced parameters
            lateralErrorThreshold = 0.12f;   // 12cm lateral
            distanceErrorThreshold = 0.25f;  // 25cm distance
            predictionLookahead = 3.5f;      // Look 3.5 units ahead
            
            Debug.Log($"[AI_Sweeper] DEFAULT MODE: Standard sweeping parameters");
        }
        
        bool collisionImminent = false;
        float collisionDistance = float.MaxValue;
        Vector2 collisionPoint = Vector2.zero;

        string currentSweepState = "None";

        // Monitor rock until it stops
        while (rockInfo != null && !rockInfo.stopped && rockRB.linearVelocity.magnitude > 0.01f)
        {
            Vector2 currentPos = rock.transform.position;

            // Find where rock SHOULD be on IDEAL trajectory at this Y coordinate
            // This is the PERFECT path (no accuracy errors) that sweepers aim to achieve
            Vector2 idealPosAtCurrentY = GetPredictedPositionAtY(idealTrajectory, currentPos.y);
            Vector2 idealPosAhead = GetPredictedPositionAtY(idealTrajectory, currentPos.y + predictionLookahead);

            // Calculate deviations from ideal trajectory
            float lateralError = currentPos.x - idealPosAtCurrentY.x;
            float distanceToGoal = sweepingGoal.y - currentPos.y;  // Distance to collision point (takeouts) or final position (draws)
            
            // ========================================
            // UPDATE TRAJECTORY VISUALIZATION
            // ========================================
            // Show current rock position (yellow crosshair) on ideal path
            UpdateTrajectoryVisualization(idealTrajectory, actualTrajectory, currentPos, idealPosAtCurrentY);
            
            // CRITICAL FIX: If idealPosAhead is invalid (Vector2.zero or behind current position),
            // use current position instead to avoid massive false shortfalls
            if (idealPosAhead == Vector2.zero || idealPosAhead.y <= currentPos.y)
            {
                idealPosAhead = currentPos; // Rock is at end of trajectory, no shortfall prediction possible
            }
            
            float predictedShortfall = sweepingGoal.y - idealPosAhead.y;  // Will we reach the goal?
            
            // SANITY CHECK: If predicted shortfall is absurdly large (>20m), something is wrong
            // This prevents spam sweeping due to bad trajectory data
            if (predictedShortfall > 20f || predictedShortfall < -20f)
            {
                Debug.LogWarning($"[AI_Sweeper] INSANE shortfall detected: {predictedShortfall:F2}m - resetting to 0 (trajectory data invalid)");
                predictedShortfall = 0f; // Don't sweep based on bad data
            }
            
            // COLLISION DETECTION: Check if rock will hit obstacles in next 2 meters
            collisionImminent = false;
            float collisionLookaheadDistance = 2.0f;
            
            // Re-simulate from current position to check for imminent collisions
            List<Vector2> lookaheadPath = trajectorySimulator.SimulateTrajectory(
                currentPos,
                rockRB.linearVelocity,
                isInTurn,
                100,
                rocksInPlay,
                forPlayerPreview: false
            );
            
            TrajectorySimulator.CollisionInfo lookaheadCollision = trajectorySimulator.GetCollisionInfo();
            
            if (lookaheadCollision.hasCollision)
            {
                collisionDistance = Vector2.Distance(currentPos, lookaheadCollision.collisionPoint);
                
                if (collisionDistance < collisionLookaheadDistance)
                {
                    collisionImminent = true;
                    collisionPoint = lookaheadCollision.collisionPoint;
                    
                    Debug.Log($"[AI_Sweeper] COLLISION IMMINENT! Distance: {collisionDistance:F2}m at {collisionPoint}");
                }
            }

            // Get sweeper skill
            float sweepSkill = GetSweeperSkill();
            float skillMultiplier = 1.0f - (0.3f * (1.0f - sweepSkill));

            // Adjust thresholds based on skill
            float lateralThreshold = lateralErrorThreshold * skillMultiplier;
            float distanceThreshold = distanceErrorThreshold * skillMultiplier;

            // DECISION LOGIC - PRIORITY ORDER WITH HYSTERESIS
            string desiredState = "None";

            // Check if we've already collided (velocity magnitude drop or collision event)
            if (!hasCollided && lookaheadCollision.hasCollision && collisionDistance < 0.1f)
            {
                hasCollided = true;
                Debug.Log($"[AI_Sweeper] COLLISION DETECTED - Auto-WHOA activated!");
                
                // IMMEDIATE WHOA on collision!
                if (currentSweepState != "None")
                {
                    sm.SweepWhoa(true);
                    currentSweepState = "None";
                    Debug.Log($"[AI_Sweeper] Sweepers STOPPED on collision - rock physics now in control");
                }
            }

            // POST-COLLISION BEHAVIOR: Context-aware sweeping - only in scoring situations
            if (hasCollided)
            {
                // ========================================
                // POST-COLLISION: SMART SCORING LOGIC
                // ========================================
                // PHILOSOPHY: After collision, only sweep if it helps SCORING
                // 
                // TWO SCENARIOS where post-collision sweeping is VALUABLE:
                // 1. Rock moving toward BUTTON (0, 6.5) → sweep to maximize distance
                // 2. Rock in SCORING POSITION behind guard → sweep to protect/extend
                // 
                // ALL OTHER scenarios → WHOA (collision physics is chaotic, sweeping is useless)
                
                Vector2 button = new Vector2(0f, 6.5f);
                Vector2 velocity = rockRB.linearVelocity;
                
                // SCENARIO 1: Check if rock is moving TOWARD button
                // Calculate direction to button from current position
                Vector2 toButton = button - currentPos;
                float distToButton = toButton.magnitude;
                
                // Dot product: positive = moving toward button, negative = moving away
                float dotProduct = Vector2.Dot(velocity.normalized, toButton.normalized);
                
                bool movingTowardButton = dotProduct > 0.5f; // At least 60° toward button (cos 60° = 0.5)
                bool closeToButton = distToButton < 2.0f; // Within 2 units of button
                
                if (movingTowardButton && closeToButton)
                {
                    // SWEEP: Rock is heading toward button and close - maximize distance!
                    desiredState = "Weight";
                    Debug.Log($"[AI_Sweeper] POST-COLLISION: Moving toward button (dot={dotProduct:F2}, dist={distToButton:F2}) → SWEEP for distance!");
                }
                else
                {
                    // SCENARIO 2: Check if rock is in scoring position behind guard
                    bool behindGuard = false;
                    bool inScoringZone = (currentPos.y >= 5.0f && currentPos.y <= 9.0f); // In the house
                    
                    if (inScoringZone)
                    {
                        // Check if there's a guard protecting this position
                        foreach (var guard in gm.gList)
                        {
                            if (guard.lastTransform == null) continue;
                            
                            Vector2 guardPos = guard.lastTransform.position;
                            
                            // Check if guard is IN FRONT (lower Y) and ALIGNED (similar X)
                            bool guardInFront = guardPos.y < currentPos.y;
                            float lateralAlignment = Mathf.Abs(guardPos.x - currentPos.x);
                            bool guardAligned = lateralAlignment < 0.6f; // Within 60cm laterally
                            
                            if (guardInFront && guardAligned)
                            {
                                behindGuard = true;
                                Debug.Log($"[AI_Sweeper] POST-COLLISION: Behind guard at ({guardPos.x:F2}, {guardPos.y:F2}), protected scoring position!");
                                break;
                            }
                        }
                    }
                    
                    if (behindGuard && inScoringZone)
                    {
                        // SWEEP: Rock is in protected scoring position - extend it!
                        desiredState = "Weight";
                        Debug.Log($"[AI_Sweeper] POST-COLLISION: Protected scoring position (Y={currentPos.y:F2}) → SWEEP to extend!");
                    }
                    else
                    {
                        // WHOA: Neither moving toward button nor in scoring position
                        desiredState = "None";
                        
                        if (!movingTowardButton && !closeToButton)
                        {
                            Debug.Log($"[AI_Sweeper] POST-COLLISION: NOT moving toward button (dot={dotProduct:F2}, dist={distToButton:F2}) → WHOA");
                        }
                        else if (!inScoringZone)
                        {
                            Debug.Log($"[AI_Sweeper] POST-COLLISION: Outside scoring zone (Y={currentPos.y:F2}) → WHOA");
                        }
                        else
                        {
                            Debug.Log($"[AI_Sweeper] POST-COLLISION: No guard protection (exposed) → WHOA");
                        }
                    }
                }
            }
            // PRE-COLLISION BEHAVIOR: Standard trajectory following
            else if (collisionImminent)
            {
                // PRIORITY 0: COLLISION AVOIDANCE (highest priority!)
                // Determine if collision is on path to goal or off-target
                float collisionOffsetX = collisionPoint.x - sweepingGoal.x;
                
                if (Mathf.Abs(collisionOffsetX) > 0.3f)
                {
                    // Collision is off-line - try to adjust line to avoid it
                    if (collisionOffsetX > 0f)
                    {
                        // Obstacle is right of goal - sweep to pull rock LEFT
                        desiredState = isInTurn ? "Curl" : "Line";
                        Debug.Log($"[AI_Sweeper] Collision avoidance - adjusting line LEFT");
                    }
                    else
                    {
                        // Obstacle is left of goal - sweep to push rock RIGHT
                        desiredState = isInTurn ? "Line" : "Curl";
                        Debug.Log($"[AI_Sweeper] Collision avoidance - adjusting line RIGHT");
                    }
                }
                else if (collisionDistance < distanceToGoal * 0.8f)
                {
                    // Collision is on-path and before goal - try to get past it faster
                    desiredState = "Critical";
                    Debug.Log($"[AI_Sweeper] Collision avoidance - HARD SWEEP to get past obstacle!");
                }
                else
                {
                    // Collision is on-path and near/past goal - can't avoid, just optimize
                    desiredState = "Weight";
                    Debug.Log($"[AI_Sweeper] Collision unavoidable - sweeping for best outcome");
                }
            }
            // PRIORITY 1: TAKEOUT SHOTS - LINE/CURL ONLY (NO WEIGHT!)
            // Takeouts are thrown with PLENTY of velocity (11+ m/s)
            // Sweeping for weight is pointless - only fix line/curl errors!
            else if (isTakeoutShot)
            {
                // HYSTERESIS: Use 50% threshold for stopping sweep (prevents oscillation)
                float stopThreshold = (currentSweepState == "Line" || currentSweepState == "Curl") 
                    ? lateralThreshold * 0.5f 
                    : lateralThreshold;
                
                // ONLY check lateral error (line accuracy)
                // Ignore shortfall completely - rock has enough speed!
                if (Mathf.Abs(lateralError) > stopThreshold)
                {
                    // CRITICAL FIX: Logic was BACKWARDS!
                    // lateralError = currentPos.x - idealPos.x
                    // - If lateralError > 0: rock is RIGHT of ideal → need to pull LEFT
                    // - If lateralError < 0: rock is LEFT of ideal → need to pull RIGHT
                    
                    if (isInTurn)
                    {
                        // IN-TURN curls LEFT (negative X)
                        // If lateralError > 0 (rock right of ideal), sweep Curl to pull LEFT
                        // If lateralError < 0 (rock left of ideal), sweep Line to pull RIGHT
                        desiredState = (lateralError > 0f) ? "Curl" : "Line";  // ✅ FIXED: Swapped!
                        Debug.Log($"[AI_Sweeper] TAKEOUT LINE CORRECTION: {lateralError:F3}m off-line (threshold={stopThreshold:F3}), sweeping {desiredState}");
                    }
                    else
                    {
                        // OUT-TURN curls RIGHT (positive X)
                        // If lateralError > 0 (rock right of ideal), sweep Line to pull LEFT
                        // If lateralError < 0 (rock left of ideal), sweep Curl to pull RIGHT
                        desiredState = (lateralError > 0f) ? "Line" : "Curl";  // ✅ FIXED: Swapped!
                        Debug.Log($"[AI_Sweeper] TAKEOUT LINE CORRECTION: {lateralError:F3}m off-line (threshold={stopThreshold:F3}), sweeping {desiredState}");
                    }
                }
                else
                {
                    // On line - no sweeping needed!
                    desiredState = "None";
                    Debug.Log($"[AI_Sweeper] TAKEOUT: On line ({lateralError:F3}m < {stopThreshold:F3}m), no sweep needed");
                }
            }
            // PRIORITY 2: NON-TAKEOUT SHOTS - Full logic (weight + line)
            else if (predictedShortfall > 1.0f)
            {
                // CRITICAL DISTANCE (rock won't reach target!)
                desiredState = "Critical";
                Debug.Log($"[AI_Sweeper] CRITICAL shortfall: {predictedShortfall:F2}m");
            }
            else if (predictedShortfall > distanceThreshold)
            {
                // PRIORITY 3: SIGNIFICANT SHORTFALL
                desiredState = "Weight";
            }
            else if (Mathf.Abs(lateralError) > lateralThreshold)
            {
                // PRIORITY 4: LATERAL ERROR (off the ideal line)
                // CRITICAL FIX: Logic was BACKWARDS!
                // lateralError = currentPos.x - idealPos.x
                // - If lateralError > 0: rock is RIGHT of ideal → need to pull LEFT
                // - If lateralError < 0: rock is LEFT of ideal → need to pull RIGHT
                
                if (isInTurn)
                {
                    // IN-TURN curls LEFT (negative X)
                    // If lateralError > 0 (rock right of ideal), sweep Curl to pull LEFT
                    // If lateralError < 0 (rock left of ideal), sweep Line to pull RIGHT
                    desiredState = (lateralError > 0f) ? "Curl" : "Line";  // ✅ FIXED: Swapped!
                }
                else
                {
                    // OUT-TURN curls RIGHT (positive X)
                    // If lateralError > 0 (rock right of ideal), sweep Line to pull LEFT
                    // If lateralError < 0 (rock left of ideal), sweep Curl to pull RIGHT
                    desiredState = (lateralError > 0f) ? "Line" : "Curl";  // ✅ FIXED: Swapped!
                }
            }

            // Apply sweeping if state changed
            if (desiredState != currentSweepState)
            {
                ApplySweepState(desiredState, isInTurn);
                currentSweepState = desiredState;

                Debug.Log($"[AI_Sweeper] Y={currentPos.y:F2}: State={desiredState}, LateralErr={lateralError:F3}, Shortfall={predictedShortfall:F2}, Collision={collisionImminent}, PostCollision={hasCollided}");
            }

            yield return new WaitForFixedUpdate();
        }

        // Rock stopped - whoa
        if (currentSweepState != "None")
        {
            sm.SweepWhoa(true);
            Debug.Log($"[AI_Sweeper] Rock stopped - WHOA");
        }
        
        // Clear trajectory visualization
        ClearTrajectoryVisualization();
    }

    /// <summary>
    /// Find predicted position at given Y coordinate on ideal trajectory
    /// </summary>
    private Vector2 GetPredictedPositionAtY(List<Vector2> predictedPath, float targetY)
    {
        if (predictedPath == null || predictedPath.Count < 2)
            return Vector2.zero;

        // Find two points that bracket the target Y
        for (int i = 0; i < predictedPath.Count - 1; i++)
        {
            Vector2 p1 = predictedPath[i];
            Vector2 p2 = predictedPath[i + 1];

            // Check if target Y is between these two points
            if ((p1.y <= targetY && p2.y >= targetY) || (p1.y >= targetY && p2.y <= targetY))
            {
                // Interpolate X position at target Y
                float t = (targetY - p1.y) / (p2.y - p1.y);
                float interpolatedX = Mathf.Lerp(p1.x, p2.x, t);

                return new Vector2(interpolatedX, targetY);
            }
        }

        // If target Y is beyond predicted path, return last point
        if (predictedPath.Count > 0)
            return predictedPath[predictedPath.Count - 1];

        return Vector2.zero;
    }

    /// <summary>
    /// Apply the desired sweeping state
    /// 
    /// SWEEPING PHYSICS MAPPING:
    /// - "Line" = STRAIGHTEN rock (one sweeper, reduces curl when curling too much)
    /// - "Curl" = ENHANCE curl (one sweeper, increases curl when not curling enough)
    /// - "Weight" = EXTEND distance (both sweepers, makes rock go further)
    /// 
    /// IN-TURN (curls LEFT):
    ///   - Line sweep: SweepLeft() → straightens (rock curling too much left)
    ///   - Curl sweep: SweepRight() → enhances curl (rock not curling enough left)
    ///   - Weight sweep: SweepWeight() → extends distance
    /// 
    /// OUT-TURN (curls RIGHT):
    ///   - Line sweep: SweepRight() → straightens (rock curling too much right)
    ///   - Curl sweep: SweepLeft() → enhances curl (rock not curling enough right)
    ///   - Weight sweep: SweepWeight() → extends distance
    /// </summary>
    private void ApplySweepState(string state, bool isInTurn)
    {
        Debug.Log($"[AI_Sweeper] ApplySweepState called: state={state}, isInTurn={isInTurn}, SweeperManager={(sm != null ? "exists" : "NULL!")}");
        
        if (sm == null)
        {
            Debug.LogError("[AI_Sweeper] SweeperManager is NULL! Cannot sweep!");
            return;
        }
        
        switch (state)
        {
            case "None":
                Debug.Log($"[AI_Sweeper] Calling sm.SweepWhoa(true) - STOP sweeping");
                sm.SweepWhoa(true);
                break;

            case "Weight":
            case "Critical":
                // Both sweepers - extend distance (linear damping reduction)
                Debug.Log($"[AI_Sweeper] Calling sm.SweepWeight(true) for {state} - EXTEND DISTANCE");
                sm.SweepWeight(true);
                break;

            case "Line":
                // One sweeper - straighten rock (reduce excessive curl)
                if (isInTurn)
                {
                    Debug.Log($"[AI_Sweeper] Calling sm.SweepLeft(true) for Line (IN-TURN) - STRAIGHTEN (reduce left curl)");
                    sm.SweepLeft(true);  // IN-TURN curls left, sweep left to straighten
                }
                else
                {
                    Debug.Log($"[AI_Sweeper] Calling sm.SweepRight(true) for Line (OUT-TURN) - STRAIGHTEN (reduce right curl)");
                    sm.SweepRight(true); // OUT-TURN curls right, sweep right to straighten
                }
                break;

            case "Curl":
                // One sweeper - enhance curl (increase curl amount)
                if (isInTurn)
                {
                    Debug.Log($"[AI_Sweeper] Calling sm.SweepRight(true) for Curl (IN-TURN) - ENHANCE CURL (increase left curl)");
                    sm.SweepRight(true); // IN-TURN: Right sweeper enhances left curl
                }
                else
                {
                    Debug.Log($"[AI_Sweeper] Calling sm.SweepLeft(true) for Curl (OUT-TURN) - ENHANCE CURL (increase right curl)");
                    sm.SweepLeft(true);  // OUT-TURN: Left sweeper enhances right curl
                }
                break;
        }
        
        Debug.Log($"[AI_Sweeper] ApplySweepState completed for {state}");
    }

    /// <summary>
    /// Get combined sweeper skill (0-1 scale)
    /// NEW: Returns >1.0 for exceptional sweepers (allows dramatic correction!)
    /// </summary>
    private float GetSweeperSkill()
    {
        if (sm.swprLStats == null || sm.swprRStats == null)
            return 0.5f; // Default medium skill

        // Combine sweep strength (accuracy) and endurance
        float leftSkill = (sm.swprLStats.sweepStrength.GetValue() / 100f + sm.swprLStats.sweepEndurance.GetValue() / 100f) * 0.5f;
        float rightSkill = (sm.swprRStats.sweepStrength.GetValue() / 100f + sm.swprRStats.sweepEndurance.GetValue() / 100f) * 0.5f;

        // Average both sweepers
        float averageSkill = (leftSkill + rightSkill) * 0.5f;
        
        // NEW: Apply amplification for high-skill sweepers!
        // 0.0-0.6 skill: Linear (0.0-0.6 output)
        // 0.6-1.0 skill: Amplified (0.6-1.3 output) - exceptional sweepers get >100% effectiveness!
        if (averageSkill > 0.6f)
        {
            // Quadratic amplification above 60% skill
            // Formula: 0.6 + (skill - 0.6)^1.5 * 1.75
            // This gives: 60% skill = 0.6, 80% skill = 0.85, 100% skill = 1.3 (30% bonus!)
            float excessSkill = averageSkill - 0.6f;
            float amplifiedExcess = Mathf.Pow(excessSkill, 1.5f) * 1.75f;
            averageSkill = 0.6f + amplifiedExcess;
            
            Debug.Log($"[Sweeper Skill] HIGH SKILL AMPLIFICATION: Base={((leftSkill + rightSkill) * 0.5f):F2} ? Amplified={averageSkill:F2} ({(averageSkill > 1f ? "EXCEPTIONAL!" : "very good")}");
        }
        
        return averageSkill; // Can be >1.0 for exceptional sweepers!
    }
}
