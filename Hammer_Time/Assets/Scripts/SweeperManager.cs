using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperManager : MonoBehaviour
{
    public Sweeper halFront;
    public SpriteRenderer halFrontSR;
    public Sweeper halBack;
    public SpriteRenderer halBackSR;

    public RockManager rm;
    public bool inturn;


    // Update is called once per frame
    public void SetupSweepers()
    {
        halFront.gameObject.SetActive(false);
        halBack.gameObject.SetActive(false);
    }

    public void Release(GameObject rock)
    {
        halFront.gameObject.SetActive(true);
        halFront.AttachToRock(rock);

        halBack.gameObject.SetActive(true);
        halBack.AttachToRock(rock);

        inturn = rm.inturn;

        if (inturn)
        {
            halFront.yOffset = 0.6f;

            halBack.yOffset = 1.6f;
        }
        else
        {
            halFront.yOffset = 1.6f;

            halBack.yOffset = 0.6f;
        }
    }

    public void SweepWeight()
    {
        halFront.gameObject.SetActive(false);
        halBack.gameObject.SetActive(true);

        halFront.Sweep();
        halBack.Sweep();
    }

    public void SweepHard()
    {
        halFront.Hard();
        halBack.Hard();
    }

    public void SweepWhoa()
    {
        halFront.Whoa();
        halBack.Whoa();
    }

    public void SweepLeft()
    {
        halFront.yOffset = 0.6f;
        halFront.Sweep();

        halBack.yOffset = 1.6f;
        halBack.Whoa();
    }

}
