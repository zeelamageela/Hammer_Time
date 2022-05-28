using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;
using MoreMountains.Feedbacks;

public class SweeperManager : MonoBehaviour
{
    public bool isSweeping;
    public SweeperParent sweeperL;
    public SweeperParent sweeperR;

    public SweeperParent sweeperRedL;
    public SweeperParent sweeperRedR;
    public SweeperParent sweeperYellowL;
    public SweeperParent sweeperYellowR;

    public CharacterStats swprLStats;
    public CharacterStats swprRStats;

    public Sweep sweep;

    public SweeperSelector sweepSel;
    
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
    public MMFeedbackFloatingText fltText;

    void Awake()
    {
        //am = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        //if (am == null)
        //{
        //    Debug.Log("Audio Manager not loaded");
        //}
        gsp = FindObjectOfType<GameSettingsPersist>();
        rockSounds = sweepSel.GetComponents<AudioSource>();

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
                    SweepWhoa(false);
            }
            if (swprLStats.sweepHealth <= 0f)
            {
                    SweepWhoa(false);
            }


        }

        if (isSweeping)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
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

        if (redTurn)
        {
            sweeperL = Instantiate(sweeperRedL, sweepSel.gameObject.transform);
            sweeperR = Instantiate(sweeperRedR, sweepSel.gameObject.transform);
            //sweeperL.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamRedColour);
            //sweeperR.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamRedColour);

            if (gsp.redTeamColour == gsp.teamColour)
            {
                sweeperL.GetComponent<CharacterStats>().sweepStrength.SetBaseValue(gsp.cStats.sweepStrength);
                sweeperR.GetComponent<CharacterStats>().sweepStrength.SetBaseValue(gsp.cStats.sweepStrength);
                sweeperL.GetComponent<CharacterStats>().sweepEndurance.SetBaseValue(gsp.cStats.sweepEndurance);
                sweeperR.GetComponent<CharacterStats>().sweepEndurance.SetBaseValue(gsp.cStats.sweepEndurance);
                sweeperL.GetComponent<CharacterStats>().sweepHealth = gsp.cStats.sweepCohesion;
                sweeperR.GetComponent<CharacterStats>().sweepHealth = gsp.cStats.sweepCohesion;
            }
            else
            {
                sweeperL.GetComponent<CharacterStats>().sweepStrength.SetBaseValue(10);
                sweeperR.GetComponent<CharacterStats>().sweepStrength.SetBaseValue(10);
                sweeperL.GetComponent<CharacterStats>().sweepEndurance.SetBaseValue(10);
                sweeperR.GetComponent<CharacterStats>().sweepEndurance.SetBaseValue(10);
                sweeperL.GetComponent<CharacterStats>().sweepHealth = 100;
                sweeperR.GetComponent<CharacterStats>().sweepHealth = 100;
            }
        }
        else
        {
            sweeperL = Instantiate(sweeperYellowL, sweepSel.gameObject.transform);
            sweeperR = Instantiate(sweeperYellowR, sweepSel.gameObject.transform);
            //sweeperL.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamYellowColour);
            //sweeperR.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamYellowColour);

            if (gsp.redTeamColour == gsp.teamColour)
            {
                sweeperL.GetComponent<CharacterStats>().sweepStrength.SetBaseValue(10);
                sweeperR.GetComponent<CharacterStats>().sweepStrength.SetBaseValue(10);
                sweeperL.GetComponent<CharacterStats>().sweepEndurance.SetBaseValue(10);
                sweeperR.GetComponent<CharacterStats>().sweepEndurance.SetBaseValue(10);
                sweeperL.GetComponent<CharacterStats>().sweepHealth = 100;
                sweeperR.GetComponent<CharacterStats>().sweepHealth = 100;
            }
            else
            {
                sweeperL.GetComponent<CharacterStats>().sweepStrength.SetBaseValue(gsp.cStats.sweepStrength);
                sweeperR.GetComponent<CharacterStats>().sweepStrength.SetBaseValue(gsp.cStats.sweepStrength);
                sweeperL.GetComponent<CharacterStats>().sweepEndurance.SetBaseValue(gsp.cStats.sweepEndurance);
                sweeperR.GetComponent<CharacterStats>().sweepEndurance.SetBaseValue(gsp.cStats.sweepEndurance);
                sweeperL.GetComponent<CharacterStats>().sweepHealth = gsp.cStats.sweepCohesion;
                sweeperR.GetComponent<CharacterStats>().sweepHealth = gsp.cStats.sweepCohesion;
            }
        }

        sweeperL.gameObject.SetActive(true);
        sweeperR.gameObject.SetActive(true);
        swprLStats = sweeperL.gameObject.GetComponent<CharacterStats>();
        swprRStats = sweeperR.gameObject.GetComponent<CharacterStats>();

        sweepSel.SetColliders();
        sweepSel.gameObject.SetActive(false);
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

        rockSounds[0].enabled = false;
        rockSounds[1].enabled = false;
        //am.Stop("Sweep");
        //am.Stop("Hard");
        sweepSel.gameObject.SetActive(false);
        sweeperL.sweep = false;
        sweeperL.hard = false;
        sweeperL.whoa = true;

        sweeperR.sweep = false;
        sweeperR.hard = false;
        sweeperR.whoa = true;
    }

    public void Release(GameObject rock, bool aiTurn)
    {
        sweepSel.gameObject.SetActive(true);
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
            sweeperL.yOffset = 1.2f;
            sweeperR.yOffset = 0.6f;
            //sweeperL.gameObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            //sweeperR.gameObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        }
        else
        {
            sweeperL.yOffset = 0.6f;
            sweeperR.yOffset = 1.2f;
            //sweeperL.gameObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            //sweeperR.gameObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        }

        ShotLocation();
    }

    public void SweepTap()
    {
        //Tap on the sweep target

        float sweepEndur = swprLStats.sweepEndurance.GetValue() + swprRStats.sweepEndurance.GetValue();
        float sweepTimer = 0.5f + (sweepEndur * 0.02f);

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

        float sweepEndur = swprLStats.sweepEndurance.GetValue() + swprLStats.sweepEndurance.GetValue();
        float sweepTimer = 0.5f + sweepEndur * 0.02f;

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

            sweeperL.yOffset = Mathf.Lerp(1.2f, 0.6f, (1 - Time.deltaTime));
            //sweeperL.yOffset = 0.6f;
            sweeperR.yOffset = Mathf.Lerp(0.6f, 1.2f, (1 - Time.deltaTime));
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

        float sweepEndur = swprRStats.sweepEndurance.GetValue() + swprRStats.sweepEndurance.GetValue();
        float sweepTimer = 0.5f + sweepEndur * 0.02f;

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

            sweeperL.yOffset = 1.2f;
            sweeperR.yOffset = 0.6f;
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
        Debug.Log("Sweeping " + call);
        if (Random.Range(0f, 1f) < 0.5f)
            skipSounds = audioHouse.transform.Find("Audio" + call).GetComponents<AudioSource>();
        else
            skipSounds = audioShoot.transform.Find("Audio" + call).GetComponents<AudioSource>();

        for (int i = 0; i < skipSounds.Length; i++)
            skipSounds[i].enabled = false;

        skipSounds[Random.Range(0, skipSounds.Length)].enabled = true;
    }


    void ShotLocation()
    {
        AI_Sweeper aiSweep = FindObjectOfType<AI_Sweeper>();
        AI_Shooter aiShoot = FindObjectOfType<AI_Shooter>();
        RockManager rm = FindObjectOfType<RockManager>();
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
}
