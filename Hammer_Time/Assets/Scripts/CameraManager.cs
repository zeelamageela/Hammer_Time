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

    // Composition easing state
    private bool isEasingComposition = false;
    private float compositionEaseStartY;
    private float compositionEaseStartScreenY;
    private float compositionEaseTargetScreenY = 0.35f;
    private Transform currentFollowTarget;
    private Coroutine deadZoneShrinkCoroutine;

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
        
        // Smooth composition easing during rock follow
        // CRITICAL: Only ease when rock is in the transition zone (between start and y=-3)
        // Stop easing once InPlayZoom takes over or rock passes y=-3
        if (isEasingComposition && currentFollowTarget != null && vcam_ft != null)
        {
            float rockY = currentFollowTarget.position.y;
            
            // Ease from start position to target (0.35f) as rock moves from start to y=-3
            // This creates a smooth transition instead of a snap
            if (rockY <= -3f)
            {
                // Reached target Y, lock to final composition and stop easing
                var composition = vcam_ft.Composition;
                composition.ScreenPosition = new Vector2(0f, compositionEaseTargetScreenY);
                vcam_ft.Composition = composition;
                isEasingComposition = false;
                Debug.Log("[CameraManager] Composition easing complete at y=-3");
            }
            else if (rockY < compositionEaseStartY)
            {
                // Ease based on rock's Y position
                float t = Mathf.InverseLerp(compositionEaseStartY, -3f, rockY);
                t = Mathf.SmoothStep(0f, 1f, t); // Smooth easing curve
                
                float easedScreenY = Mathf.Lerp(compositionEaseStartScreenY, compositionEaseTargetScreenY, t);
                
                var composition = vcam_ft.Composition;
                composition.ScreenPosition = new Vector2(0f, easedScreenY);
                vcam_ft.Composition = composition;
            }
        }
    }

    public void ShotSetup()
    {
        vcam.enabled = false;
        
        // Cinemachine 3.x: Lens properties
        var lens = vcam.Lens;
        lens.OrthographicSize = 8.5f;
        vcam.Lens = lens;
        
        Debug.Log("Shot Setup");
        main.depth = 1;
        house.depth = -1;
        ui.depth = 3;
        top.depth = 2;
        aim.depth = -1;

        main.rect = new Rect(new Vector2(0f, 0f), new Vector2(1f, 1f));
        
        // Smooth 0.8s transition from rock stopped position to shot setup
        if (vcam_ft != null)
        {
            // Damping of 1.2 gives approximately 0.8s smooth transition
            vcam_ft.Damping = new Vector3(1.2f, 1.2f, 1.2f);
        }

        vcam_ft.TargetOffset = new Vector3(0f, -12.5f, 0f); // Offset to keep rock centered

        vcam.Target.TrackingTarget = launcher;
        
        // Disable composition easing (not needed during setup)
        isEasingComposition = false;
        
        vcam.enabled = true;
    }

    public void Trajectory()
    {
        //Debug.Log("Trajectory");
        main.depth = 1;
        house.depth = -1;
        ui.depth = 3;

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

    public void RockFollow(Transform tFollowTarget)
    {
        //Debug.Log("Rock Follow");
        
        // CRITICAL: Lock camera X position to 0 IMMEDIATELY
        // This prevents Cinemachine from tracking rock's X position
        Vector3 camPos = vcam.transform.position;
        camPos.x = 0f;
        vcam.transform.position = camPos;
        
        // Cinemachine 3.x: Configure to follow rock AND keep entire play area visible!
        if (vcam_ft != null)
        {
            // LOCKED X-AXIS: Camera stays centered (X=0), only follows Y-axis
            // Damping X=9999 means camera will NEVER follow X axis
            vcam_ft.Damping = new Vector3(9999f, 0f, 9999f); // X/Z locked completely, Y follows instantly
            
            // Configure composition to keep entire play area in frame
            var composition = vcam_ft.Composition;
            
            // START EASING: Calculate where rock currently is on screen
            Vector3 rockScreenPos = main.WorldToViewportPoint(tFollowTarget.position);
            compositionEaseStartScreenY = rockScreenPos.y;
            compositionEaseStartY = tFollowTarget.position.y;
            compositionEaseTargetScreenY = 0.35f; // Target: lower third of screen
            currentFollowTarget = tFollowTarget;
            isEasingComposition = true;
            
            // Start at current position, will ease to target in Update()
            composition.ScreenPosition = new Vector2(0f, compositionEaseStartScreenY);
            
            // LARGER INITIAL DEAD ZONE: Prevents hitch when transitioning from pullback to follow
            // This gives the rock some "breathing room" before camera starts chasing
            composition.DeadZone = new ScreenComposerSettings.DeadZoneSettings
            {
                Enabled = true,
                Size = new Vector2(0.3f, 0.3f) // Larger dead zone on release (was 0.05f)
            };
            
            // Wide hard limits - let rock move across screen freely
            composition.HardLimits = new ScreenComposerSettings.HardLimitSettings
            {
                Enabled = true,
                Size = new Vector2(0.95f, 0.95f), // Almost full screen
                Offset = Vector2.zero
            };
            
            vcam_ft.Composition = composition;
            
            // Set orthographic size to show full play area (hog line to back of house)
            // Distance from hog line (y=-4) to back of house (y=8) = 12 units
            // Add buffer: ortho size = 7 shows ~14 units vertically
            vcam_ft.CameraDistance = 10f;
            
            var lens = vcam.Lens;
            lens.OrthographicSize = 7f; // Shows full play area!
            vcam.Lens = lens;
            
            // CRITICAL: No target offset (prevents rotation following!)
            vcam_ft.TargetOffset = Vector3.zero;
        }
        
        aim.depth = -1;
        main.depth = 1;
        house.depth = -1;
        top.depth = -1;
        ui.depth = 3;

        // Cinemachine 3.x: Track POSITION only (not rotation!)
        vcam.Target.TrackingTarget = tFollowTarget;
        
        // CRITICAL: Lock camera rotation to prevent spinning with rock!
        if (vcam != null)
        {
            vcam.transform.rotation = Quaternion.identity; // Lock to world up
        }
        
        vcam.enabled = true;
        
        // Start dead zone shrinking coroutine for smooth transition
        if (deadZoneShrinkCoroutine != null)
        {
            StopCoroutine(deadZoneShrinkCoroutine);
        }
        deadZoneShrinkCoroutine = StartCoroutine(ShrinkDeadZone());
        
        Debug.Log($"[CameraManager] RockFollow started - initial screen Y: {compositionEaseStartScreenY:F2}, will ease to {compositionEaseTargetScreenY:F2}");
    }

    public void RockZoom(Transform tFollowTarget)
    {
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
    
    private IEnumerator ShrinkDeadZone()
    {
        // Gradually shrink dead zone from large (0.3) to small (0.05) over 1 second
        // This prevents the initial hitch while still allowing responsive tracking later
        float elapsed = 0f;
        float duration = 1.0f;
        float startSize = 0.3f;
        float endSize = 0.05f;
        
        while (elapsed < duration && vcam_ft != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Ease-out curve for smooth shrinking
            t = 1f - Mathf.Pow(1f - t, 2f);
            
            float currentSize = Mathf.Lerp(startSize, endSize, t);
            
            var composition = vcam_ft.Composition;
            composition.DeadZone = new ScreenComposerSettings.DeadZoneSettings
            {
                Enabled = true,
                Size = new Vector2(currentSize, currentSize)
            };
            vcam_ft.Composition = composition;
            
            yield return null;
        }
        
        Debug.Log("[CameraManager] Dead zone shrink complete - now using tight tracking");
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
        // CRITICAL: Disable composition easing when InPlayZoom takes over
        // This prevents fighting between easing logic and zoom logic
        isEasingComposition = false;
        
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
        Debug.Log("House View");

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

    public void TopViewAuto()
    {
        Debug.Log("Top View Auto");

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
        Debug.Log("Top View");

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
