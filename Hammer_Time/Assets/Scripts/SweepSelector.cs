using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweepSelector : MonoBehaviour
{
    public Animator sweeper1;
    public Animator sweeper2;

    public SpriteRenderer sweepFRspr;
    public SpriteRenderer sweepFLspr;
    public SpriteRenderer sweepRRspr;
    public SpriteRenderer sweepRLspr;

    public Animator sweepFRAnim;
    public Animator sweepFLAnim;
    public Animator sweepRRAnim;
    public Animator sweepRLAnim;

    public RockManager rm;
    bool inturn;

    private void Start()
    {
        sweepFLAnim.enabled = false;
        sweepFLspr.enabled = false;

        sweepFRAnim.enabled = false;
        sweepFRspr.enabled = false;

        sweepRLAnim.enabled = false;
        sweepRLspr.enabled = false;

        sweepRRAnim.enabled = false;
        sweepRRspr.enabled = false;
    }

    public void SetupSweepers()
    {
        inturn = rm.inturn;

        if (inturn)
        {
            sweepFLAnim.enabled = true;
            sweepFLspr.enabled = true;
            sweepFLAnim.SetFloat("Speed", 1.0f);
            sweepFLAnim.SetBool("Whoa", true);

            sweepRRAnim.enabled = true;
            sweepRRspr.enabled = true;
            sweepRRAnim.SetFloat("Speed", 1.0f);
            sweepRRAnim.SetBool("Whoa", true);

            sweepFRAnim.enabled = false;
            sweepFRspr.enabled = false;

            sweepRLAnim.enabled = false;
            sweepRLspr.enabled = false;
        }
        else
        {
            sweepFRAnim.enabled = true;
            sweepFRspr.enabled = true;
            sweepFRAnim.SetFloat("Speed", 1.0f);
            sweepFRAnim.SetBool("Whoa", true);

            sweepRLAnim.enabled = true;
            sweepRLspr.enabled = true;
            sweepRLAnim.SetFloat("Speed", 1.0f);
            sweepRLAnim.SetBool("Whoa", true);

            sweepFLAnim.enabled = false;
            sweepFLspr.enabled = false;

            sweepRRAnim.enabled = false;
            sweepRRspr.enabled = false;
        }
    }

    public void SweepRight()
    {
        sweepFRAnim.enabled = true;
        sweepFRspr.enabled = true;
        sweepFRAnim.SetFloat("Speed", 1.5f);
        sweepFRAnim.SetBool("Whoa", false);

        sweepRLAnim.enabled = true;
        sweepRLspr.enabled = true;
        sweepRLAnim.SetBool("Whoa", true);

        sweepFLAnim.enabled = false;
        sweepFLspr.enabled = false;

        sweepRRAnim.enabled = false;
        sweepRRspr.enabled = false;
    }

    public void SweepLeft()
    {
        sweepFLAnim.enabled = true;
        sweepFLspr.enabled = true;
        sweepFLAnim.SetFloat("Speed", 1.5f);
        sweepFLAnim.SetBool("Whoa", false);

        sweepRRAnim.enabled = true;
        sweepRRspr.enabled = true;
        sweepRRAnim.SetFloat("Speed", 1.0f);
        sweepRRAnim.SetBool("Whoa", true);

        sweepFRAnim.enabled = false;
        sweepFRspr.enabled = false;

        sweepRLAnim.enabled = false;
        sweepRLspr.enabled = false;
    }

    public void SweepWeight()
    {
        inturn = rm.inturn;

        if (inturn)
        {
            sweepFLAnim.enabled = true;
            sweepFLspr.enabled = true;
            sweepFLAnim.SetFloat("Speed", 1.0f);
            sweepFLAnim.SetBool("Whoa", false);

            sweepRRAnim.enabled = true;
            sweepRRspr.enabled = true;
            sweepRRAnim.SetFloat("Speed", 1.0f);
            sweepRRAnim.SetBool("Whoa", false);

            sweepFRAnim.enabled = false;
            sweepFRspr.enabled = false;

            sweepRLAnim.enabled = false;
            sweepRLspr.enabled = false;
        }
        else
        {
            sweepFRAnim.enabled = true;
            sweepFRspr.enabled = true;
            sweepFRAnim.SetFloat("Speed", 1.0f);
            sweepFRAnim.SetBool("Whoa", false);

            sweepRLAnim.enabled = true;
            sweepRLspr.enabled = true;
            sweepRLAnim.SetFloat("Speed", 1.0f);
            sweepRLAnim.SetBool("Whoa", false);

            sweepFLAnim.enabled = false;
            sweepFLspr.enabled = false;

            sweepRRAnim.enabled = false;
            sweepRRspr.enabled = false;
        }
    }

    public void SweepHard()
    {
        inturn = rm.inturn;

        if (inturn)
        {
            sweepFLAnim.enabled = true;
            sweepFLspr.enabled = true;
            sweepFLAnim.SetFloat("Speed", 2.0f);
            sweepFLAnim.SetBool("Whoa", false);

            sweepRRAnim.enabled = true;
            sweepRRspr.enabled = true;
            sweepRRAnim.SetFloat("Speed", 2.0f);
            sweepRRAnim.SetBool("Whoa", false);

            sweepFRAnim.enabled = false;
            sweepFRspr.enabled = false;

            sweepRLAnim.enabled = false;
            sweepRLspr.enabled = false;
        }
        else
        {
            sweepFRAnim.enabled = true;
            sweepFRspr.enabled = true;
            sweepFRAnim.SetFloat("Speed", 2.0f);
            sweepFRAnim.SetBool("Whoa", false);

            sweepRLAnim.enabled = true;
            sweepRLspr.enabled = true;
            sweepRLAnim.SetFloat("Speed", 2.0f);
            sweepRLAnim.SetBool("Whoa", false);

            sweepFLAnim.enabled = false;
            sweepFLspr.enabled = false;

            sweepRRAnim.enabled = false;
            sweepRRspr.enabled = false;
        }
    }

    public void SweepWhoa()
    {
        inturn = rm.inturn;

        if (inturn)
        {
            sweepFLAnim.enabled = true;
            sweepFLspr.enabled = true;
            sweepFLAnim.SetFloat("Speed", 1.0f);
            sweepFLAnim.SetBool("Whoa", true);

            sweepRRAnim.enabled = true;
            sweepRRspr.enabled = true;
            sweepRRAnim.SetFloat("Speed", 1.0f);
            sweepRRAnim.SetBool("Whoa", true);

            sweepFRAnim.enabled = false;
            sweepFRspr.enabled = false;

            sweepRLAnim.enabled = false;
            sweepRLspr.enabled = false;
        }
        else
        {
            sweepFRAnim.enabled = true;
            sweepFRspr.enabled = true;
            sweepFRAnim.SetFloat("Speed", 1.0f);
            sweepFRAnim.SetBool("Whoa", true);

            sweepRLAnim.enabled = true;
            sweepRLspr.enabled = true;
            sweepRLAnim.SetFloat("Speed", 1.0f);
            sweepRLAnim.SetBool("Whoa", true);

            sweepFLAnim.enabled = false;
            sweepFLspr.enabled = false;

            sweepRRAnim.enabled = false;
            sweepRRspr.enabled = false;
        }
    }

    public void SweepEnd()
    {
        sweepFLAnim.enabled = false;
        sweepFLspr.enabled = false;

        sweepFRAnim.enabled = false;
        sweepFRspr.enabled = false;

        sweepRLAnim.enabled = false;
        sweepRLspr.enabled = false;

        sweepRRAnim.enabled = false;
        sweepRRspr.enabled = false;
    }
}
