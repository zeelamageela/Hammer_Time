using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperManager : MonoBehaviour
{

    public Sweeper sweeperL;
    public Sweeper sweeperR;

    public Sweeper sweeperRedL;
    public Sweeper sweeperRedR;
    public Sweeper sweeperYellowL;
    public Sweeper sweeperYellowR;

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

    private void Awake()
    {
        am = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        if (am == null)
        {
            Debug.Log("Audio Manager not loaded");
        }

        rockSounds = sweepSel.GetComponents<AudioSource>();
    }

    private void Update()
    {
        if (swprLStats & swprRStats)
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
        
        
    }

    public void SetupSweepers(bool redTurn)
    {
        sweeperRedL.gameObject.SetActive(false);
        sweeperRedR.gameObject.SetActive(false);
        sweeperYellowL.gameObject.SetActive(false);
        sweeperYellowR.gameObject.SetActive(false);

        if (redTurn)
        {
            sweeperL = sweeperRedL;
            sweeperR = sweeperRedR;
        }
        else
        {
            sweeperL = sweeperYellowL;
            sweeperR = sweeperYellowR;
        }

        sweeperL.gameObject.SetActive(true);
        sweeperR.gameObject.SetActive(true);
        swprLStats = sweeperL.gameObject.GetComponent<CharacterStats>();
        swprRStats = sweeperR.gameObject.GetComponent<CharacterStats>();

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
        sweeperRedL.gameObject.SetActive(false);
        sweeperRedR.gameObject.SetActive(false);
        sweeperYellowL.gameObject.SetActive(false);
        sweeperYellowR.gameObject.SetActive(false);

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
            sweepButton.SetActive(true);
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
            sweeperL.yOffset = 1.1f;

            sweeperR.yOffset = 0.6f;
        }
        else
        {
            sweeperL.yOffset = 0.6f;

            sweeperR.yOffset = 1.1f;
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
            sweepButton.SetActive(true);
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
            sweepButton.SetActive(true);
            hardButton.SetActive(false);
        }
    }

    public void SweepLeft(bool aiTurn)
    {
        //am.Play("Sweep");
        rockSounds[0].enabled = true;
        rockSounds[1].enabled = false;
        rockSounds[0].pitch = 1f;
        rockSounds[1].pitch = 1f;
        sweep.OnLeft();
        sweeperL.yOffset = 0.6f;
        sweeperL.Sweep();

        sweeperR.yOffset = 1.1f;
        sweeperR.Whoa();

        if (!aiTurn)
        {
            sweepButton.SetActive(true);
            hardButton.SetActive(false);
            whoaButton.SetActive(true);
        }
    }

    public void SweepRight(bool aiTurn)
    {
        //am.Play("Sweep");
        rockSounds[0].enabled = false;
        rockSounds[1].enabled = true;
        rockSounds[0].pitch = 1f;
        rockSounds[1].pitch = 1f;

        sweep.OnRight();
        sweeperL.yOffset = 1.1f;
        sweeperL.Whoa();

        sweeperR.yOffset = 0.6f;
        sweeperR.Sweep();

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
