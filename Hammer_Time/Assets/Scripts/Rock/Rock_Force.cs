using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;

public class Rock_Force : MonoBehaviour
{
    private Rigidbody2D body;

    public float turnValue = 60f;
    public Vector2 curl;
    public float scaleFactor;
    
    [Header("Physics Tuning")]
    [Tooltip("Spring tension multiplier - affects initial velocity from same pull distance. 0.5 = half tension, 1.0 = normal")]
    public float springTensionMultiplier = 1.0f;
    
    [Tooltip("Curl force multiplier - tune this to maintain trajectory shape at different speeds")]
    public float curlForceMultiplier = 1.0f;
    
    // Sweeping control (modified in real-time by Sweep.cs)
    private Sweep sweepController;

    float velX = 0f;
    float velY = 0f;
    bool turnStart;
    bool forceStart;
    //bool debugVertex;
    public bool flipAxis = false;
    //public bool moving;
    int dirMult = 1;

    GameObject trajLineGO;
    TrajectoryLine trajLine;
    AudioManager am;
    AudioSource[] rockSounds;

    public HapticClip slideHap;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        
        // Find sweep controller to read real-time curl multiplier
        sweepController = FindFirstObjectByType<Sweep>();
        
        // ⚠️ QUICK TEST MODE: Lock all physics multipliers to 1.0 for perfect determinism!
        // This ensures trajectory is 100% predictable and matches the visual preview exactly
        // CRITICAL: Only enable if QuickTestMode is set AND this is a debug game (gsp.debug = true)
        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        bool isQuickTestMode = PlayerPrefs.GetInt("QuickTestMode", 0) == 1 && (gsp != null && gsp.debug);
        
        if (isQuickTestMode)
        {
            springTensionMultiplier = 1.0f;
            curlForceMultiplier = 1.0f;
            Debug.Log($"[Rock_Force] ⚠️ QUICK TEST MODE: Physics multipliers LOCKED to 1.0 (perfect determinism)");
        }

        am = FindFirstObjectByType<AudioManager>();
        rockSounds = GetComponents<AudioSource>();
    }


    public void Release()
    {
        if (flipAxis)
            dirMult = -1;
        else
            dirMult = 1;

        GetComponent<SpriteRenderer>().enabled = true;
        
        // DETERMINISTIC: Restore damping NOW (was disabled during launch)
        // This is when the rock "officially" starts experiencing ice friction
        body.linearDamping = 0.38f;
        body.angularDamping = 0.32f;
        
        Debug.Log($"[Rock_Force Release] Velocity: {body.linearVelocity.magnitude:F2} m/s, flipAxis: {flipAxis}");
        
        // Apply spring tension multiplier if configured (for variance tuning later)
        if (springTensionMultiplier != 1.0f)
        {
            body.linearVelocity *= springTensionMultiplier;
            Debug.Log($"[Rock_Force] Tension multiplier: {springTensionMultiplier:F2}x - Velocity: {body.linearVelocity.magnitude:F2} m/s");
        }
        
        // REAL CURLING: Apply spin NOW (at hog line, not at launch!)
        // This is when the player "releases" the rock, giving it rotation
        turnStart = true;
        forceStart = true;
    }

    // Frame counter for detailed logging
    private int frameCounter = 0;
    private float totalDistance = 0f;
    private Vector2 lastPosition;
    
    void FixedUpdate()
    {
        velX = body.angularVelocity;

        Vector2 vel = new Vector2(velX * scaleFactor, velY);

        float audVel = am.maxVol * (body.linearVelocity.y / 4f) ;
        rockSounds[1].volume = audVel;

        HapticController.Load(slideHap);
        HapticController.Loop(true);
        HapticController.Play();
        HapticController.clipLevel = audVel * 4f;

        if (turnStart == true)
        {
            body.AddTorque(dirMult * turnValue * Mathf.Deg2Rad, ForceMode2D.Impulse);
            turnStart = false;

            // DIAGNOSTIC: Start frame-by-frame logging after hog line
            frameCounter = 0;
            lastPosition = body.position;
            totalDistance = 0f;
            //Debug.Log($"[Rock FixedUpdate] === FRAME-BY-FRAME LOGGING STARTED ===");

            //Debug.Log("vertex 1 is " + body.position.x + ", " + body.position.y + Time.deltaTime);
        }

        if (Mathf.Abs(body.linearVelocity.y) < 0.01f && Mathf.Abs(body.linearVelocity.x) < 0.01f)
        {
            //Debug.Log($"[Rock STOPPED] Final Position: ({body.position.x:F3}, {body.position.y:F3}) | Total Distance: {totalDistance:F3}");
            //Debug.Log("Velocity below 0.01");
            //am.Stop("RockScrape");
            GetComponent<Rock_Info>().stopped = true;
            GetComponent<Rock_Info>().rest = true;
            body.linearDamping = 0.95f;
            HapticController.Stop();
        }

        if (forceStart == true)
        {
            // DIAGNOSTIC: Log every 5 frames (0.1 seconds)
            if (frameCounter % 5 == 0)
            {
                float frameDist = Vector2.Distance(body.position, lastPosition);
                totalDistance += frameDist;
                
                //Debug.Log($"[Rock Frame {frameCounter}] Pos: ({body.position.x:F3}, {body.position.y:F3}) | " +
                //         $"Vel: {body.linearVelocity.magnitude:F3} m/s | " +
                //         $"AngVel: {body.angularVelocity:F2} | " +
                //         $"Damping: {body.linearDamping:F3} | " +
                //         $"FrameDist: {frameDist:F4} | TotalDist: {totalDistance:F3}");
                
                lastPosition = body.position;
            }
            frameCounter++;
            
            // Keep constant friction - no progressive stickiness
            // The linearDamping was set at hog line and stays constant
            
            //Debug.Log("Curl Force");
            // Apply curl force WITH SWEEPING MULTIPLIER!
            // If sweeping, the curl force is amplified (curl) or reduced (line) in REAL-TIME!
            float sweepCurlMult = (sweepController != null && sweepController.isSweeping) 
                ? sweepController.activeCurlMultiplier 
                : 1.0f;
            
            // DIAGNOSTIC: Log when sweeping is active
            if (sweepController != null && sweepController.isSweeping && frameCounter % 30 == 0)
            {
                Debug.Log($"[Rock_Force SWEEPING] Frame {frameCounter}: sweepMult={sweepCurlMult:F2}, curl.x={curl.x:F3}, angularVel={body.angularVelocity:F2}");
            }
            
            Vector2 scaledCurl = curl * curlForceMultiplier * sweepCurlMult; // ? SWEEPING MODIFIES CURL FORCE HERE!
            body.AddForce(scaledCurl * vel, ForceMode2D.Force);
            
            //Debug.Log("curl is " + curl.x);
            if (Mathf.Abs(body.linearVelocity.y) < 0.01f && Mathf.Abs(body.linearVelocity.x) < 0.01f)
            {
                //Debug.Log($"[Rock STOPPED] Final Position: ({body.position.x:F3}, {body.position.y:F3}) | Total Distance: {totalDistance:F3}");
                //Debug.Log("Velocity below 0.01");
                //am.Stop("RockScrape");
                GetComponent<Rock_Info>().stopped = true;
                GetComponent<Rock_Info>().rest = true;
                body.linearDamping = 0.95f;
                HapticController.Stop();
            }

            //if (debugVertex)
            //{
            //    if (body.velocity.x <= 0.01f)
            //    {
            //        Debug.Log("vertex 1 is " + body.position.x + ", " + body.position.y + Time.deltaTime);
            //        debugVertex = false;
            //    }
            //}
            
        }

    }

}
