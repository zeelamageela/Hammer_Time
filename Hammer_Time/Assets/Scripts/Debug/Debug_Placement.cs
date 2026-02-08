using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class Debug_Placement : MonoBehaviour
{
    public GameManager gm;
    GameObject rock;
    Rigidbody2D rb;

    Rock_Flick rockFlick;
    Rock_Info rockInfo;
    Rock_Colliders rockCols;

    public Vector2 buttonPosition;

    public CinemachineVirtualCamera vcam;
    Transform tFollowTarget;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            //Start Coroutine OnPlaceRock(position)
            StartCoroutine(OnPlaceRock("Button"));
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(OnPlaceRock("Top Four Foot"));
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(OnPlaceRock("Back Four Foot"));
        }
        
        // W KEY: Spawn 4 red rocks in a line for AI takeout testing
        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(SpawnTakeoutTestRocks());
        }
    }

    IEnumerator OnPlaceRock(string rockPlacement)
    {
        Debug.Log("Placing a rock at " + rockPlacement);
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();
        rockCols = rock.GetComponent<Rock_Colliders>();

        rock.GetComponent<SpringJoint2D>().enabled = false;
        rockFlick.enabled = false;

        yield return new WaitForFixedUpdate();

        switch (rockPlacement)
        {
            case "Button":
                //position = 0
                rock.transform.position = buttonPosition;

                break;
            case "Back Four Foot":
                //position = 1
                Vector2 rockPos = buttonPosition + (Random.insideUnitCircle * 1.5f);
                rock.transform.position = rockPos;

                break;
            case "Top Four Foot":
                //position = 2
                rock.transform.position = new Vector2(Random.Range(-1.4f, 1.4f), Random.Range(0.05f, 5f));
                break;
        }

        yield return new WaitForFixedUpdate();

        tFollowTarget = rock.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;


        rockCols.shotTaken = true;
        rockInfo.released = true;
        rockInfo.inPlay = true;
        rockInfo.stopped = true;
        rockInfo.rest = true;
        rockInfo.inHouse = true;
        rockInfo.outOfPlay = false;

    }
    
    /// <summary>
    /// W KEY: Spawn rocks at strategic positions around the house for comprehensive AI testing
    /// Tests dynamic curl compensation across different distances and lateral positions
    /// </summary>
    IEnumerator SpawnTakeoutTestRocks()
    {
        Debug.Log("[TAKEOUT TEST] Spawning test rocks across the house...");
        
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        if (gsp == null)
        {
            Debug.LogError("[TAKEOUT TEST] GameSettingsPersist not found!");
            yield break;
        }
        
        // Define strategic test positions across the house
        // Format: (X, Y) where X=lateral position, Y=distance from hack
        Vector2[] positions = new Vector2[]
        {
            // HOUSE ROCKS (for takeout testing)
            new Vector2(0f, 6.5f),        // 1. Center button - baseline test
            new Vector2(1.0f, 6.5f),      // 2. Right side button - lateral factor test
            new Vector2(-1.0f, 6.5f),     // 3. Left side button - lateral factor test
            new Vector2(0.5f, 7.2f),      // 4. Back right - distance + lateral
            new Vector2(-0.5f, 5.8f),     // 5. Front left - short distance + lateral
            
            // GUARDS (for peel/raise testing)
            new Vector2(0f, 3.0f),        // 6. Center guard - short distance
            new Vector2(0.8f, 3.5f),      // 7. Right corner guard - short + lateral
            new Vector2(-0.8f, 3.5f),     // 8. Left corner guard - short + lateral
        };
        
        string[] positionNames = new string[]
        {
            "Center Button",
            "Right Side Button (+1.0)",
            "Left Side Button (-1.0)",
            "Back Right (+0.5, 7.2)",
            "Front Left (-0.5, 5.8)",
            "Center Guard (3.0)",
            "Right Corner Guard (+0.8)",
            "Left Corner Guard (-0.8)"
        };
        
        int rocksPlaced = 0;
        
        // Find red team rocks to place
        for (int i = 0; i < gm.rockList.Count && rocksPlaced < positions.Length; i++)
        {
            Rock_Info info = gm.rockList[i].rockInfo;
            
            // Only use rocks from player's team (red) that haven't been used
            if (info.teamName == gsp.redTeamName && !info.shotTaken)
            {
                GameObject testRock = gm.rockList[i].rock;
                
                // Disable throwing components
                testRock.GetComponent<SpringJoint2D>().enabled = false;
                testRock.GetComponent<Rock_Flick>().enabled = false;
                testRock.transform.parent = null;
                
                // Position the rock
                testRock.transform.position = positions[rocksPlaced];
                
                // Enable visual and physics
                testRock.GetComponent<SpriteRenderer>().enabled = true;
                testRock.GetComponent<CircleCollider2D>().enabled = true;
                testRock.GetComponent<CircleCollider2D>().radius = 0.14f;
                testRock.GetComponent<Rock_Release>().enabled = true;
                testRock.GetComponent<Rock_Force>().enabled = true;
                testRock.GetComponent<Rock_Colliders>().enabled = true;
                
                // Mark as in play
                info.shotTaken = true;
                info.released = true;
                info.inPlay = true;
                info.stopped = true;
                info.rest = true;
                info.inHouse = (positions[rocksPlaced].y > 5.0f); // Only house rocks are "in house"
                info.outOfPlay = false;
                info.placed = true;
                
                Debug.Log($"[TAKEOUT TEST] #{rocksPlaced + 1}: {positionNames[rocksPlaced]} at ({positions[rocksPlaced].x:F2}, {positions[rocksPlaced].y:F2})");
                
                rocksPlaced++;
                yield return new WaitForEndOfFrame();
            }
        }
        
        Debug.Log($"[TAKEOUT TEST] ? Spawned {rocksPlaced} red rocks across the house!");
        Debug.Log("[TAKEOUT TEST] TEST POSITIONS:");
        Debug.Log("  HOUSE ROCKS (Y > 5.0):");
        Debug.Log("    1. Center Button (0, 6.5) - Baseline test");
        Debug.Log("    2. Right Side (+1.0, 6.5) - Lateral factor test");
        Debug.Log("    3. Left Side (-1.0, 6.5) - Lateral factor test");
        Debug.Log("    4. Back Right (+0.5, 7.2) - Distance + lateral");
        Debug.Log("    5. Front Left (-0.5, 5.8) - Short distance + lateral");
        Debug.Log("  GUARDS (Y < 5.0):");
        Debug.Log("    6. Center (0, 3.0) - Short distance");
        Debug.Log("    7. Right Corner (+0.8, 3.5) - Short + lateral");
        Debug.Log("    8. Left Corner (-0.8, 3.5) - Short + lateral");
        Debug.Log("[TAKEOUT TEST] INSTRUCTIONS:");
        Debug.Log("  1. Throw your yellow rock out of play (miss on purpose)");
        Debug.Log("  2. AI will take out the red rocks one by one");
        Debug.Log("  3. Watch accuracy: Does it hit dead-on or miss?");
        Debug.Log("  4. Check BOTH in-turn and out-turn shots");
        Debug.Log("  5. All 8 positions should be accurate (±0.02 units)");
    }
}
