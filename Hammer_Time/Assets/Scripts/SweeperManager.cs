using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperManager : MonoBehaviour
{

    public Sweeper sweeperL;
    public Sweeper sweeperR;

    public Sweep sweep;

    public SweeperSelector sweepSel;

    public RockManager rm;
    public bool inturn;

    public void Update()
    {
        if(sweeperL.sweep)
        {
            SweepLeft();
            sweep.OnLeft();
        }
        if (sweeperR.sweep)
        {
            SweepRight();
            sweep.OnRight();
        }
    }
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
    }

    public void Release(GameObject rock)
    {
        sweepSel.gameObject.SetActive(true);
        sweepSel.AttachToRock(rock);
        inturn = rm.inturn;

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
    }

    public void SweepHard()
    {
        sweeperL.Hard();
        sweeperR.Hard();
    }

    public void SweepWhoa()
    {
        sweeperL.Whoa();
        sweeperR.Whoa();
    }

    public void SweepLeft()
    {
        sweeperL.yOffset = 0.6f;
        sweeperL.Sweep();

        sweeperR.yOffset = 1.1f;
        sweeperR.Whoa();
    }

    public void SweepRight()
    {
        sweeperL.yOffset = 1.1f;
        sweeperL.Whoa();

        sweeperR.yOffset = 0.6f;
        sweeperR.Sweep();
    }
}
