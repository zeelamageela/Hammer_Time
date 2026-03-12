using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class AI_Sweeper : MonoBehaviour
{
    public AIManager aim;
    public GameManager gm;
    public SweeperManager sm;
    public MMF_Player fltFbk;
    public MMF_FloatingText fltText;

    private void Start()
    {
        fltText = fltFbk.GetFeedbackOfType<MMF_FloatingText>();
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
    /// </summary>
    public void StartPhysicsBasedSweeping(Rigidbody2D rockRB, Vector2 initialVelocity, bool isInTurn, Vector2 targetPosition, string shotType, int currentRockNumber)
    {
        StartCoroutine(MonitorAndSweepCoroutine(rockRB, initialVelocity, isInTurn, targetPosition, shotType, currentRockNumber));
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
                    fltText.Value = "Sweep!!";
                else
                    fltText.Value = "Leave it!!";
                //fltText.Play(rockRB.position, 1.25f);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                velLimit = ((4f - 3f) * ((target.y - 5.225f) / 2.55f)) + 3f;
                if (rockRB.linearVelocity.y <= velLimit)
                    fltText.Value = "SWEEP!!";
                else
                    fltText.Value = "Nope!!";

                //fltText.Play(rockRB.position, 1.4f);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                velLimit = ((2.8f - 1.85f) * ((target.y - 5.225f) / 2.55f)) + 1.85f;
                if (rockRB.linearVelocity.y <= velLimit)
                    fltText.Value = "SWEEEEEP!!";
                else if (inturn && rock.transform.position.x <= target.x - 0.65f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= target.x + 0.65f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Leave it!!";

                //fltText.Play(rockRB.position, 1.6f);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                velLimit = ((1.55f - 1.25f) * ((target.y - 5.225f) / 2.55f)) + 1.25f;
                if (rockRB.linearVelocity.y <= velLimit)
                    fltText.Value = "SWEEEEEEEEEP HARD!!";
                else if (inturn && rock.transform.position.x <= target.x - 0.55f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= target.x + 0.55f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "It's good!!";

                //fltText.Play(rockRB.position, 1.75f);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.linearVelocity.x + ", " + rockRB.linearVelocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                velLimit = ((1f - 0.5f) * ((target.y - 5.225f) / 2.55f)) + 0.5f;
                if (rockRB.linearVelocity.y <= velLimit)
                    fltText.Value = "HARRRRRD!! GO GO GO!!!";
                else if (inturn && rock.transform.position.x <= target.x - 0.4f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= target.x + 0.4f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "It's good!!";

                //fltText.Play(rockRB.position, 1.85f);
                yield return new WaitUntil(() => rock.transform.position.y >= target.x);
                velLimit = 0.5f * ((target.y - 5.225f) / 2.55f);
                if (rockRB.linearVelocity.y >= velLimit)
                    fltText.Value = "We're there!!";

                //fltText.Play(rockRB.position, intensity);
                break;

            case "Guard To Target":

                break;

            default:
                break;
        }

        //fltText.Play(rockRB.position);
    }
    
    // ========== PHYSICS-BASED SWEEPING SYSTEM ==========
    // New intelligent sweeping that predicts trajectory and corrects deviations in real-time
    
    /// <summary>
    /// Monitor rock position vs predicted trajectory and make sweeping decisions
    /// PHILOSOPHY:
    /// 1. Predict clean trajectory before errors accumulate
    /// 2. Priority: Collision avoidance > Distance > Line accuracy
    /// 3. Post-collision: Scoring position > Cover behind rocks
    /// 4. Sculpt rock back to ideal trajectory using intelligent sweep state changes
    /// </summary>
    private IEnumerator MonitorAndSweepCoroutine(Rigidbody2D rockRB, Vector2 initialVelocity, bool isInTurn, Vector2 targetPosition, string shotType, int currentRockNumber)
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

        // Generate CLEAN predicted path from launch position (before errors)
        Vector2 launcherPos = new Vector2(0f, -25f);
        List<GameObject> rocksInPlay = new List<GameObject>();
        foreach (var rockEntry in gm.rockList)
        {
            if (rockEntry.rock != null && rockEntry.rock.activeInHierarchy && rockEntry.rockInfo.inPlay && rockEntry.rock != rock)
            {
                rocksInPlay.Add(rockEntry.rock);
            }
        }

        // CRITICAL: Predict clean trajectory from initial launch
        List<Vector2> cleanTrajectory = trajectorySimulator.SimulateTrajectory(
            launcherPos,
            initialVelocity,
            isInTurn,
            250,
            rocksInPlay,
            forPlayerPreview: false
        );

        Debug.Log($"[AI_Sweeper] Monitoring started - clean trajectory has {cleanTrajectory.Count} points");

        // Wait until rock crosses hog line (Y > -16.15)
        while (rock.transform.position.y < -16.15f)
        {
            yield return new WaitForFixedUpdate();
        }

        Debug.Log($"[AI_Sweeper] Rock crossed hog line - sweeping enabled!");

        // Sweeping thresholds (skill-adjusted)
        float lateralErrorThreshold = 0.12f; // 12cm lateral error
        float distanceErrorThreshold = 0.25f; // 25cm distance error
        float predictionLookahead = 3.5f; // Look 3.5 units ahead
        
        bool collisionImminent = false;
        float collisionDistance = float.MaxValue;
        Vector2 collisionPoint = Vector2.zero;

        string currentSweepState = "None";

        // Monitor rock until it stops
        while (rockInfo != null && !rockInfo.stopped && rockRB.linearVelocity.magnitude > 0.01f)
        {
            Vector2 currentPos = rock.transform.position;

            // Find where rock SHOULD be on clean trajectory at this Y coordinate
            Vector2 idealPosAtCurrentY = GetPredictedPositionAtY(cleanTrajectory, currentPos.y);
            Vector2 idealPosAhead = GetPredictedPositionAtY(cleanTrajectory, currentPos.y + predictionLookahead);

            // Calculate deviations from ideal trajectory
            float lateralError = currentPos.x - idealPosAtCurrentY.x;
            float distanceToTarget = targetPosition.y - currentPos.y;
            float predictedShortfall = targetPosition.y - idealPosAhead.y;
            
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

            // DECISION LOGIC - PRIORITY ORDER
            string desiredState = "None";

            // Check if we've already collided (velocity magnitude drop or collision event)
            if (!hasCollided && lookaheadCollision.hasCollision && collisionDistance < 0.1f)
            {
                hasCollided = true;
                Debug.Log($"[AI_Sweeper] POST-COLLISION MODE ACTIVATED");
            }

            // POST-COLLISION BEHAVIOR: Different priorities after hitting a rock
            if (hasCollided)
            {
                // Strategy after collision:
                // 1. Get to scoring position (in house)
                // 2. Get behind cover (guard rocks)
                
                // Check if we're heading toward house
                bool headingToHouse = currentPos.y < 6.5f && distanceToTarget > 0f;
                
                if (headingToHouse && predictedShortfall > distanceThreshold)
                {
                    // Help rock reach house after collision
                    desiredState = "Weight";
                    Debug.Log($"[AI_Sweeper] POST-COLLISION: Sweeping to reach house");
                }
                else if (currentPos.y > 6.5f && currentPos.y < 7.5f)
                {
                    // In house - check if we need fine positioning
                    if (Vector2.Distance(currentPos, targetPosition) > 0.3f && predictedShortfall > 0.1f)
                    {
                        desiredState = "Weight";
                        Debug.Log($"[AI_Sweeper] POST-COLLISION: Fine positioning in house");
                    }
                    else
                    {
                        desiredState = "None";
                    }
                }
                else
                {
                    // Beyond house or stopped - whoa
                    desiredState = "None";
                }
            }
            // PRE-COLLISION BEHAVIOR: Standard trajectory following
            else
            {
                // PRIORITY 0: COLLISION AVOIDANCE (highest priority!)
                if (collisionImminent)
                {
                    // Determine if collision is on path to target or off-target
                    float collisionOffsetX = collisionPoint.x - targetPosition.x;
                    
                    if (Mathf.Abs(collisionOffsetX) > 0.3f)
                    {
                        // Collision is off-line - try to adjust line to avoid it
                        if (collisionOffsetX > 0f)
                        {
                            // Obstacle is right of target - sweep to pull rock LEFT
                            desiredState = isInTurn ? "Curl" : "Line";
                            Debug.Log($"[AI_Sweeper] Collision avoidance - adjusting line LEFT");
                        }
                        else
                        {
                            // Obstacle is left of target - sweep to push rock RIGHT
                            desiredState = isInTurn ? "Line" : "Curl";
                            Debug.Log($"[AI_Sweeper] Collision avoidance - adjusting line RIGHT");
                        }
                    }
                    else if (collisionDistance < distanceToTarget * 0.8f)
                    {
                        // Collision is on-path and before target - try to get past it faster
                        desiredState = "Critical";
                        Debug.Log($"[AI_Sweeper] Collision avoidance - HARD SWEEP to get past obstacle!");
                    }
                    else
                    {
                        // Collision is on-path and near/past target - can't avoid, just optimize
                        desiredState = "Weight";
                        Debug.Log($"[AI_Sweeper] Collision unavoidable - sweeping for best outcome");
                    }
                }
                // PRIORITY 1: CRITICAL DISTANCE (rock won't reach target!)
                else if (predictedShortfall > 1.0f)
                {
                    desiredState = "Critical";
                    Debug.Log($"[AI_Sweeper] CRITICAL shortfall: {predictedShortfall:F2}m");
                }
                // PRIORITY 2: SIGNIFICANT SHORTFALL
                else if (predictedShortfall > distanceThreshold)
                {
                    desiredState = "Weight";
                }
                // PRIORITY 3: LATERAL ERROR (off the ideal line)
                else if (Mathf.Abs(lateralError) > lateralThreshold)
                {
                    if (isInTurn)
                    {
                        // IN-TURN curls LEFT (negative X)
                        // If lateralError > 0 (rock is right of ideal), sweep Line to straighten
                        // If lateralError < 0 (rock is left of ideal), sweep Curl to straighten
                        desiredState = (lateralError > 0f) ? "Line" : "Curl";
                    }
                    else
                    {
                        // OUT-TURN curls RIGHT (positive X)
                        // If lateralError < 0 (rock is left of ideal), sweep Line to straighten
                        // If lateralError > 0 (rock is right of ideal), sweep Curl to straighten
                        desiredState = (lateralError < 0f) ? "Line" : "Curl";
                    }
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
    /// </summary>
    private void ApplySweepState(string state, bool isInTurn)
    {
        switch (state)
        {
            case "None":
                sm.SweepWhoa(true);
                break;

            case "Weight":
            case "Critical":
                // Both sweepers - maximum distance extension
                sm.SweepWeight(true);
                break;

            case "Line":
                // One sweeper on curl side - straighten the rock
                if (isInTurn)
                    sm.SweepLeft(true);  // IN-TURN: Left sweeper
                else
                    sm.SweepRight(true); // OUT-TURN: Right sweeper
                break;

            case "Curl":
                // One sweeper on opposite side - increase curl
                if (isInTurn)
                    sm.SweepRight(true); // IN-TURN: Right sweeper
                else
                    sm.SweepLeft(true);  // OUT-TURN: Left sweeper
                break;
        }
    }

    /// <summary>
    /// Get combined sweeper skill (0-1 scale)
    /// </summary>
    private float GetSweeperSkill()
    {
        if (sm.swprLStats == null || sm.swprRStats == null)
            return 0.5f; // Default medium skill

        // Combine sweep strength (accuracy) and endurance
        float leftSkill = (sm.swprLStats.sweepStrength.GetValue() / 100f + sm.swprLStats.sweepEndurance.GetValue() / 100f) * 0.5f;
        float rightSkill = (sm.swprRStats.sweepStrength.GetValue() / 100f + sm.swprRStats.sweepEndurance.GetValue() / 100f) * 0.5f;

        // Average both sweepers
        return (leftSkill + rightSkill) * 0.5f;
    }
}
