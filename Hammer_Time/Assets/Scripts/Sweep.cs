using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sweep : MonoBehaviour
{
    public GameManager gm;
    public SweeperManager sm;

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

    void Start()
    {
        //whoaButton.gameObject.SetActive(false);
        //sweepButton.gameObject.SetActive(false);
        //hardButton.gameObject.SetActive(false);

        //sweeper.gameObject.SetActive(false);
        sm.SetupSweepers();
        rm = GetComponent<RockManager>();
    }

    public void EnterSweepZone()
    {
        //sweepButton.gameObject.SetActive(true);
        //sweeper.gameObject.SetActive(true);

        //sweepSel.SetupSweepers();
    }

    public void ExitSweepZone()
    {
        //sweepButton.gameObject.SetActive(false);
        //whoaButton.gameObject.SetActive(false);
        //hardButton.gameObject.SetActive(false);

        //sweeper.gameObject.SetActive(false);
        OnWhoa();
        sm.SetupSweepers();
        //sweepSel.SweepEnd();
    }

    public void OnSweep()
    {
        //sweepButton.gameObject.SetActive(false);
        //hardButton.gameObject.SetActive(true);
        //whoaButton.gameObject.SetActive(true);

        //sweeper.Sweep();
        StartCoroutine(SweepWeight());
    }

    public void OnHard()
    {
        //sweepButton.gameObject.SetActive(false);
        //hardButton.gameObject.SetActive(false);
        //whoaButton.gameObject.SetActive(true);

        //sweeper.Hard();
        StartCoroutine(SweepHard());
    }

    public void OnLeft()
    {
        //sweepButton.gameObject.SetActive(true);
        //hardButton.gameObject.SetActive(false);
        //whoaButton.gameObject.SetActive(true);

        if (rm.inturn)
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
        //sweepButton.gameObject.SetActive(true);
        //hardButton.gameObject.SetActive(false);
        //whoaButton.gameObject.SetActive(true);

        if (rm.inturn)
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
        StartCoroutine(Whoa());

        //sweepButton.gameObject.SetActive(true);
        //hardButton.gameObject.SetActive(false);
        //whoaButton.gameObject.SetActive(false);
        
        //sweeper.Whoa();
    }

    IEnumerator SweepWeight()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();

        //rb.angularDrag = rb.angularDrag + (5f * (sweepAmt));

        yield return new WaitForSeconds(sweepTime);
        //sm.SweepWeight();
        //sweepSel.SweepWeight();
        float curl = rock.GetComponent<Rock_Force>().curl.x + (5f * sweepAmt);
        rock.GetComponent<Rock_Force>().curl.x = curl;

        //Debug.Log("Curl is " + rock.GetComponent<Rock_Force>().curl.x);
        rb.drag = (rb.drag - (sweepAmt));

    }

    IEnumerator SweepHard()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rock.GetComponent<Rock_Force>().curl.x = -0.5f;

        rb.drag = 0.38f;
        rb.angularDrag = 0.24f;

        yield return new WaitForSeconds(sweepTime);
        //sm.SweepHard();
        //sweepSel.SweepHard();
        rb.drag = (rb.drag - (1.5f * sweepAmt));

        float curl = rock.GetComponent<Rock_Force>().curl.x + (10f * sweepAmt);
        rock.GetComponent<Rock_Force>().curl.x = curl;

        Debug.Log("Curl is " + rock.GetComponent<Rock_Force>().curl.x);
    }

    IEnumerator SweepLine(bool inturn)
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rock.GetComponent<Rock_Force>().curl.x = -0.5f;

        rb.drag = 0.38f;
        rb.angularDrag = 0.24f;

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

        rb.drag = (rb.drag - (sweepAmt / 2f));

        float curl = rock.GetComponent<Rock_Force>().curl.x + (8f * sweepAmt);
        rock.GetComponent<Rock_Force>().curl.x = curl;
    }

    IEnumerator SweepCurl(bool inturn)
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rock.GetComponent<Rock_Force>().curl.x = -0.5f;

        rb.drag = 0.38f;
        rb.angularDrag = 0.24f;

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

        rb.drag = (rb.drag - (sweepAmt / 2f));

        float curl = rock.GetComponent<Rock_Force>().curl.x - (4f * sweepAmt);
        rock.GetComponent<Rock_Force>().curl.x = curl;
    }

    IEnumerator Whoa()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(sweepTime);
        //sm.SweepWhoa();
        //sweepSel.SweepWhoa();
        rock.GetComponent<Rock_Force>().curl.x = -0.5f;
        
        rb.drag = 0.38f;
        rb.angularDrag = 0.24f;
    }
}
