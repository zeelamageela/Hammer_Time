using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public Camera main;
    public Camera house;
    public Camera ui;
    public Camera top;
    public Camera persp;
    public Camera aim;

    public Transform aimMover;
    public Transform trajTarget;
    public Transform launcher;
    public GameManager gm;

    public GameObject vcam_go;
    public CinemachineCamera vcam;
    public CinemachinePositionComposer vcam_ft;

    public HouseClick houseClick;

    // Following a rock (see RockFollow) and framing the next shot (see ShotSetup): the virtual camera sits exactly on
    // an anchor that this script moves
    const float ShotSize = 8.5f;          // orthographic size while aiming
    const float ShotViewLead = 1f;        // camera height over the launcher while aiming: hack mid-screen, room below to pull back
    const int LivePriority = 20;          // above the scene's other virtual camera ("CM vcam2", which tracks the aim circle)
    const float FollowSize = 7f;          // orthographic size while following
    const float FollowLead = 4.2f;        // how far above the rock the camera settles (a fifth up the screen, over the score bar)
    const float FollowSettleDistance = 10f; // a thrown rock is framed after travelling this far...
    const float FollowSettleSeconds = 1f;   // ...one picked up further down the ice, after this long
    const float ThrownFromY = -20f;         // a follow starting below this is a delivery
    const float FurthestBehindHack = 1f;    // about as far behind the launcher as the shooter is ever drawn holding the rock
    CinemachineConfiner2D confiner;
    float confinerDamping;
    Transform followed, followAnchor;
    float followStartY, followStartLead, followStartSize, followStartTime;

    private void Start()
    {
        // Cinemachine 3.x: Get component directly from the vcam GameObject
        if (vcam != null && vcam_ft == null)
        {
            vcam_ft = vcam.GetComponent<CinemachinePositionComposer>();
            
            if (vcam_ft == null)
            {
                Debug.LogWarning("[CameraManager] CinemachinePositionComposer not found on vcam! Add it manually in Inspector.");
            }
        }
        
        main.depth = 1;
        house.depth = -1;
        ui.depth = 3;
        top.depth = 2;

        if (vcam != null) vcam.Priority = LivePriority;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            TopView();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HouseView();
        }

        // CRITICAL: Lock camera X position to 0 when following a rock
        // This prevents runaway camera when rocks go out of bounds
        if (vcam != null && vcam.enabled && vcam.Target.TrackingTarget != launcher)
        {
            Vector3 camPos = vcam.transform.position;
            if (Mathf.Abs(camPos.x) > 0.01f) // If camera drifted from center
            {
                camPos.x = 0f; // Force back to center
                vcam.transform.position = camPos;
            }
        }

        // Update screen position dynamically ONLY when not actively following a rock
        // This prevents constant composition changes that cause jitter
        if (vcam_ft != null && vcam.Target.TrackingTarget == launcher)
        {
            var composition = vcam_ft.Composition;
            float newY = Mathf.Lerp(0.85f, 0.75f, (-10f - main.transform.position.y) / 6f);
            
            // Only update if changed significantly (avoid micro-updates)
            if (Mathf.Abs(composition.ScreenPosition.y - newY) > 0.01f)
            {
                composition.ScreenPosition = new Vector2(composition.ScreenPosition.x, newY);
                vcam_ft.Composition = composition;
            }
        }
        
        FollowRock();
    }

    /// <summary>
    /// Hands the camera to the anchor: it sits exactly on it, X centred, with nothing else (screen offset, dead zone, limits,
    /// damping) pulling it about. The edge confiner still keeps it on the ice, but without its damping, which carried one
    /// view's correction into the next and let it go over seconds.
    /// </summary>
    void RideAnchor()
    {
        if (vcam_ft != null)
        {
            var composition = vcam_ft.Composition;
            composition.ScreenPosition = Vector2.zero;
            composition.DeadZone = new ScreenComposerSettings.DeadZoneSettings { Enabled = false, Size = Vector2.zero };
            composition.HardLimits = new ScreenComposerSettings.HardLimitSettings { Enabled = false, Size = Vector2.one, Offset = Vector2.zero };
            vcam_ft.Composition = composition;
            vcam_ft.Damping = Vector3.zero;
            vcam_ft.CameraDistance = 10f;
            vcam_ft.TargetOffset = Vector3.zero;
        }

        if (confiner == null)
        {
            confiner = vcam.GetComponent<CinemachineConfiner2D>();
            if (confiner != null) confinerDamping = confiner.Damping;
        }
        if (confiner != null) confiner.Damping = 0f;

        vcam.Target.TrackingTarget = followAnchor;
        vcam.transform.rotation = Quaternion.identity;
        // This has to be the camera on screen. It used to win that by being switched off and on again for every shot.
        vcam.Priority = LivePriority;
        vcam.enabled = true;
    }

    /// <summary>
    /// Moves the anchor with the rock. It starts exactly where the camera is, so there is no jump and no wait, and eases the
    /// rock into its place on screen as it travels.
    /// </summary>
    void FollowRock()
    {
        if (followed == null) return;
        float y = FollowY(followed);
        float t = followStartY < ThrownFromY
            ? Mathf.InverseLerp(followStartY, followStartY + FollowSettleDistance, y)
            : Mathf.Clamp01((Time.time - followStartTime) / FollowSettleSeconds);
        t = Mathf.SmoothStep(0f, 1f, t);

        followAnchor.position = new Vector3(0f, y + Mathf.Lerp(followStartLead, FollowLead, t), followed.position.z);

        var lens = vcam.Lens;
        lens.OrthographicSize = Mathf.Lerp(followStartSize, FollowSize, t);
        vcam.Lens = lens;
    }

    /// <summary>
    /// The height the camera frames a rock at. A pulled-back rock can sit well behind the hack until the push-off moves it
    /// up to where the shooter is drawn holding it (see RockSlide.PushOff); the camera ignores that stretch, so it never jumps.
    /// </summary>
    float FollowY(Transform rock)
    {
        return Mathf.Max(rock.position.y, launcher.position.y - FurthestBehindHack);
    }

    /// <summary>Hands the camera back to whatever frames it next.</summary>
    void StopFollowing()
    {
        followed = null;
        if (confiner != null) confiner.Damping = confinerDamping;
    }

    /// <summary>
    /// Cuts straight to the view for the next shot. A player gets the hack mid-screen, with room below it to pull back. A
    /// computer turn needs no room, so it opens on the view the rock is followed with: the hack near the bottom.
    /// </summary>
    public void ShotSetup(bool computerTurn = false)
    {
        StopFollowing();
        if (followAnchor == null) followAnchor = new GameObject("Camera Follow Anchor").transform;

        float lead = computerTurn ? FollowLead : ShotViewLead;
        followAnchor.position = new Vector3(0f, launcher.position.y + lead, launcher.position.z);
        var lens = vcam.Lens;
        lens.OrthographicSize = computerTurn ? FollowSize : ShotSize;
        vcam.Lens = lens;
        RideAnchor();
        vcam.PreviousStateIsValid = false;   // a cut: nothing carries over from the view before

        GameLog.Info("Shot Setup");
        main.depth = 1;
        house.depth = -1;
        ui.depth = 3;
        top.depth = 2;
        aim.depth = -1;

        main.rect = new Rect(new Vector2(0f, 0f), new Vector2(1f, 1f));
    }

    public void PowerPhaseSetup()
    {
        StopFollowing();
        // Configure camera for flick shot power phase (drag input from launcher to hog line)
        // Shows Y=-25 (launcher) to Y=-16 (hog line) = 9 units vertical span
        vcam.enabled = false;
        
        // Set orthographic size to comfortably fit drag zone with buffer
        // 9 units of drag zone + buffer = 6.5 ortho size (shows ~13 units vertically)
        var lens = vcam.Lens;
        lens.OrthographicSize = 6.5f;
        vcam.Lens = lens;
        
        GameLog.Info("[CameraManager] Power Phase Setup - Framing drag zone (Y=-25 to Y=-16)");
        
        // Configure camera depths
        main.depth = 1;
        house.depth = -1;
        ui.depth = 3;
        top.depth = 2;
        aim.depth = -1;  // Disable aim camera
        
        main.rect = new Rect(new Vector2(0f, 0f), new Vector2(1f, 1f));
        
        // Instant transition (no damping) for immediate power phase feedback
        if (vcam_ft != null)
        {
            vcam_ft.Damping = new Vector3(0f, 0f, 0f);  // Instant positioning
            
            // Position launcher low on screen (bottom quarter) so player sees full drag path above
            var composition = vcam_ft.Composition;
            float screenY = 0.20f;  // Bottom quarter positioning
            composition.ScreenPosition = new Vector2(0f, screenY);
            
            // Calculate target offset to align screen position correctly
            // This ensures the launcher appears at the specified screen position
            float targetOffsetY = -(lens.OrthographicSize * (1f - screenY));
            vcam_ft.TargetOffset = new Vector3(0f, targetOffsetY, 0f);
            
            // Tight dead zone for responsive camera during power input
            composition.DeadZone = new ScreenComposerSettings.DeadZoneSettings
            {
                Enabled = true,
                Size = new Vector2(0.05f, 0.05f)  // Tight dead zone
            };
            
            // Standard hard limits
            composition.HardLimits = new ScreenComposerSettings.HardLimitSettings
            {
                Enabled = true,
                Size = new Vector2(0.9f, 0.9f),
                Offset = Vector2.zero
            };
            
            vcam_ft.Composition = composition;
        }
        
        // Track launcher position (where player starts power drag)
        vcam.Target.TrackingTarget = launcher;
        
        vcam.enabled = true;
        
        GameLog.Info($"[CameraManager] Power phase camera active - Ortho size: {lens.OrthographicSize}, Screen Y: 0.20");
    }
    
    // Power phase constants (match FlickShotController values)
    private float powerDragStartY = -25f;  // Launcher Y
    private float powerDragTargetY = -16f;  // Hog line Y

    public void Trajectory()
    {
        //Debug.Log("Trajectory");
        main.depth = 1;
        house.depth = -1;

        aim.orthographicSize = Mathf.Lerp(3.5f, 4.2f, (trajTarget.position.y) / 10f);

        if (trajTarget.position.y > 5.75f)
        {
            aim.depth = 3;
            aimMover.position = new Vector3(0f, 5.75f, 0f);
        }
        else if (trajTarget.position.y > 3.5f)
        {
            aim.depth = 3;
            aimMover.position = new Vector3(0f, trajTarget.position.y, 0f);
        }
        else if (trajTarget.position.y > 0f)
        {
            aim.depth = 3;
            aimMover.position = new Vector3(0f, 3.5f, 0f);
        }
        else
        {
            aim.depth = -1;
            aimMover.position = new Vector3(0f, 3.5f, 0f);
        }

        // UI camera must always render above the aim camera.
        // When aim is active (depth 3), push UI to depth 4 so the spotlight
        // overlay and other HUD elements composite on top of the aim camera view.
        ui.depth = aim.depth > 0 ? aim.depth + 1 : 3;
        //vcam.LookAt = trajTarget;
        //vcam.Follow = trajTarget;
        //vcam.enabled = true;
    }

    public void Perspective()
    {
        if (persp.depth == 5)
        {
            persp.depth = -1;
        }
        else if (persp.depth == -1)
        {
            persp.depth = 5;
        }
    }

    /// <summary>Follows a rock up and down the ice, starting at once from wherever the camera is now.</summary>
    public void RockFollow(Transform tFollowTarget)
    {
        if (followAnchor == null) followAnchor = new GameObject("Camera Follow Anchor").transform;

        followed = tFollowTarget;
        followStartY = FollowY(tFollowTarget);
        followStartLead = main.transform.position.y - followStartY;
        followStartSize = main.orthographicSize;
        followStartTime = Time.time;
        FollowRock();
        RideAnchor();

        aim.depth = -1;
        main.depth = 1;
        house.depth = -1;
        top.depth = -1;
        ui.depth = 3;
    }

    public void RockZoom(Transform tFollowTarget)
    {
        StopFollowing();
        main.depth = 1;
        house.depth = -1;
        top.depth = -1;
        ui.depth = 3;
        
        // Cinemachine 3.x: Target tracking
        vcam.Target.TrackingTarget = tFollowTarget;
        vcam.enabled = true;

        // Cinemachine 3.x: Lens properties
        var lens = vcam.Lens;
        lens.OrthographicSize = 3f;
        vcam.Lens = lens;
    }
    
    public void ZoomOutTrack(float dist)
    {
        // Cinemachine 3.x: Lens is a struct, must get/set
        var lens = vcam.Lens;
        lens.OrthographicSize = ((8.5f - 3.5f) * ((dist) / 3.5f)) + 3.5f;
        vcam.Lens = lens;
    }
    public void InPlayZoom(float dist)
    {
        StopFollowing();
        
        // Cinemachine 3.x: Smooth zoom based on distance from house
        var lens = vcam.Lens;
        float orthoSize = ((8.5f - 4.5f) * ((dist) / 10.5f)) + 4.5f;
        lens.OrthographicSize = orthoSize;
        vcam.Lens = lens;
        
        // DYNAMIC SCREEN POSITION: Rock moves up screen as it gets closer to house
        if (vcam_ft != null)
        {
            // As dist decreases (rock gets closer to house), rock moves higher on screen
            // dist = 10.5 (far from house) ? screen Y = 0.15 (bottom of screen)
            // dist = 0 (at house) ? screen Y = 0.25 (quarter from bottom)
            // FIXED: Lerp was backwards - start at 0.15, end at 0.25
            float screenY = Mathf.Lerp(0.25f, 0.15f, dist / 10.5f);
            
            var composition = vcam_ft.Composition;
            composition.ScreenPosition = new Vector2(0f, screenY);
            vcam_ft.Composition = composition;

            // Calculate target offset so y=0 stays at bottom of screen
            float targetOffsetY = -(orthoSize * (1f - screenY));
            vcam_ft.TargetOffset = new Vector3(0f, targetOffsetY, 0f);

            // Hard limits: Max distance before camera MUST adjust
            composition.HardLimits = new ScreenComposerSettings.HardLimitSettings
            {
                Enabled = true,
                Size = new Vector2(2f, 0.01f), // Very wide limits - let rock move freely
                Offset = Vector2.zero
            };
            
            vcam_ft.Composition = composition;
        }
    }

    public void HouseView()
    {
        GameLog.Info("House View");

        if (house.depth == 5)
        {
            ui.depth = 3;
            house.depth = -1;
        }
        else if (house.depth == -1)
        {
            ui.depth = 6;
            house.depth = 5;
        }
    }

    public void HouseViewOn()
    {
        GameLog.Info("[CameraManager] HouseViewOn");
        house.depth = 5;
        ui.depth = 6;
    }

    public void HouseViewOff()
    {
        GameLog.Info("[CameraManager] HouseViewOff");
        house.depth = -1;
        ui.depth = 3;
    }

    public void TopViewAuto()
    {
        GameLog.Info("Top View Auto");

        if (top.depth == 2)
        {
            top.depth = -1;
        }
        else if (top.depth == -1)
        {
            top.depth = 2;
        }
    }

    public void TopView()
    {
        GameLog.Info("Top View");

        if (top.depth == 2)
        {
            top.depth = -1;
        }
        else if (top.depth == -1)
        {
            top.depth = 2;
        }
    }
}
