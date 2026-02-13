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
    
    [Tooltip("Ice friction multiplier - lower = less friction. Tune this to match distance at lower spring tension.")]
    public float iceFrictionMultiplier = 1.0f;
    
    [Tooltip("Curl force multiplier - tune this to maintain trajectory shape at different speeds")]
    public float curlForceMultiplier = 1.0f;
    
    [Tooltip("Base linear damping from Rigidbody2D (auto-captured on start)")]
    [SerializeField] private float baseDamping = 0.38f;

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
        
        // Capture the base damping from Rigidbody2D component
        baseDamping = body.linearDamping;
        
        // Apply ice friction multiplier to damping
        body.linearDamping = baseDamping * iceFrictionMultiplier;
        
        Debug.Log($"[Rock_Force] Base Damping: {baseDamping:F3}, Ice Friction Mult: {iceFrictionMultiplier:F2}, Final Damping: {body.linearDamping:F3}");

        am = FindFirstObjectByType<AudioManager>();
        rockSounds = GetComponents<AudioSource>();
    }


    public void Release()
    {
        if (flipAxis)
            dirMult = -1;
        else
            dirMult = 1;
        Debug.Log("flipAxis is " + flipAxis);

        GetComponent<SpriteRenderer>().enabled = true;
        
        // Apply spring tension multiplier to initial velocity from spring
        // This allows same pullback distance to produce less velocity
        if (springTensionMultiplier != 1.0f)
        {
            body.linearVelocity *= springTensionMultiplier;
            Debug.Log($"[Rock_Force] Spring tension: {springTensionMultiplier:F2}x - Velocity: {body.linearVelocity.magnitude:F2} m/s");
        }
        
        turnStart = true;
        forceStart = true;
        //debugVertex = true;
        return;
    }

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
            //Debug.Log("Rotate");
            turnStart = false;

            //Debug.Log("vertex 1 is " + body.position.x + ", " + body.position.y + Time.deltaTime);
        }

        if (forceStart == true)
        {
            //Debug.Log("Curl Force");
            // Apply curl force multiplier for trajectory tuning
            Vector2 scaledCurl = curl * curlForceMultiplier;
            body.AddForce(scaledCurl * vel, ForceMode2D.Force);
            
            //Debug.Log("curl is " + curl.x);
            if (Mathf.Abs(body.linearVelocity.y) < 0.01f && Mathf.Abs(body.linearVelocity.x) < 0.01f)
            {
                //Debug.Log("Velocity below 0.01");
                //am.Stop("RockScrape");
                GetComponent<Rock_Info>().stopped = true;
                GetComponent<Rock_Info>().rest = true;
                body.linearDamping = 0.55f;
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
