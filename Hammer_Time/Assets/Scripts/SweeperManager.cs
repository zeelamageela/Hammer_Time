using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperManager : MonoBehaviour
{

    public Sweeper sweeperL;
    public Sweeper sweeperR;

    public Sweep sweep;

    public SweeperSelector sweepSel;

    public GameObject sweepButton;
    public GameObject hardButton;
    public GameObject whoaButton;
    public RockManager rm;
    public bool inturn;

    //public void Update()
    //{
    //    if(sweeperL.sweep)
    //    {
    //        SweepLeft();
    //    }
    //    if (sweeperR.sweep)
    //    {
    //        SweepRight();
    //    }
    //}
    // Update is called once per frame
    public void SetupSweepers()
    {
        sweepSel.gameObject.SetActive(false);
        sweeperL.sweep = false;
        sweeperL.hard = false;
        sweeperL.whoa = true;

        sweeperL.sweep = false;
        sweeperL.hard = false;
        sweeperL.whoa = true;

        sweepButton.SetActive(true);
    }

    public void Release(GameObject rock)
    {
        sweepSel.gameObject.SetActive(true);
        sweepSel.AttachToRock(rock);
        inturn = rm.inturn;
        //sweep.OnWhoa();
        sweepButton.SetActive(true);
        hardButton.SetActive(false);
        whoaButton.SetActive(false);
        if (inturn)
        {
            sweeperL.yOffset = 0.6f;

            sweeperR.yOffset = 1.1f;
        }
        else
        {
            sweeperL.yOffset = 1.1f;

            sweeperR.yOffset = 0.6f;
        }
    }

    public void SweepWeight()
    {
        sweeperL.Sweep();
        sweeperR.Sweep();
        sweep.OnSweep();
        sweepButton.SetActive(false);
        hardButton.SetActive(true);
        whoaButton.SetActive(true);
    }

    public void SweepHard()
    {
        sweeperL.Hard();
        sweeperR.Hard();
        sweep.OnHard();
        hardButton.SetActive(false);
        sweepButton.SetActive(true);
        whoaButton.SetActive(true);
    }

    public void SweepWhoa()
    {
        sweeperL.Whoa();
        sweeperR.Whoa();
        sweep.OnWhoa();
        whoaButton.SetActive(false);
        sweepButton.SetActive(true);
        hardButton.SetActive(false);
    }

    public void SweepLeft()
    {
        sweep.OnLeft();
        sweeperL.yOffset = 0.6f;
        sweeperL.Sweep();

        sweeperR.yOffset = 1.1f;
        sweeperR.Whoa();

        sweepButton.SetActive(true);
        hardButton.SetActive(false);
        whoaButton.SetActive(true);
    }

    public void SweepRight()
    {
        sweep.OnRight();
        sweeperL.yOffset = 1.1f;
        sweeperL.Whoa();

        sweeperR.yOffset = 0.6f;
        sweeperR.Sweep();

        sweepButton.SetActive(true);
        hardButton.SetActive(false);
        whoaButton.SetActive(true);
    }
}
