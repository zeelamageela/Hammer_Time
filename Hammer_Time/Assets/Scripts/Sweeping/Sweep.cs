using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

public class Sweep : MonoBehaviour
{
    public GameManager gm;
    public SweeperManager sm;
    public MMFeedbackFloatingText fltText;

    RockManager rm;
    GameObject rock;
    Rigidbody2D rb;

    public GameObject sweeperGO;
    public Sweeper sweeper;
    //public SweepSelector sweepSel;
    public Button sweepButton;
    public Button hardButton;
    public Button whoaButton;
    public Button leftButton;
    public Button rightButton;

    public int sweepTime;
    public float sweepAmt;

    float statCalc;
    float statEndur;

    void Start()
    {
        rm = GetComponent<RockManager>();
    }

    private void Update()
    {
        fltText.Direction = sm.sweepSel.moveDirection;
    }

    public void EnterSweepZone()
    {
        //sweepButton.gameObject.SetActive(true);
        //sweeper.gameObject.SetActive(true);

        //sweepSel.SetupSweepers();
    }

    public void ExitSweepZone()
    {
        OnWhoa();
        sm.ResetSweepers();
    }

    public void OnSweep()
    {
        statCalc = sm.swprLStats.sweepStrength.GetValue() + sm.swprRStats.sweepStrength.GetValue();
        statEndur = sm.swprLStats.sweepEndurance.GetValue() + sm.swprRStats.sweepEndurance.GetValue();
        fltText.Direction = sm.sweepSel.moveDirection;

        //Time.timeScale = 0.25f;
        StartCoroutine(SweepWeight());
    }

    public void OnHard()
    {

        statCalc = sm.swprLStats.sweepStrength.GetValue() + sm.swprRStats.sweepStrength.GetValue();
        StartCoroutine(SweepHard());
    }

    public void OnLeft()
    {
        statCalc = sm.swprLStats.sweepStrength.GetValue();
        if (!rm.inturn)
        {
            StartCoroutine(SweepLine(true));
        }
        else
        {
            StartCoroutine(SweepCurl(false));
        }
        
    }

    public void OnRight()
    {
        statCalc = sm.swprRStats.sweepStrength.GetValue();
        if (!rm.inturn)
        {
            StartCoroutine(SweepCurl(true));
        }
        else
        {
            StartCoroutine(SweepLine(false));
        }
    }

    public void OnWhoa()
    {
        Time.timeScale = 1f;
        StartCoroutine(Whoa());
    }

    IEnumerator SweepWeight()
    {
        Debug.Log("Rock being swept - " + gm.rockList[gm.rockCurrent].rock.name);
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();

        //fltText.Value = "SWEEP!";
        //fltText.TargetTransform = gm.rockList[gm.rockCurrent].rock.transform;
        //fltText.Direction = sm.sweepSel.moveDirection;
        //fltText.Play(gm.rockList[gm.rockCurrent].rock.transform.position);

        //rb.angularDrag = rb.angularDrag + (5f * (sweepAmt));

        yield return new WaitForSeconds(sweepTime);
        //sm.SweepWeight();
        //sweepSel.SweepWeight();
        float curl = rock.GetComponent<Rock_Force>().curl.x + ((statCalc / 2f) * sweepAmt);
        rock.GetComponent<Rock_Force>().curl.x = curl;

        Debug.Log("Curl is " + rock.GetComponent<Rock_Force>().curl.x);


        Debug.Log("Sweep Amount is " + ((statCalc / 2f) * sweepAmt));

        rb.drag -= sweepAmt * statCalc / 2f;

        if (gm.debug)
        {
            fltText.Value = "Drag - " + rb.drag.ToString() + " /-/ Curl - " + rock.GetComponent<Rock_Force>().curl.x;
            fltText.TargetTransform = gm.rockList[gm.rockCurrent].rock.transform;
            fltText.Direction = sm.sweepSel.moveDirection;
            fltText.Play(gm.rockList[gm.rockCurrent].rock.transform.position, 2f);
        }
    }

    IEnumerator SweepHard()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rock.GetComponent<Rock_Force>().curl.x = -0.5f;

        rb.drag = 0.38f;
        rb.angularDrag = 0.32f;

        yield return new WaitForSeconds(sweepTime);
        //sm.SweepHard();
        //sweepSel.SweepHard();
        rb.drag = (rb.drag - (1.5f * sweepAmt));

        float curl = rock.GetComponent<Rock_Force>().curl.x + ((statCalc / 2f) * sweepAmt);
        rock.GetComponent<Rock_Force>().curl.x = curl;

        Debug.Log("Curl is " + rock.GetComponent<Rock_Force>().curl.x);
    }

    IEnumerator SweepLine(bool inturn)
    {
        sm.CallOut("Line");
        rock = gm.rockList[gm.rockCurrent].rock;
        fltText.Value = "LINE!";
        //fltText.TargetTransform = gm.rockList[gm.rockCurrent].rock.transform;
        //fltText.Direction = sm.sweepSel.moveDirection;
        //fltText.Play(gm.rockList[gm.rockCurrent].rock.transform.position);
        //rb = rock.GetComponent<Rigidbody2D>();
        rock.GetComponent<Rock_Force>().curl.x = -0.5f;

        rb.drag = 0.38f;
        rb.angularDrag = 0.32f;

        yield return new WaitForSeconds(sweepTime);

        //if (inturn)
        //{
        //    sm.SweepLeft();
        //    //sweepSel.SweepLeft();
        //}
        //else
        //{
        //    sm.SweepRight();
        //    //sweepSel.SweepRight();
        //}

        rb.drag -= sweepAmt * statCalc / 4f;

        float curl = rock.GetComponent<Rock_Force>().curl.x + (statCalc * sweepAmt * 5f);
        rock.GetComponent<Rock_Force>().curl.x = curl;
        Debug.Log("Curl is " + rock.GetComponent<Rock_Force>().curl.x);

        if (gm.debug)
        {
            fltText.Value = "Drag - " + rb.drag.ToString() + " --- Curl - " + rock.GetComponent<Rock_Force>().curl.x;
            fltText.TargetTransform = gm.rockList[gm.rockCurrent].rock.transform;
            fltText.Direction = sm.sweepSel.moveDirection;
            fltText.Play(gm.rockList[gm.rockCurrent].rock.transform.position, 2f);
        }
    }

    IEnumerator SweepCurl(bool inturn)
    {
        sm.CallOut("Curl");
        rock = gm.rockList[gm.rockCurrent].rock;
        //fltText.Value = "CURL!";
        //fltText.TargetTransform = gm.rockList[gm.rockCurrent].rock.transform;
        //fltText.Direction = sm.sweepSel.moveDirection;
        //fltText.Play(gm.rockList[gm.rockCurrent].rock.transform.position);
        rb = rock.GetComponent<Rigidbody2D>();
        rock.GetComponent<Rock_Force>().curl.x = -0.5f;

        rb.drag = 0.38f;
        rb.angularDrag = 0.32f;

        yield return new WaitForSeconds(sweepAmt);

        //if (inturn)
        //{
        //    sm.SweepRight();
        //    //sweepSel.SweepRight();
        //}
        //else
        //{
        //    sm.SweepLeft();
        //    //sweepSel.SweepLeft();
        //}

        rb.drag -= sweepAmt * statCalc / 4f;

        float curl = rock.GetComponent<Rock_Force>().curl.x - (sweepAmt * statCalc * 5f);
        rock.GetComponent<Rock_Force>().curl.x = curl;
        Debug.Log("Curl is " + rock.GetComponent<Rock_Force>().curl.x);

        if (gm.debug)
        {
            fltText.Value = "Drag - " + rb.drag.ToString() + " /-/ Curl - " + rock.GetComponent<Rock_Force>().curl.x;
            fltText.TargetTransform = gm.rockList[gm.rockCurrent].rock.transform;
            fltText.Direction = sm.sweepSel.moveDirection;
            fltText.Play(gm.rockList[gm.rockCurrent].rock.transform.position);
        }
    }

    IEnumerator Whoa()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(sweepTime);
        //sm.SweepWhoa();
        //sweepSel.SweepWhoa();

        rock.GetComponent<Rock_Force>().curl.x = -0.5f;

        Time.timeScale = 1f;
        rb.drag = 0.38f;
        rb.angularDrag = 0.32f;
        //Debug.Log("Curl is " + rock.GetComponent<Rock_Force>().curl.x);

        if (gm.debug)
        {
            fltText.Value = "Drag - " + rb.drag.ToString() + " /-/ Curl - " + rock.GetComponent<Rock_Force>().curl.x;
            fltText.TargetTransform = gm.rockList[gm.rockCurrent].rock.transform;
            fltText.Direction = sm.sweepSel.moveDirection;
            fltText.Play(gm.rockList[gm.rockCurrent].rock.transform.position);
        }
    }
}
