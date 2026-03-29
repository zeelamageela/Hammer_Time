using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class Rock_Flick : MonoBehaviour
{
    public Rigidbody2D rb;

    public float releaseTime = 0f; // DETERMINISTIC: No wait needed, release immediately!

    public bool isPressed = false;
    public bool mouseUp = false;
    //public bool shotTaken = false;
    public bool isPressedAI = false;

    GameObject launcher;
    Rigidbody2D launcher_rb;

    AudioManager am;
    Vector2 startPoint;
    Vector2 endPoint;
    public Vector2 springDirection;
    public float springDistance;
    public bool springReleased;

    Transform tFollowTarget;
    public GameObject vcam_go;
    public CinemachineVirtualCamera vcam;

    public GameManager gm;

    GameObject trajLineGO;
    TrajectoryLine trajLine;
    GameObject shootKnobGO;
    ShootingKnob shootKnob;
    GameObject mouseKnobGO;
    
    // Flick shot controller
    private FlickShotController flickShotController;
    private bool isFlickShotMode = false;

    Vector3 lastMouseCoordinate = Vector3.zero;

    Vector2 posScale = new Vector2(1f / 16f, 1f);

    public bool story;
    AudioSource[] rockSounds;

    void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        GetComponent<SpriteRenderer>().enabled = false;


        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();

        trajLineGO = GameObject.Find("TrajectoryLine");
        trajLine = trajLineGO.GetComponent<TrajectoryLine>();
        //trajLine.DrawTrajectory();

        launcher = GameObject.FindWithTag("Launcher");
        launcher_rb = launcher.GetComponent<Rigidbody2D>();

        shootKnobGO = GameObject.Find("ShootingKnob");
        shootKnob = shootKnobGO.GetComponent<ShootingKnob>();

        GetComponent<SpringJoint2D>().connectedBody = launcher_rb;
        GetComponent<Rock_Colliders>().enabled = false;

        vcam_go = GameObject.Find("CM vcam1");
        vcam = vcam_go.GetComponent<CinemachineVirtualCamera>();

        gameObject.transform.parent = null;
        gameObject.transform.position = launcher.transform.position;

        GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<CircleCollider2D>().radius = 1.5f;
        mouseUp = false;

        rockSounds = GetComponents<AudioSource>();
        //Debug.Log(rockSounds[0].clip.name + " - " + rockSounds[1].clip.name);
        
        // Initialize flick shot controller if it exists
        flickShotController = GetComponent<FlickShotController>();
        if (flickShotController != null)
        {
            isFlickShotMode = GameVisualizationSettings.Instance.FlickShotMode;
            GameVisualizationSettings.Instance.OnFlickShotModeChanged += OnFlickShotModeChanged;
            Debug.Log($"[Rock_Flick] FlickShotController found - Mode: {(isFlickShotMode ? "ENABLED" : "DISABLED")}");
            
            // If flick shot mode is enabled, start it automatically
            if (isFlickShotMode)
            {
                flickShotController.StartFlickShot();
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from flick shot mode changes
        if (GameVisualizationSettings.Instance != null)
        {
            GameVisualizationSettings.Instance.OnFlickShotModeChanged -= OnFlickShotModeChanged;
        }
    }
    
    private void OnFlickShotModeChanged(bool enabled)
    {
        isFlickShotMode = enabled;
        Debug.Log($"[Rock_Flick] Flick shot mode changed to: {(enabled ? "ENABLED" : "DISABLED")}");
    }

    void Update()
    {
        if (isPressed)
        {
            rb.position = Vector2.Scale(Camera.main.ScreenToWorldPoint(Input.mousePosition), posScale);
            shootKnob.mouseCircle.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            shootKnob.mouseCircle.GetComponent<SpriteRenderer>().enabled = true;
            trajLine.DrawTrajectory();

            shootKnob.ParentToRock();
            //if (Input.GetKeyDown(KeyCode.O))
            //{
            //    GetComponent<Rock_Force>().flipAxis = true;
            //}
            //if (Input.GetKeyDown(KeyCode.I))
            //{
            //    GetComponent<Rock_Force>().flipAxis = false;
            //}

            OnDrag();
        }

        if (isPressedAI)
        {
            OnDrag();
        }

        if (mouseUp)
        {
            isPressed = false;
            rb.isKinematic = false;
            Debug.Log("Release Pullback is " + rb.position.x + ", " + rb.position.y);
            GetComponent<CircleCollider2D>().radius = 0.14f;
            StartCoroutine(Release());
            Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
            trajLine.Release();
            shootKnob.UnParentandHide();
        }

        if (springReleased)
        {
            isPressed = false;
        }
    }

    void OnMouseDown()
    {
        if (!GetComponent<Rock_Info>().released)
        {
            // CRITICAL: In flick shot mode, block all clicks once aim is set
            if (isFlickShotMode && flickShotController != null)
            {
                // Check if we're in AimSet or PowerPhase - block all clicks!
                System.Reflection.PropertyInfo phaseProp = flickShotController.GetType().GetProperty("currentPhase");
                if (phaseProp != null)
                {
                    object phase = phaseProp.GetValue(flickShotController);
                    string phaseName = phase.ToString();
                    
                    if (phaseName == "AimSet" || phaseName == "PowerPhase" || phaseName == "Released")
                    {
                        Debug.Log($"[Rock_Flick] Ignoring click - in {phaseName} phase (aim locked)");
                        return; // Block clicks once aim is set!
                    }
                }
            }
            
            // In flick shot mode, we still allow normal pullback to work
            // (Don't skip the normal pullback logic!)
            
            //if red has hammer
            Debug.Log("Clicked on Rock");
            if (gm.redHammer)
            {
                //if the rock is red
                if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
                {
                    //if the ai team is not red
                    if (!gm.aiTeamRed)
                    {
                        GetComponent<SpringJoint2D>().dampingRatio = 0.2f;
                        GetComponent<SpringJoint2D>().frequency = 1.5f;
                        isPressed = true;
                        rb.isKinematic = true;
                    }
                }
                //if the rock is yellow
                else if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
                {
                    //if the ai team is not yellow
                    if (!gm.aiTeamYellow)
                    {
                        GetComponent<SpringJoint2D>().dampingRatio = 0.2f;
                        GetComponent<SpringJoint2D>().frequency = 1.5f;
                        isPressed = true;
                        rb.isKinematic = true;
                    }
                }
            }
            else
            {
                //if the rock is red
                if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
                {
                    //if the ai team is not red
                    if (!gm.aiTeamRed)
                    {
                        GetComponent<SpringJoint2D>().dampingRatio = 0.2f;
                        GetComponent<SpringJoint2D>().frequency = 1.5f;
                        isPressed = true;
                        rb.isKinematic = true;
                    }
                }
                //if the rock is yellow
                else if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
                {
                    //if the ai team is not yellow
                    if (!gm.aiTeamYellow)
                    {
                        GetComponent<SpringJoint2D>().dampingRatio = 0.2f;
                        GetComponent<SpringJoint2D>().frequency = 1.5f;
                        isPressed = true;
                        rb.isKinematic = true;
                    }
                }
            }

            shootKnob.mouseCircle.GetComponent<SpriteRenderer>().enabled = true;
            shootKnobGO.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    void OnDrag()
    {
        startPoint = rb.position;
        GetComponent<SpriteRenderer>().enabled = false;
        endPoint = GetComponent<SpringJoint2D>().connectedBody.transform.position;
        springDistance = Vector2.Distance(startPoint, endPoint);

        springDirection = (Vector2)Vector3.Normalize(endPoint - startPoint);
    }

    public void OnMouseUp()
    {
        if (!GetComponent<Rock_Info>().released)
        {
            // CRITICAL: In flick shot mode, don't fire on release - just hold position
            if (isFlickShotMode && flickShotController != null)
            {
                // IMPORTANT: Capture rock position BEFORE anything else
                Vector2 aimPosition = rb.position;
                Vector2 launcherPosition = launcher.transform.position;
                
                // Calculate and pass aim direction to controller
                flickShotController.SetAimPosition(aimPosition, launcherPosition);
                
                // Keep the rock at the aimed position (don't fire, don't reset)
                // Player will click launcher to start power phase
                isPressed = false;
                
                // CRITICAL: Keep rock KINEMATIC to hold it in place
                // DON'T let spring pull it back - we want to adjust aim!
                rb.isKinematic = true;
                rb.linearVelocity = Vector2.zero;
                
                // Disable spring so it doesn't interfere
                SpringJoint2D spring = GetComponent<SpringJoint2D>();
                if (spring != null)
                {
                    spring.enabled = false;
                }
                
                Debug.Log($"[Rock_Flick] Flick shot mode: Aim set at position {aimPosition}. Click launcher to start power flick (or drag rock to adjust aim).");
                
                // Disable rock collider so we can't click it again
                CircleCollider2D rockCollider = GetComponent<CircleCollider2D>();
                if (rockCollider != null)
                {
                    rockCollider.enabled = false;
                    Debug.Log("[Rock_Flick] Rock collider disabled - can't click rock anymore");
                }
                
                // Keep shooting knob visible at aimed position
                shootKnob.mouseCircle.GetComponent<SpriteRenderer>().enabled = false; // Hide mouse circle
                shootKnobGO.GetComponent<SpriteRenderer>().enabled = true; // Keep shooting knob visible
                
                return; // Skip normal release logic
            }
            
            //if red has hammer
            if (!story && gm.redHammer && !GetComponent<Rock_Info>().released)
            {
                //if the rock is red
                if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
                {
                    //if the ai team is not red
                    if (!gm.aiTeamRed)
                    {
                        isPressed = false;
                        rb.isKinematic = false;

                        if (rb.position.y >= -24f)
                        {
                            RockReset();
                        }
                        else if (Mathf.Abs(rb.position.x) >= 0.34f)
                        {
                            RockReset();
                        }
                        else if (springDistance <= 1.5f)  // DETERMINISTIC: Allow light shots
                        {
                            RockReset();
                        }
                        else
                        {
                            GetComponent<CircleCollider2D>().radius = 0.14f;
                            StartCoroutine(Release());
                            Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
                            trajLine.Release();
                            shootKnob.UnParentandHide();
                        }
                    }
                }
                //if the rock is yellow
                else if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
                {
                    //if the ai team is not yellow
                    if (!gm.aiTeamYellow)
                    {
                        isPressed = false;
                        rb.isKinematic = false;

                        Debug.Log("What the fuckkkk");
                        if (rb.position.y >= -24f)
                        {
                            RockReset();
                        }
                        else if (Mathf.Abs(rb.position.x) >= 0.34f)
                        {
                            RockReset();
                        }
                        else if (springDistance <= 1.5f)  // DETERMINISTIC: Allow light shots
                        {
                            RockReset();
                        }
                        else
                        {
                            GetComponent<CircleCollider2D>().radius = 0.14f;
                            StartCoroutine(Release());
                            Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
                            trajLine.Release();
                            shootKnob.UnParentandHide();
                        }
                    }
                }
            }
            //if yellow has the hammer
            else if (!story && !gm.redHammer && !GetComponent<Rock_Info>().released)
            {
                //if the rock is yellow
                if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
                {
                    //if the ai team is not yellow
                    if (!gm.aiTeamRed)
                    {
                        isPressed = false;
                        rb.isKinematic = false;

                        if (rb.position.y >= -24f)
                        {
                            RockReset();
                        }
                        else if (Mathf.Abs(rb.position.x) >= 0.34f)
                        {
                            RockReset();
                        }
                        else if (springDistance <= 1.5f)  // DETERMINISTIC: Allow light shots
                        {
                            RockReset();
                        }
                        else
                        {
                            GetComponent<CircleCollider2D>().radius = 0.14f;
                            StartCoroutine(Release());
                            //Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
                            trajLine.Release();
                            shootKnob.UnParentandHide();
                        }
                    }
                }
                //if the rock is red
                else if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
                {
                    //if the ai team is not red
                    if (!gm.aiTeamYellow)
                    {
                        isPressed = false;
                        rb.isKinematic = false;

                        if (rb.position.y >= -24f)
                        {
                            RockReset();
                        }
                        else if (Mathf.Abs(rb.position.x) >= 0.34f)
                        {
                            RockReset();
                        }
                        else if (springDistance <= 1.5f)  // DETERMINISTIC: Allow light shots
                        {
                            RockReset();
                        }
                        else
                        {
                            GetComponent<CircleCollider2D>().radius = 0.14f;
                            StartCoroutine(Release());
                            //Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
                            trajLine.Release();
                            shootKnob.UnParentandHide();
                        }
                    }
                }
            }
            else
            {
                StartCoroutine(RockResetStory());
            }

            shootKnob.mouseCircle.GetComponent<SpriteRenderer>().enabled = false;
            shootKnobGO.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void RockReset()
    {
        transform.position = launcher_rb.position;
        GetComponent<SpringJoint2D>().dampingRatio = 1f;
        GetComponent<SpringJoint2D>().frequency = 10000f;
    }

    IEnumerator RockResetStory()
    {
        GetComponent<SpringJoint2D>().dampingRatio = 1f;
        GetComponent<SpringJoint2D>().frequency = 10000f;
        transform.position = launcher_rb.position;

        isPressed = false;
        yield return new WaitForEndOfFrame();
        GetComponent<SpringJoint2D>().dampingRatio = 0.2f;
        GetComponent<SpringJoint2D>().frequency = 1.5f;
    }

    IEnumerator Release()
    {
        mouseUp = false;
        springReleased = true;

        Debug.Log($"[Rock_Flick START] rb.isKinematic: {rb.isKinematic}, linearDamping: {rb.linearDamping}");
        
        // CRITICAL: Make sure kinematic is OFF!
        rb.isKinematic = false;
        
        // CRITICAL: Disable damping COMPLETELY during launch!
        // Damping will be restored by Rock_Force.Release() when rock crosses hog line
        rb.linearDamping = 0f;
        
        // DETERMINISTIC VELOCITY: Calculate from pullback distance using TrajectoryLine parameters
        // CRITICAL: Pass TrajectoryLine parameters to match AI calculations!
        Vector2 velocity = TrajectorySimulator.CalculateInitialVelocityFromPullback(
            rb.position, 
            launcher.transform.position,
            trajLine.velocityMultiplier,
            trajLine.minPullbackDistance,
            trajLine.maxPullbackDistance,
            trajLine.minVelocity,
            trajLine.maxVelocity
        );
        
        Debug.Log($"[Rock_Flick] CALCULATED velocity: {velocity.magnitude:F2} m/s from pullback distance: {springDistance:F3} (using TrajectoryLine params)");
        
        // Show detailed feedback callout for normal shot (4 separate stacking callouts!)
        if (TextCalloutManager.Instance != null)
        {
            float actualVelocity = velocity.magnitude;
            float minVel = trajLine.minVelocity;
            float maxVel = trajLine.maxVelocity;
            
            // FIXED: Use rock's actual Rigidbody2D position
            Vector3 rockPosition = (Vector3)rb.position;
            
            // Show 4 SEPARATE callouts that will stack vertically
            // Each callout shows at same position - stacking system handles vertical spacing
            
            // Callout 1: Shot Released
            TextCalloutManager.Instance.ShowCallout(
                rockPosition + Vector3.up * 0.5f,
                "Shot Released!",
                followTarget: true,
                target: transform,
                duration: 3f
            );
            
            // Callout 2: Velocity
            TextCalloutManager.Instance.ShowCallout(
                rockPosition + Vector3.up * 0.5f,
                $"Velocity: {actualVelocity:F2} m/s",
                followTarget: true,
                target: transform,
                duration: 6f
            );
            
            // Callout 3: Range
            TextCalloutManager.Instance.ShowCallout(
                rockPosition + Vector3.up * 0.5f,
                $"Range: {minVel:F1}-{maxVel:F1} m/s",
                followTarget: true,
                target: transform,
                duration: 3f
            );
            
            // Callout 4: Pullback
            TextCalloutManager.Instance.ShowCallout(
                rockPosition + Vector3.up * 0.5f,
                $"Pullback: {springDistance:F2}m",
                followTarget: true,
                target: transform,
                duration: 3f
            );
            
            Debug.Log($"[Rock_Flick] Normal shot: 4 stacked callouts displayed at {rockPosition}");
        }
        
        // CRITICAL: Disable spring FIRST to prevent it from interfering!
        GetComponent<SpringJoint2D>().enabled = false;
        
        Debug.Log($"[Rock_Flick] Spring disabled, damping disabled (will restore at hog line)");
        
        // Set velocity directly - no spring physics!
        rb.linearVelocity = velocity;
        
        Debug.Log($"[Rock_Flick] ACTUAL rb.linearVelocity AFTER setting: {rb.linearVelocity.magnitude:F2} m/s");
        
        launcher.GetComponent<Collider2D>().enabled = true;
        
        // Start rock timer display
        RockTimerDisplay timerDisplay = GetComponent<RockTimerDisplay>();
        if (timerDisplay != null)
        {
            timerDisplay.StartTimer();
            Debug.Log("[Rock_Flick] Rock timer started!");
        }
        
        yield return new WaitForSeconds(releaseTime);
        rockSounds[1].enabled = true;
        this.enabled = false;

        yield return new WaitForFixedUpdate();
        
        Debug.Log($"[Rock_Flick] rb.linearVelocity AFTER FixedUpdate: {rb.linearVelocity.magnitude:F2} m/s | Pos: ({rb.position.x:F3}, {rb.position.y:F3})");
    }

    void ShotLocation()
    {
        AI_Sweeper aiSweep = FindObjectOfType<AI_Sweeper>();
        AI_Shooter aiShoot = FindObjectOfType<AI_Shooter>();
        RockManager rm = FindObjectOfType<RockManager>();
        Vector3 aimPos = trajLine.aimCircle.transform.position;

        string shotType;

        //aim circle is in the house
        if (Vector2.Distance(new Vector2(0f, 6.5f), new Vector2(aimPos.x, aimPos.y)) < 1.5f)
        {
            //Button
            if (Vector2.Distance(new Vector2(0f, 6.5f), new Vector2(aimPos.x, aimPos.y)) < 0.25f)
            {
                shotType = "Button";
            }
            //in the centre
            else if (Mathf.Abs(aimPos.x) < 0.25f)
            {
                //in the front of the house
                if (aimPos.y < 6.5f)
                {
                    //in the four foot
                    if (aimPos.y < 6f)
                    {
                        shotType = "Top Four Foot";
                    }
                    else
                    {
                        shotType = "Top Twelve Foot";
                    }
                }
                else
                {
                    //in the four foot
                    if (aimPos.y < 7f)
                    {
                        shotType = "Back Four Foot";
                    }
                    else
                    {
                        shotType = "Back Twelve Foot";
                    }
                }
            }
            //on the left
            else if (aimPos.x < 0f)
            {
                //in the four foot
                if (Vector2.Distance(new Vector2(0f, 6.5f), new Vector2(aimPos.x, aimPos.y)) < 0.75f)
                {
                    shotType = "Left Four Foot";
                }
                else
                {
                    shotType = "Left Twelve Foot";
                }
            }
            //on the right
            else if (aimPos.x > 0f)
            {
                //in the four foot
                if (Vector2.Distance(new Vector2(0f, 6.5f), new Vector2(aimPos.x, aimPos.y)) < 0.75f)
                {
                    shotType = "Right Four Foot";
                }
                else
                {
                    shotType = "Right Twelve Foot";
                }
            }
            //something has gone wrong
            else
            {
                shotType = "Default - No Shot";
            }
        }
        //outside the house
        else
        {
            //in the centre
            if (Mathf.Abs(aimPos.x) < 0.35f)
            {
                if (aimPos.y < 2f)
                {
                    if (aimPos.y < 4f)
                    {
                        shotType = "High Centre Guard";
                    }
                    else
                    {
                        shotType = "Centre Guard";
                    }
                }
                else
                {
                    shotType = "Tight Centre Guard";
                }
            }
            //on the left
            else if (aimPos.x < 0f)
            {
                if (aimPos.y < 2f)
                {
                    if (aimPos.y < 4f)
                    {
                        shotType = "Left High Corner Guard";
                    }
                    else
                    {
                        shotType = "Left Corner Guard";
                    }
                }
                else
                {
                    shotType = "Left Tight Corner Guard";
                }
            }
            //on the right
            else if (aimPos.x > 0f)
            {
                if (aimPos.y < 2f)
                {
                    if (aimPos.y < 4f)
                    {
                        shotType = "Right High Corner Guard";
                    }
                    else
                    {
                        shotType = "Right Corner Guard";
                    }
                }
                else
                {
                    shotType = "Right Tight Corner Guard";
                }
            }
            else
            {
                shotType = "Default - No Shot";
            }
        }


        aiSweep.OnSweep(false, shotType, new Vector2(aimPos.x, aimPos.y), rm.inturn);
    }
}
