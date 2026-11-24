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

    // Used by the Free Guard Zone rule to remove the offending rock as the penalty
    // for knocking a protected opponent guard out of play.
    public void ForceOutOfPlay()
    {
        if (!outOfPlay && !outOfPlayCoroutineStarted)
        {
            outOfPlayCoroutineStarted = true;
            StartCoroutine(OutOfPlay());
        }
    }

    // Used by the Free Guard Zone rule to put a displaced rock back exactly as it was
    // before an illegal shot - including a rock that had already been marked out of
    // play and mid-way through its OutOfPlay() animation/deactivation delay, whose
    // Animator would otherwise leave the sprite stuck on whatever frame it reached.
    public void RestoreForFreeGuardZone(Vector2 position, float rotation, Sprite sprite)
    {
        StopAllCoroutines();
        outOfPlayCoroutineStarted = false;
        outOfPlay = false;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;

        Animator animator = GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = sprite;
        }

        body.position = position;
        body.rotation = rotation;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
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
            // Rock-on-rock contact is Box2D's (restitution 0.99, friction 0.25 from Rock Material); RockCollision and the aim-line
            // simulator reproduce it. There used to be a "frozen rock" override here with fixed ratios; it never fired in play (0 of 56 logged
            // hits) and Box2D plus the shared RockCollision model already decide a frozen pair, so it was removed (see PHYSICS_COLLISIONS.md).

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
            if (isActiveAndEnabled && gameObject.activeInHierarchy)
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
