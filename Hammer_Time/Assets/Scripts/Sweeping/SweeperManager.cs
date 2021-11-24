using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (sweeperL & sweeperR & swprLStats & swprRStats)
        {
            swprLStats = sweeperL.gameObject.GetComponent<CharacterStats>();
            swprRStats = sweeperR.gameObject.GetComponent<CharacterStats>();

            if (swprRStats.sweepHealth <= 0f)
            {
                if (rm.gm.aiTeamRed)
                    SweepWhoa(rm.gm.aiTeamRed);
                else if (rm.gm.aiTeamYellow)
                    SweepWhoa(rm.gm.aiTeamYellow);
                else
                    SweepWhoa(false);
            }
            if (swprLStats.sweepHealth <= 0f)
            {
                if (rm.gm.aiTeamRed | rm.gm.aiTeamYellow)
                    SweepWhoa(true);
                else
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
            sweeperL.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamRedColour);
            sweeperR.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamRedColour);

            if (gsp.redTeamColour == gsp.teamColour)
            {
                sweeperL.GetComponent<CharacterStats>().sweepStrength.SetBaseValue(gsp.cStats.sweepStrength);
                sweeperR.GetComponent<CharacterStats>().sweepStrength.SetBaseValue(gsp.cStats.sweepStrength);
                sweeperL.GetComponent<CharacterStats>().sweepEndurance.SetBaseValue(gsp.cStats.sweepEndurance);
                sweeperR.GetComponent<CharacterStats>().sweepEndurance.SetBaseValue(gsp.cStats.sweepEndurance);
                sweeperL.GetComponent<CharacterStats>().sweepHealth = gsp.cStats.sweepHealth;
                sweeperR.GetComponent<CharacterStats>().sweepHealth = gsp.cStats.sweepHealth;
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
            sweeperL.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamYellowColour);
            sweeperR.GetComponent<CharColourChanger>().TeamColour(FindObjectOfType<TeamManager>().teamYellowColour);

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
                sweeperL.GetComponent<CharacterStats>().sweepHealth = gsp.cStats.sweepHealth;
                sweeperR.GetComponent<CharacterStats>().sweepHealth = gsp.cStats.sweepHealth;
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
            isSweeping = true;
            rockSounds[0].enabled = true;
            rockSounds[1].enabled = true;
            rockSounds[0].pitch = 1f;
            rockSounds[1].pitch = 1f;
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

        float sweepEndur = swprLStats.sweepEndurance.GetValue() + swprRStats.sweepEndurance.GetValue();
        float sweepTimer = sweepEndur * 0.02f;

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

        float sweepEndur = swprLStats.sweepEndurance.GetValue() + swprRStats.sweepEndurance.GetValue();
        float sweepTimer = sweepEndur * 0.02f;

        if (timeLeft < sweepTimer)
            timeLeft = sweepTimer;
        Debug.Log("Sweep Timer is " + sweepTimer + " seconds, and isSweeping is " + isSweeping);

        if (Random.Range(0f, 1f) < 0.25f)
            CallOut("Sweep");

        if (isSweeping == false)
        {
            CallOut("Sweep");
            isSweeping = true;
            isSweeping = true;
            //am.Play("Sweep");
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
        isSweeping = true;
        //am.Play("Sweep");
        if (timeLeft < sweepTimer)
            timeLeft = sweepTimer;
        Debug.Log("Sweep Timer is " + sweepTimer + " seconds, and isSweeping is " + isSweeping);

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
        isSweeping = true;
        //am.Play("Sweep");
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
}
