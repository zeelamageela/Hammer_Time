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

    // Update is called once per frame
    void Update()
    {
        if (outOfPlay)
        {
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

        if (GetComponent<Rock_Info>().shotTaken && GetComponent<Rock_Info>().stopped && transform.position.y < 0f)
        {
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
        yield return new WaitForSeconds(0.5f);

        body.velocity = Vector2.zero;
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

            Debug.Log("Far Hogline velocity is " + GetComponent<Rigidbody2D>().velocity.x + ", " + GetComponent<Rigidbody2D>().velocity.y);
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
            Debug.Log("House velocity is " + GetComponent<Rigidbody2D>().velocity.x + ", " + GetComponent<Rigidbody2D>().velocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rock"))
        {
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

            Debug.Log("Time scale is " + Time.timeScale);

            //GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            if (gm.redHammer)
            {
                //if the rock is red
                if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
                {
                    //is it aiTeam
                    sm.SweepHit(gm.aiTeamRed);
                }
                //if the rock is yellow
                else if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
                {
                    //is the aiTeam yellow
                    sm.SweepHit(gm.aiTeamYellow);
                }
            }
            else if (!gm.redHammer)
            {
                //if the rock is yellow
                if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
                {
                    sm.SweepHit(gm.aiTeamYellow);
                }
                //if the rock is red
                else if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
                {
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
