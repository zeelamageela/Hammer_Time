using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;

public class Rock_Colliders : MonoBehaviour
{
    public Collider2D InPlay_Collider;
    public Collider2D boards_collider;
    public Collider2D house_collider;
    public Collider2D launchCollider;

    private Rigidbody2D body;

    public bool outOfPlay = false;
    public bool inPlay = false;
    public bool hit = false;
    public bool inHouse = false;
    public bool shotTaken = false;
    public bool guard = false;
    private bool outOfPlayCoroutineStarted = false;  // NEW: Track if coroutine already running
    private float fixedDeltaTime;

    AudioManager am;
    SweeperManager sm;
    GameManager gm;
    AudioSource[] rockSounds;

    public HapticClip outHap;
    public HapticClip sideHap;
    public HapticClip hitHap;

// Start is called before the first frame update
void Awake()
{
    body = GetComponent<Rigidbody2D>();

    GameObject InPlay = GameObject.Find("InPlay_Collider");
    InPlay_Collider = InPlay.GetComponent<Collider2D>();

    GameObject boards = GameObject.Find("BG/Boards_CREATED");
    boards_collider = boards.GetComponent<Collider2D>();

    GameObject house = GameObject.Find("House");
    house_collider = house.GetComponent<Collider2D>();

    GameObject launch = GameObject.Find("Launcher");
    launchCollider = launch.GetComponent<Collider2D>();
    launchCollider.enabled = false;

    rockSounds = GetComponents<AudioSource>();
    am = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    sm = GameObject.FindGameObjectWithTag("SweeperManager").GetComponent<SweeperManager>();
    gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    fixedDeltaTime = Time.fixedDeltaTime;
}
    
/// <summary>
/// Reset flags when rock is re-enabled (pooled rocks get reused)
/// </summary>
void OnEnable()
{
    outOfPlayCoroutineStarted = false;
    outOfPlay = false;
}

    // Update is called once per frame
    void Update()
    {
        // CRITICAL FIX: Only start OutOfPlay coroutine ONCE
        // Don't call it every frame - it causes RockFollow() to be called repeatedly
        if (outOfPlay && !outOfPlayCoroutineStarted)
        {
            outOfPlayCoroutineStarted = true;
            StartCoroutine(OutOfPlay());
        }

        if (inPlay & !inHouse & body.position.x <= 6.5f)
        {
            guard = true;

        }
        //if (gameObject.transform.position.y >= -3f)
        //{
        //    Debug.Log("-3 velocity is " + GetComponent<Rigidbody2D>().velocity.x + ", " + GetComponent<Rigidbody2D>().velocity.y);
        //}

        // CRITICAL FIX: Only start OutOfPlay ONCE when rock stops past y=0
        if (GetComponent<Rock_Info>().shotTaken && GetComponent<Rock_Info>().stopped && transform.position.y < 0f && !outOfPlayCoroutineStarted)
        {
            outOfPlayCoroutineStarted = true;
            StartCoroutine(OutOfPlay());
        }
    }

    IEnumerator OutOfPlay()
    {
        HapticController.Play(outHap);
        //Handheld.Vibrate();
        outOfPlay = true;
        //am.Play("OutOfPlay");
        //body.velocity = Vector2.zero;
        //body.angularVelocity = 0f;
        GetComponent<Collider2D>().enabled = false;

        GetComponent<Rock_Info>().stopped = true;
        GetComponent<Rock_Info>().rest = true;


        if (gm.rockCurrent == GetComponent<Rock_Info>().rockIndex)
        {
            for (int i = 0; i < gm.rockCurrent; i++)
            {
                if (gm.rockList[i].rockInfo.moving && gm.rockList[i].rockInfo.inPlay)
                {
                    gm.cm.RockFollow(gm.rockList[i].rock.transform);
                    //gm.cm.HouseView();
                }
            }
        }

        gameObject.GetComponent<Animator>().enabled = true;

        yield return new WaitForSeconds(0.5f);
        body.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }


    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == launchCollider)
        {
            //Debug.Log("Shot is taken");
            shotTaken = true;
            launchCollider.enabled = false;
            //Debug.Log("Hogline velocity is " + GetComponent<Rigidbody2D>().velocity.x + ", " + GetComponent<Rigidbody2D>().velocity.y);
        }
        if (collider == InPlay_Collider)
        {
            Debug.Log("rock is in play");
            inPlay = true;

            Debug.Log("Far Hogline velocity is " + GetComponent<Rigidbody2D>().linearVelocity.x + ", " + GetComponent<Rigidbody2D>().linearVelocity.y);
        }

        if (collider == boards_collider)
        {
            outOfPlay = true;
            inPlay = false;
            Debug.Log("trigger boards");
            HapticController.Play(sideHap);
        }

        if (collider == house_collider)
        {
            inHouse = true;
            Debug.Log("In House");
            Debug.Log("House velocity is " + GetComponent<Rigidbody2D>().linearVelocity.x + ", " + GetComponent<Rigidbody2D>().linearVelocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rock"))
        {
            // REAL CURLING PHYSICS: Check for frozen rocks (rocks touching each other)
            // A "frozen" rock is one that's stationary AND very close to another rock
            Rigidbody2D thisRB = GetComponent<Rigidbody2D>();
            Rigidbody2D otherRB = collision.rigidbody;
            Rock_Info otherRockInfo = collision.gameObject.GetComponent<Rock_Info>();
            
            bool thisWasStationary = thisRB.linearVelocity.magnitude < 0.1f;
            bool otherWasStationary = otherRB.linearVelocity.magnitude < 0.1f;
            
            // FROZEN ROCK DETECTION: Check if there's ANOTHER rock touching the one we just hit
            // This indicates a "frozen pair" - two rocks sitting together
            bool otherRockIsFrozen = false;
            GameObject frozenPartner = null;
            
            if (otherWasStationary)
            {
                // Check for nearby rocks (within touching distance)
                Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(
                    collision.transform.position, 
                    0.32f, // Slightly more than 2x rock radius (0.14 * 2 = 0.28)
                    LayerMask.GetMask("Default") // Adjust layer if needed
                );
                
                foreach (Collider2D nearby in nearbyColliders)
                {
                    if (nearby.gameObject == collision.gameObject || nearby.gameObject == gameObject)
                        continue; // Skip self and the rock we just hit
                    
                    if (nearby.CompareTag("Rock"))
                    {
                        Rock_Info nearbyInfo = nearby.GetComponent<Rock_Info>();
                        if (nearbyInfo != null && nearbyInfo.inPlay && nearbyInfo.stopped)
                        {
                            otherRockIsFrozen = true;
                            frozenPartner = nearby.gameObject;
                            Debug.Log($"[Frozen Rock] Detected frozen pair: {collision.gameObject.name} + {nearby.gameObject.name}");
                            break;
                        }
                    }
                }
            }
            
            // FROZEN ROCK COLLISION RESPONSE
            if (otherRockIsFrozen && frozenPartner != null)
            {
                Debug.Log($"[Frozen Rock] Applying realistic frozen rock physics");
                
                // Get collision data
                Vector2 collisionNormal = collision.GetContact(0).normal;
                Vector2 impactVelocity = thisRB.linearVelocity;
                float impactSpeed = impactVelocity.magnitude;
                
                // REALISTIC FROZEN ROCK BEHAVIOR:
                // 1. Struck rock (other) stays mostly still (absorbs ~15% of momentum)
                // 2. Frozen partner (third rock) receives ~70% of momentum
                // 3. Shooting rock (this) bounces back with ~15% momentum
                
                // Calculate momentum transfer
                Vector2 impactDirection = impactVelocity.normalized;
                
                // 1. STRUCK ROCK: Minimal movement (just slides a tiny bit)
                otherRB.linearVelocity = impactDirection * (impactSpeed * 0.15f);
                
                // 2. FROZEN PARTNER: Receives most of the momentum
                Rigidbody2D partnerRB = frozenPartner.GetComponent<Rigidbody2D>();
                if (partnerRB != null)
                {
                    // Direction from struck rock to partner
                    Vector2 toPartner = (frozenPartner.transform.position - collision.transform.position).normalized;
                    partnerRB.linearVelocity = toPartner * (impactSpeed * 0.65f);
                    
                    // Wake up the frozen partner
                    Rock_Info partnerInfo = frozenPartner.GetComponent<Rock_Info>();
                    if (partnerInfo != null)
                    {
                        partnerInfo.moving = true;
                        partnerInfo.stopped = false;
                        partnerInfo.rest = false;
                    }
                    
                    Debug.Log($"[Frozen Rock] Partner velocity: {partnerRB.linearVelocity.magnitude:F2} m/s");
                }
                
                // 3. SHOOTING ROCK: Bounces back with reduced velocity
                thisRB.linearVelocity = -impactDirection * (impactSpeed * 0.20f);
                
                Debug.Log($"[Frozen Rock] Struck rock velocity: {otherRB.linearVelocity.magnitude:F2} m/s | Shooter bounce: {thisRB.linearVelocity.magnitude:F2} m/s");
            }
            
            // NORMAL COLLISION (not frozen)
            // Let Unity's physics handle it, but we can fine-tune if needed
            
            //sm.SweepHit(false);
            collision.gameObject.GetComponent<Rock_Info>().moving = true;
            collision.gameObject.GetComponent<Rock_Info>().stopped = false;
            collision.gameObject.GetComponent<Rock_Info>().rest = false;
            hit = true;


            //Debug.Log("Hit!");
            //am.Play("Hit");
            rockSounds[0].volume = collision.relativeVelocity.magnitude * am.maxVol;
            rockSounds[0].enabled = true;
            HapticController.Play(hitHap);
            HapticController.clipLevel = collision.relativeVelocity.magnitude;
            //Debug.Log("Relative Velocity - " + collision.relativeVelocity.magnitude);

            //Time.timeScale = 0f;
            //Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
            //SweeperSelector sweepSel = FindObjectOfType<SweeperSelector>();
            //sweepSel.PostHitSelect();
            //SlowMotion slowMo = new SlowMotion();

            //slowMo.SlowdownTime(true);

            //Debug.Log("Time scale is " + Time.timeScale);

            //GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            if (gm.redHammer)
            {
                //if the rock is red
                if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
                {
                    //is it aiTeam
                    if (sm != null)
                        sm.SweepHit(gm.aiTeamRed);
                }
                //if the rock is yellow
                else if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
                {
                    if (sm != null)
                        //is the aiTeam yellow
                        sm.SweepHit(gm.aiTeamYellow);
                }
            }
            else if (!gm.redHammer)
            {
                //if the rock is yellow
                if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
                {
                    if (sm != null)
                        sm.SweepHit(gm.aiTeamYellow);
                }
                //if the rock is red
                else if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
                {
                    if (sm != null)
                        sm.SweepHit(gm.aiTeamRed);
                }
            }
        }

        if (collision.gameObject.CompareTag("Boards"))
        {
            outOfPlay = true;
            inPlay = false;
            Debug.Log("collider boards");
            StartCoroutine(OutOfPlay());
            rockSounds[2].enabled = true;
            HapticController.Play(sideHap);
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider == InPlay_Collider)
        {
            Debug.Log("Out");
            inPlay = false;
            inHouse = false;
            if (gameObject.activeSelf)
                StartCoroutine(OutOfPlay());

            rockSounds[2].enabled = true;
        }

        if (collider == house_collider)
        {
            Debug.Log("Out of House");
            inHouse = false;
        }
    }

}
