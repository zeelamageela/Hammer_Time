using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;
using MoreMountains.Feedbacks;

public class SweeperManager : MonoBehaviour
{
    public TeamManager tm;

    public bool isSweeping;
    public bool isTeeSweeping;  // Track if T-line sweeper is active
    public SweeperParent sweeperL;
    public SweeperParent sweeperR;
    public SweeperParent sweeperT;

    public SweeperParent sweeperRedL;
    public SweeperParent sweeperRedR;
    public SweeperParent sweeperRedTee;

    public SweeperParent sweeperYellowL;
    public SweeperParent sweeperYellowR;
    public SweeperParent sweeperYellowTee;
    
    private SweeperParent activeTeeSweeper;  // Current active T-line sweeper

    public CharacterStats swprLStats;
    public CharacterStats swprRStats;
    public CharacterStats swprRTStats;  // Red Tee sweeper stats (skip)
    public CharacterStats swprYTStats;  // Yellow Tee sweeper stats (skip)

    public Sweep sweep;

    public SweeperSelector sweepSel;
    public TeeSweeperController teeController;  // New: Independent T-line sweeper system
    
    public GameObject sweepButton;
    public GameObject hardButton;
    public GameObject whoaButton;
    public RockManager rm;
    public AudioManager am;
    public bool inturn;

    AudioSource[] rockSounds;
    AudioSource[] skipSounds;
    public GameObject audioShoot;
    public GameObject audioHouse;

    GameSettingsPersist gsp;
    float timeLeft;
    float sweepTimer;

    public GameObject aimCircle;
    public HapticClip sweepHap;
    public MMF_Player fltFdbk;
    public MMF_FloatingText fltText;

    void Awake()
    {
        //am = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        //if (am == null)
        //{
        //    Debug.Log("Audio Manager not loaded");
        //}
        gsp = FindFirstObjectByType<GameSettingsPersist>();
        rockSounds = sweepSel.GetComponents<AudioSource>();
        fltText = fltFdbk.GetFeedbackOfType<MMF_FloatingText>();
        //float sweepEndur = swprLStats.sweepEndurance.GetValue() + swprRStats.sweepEndurance.GetValue();
        //sweepTimer = sweepEndur * 0.02f;
    }

    private void Update()
    {
        if (sweeperL && sweeperL.transform.parent.gameObject.activeSelf)
        {
            swprLStats = sweeperL.gameObject.GetComponent<CharacterStats>();
            swprRStats = sweeperR.gameObject.GetComponent<CharacterStats>();

            if (swprRStats.sweepHealth <= 0f)
            {
                swprRStats.sweepHealth = 0f;
                SweepWhoa(false);
            }
            if (swprLStats.sweepHealth <= 0f)
            {
                swprLStats.sweepHealth = 0f;
                SweepWhoa(false);
            }

        }

        if (isSweeping)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
                //timeLeft = 0f;
                Debug.Log("Whoa called in Tap Timer");
                rockSounds[0].enabled = false;
                rockSounds[1].enabled = false;
                HapticController.Stop();
                sweeperL.Whoa();
                sweeperR.Whoa();
                sweep.OnWhoa();
                whoaButton.SetActive(false);
                isSweeping = false;
            }
        }
    }

    public void SetupSweepers(bool redTurn)
    {
        //sweeperRedL.gameObject.SetActive(false);
        //sweeperRedR.gameObject.SetActive(false);
        //sweeperYellowL.gameObject.SetActive(false);
        //sweeperYellowR.gameObject.SetActive(false);
        GameManager gm = FindFirstObjectByType<GameManager>();
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        if (redTurn)
        {
            sweeperL = Instantiate(sweeperRedL, sweepSel.gameObject.transform);
            sweeperR = Instantiate(sweeperRedR, sweepSel.gameObject.transform);
            sweeperRedTee = Instantiate(sweeperRedTee, sweepSel.tSweepParent.transform);
            sweeperYellowTee = Instantiate(sweeperYellowTee, sweepSel.tSweepParent.transform);
            //sweeperL.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamRedColour);
            //sweeperR.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamRedColour);

            if (gsp.redTeamName == cm.teamName)
            {
                tm.SetSweepers(sweeperL.GetComponent<CharacterStats>(), sweeperR.GetComponent<CharacterStats>(), sweeperYellowTee.GetComponent<CharacterStats>(), gm.rockCurrent, false);
            }
            else
            {
                tm.SetSweepers(sweeperL.GetComponent<CharacterStats>(), sweeperR.GetComponent<CharacterStats>(), sweeperRedTee.GetComponent<CharacterStats>(), gm.rockCurrent, true);
            }
        }
        else
        {
            sweeperL = Instantiate(sweeperYellowL, sweepSel.gameObject.transform);
            sweeperR = Instantiate(sweeperYellowR, sweepSel.gameObject.transform);
            sweeperRedTee = Instantiate(sweeperRedTee, sweepSel.tSweepParent.transform);
            sweeperYellowTee = Instantiate(sweeperYellowTee, sweepSel.tSweepParent.transform);
            //sweeperL.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamYellowColour);
            //sweeperR.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamYellowColour);

            if (gsp.redTeamName != cm.teamName)
            {
                tm.SetSweepers(sweeperL.GetComponent<CharacterStats>(), sweeperR.GetComponent<CharacterStats>(), sweeperRedTee.GetComponent<CharacterStats>(), gm.rockCurrent, false);
            }
            else
            {
                tm.SetSweepers(sweeperL.GetComponent<CharacterStats>(), sweeperR.GetComponent<CharacterStats>(), sweeperYellowTee.GetComponent<CharacterStats>(), gm.rockCurrent, true);
            }
        }

        sweeperL.gameObject.SetActive(true);
        sweeperR.gameObject.SetActive(true);
        swprLStats = sweeperL.gameObject.GetComponent<CharacterStats>();
        swprRStats = sweeperR.gameObject.GetComponent<CharacterStats>();
        
        // Determine if it's AI turn
        GameManager gmRef = FindFirstObjectByType<GameManager>();
        bool isAITurn = (redTurn && gmRef != null && gmRef.aiTeamRed) || 
                        (!redTurn && gmRef != null && gmRef.aiTeamYellow);
        
        // Disable regular sweeper colliders during AI turns (they don't need tap detection)
        if (isAITurn)
        {
            Collider2D sweeperLCol = sweeperL.GetComponent<Collider2D>();
            Collider2D sweeperRCol = sweeperR.GetComponent<Collider2D>();
            if (sweeperLCol != null) sweeperLCol.enabled = false;
            if (sweeperRCol != null) sweeperRCol.enabled = false;
            Debug.Log("[SweeperManager] AI turn - regular sweeper colliders DISABLED");
        }
        else
        {
            Collider2D sweeperLCol = sweeperL.GetComponent<Collider2D>();
            Collider2D sweeperRCol = sweeperR.GetComponent<Collider2D>();
            if (sweeperLCol != null) sweeperLCol.enabled = true;
            if (sweeperRCol != null) sweeperRCol.enabled = true;
            Debug.Log("[SweeperManager] Player turn - regular sweeper colliders ENABLED");
        }

        sweepSel.SetColliders();
        sweepSel.gameObject.SetActive(false);
        
        // CRITICAL: Keep tSweepParent ACTIVE so TeeSweeperController Update() runs!
        // TeeSweeperController needs Update() to detect player taps on rocks
        sweepSel.tSweepParent.SetActive(true);
        Debug.Log("[SweeperManager] tSweepParent ACTIVE for tap detection");
        
        // Initialize TeeSweeperController
        if (teeController == null)
        {
            teeController = sweepSel.tSweepParent.GetComponent<TeeSweeperController>();
            if (teeController == null)
            {
                teeController = sweepSel.tSweepParent.AddComponent<TeeSweeperController>();
            }
        }
        
        // Set up controller using Initialize method
        teeController.Initialize(
            this,
            rm,
            FindFirstObjectByType<GameManager>(),
            sweep,
            sweeperRedTee,
            sweeperYellowTee,
            rockSounds,
            sweepButton,
            whoaButton
        );
        
        // Use the isAITurn variable from above (already calculated)
        if (isAITurn)
        {
            teeController.DisableColliders();
            Debug.Log("[SweeperManager] AI turn - tee sweeper colliders DISABLED");
        }
        else
        {
            teeController.EnableColliders();
            Debug.Log("[SweeperManager] Player turn - tee sweeper colliders ENABLED");
        }
        
        Debug.Log("[SweeperManager] TeeSweeperController initialized");
        
        sweeperL.sweep = false;
        sweeperL.hard = false;
        sweeperL.whoa = true;

        sweeperR.sweep = false;
        sweeperR.hard = false;
        sweeperR.whoa = true;
    }

    public void ResetSweepers()
    {

        //sweeperRedL.gameObject.SetActive(false);
        //sweeperRedR.gameObject.SetActive(false);
        //sweeperYellowL.gameObject.SetActive(false);
        //sweeperYellowR.gameObject.SetActive(false);

        Destroy(sweeperL.gameObject);
        Destroy(sweeperR.gameObject);
        Destroy(sweeperRedTee.gameObject);
        Destroy(sweeperYellowTee.gameObject);

        rockSounds[0].enabled = false;
        rockSounds[1].enabled = false;
        //am.Stop("Sweep");
        //am.Stop("Hard");
        sweepSel.gameObject.SetActive(false);
        
        // Keep tSweepParent active for tap detection in next shot
        // sweepSel.tSweepParent.SetActive(false);  ? DON'T disable!
        
        sweeperL.sweep = false;
        sweeperL.hard = false;
        sweeperL.whoa = true;

        sweeperR.sweep = false;
        sweeperR.hard = false;
        sweeperR.whoa = true;
        
        // Reset T-line sweeping state
        isTeeSweeping = false;
        activeTeeSweeper = null;
        
        // Force detach TeeSweeperController
        if (teeController != null)
        {
            teeController.ForceDetach();
        }
    }

    public void Release(GameObject rock, bool aiTurn)
    {
        sweepSel.gameObject.SetActive(true);

        // Keep tSweepParent active for tap detection
        // sweepSel.tSweepParent.SetActive(false);  ? DON'T disable!
        
        sweepSel.AttachToRock(rock);
        
        inturn = rm.inturn;
        //sweep.OnWhoa();
        if (!aiTurn)
        {
            sweepButton.SetActive(false);
            hardButton.SetActive(false);
            whoaButton.SetActive(false);
        }
        else
        {
            sweepButton.SetActive(false);
            hardButton.SetActive(false);
            whoaButton.SetActive(false);
        }

        if (inturn)
        {
            sweeperL.yOffset = 0.69f;
            sweeperR.yOffset = 0.39f;
            //sweeperL.gameObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            //sweeperR.gameObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        }
        else
        {
            sweeperL.yOffset = 0.39f;
            sweeperR.yOffset = 0.69f;
            //sweeperL.gameObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            //sweeperR.gameObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        }

        ShotLocation();
    }

    public void SweepTap()
    {
        //Tap on the sweep target
        float sweepTimer = 0.25f;  // Match sweepTapDuration from Sweep.cs

        if (timeLeft < sweepTimer)
            timeLeft = sweepTimer;
        Debug.Log("Sweep Timer is " + sweepTimer + " seconds, and isSweeping is " + isSweeping);

        if (Random.Range(0f, 1f) < 0.25f)
            CallOut("Sweep");

        if (isSweeping == false)
        {
            CallOut("Sweep");
            //fltText.Value = "SWEEP!";
            //fltText.TargetTransform = sweepSel.transform;
            //fltText.Play(sweepSel.transform.position);
            isSweeping = true;
            rockSounds[0].enabled = true;
            rockSounds[1].enabled = true;
            rockSounds[0].pitch = 1f;
            rockSounds[1].pitch = 1f;
            HapticController.Load(sweepHap);
            HapticController.Loop(true);
            HapticController.Play();
            //HapticController.clipFrequencyShift = 1f;
            sweeperL.Sweep();
            sweeperR.Sweep();
            sweep.OnSweep();
        }

        //set the buttons
        sweepButton.SetActive(false);
        hardButton.SetActive(false);
        whoaButton.SetActive(false);

        //StartCoroutine(TapTimer());
    }

    public void SweepTapLeft()
    {
        //Tap on the sweep target

        float sweepTimer = 0.25f;  // Match sweepTapDuration from Sweep.cs

        if (timeLeft < sweepTimer)
            timeLeft = sweepTimer;
        Debug.Log("Sweep Timer is " + sweepTimer + " seconds, and isSweeping is " + isSweeping);

        if (Random.Range(0f, 1f) < 0.25f)
            CallOut("Sweep");

        if (isSweeping == false)
        {
            CallOut("Sweep");
            isSweeping = true;
            rockSounds[0].enabled = true;
            rockSounds[1].enabled = false;
            rockSounds[0].pitch = 1f;
            rockSounds[1].pitch = 1f;
            HapticController.Load(sweepHap);
            HapticController.Loop(true);
            HapticController.Play();
            //HapticController.clipFrequencyShift = 1f;
            sweep.OnLeft();
            //sweeperL.gameObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            sweeperL.Sweep();

            //sweeperR.gameObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            sweeperR.Whoa();

            sweeperL.yOffset = 0.39f;
            //sweeperL.yOffset = 0.6f;
            sweeperR.yOffset = 0.69f;
            //sweeperR.yOffset = 1.2f;
        }

        //set the buttons
        sweepButton.SetActive(false);
        hardButton.SetActive(false);
        whoaButton.SetActive(false);
    }

    public void SweepTapRight()
    {
        //Tap on the sweep target
        float sweepTimer = 0.25f;  // Match sweepTapDuration from Sweep.cs

        if (timeLeft < sweepTimer)
            timeLeft = sweepTimer;
        Debug.Log("Sweep Timer is " + sweepTimer + " seconds, and isSweeping is " + isSweeping);

        if (Random.Range(0f, 1f) < 0.25f)
            CallOut("Sweep");

        if (isSweeping == false)
        {
            CallOut("Sweep");
            isSweeping = true;
            //isSweeping = true;
            //am.Play("Sweep");
            rockSounds[0].enabled = false;
            rockSounds[1].enabled = true;
            rockSounds[0].pitch = 1f;
            rockSounds[1].pitch = 1f;
            HapticController.Load(sweepHap);
            HapticController.Loop(true);
            HapticController.Play();
            //HapticController.clipFrequencyShift = 1f;

            sweep.OnRight();
            //sweeperL.gameObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            sweeperL.Whoa();

            //sweeperR.gameObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            sweeperR.Sweep();

            sweeperL.yOffset = 0.69f;
            sweeperR.yOffset = 0.39f;
        }

        //set the buttons
        sweepButton.SetActive(false);
        hardButton.SetActive(false);
        whoaButton.SetActive(false);
    }

    IEnumerator TapTimer()
    {
        float sweepEndur = swprLStats.sweepEndurance.GetValue() + swprRStats.sweepEndurance.GetValue();
        float sweepTimer = sweepEndur * 0.02f;
        Debug.Log("Sweep Timer is " + sweepTimer + " seconds, and isSweeping is " + isSweeping);
        yield return new WaitForSeconds(sweepTimer);

        if (isSweeping == false)
        {
            Debug.Log("Whoa called in Tap Timer");
            rockSounds[0].enabled = false;
            rockSounds[1].enabled = false;
            sweeperL.Whoa();
            sweeperR.Whoa();
            sweep.OnWhoa();
            whoaButton.SetActive(false);
        }
    }

    public void SweepWeight(bool aiTurn)
    {
        CallOut("Sweep");

        rockSounds[0].enabled = true;
        rockSounds[1].enabled = true;
        rockSounds[0].pitch = 1f;
        rockSounds[1].pitch = 1f;
        sweeperL.Sweep();
        sweeperR.Sweep();
        sweep.OnSweep();

        if (!aiTurn)
        {
            sweepButton.SetActive(false);
            hardButton.SetActive(true);
            whoaButton.SetActive(true);
        }
    }

    public void SweepHard(bool aiTurn)
    {
        CallOut("Hard");

        rockSounds[0].enabled = true;
        rockSounds[1].enabled = true;
        rockSounds[0].pitch = 1.4f;
        rockSounds[1].pitch = 1.4f;
        sweeperL.Hard();
        sweeperR.Hard();
        sweep.OnHard();

        if (!aiTurn)
        {
            hardButton.SetActive(false);
            sweepButton.SetActive(false);
            whoaButton.SetActive(true);
        }
    }

    public void SweepHit(bool aiTurn)
    {
        rockSounds[0].enabled = false;
        rockSounds[1].enabled = false;
        rockSounds[0].pitch = 1f;
        rockSounds[1].pitch = 1f;

        if (sweeperL != null)
            sweeperL.Whoa();
        if (sweeperR != null)
            sweeperR.Whoa();
        sweep.OnWhoa();

        if (!aiTurn)
        {
            whoaButton.SetActive(false);
            sweepButton.SetActive(false);
            hardButton.SetActive(false);
        }
    }

    public void SweepWhoa(bool aiTurn)
    {
        //am.Stop("Sweep");
        //am.Stop("Hard");
        rockSounds[0].enabled = false;
        rockSounds[1].enabled = false;
        rockSounds[0].pitch = 1f;
        rockSounds[1].pitch = 1f;

        CallOut("Whoa");
        sweeperL.Whoa();
        sweeperR.Whoa();
        sweep.OnWhoa();

        if (!aiTurn)
        {
            whoaButton.SetActive(false);
            sweepButton.SetActive(false);
            hardButton.SetActive(false);
        }
    }

    public void SweepLeft(bool aiTurn)
    {

        rockSounds[0].enabled = true;
        rockSounds[1].enabled = false;
        rockSounds[0].pitch = 1f;
        rockSounds[1].pitch = 1f;
        sweep.OnLeft();
        //sweeperL.gameObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        sweeperL.Sweep();

        //sweeperR.gameObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        sweeperR.Whoa();

        sweeperL.yOffset = Mathf.Lerp(1.2f, 0.6f, (1 - Time.deltaTime));
        sweeperL.yOffset = 0.6f;
        sweeperR.yOffset = Mathf.Lerp(0.6f, 1.2f, (1 - Time.deltaTime));
        sweeperR.yOffset = 1.2f;
        if (!aiTurn)
        {
            sweepButton.SetActive(true);
            hardButton.SetActive(false);
            whoaButton.SetActive(true);
        }
    }

    public void SweepRight(bool aiTurn)
    {
        rockSounds[0].enabled = false;
        rockSounds[1].enabled = true;
        rockSounds[0].pitch = 1f;
        rockSounds[1].pitch = 1f;

        sweep.OnRight();
        //sweeperL.gameObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        sweeperL.Whoa();

        //sweeperR.gameObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        sweeperR.Sweep();

        sweeperL.yOffset = 1.2f;
        sweeperR.yOffset = 0.6f;
        if (!aiTurn)
        {
            sweepButton.SetActive(true);
            hardButton.SetActive(false);
            whoaButton.SetActive(true);
        }
    }

    public void CallOut(string call)
    {
        am = FindFirstObjectByType<AudioManager>();

        //Debug.Log("Sweeping " + call);
        if (Random.Range(0f, 1f) < 0.5f)
            skipSounds = audioHouse.transform.Find("Audio" + call).GetComponents<AudioSource>();
        else
            skipSounds = audioShoot.transform.Find("Audio" + call).GetComponents<AudioSource>();

        for (int i = 0; i < skipSounds.Length; i++)
            skipSounds[i].enabled = false;

        int rndCall = Random.Range(0, skipSounds.Length);
        skipSounds[rndCall].volume = am.maxVol;
        skipSounds[rndCall].enabled = true;
    }


    void ShotLocation()
    {
        AI_Sweeper aiSweep = FindFirstObjectByType<AI_Sweeper>();
        AI_Shooter aiShoot = FindFirstObjectByType<AI_Shooter>();
        RockManager rm = FindFirstObjectByType<RockManager>();
        Vector3 aimPos = aimCircle.transform.position;

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
    
    // ========== T-LINE SWEEPING (OPPONENT ROCKS) ==========
    
    /// <summary>
    /// Activate T-line sweepers when opponent rock crosses Y=6.5
    /// LEGACY METHOD: Redirects to TeeSweeperController for backwards compatibility
    /// New code should use TeeSweeperController directly
    /// </summary>
    public void ActivateTeeSweepers(GameObject opponentRock, bool isRedRock)
    {
        Debug.Log("[SweeperManager] ActivateTeeSweepers called - redirecting to TeeSweeperController");
        
        // This method kept for backwards compatibility
        // TeeSweeperController now handles all T-line sweeping via tap detection
    }
    
    /// <summary>
    /// Deactivate T-line sweepers when rock stops or goes out of play
    /// </summary>
    public void DeactivateTeeSweepers()
    {
        if (activeTeeSweeper != null)
        {
            activeTeeSweeper.Whoa();
            activeTeeSweeper.gameObject.SetActive(false);
        }
        
        sweepSel.tSweepParent.SetActive(false);
        isTeeSweeping = false;
        activeTeeSweeper = null;
        
        // Hide sweep buttons
        sweepButton.SetActive(false);
        whoaButton.SetActive(false);
        
        Debug.Log("[T-Line Sweep] Deactivated T-line sweepers");
    }
    
    /// <summary>
    /// Player taps to sweep opponent rock behind T-line
    /// Uses skip's sweeping stats (single sweeper only)
    /// </summary>
    public void SweepTeeTap()
    {
        if (activeTeeSweeper == null)
        {
            Debug.LogWarning("[T-Line Sweep] No active tee sweeper to tap!");
            return;
        }
        
        // Get sweeper stats (skip's stats)
        CharacterStats teeStats = activeTeeSweeper.GetComponent<CharacterStats>();
        if (teeStats == null)
        {
            Debug.LogError("[T-Line Sweep] Tee sweeper has no CharacterStats!");
            return;
        }
        
        // Calculate sweep duration based on skip's endurance
        float sweepEndur = teeStats.sweepEndurance.GetValue();
        float sweepTimer = sweepEndur * 0.02f;
        
        if (timeLeft < sweepTimer)
            timeLeft = sweepTimer;
        
        Debug.Log($"[T-Line Sweep] Sweep Timer is {sweepTimer} seconds, isTeeSweeping={isTeeSweeping}");
        
        // Random callout
        if (Random.Range(0f, 1f) < 0.25f)
            CallOut("Sweep");
        
        if (!isTeeSweeping)
        {
            CallOut("Sweep");
            isTeeSweeping = true;
            
            // Enable sweeping sound (single sweeper)
            rockSounds[0].enabled = true;
            rockSounds[1].enabled = false;  // Only one sweeper for T-line
            rockSounds[0].pitch = 1f;
            
            // Haptics
            HapticController.Load(sweepHap);
            HapticController.Loop(true);
            HapticController.Play();
            
            // Activate sweeper animation
            activeTeeSweeper.Sweep();
            
            // Apply sweep physics to opponent rock
            sweep.OnSweep();
        }
        
        // Update button states
        sweepButton.SetActive(false);
        whoaButton.SetActive(true);
        hardButton.SetActive(false);
    }
    
    /// <summary>
    /// Stop sweeping opponent rock behind T-line
    /// </summary>
    public void SweepTeeWhoa()
    {
        if (activeTeeSweeper == null) return;
        
        rockSounds[0].enabled = false;
        rockSounds[1].enabled = false;
        HapticController.Stop();
        
        CallOut("Whoa");
        activeTeeSweeper.Whoa();
        sweep.OnWhoa();
        
        isTeeSweeping = false;
        
        // Update buttons
        whoaButton.SetActive(false);
        sweepButton.SetActive(true);
        hardButton.SetActive(false);
        
        Debug.Log("[T-Line Sweep] Player called Whoa on T-line sweep");
    }
}
