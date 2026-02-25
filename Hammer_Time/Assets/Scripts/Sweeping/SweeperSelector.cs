using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperSelector : MonoBehaviour
{
    public GameObject halL;
    public GameObject halR;

    public SweeperParent sweeperL;
    public SweeperParent sweeperR;

    public GameObject tSweepParent;

    public SweeperParent sweeperRedTee;
    public SweeperParent sweeperYellowTee;

    public Collider2D sweeperLCol;
    public Collider2D sweeperRCol;
    public Collider2D sweepZoneCol;
    public Collider2D sweeperTeeCol;

    public GameObject panel;
    public RockManager rm;
    public SweeperManager sm;
    public Sweep sweep;
    bool inturn;
    Rigidbody2D rockRB;
    Rigidbody2D rock2RB;
    public Transform launcher;
    bool aiTurn;

    public Vector2 moveDirection;
    public Vector2 moveDirection2;
    
    // Auto-follow collision detection
    private Vector2 houseCenter = new Vector2(0f, 6.5f);
    private float lastCollisionCheckTime = 0f;
    private const float COLLISION_CHECK_INTERVAL = 0.1f;

    private void Update()
    {
        if (rockRB != null)
        {
            Vector3 followSpot = new Vector3((rockRB.position.x), (rockRB.position.y), 0f);
            transform.position = followSpot;

            if (rock2RB != null)
            {
            Vector3 followSpot2 = new Vector3((rock2RB.position.x), (rock2RB.position.y), 0f);
            tSweepParent.transform.position = followSpot2;

            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            Vector2 rockPos = new Vector2(rockRB.position.x, rockRB.position.y);
            Vector2 launchPos = new Vector2(launcher.position.x, launcher.position.y);
            Vector2 rockDirection = (Vector2)Vector3.Normalize(rockPos - launchPos);

            //float angle = Mathf.Atan2(rockDirection.x, rockDirection.y) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 45f));
            //if (angle <= 45f)
            //{
            //    sweeperParent.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            //}

            
            moveDirection = rockRB.linearVelocity;

            if (moveDirection != Vector2.zero)
            {
                if (Mathf.Abs(moveDirection.x) > 0.02f | Mathf.Abs(moveDirection.y) > 0.005f)
                {
                    float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis((angle - 90f), Vector3.forward);
                    //Debug.Log("Angle is " + angle);
                    if (transform.rotation.z > 30f)
                    {
                        sweeperL.yOffset = 1f;
                        sweeperR.yOffset = 0.5f;
                    }
                    if (transform.rotation.z < -30f)
                    {
                        sweeperL.yOffset = 0.5f;
                        sweeperR.yOffset = 1f;
                    }
                }
            }
            if (rock2RB != null)
            {
            moveDirection2 = rock2RB.linearVelocity;

            if (moveDirection != Vector2.zero)
            {
                if (Mathf.Abs(moveDirection2.x) > 0.02f | Mathf.Abs(moveDirection2.y) > 0.005f)
                {
                    float angle = Mathf.Atan2(moveDirection2.y, moveDirection2.x) * Mathf.Rad2Deg;
                    tSweepParent.transform.rotation = Quaternion.AngleAxis((angle - 90f), Vector3.forward);
                    //Debug.Log("Angle is " + angle);
                    if (tSweepParent.transform.rotation.z > 30f)
                    {
                        sweeperRedTee.yOffset = 0.6f;
                        sweeperYellowTee.yOffset = 0.6f;
                    }
                    if (tSweepParent.transform.rotation.z < -30f)
                    {
                        sweeperL.yOffset = 0.5f;
                        sweeperR.yOffset = 1f;
                    }
                }
            }

            }
            // Auto-follow collision detection
            CheckForStrategicRockSwitch();
            
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit.collider == sweepZoneCol)
                {
                    //sm.isSweeping = true;
                    sm.SweepTap();
                        
                    Debug.Log(hit.collider.gameObject.name);
                }

                if (hit.collider == sweeperLCol)
                {
                    sm.SweepTapLeft();

                    Debug.Log(hit.collider.gameObject.name);
                }

                if (hit.collider == sweeperRCol)
                {
                    sm.SweepTapRight();

                    Debug.Log(hit.collider.gameObject.name);
                }

                if (hit.collider.gameObject.layer == 3)
                {
                    ReAttachToRock(hit.collider.gameObject);
                    Debug.Log(hit.collider.gameObject.name);
                    Debug.Log(hit.collider.gameObject.layer);
                }

            }
        }
    }

    /// <summary>
    /// ReAttachToRock - Handles tapping rocks to switch sweepers
    /// - Player's own rocks: Switch regular sweepers to follow this rock
    /// - Opponent rocks behind T-line: Handled by TeeSweeperController
    /// </summary>
    public void ReAttachToRock(GameObject rock)
    {
        Rock_Info rockInfo = rock.GetComponent<Rock_Info>();
        if (rockInfo == null || !rockInfo.moving)
        {
            Debug.Log("[SweeperSelector] Rock not moving or no Rock_Info");
            return;
        }
        
        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        
        if (gsp == null || cm == null) return;
        
        // Check if this is player's own rock
        bool isPlayerRock = (rockInfo.teamName == cm.teamName);
        
        if (isPlayerRock)
        {
            // Player tapped their own rock - switch regular sweepers to follow it
            Debug.Log($"[SweeperSelector] Switching regular sweepers to follow {rock.name}");
            rockRB = rock.GetComponent<Rigidbody2D>();
            
            // Keep sweepers active - don't interrupt current sweeping state
            // Player can continue sweeping the new rock
        }
        else if (rock.transform.position.y > 6.5f)
        {
            // Opponent rock behind T-line - let TeeSweeperController handle it
            Debug.Log("[SweeperSelector] Opponent rock behind T-line - TeeSweeperController will handle");
        }
    }

    public void AttachToRock(GameObject rock)
    {
        rockRB = rock.GetComponent<Rigidbody2D>();
        //rj.connectedBody = rockRB;
    }

    public void SetColliders()
    {
        sweeperL = sm.sweeperL;
        sweeperLCol = sweeperL.GetComponent<BoxCollider2D>();
        sweeperR = sm.sweeperR;
        sweeperRCol = sweeperR.GetComponent<BoxCollider2D>(); 
        sweeperRedTee = sm.sweeperRedTee;
        sweeperYellowTee = sm.sweeperYellowTee;
        sweeperTeeCol = sweeperRedTee.GetComponent<BoxCollider2D>();
    }
    
    /// <summary>
    /// Check if we should auto-switch to a more strategic rock after collision
    /// Used for tap-backs, run-backs, and double takeouts
    /// </summary>
    void CheckForStrategicRockSwitch()
    {
        // Only check periodically to avoid performance issues
        if (Time.time - lastCollisionCheckTime < COLLISION_CHECK_INTERVAL) return;
        lastCollisionCheckTime = Time.time;
        
        if (rockRB == null) return;
        
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null || gm.rockList == null) return;
        
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        if (cm == null) return;
        
        GameObject currentRock = rockRB.gameObject;
        Rock_Info currentRockInfo = currentRock.GetComponent<Rock_Info>();
        if (currentRockInfo == null || !currentRockInfo.moving) return;
        
        // Get current rock's distance to house center
        float currentDistToHouse = Vector2.Distance(rockRB.position, houseCenter);
        
        // Find all moving rocks of player's team
        GameObject bestRock = null;
        float bestScore = float.MinValue;
        
        foreach (var rockEntry in gm.rockList)
        {
            if (rockEntry.rock == null || !rockEntry.rock.activeInHierarchy) continue;
            if (rockEntry.rock == currentRock) continue;  // Skip current rock
            
            Rock_Info rockInfo = rockEntry.rockInfo;
            if (rockInfo == null || !rockInfo.moving) continue;
            
            // Only consider player's own rocks
            if (rockInfo.teamName != cm.teamName) continue;
            
            Rigidbody2D otherRB = rockEntry.rock.GetComponent<Rigidbody2D>();
            if (otherRB == null) continue;
            
            // Check if this rock is moving towards the house
            Vector2 toHouse = houseCenter - otherRB.position;
            float dotProduct = Vector2.Dot(otherRB.linearVelocity.normalized, toHouse.normalized);
            
            // Only consider rocks moving towards house (dot > 0.5 = roughly 60 degrees)
            if (dotProduct < 0.5f) continue;
            
            // Calculate strategic score:
            // - Higher score for rocks closer to house center
            // - Higher score for rocks moving faster
            // - Higher score for rocks heading more directly towards center
            float distToHouse = Vector2.Distance(otherRB.position, houseCenter);
            float velocity = otherRB.linearVelocity.magnitude;
            
            // Strategic score: prioritize rocks that:
            // 1. Are moving towards house (dotProduct weight)
            // 2. Are closer to house center (inverse distance)
            // 3. Are moving with decent velocity
            float strategicScore = dotProduct * 2f + (1f / (distToHouse + 1f)) * 3f + velocity * 0.5f;
            
            // CRITICAL: Only switch if other rock is significantly MORE strategic
            // Must be heading closer to center than current rock
            if (distToHouse < currentDistToHouse - 0.3f && strategicScore > bestScore)
            {
                bestScore = strategicScore;
                bestRock = rockEntry.rock;
            }
        }
        
        // If we found a more strategic rock, switch to it
        if (bestRock != null)
        {
            Debug.Log($"[SweeperSelector] AUTO-FOLLOW: Switching from {currentRock.name} to {bestRock.name} (more strategic)");
            rockRB = bestRock.GetComponent<Rigidbody2D>();
            
            // Notify player via callout
            if (sm != null)
            {
                sm.CallOut("Sweep");  // "SWEEP!" callout to alert player
            }
        }
    }
}
