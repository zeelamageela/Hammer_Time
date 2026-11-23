using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;

public class Rock_Force : MonoBehaviour
{
    private Rigidbody2D body;

    [Tooltip("UNUSED since the shared physics model: spin at the hog line is RockPhysicsProfile.spinOnRelease.")]
    public float turnValue = 60f;
    [Tooltip("Read-only mirror of the curl the model is using (set at release).")]
    public Vector2 curl;
    [Tooltip("UNUSED since the shared physics model: see RockPhysicsProfile.curlScale.")]
    public float scaleFactor;
    
    [Header("Physics Tuning")]
    [Tooltip("Spring tension multiplier - affects initial velocity from same pull distance. 0.5 = half tension, 1.0 = normal")]
    public float springTensionMultiplier = 1.0f;
    
    [Tooltip("Curl force multiplier - tune this to maintain trajectory shape at different speeds")]
    public float curlForceMultiplier = 1.0f;
    
    // Sweeping control (modified in real-time by Sweep.cs)
    private Sweep sweepController;

    //bool debugVertex;
    public bool flipAxis = false;
    //public bool moving;

    // Friction, spin and curl are applied by RockSlide (the shared RockPhysics model), which also drives the
    // trajectory simulator. This component decides when a rock is released and keeps the audio/haptics/rest flags.
    RockSlide slide;
    bool released;

    GameObject trajLineGO;
    TrajectoryLine trajLine;
    AudioManager am;
    AudioSource[] rockSounds;

    public HapticClip slideHap;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        slide = GetComponent<RockSlide>();
        if (slide == null) slide = gameObject.AddComponent<RockSlide>();
        
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

    void OnDisable()
    {
        // CRITICAL: Stop any looping haptics when rock is disabled
        // This prevents the slide haptic from continuing after scene changes or rock pooling
        HapticController.Loop(false);
        HapticController.Stop();
    }

    void OnDestroy()
    {
        // CRITICAL: Stop any looping haptics when rock is destroyed/game ends
        // This prevents the slide haptic from continuing after game reset
        HapticController.Loop(false);
        HapticController.Stop();
    }


    public void Release()
    {
        GetComponent<SpriteRenderer>().enabled = true;

        // Drag and spin now start (they were off during the launch). RockSlide applies them from here on.
        Debug.Log($"[Rock_Force Release] Velocity: {body.linearVelocity.magnitude:F2} m/s, flipAxis: {flipAxis}");

        // Apply spring tension multiplier if configured (for variance tuning later)
        if (springTensionMultiplier != 1.0f)
        {
            body.linearVelocity *= springTensionMultiplier;
            Debug.Log($"[Rock_Force] Tension multiplier: {springTensionMultiplier:F2}x - Velocity: {body.linearVelocity.magnitude:F2} m/s");
        }

        // In real curling FAST rocks curl LESS and SLOW rocks curl MORE: the strength is picked from the launch speed
        // inside the shared model (RockPhysicsProfile.CurlMagnitude), using the same speed range as the trajectory preview.
        TrajectoryLine trajLine = FindFirstObjectByType<TrajectoryLine>();
        float minVelocity = (trajLine != null) ? trajLine.minVelocity : 1f;
        float maxVelocity = (trajLine != null) ? trajLine.maxVelocity : 8f;

        // In-turn (flipAxis) spins clockwise and curls right; out-turn the opposite. Spin is applied on the next step.
        slide.Release(flipAxis, minVelocity, maxVelocity);
        curl = slide.Curl;
        released = true;

        Debug.Log($"[Rock_Force Curl] flipAxis={flipAxis}, launch speed {body.linearVelocity.magnitude:F2}, curl.x={curl.x:F3}");
    }

    void FixedUpdate()
    {
        Vector2 vel = body.linearVelocity;

        float audVel = am.maxVol * (vel.y / 4f);
        rockSounds[1].volume = audVel;

        HapticController.Load(slideHap);
        HapticController.Loop(true);
        HapticController.Play();
        HapticController.clipLevel = audVel * 4f;

        // Sweeping can strengthen or weaken curl in real time; RockSlide applies the result.
        float sweepCurlMult = (sweepController != null && sweepController.isSweeping)
            ? sweepController.activeCurlMultiplier
            : 1.0f;
        slide.CurlMultiplier = curlForceMultiplier * sweepCurlMult;

        if (Mathf.Abs(vel.y) < 0.01f && Mathf.Abs(vel.x) < 0.01f)
        {
            if (released) ShotAudit.LogRest(body.position);
            GetComponent<Rock_Info>().stopped = true;
            GetComponent<Rock_Info>().rest = true;
            HapticController.Stop();
        }
    }

}
