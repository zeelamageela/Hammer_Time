using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sweep : MonoBehaviour
{
    public GameManager gm;

    GameObject rock;
    Rigidbody2D rb;

    public Button sweepButton;
    public Button whoaButton;

    public int sweepTime;
    public float sweepAmt;

    void Start()
    {
        whoaButton.gameObject.SetActive(false);
        sweepButton.gameObject.SetActive(false);
    }

    public void EnterSweepZone()
    {
        sweepButton.gameObject.SetActive(true);
    }

    public void ExitSweepZone()
    {
        sweepButton.gameObject.SetActive(false);
        whoaButton.gameObject.SetActive(false);
    }

    public void OnSweep()
    {
        StartCoroutine(SweepCurl());

        sweepButton.gameObject.SetActive(false);
        whoaButton.gameObject.SetActive(true);
    }

    public void OnLine()
    {
        StartCoroutine(SweepLine());
    }

    public void OnCurl()
    {
        StartCoroutine(SweepCurl());
    }
    public void OnWhoa()
    {
        StartCoroutine(Whoa());

        sweepButton.gameObject.SetActive(true);
        whoaButton.gameObject.SetActive(false);
    }

    IEnumerator SweepWeight()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();

        //rb.angularDrag = rb.angularDrag + (5f * (sweepAmt));

        float curl = rock.GetComponent<Rock_Force>().curl.x + (5f * sweepAmt);
        rock.GetComponent<Rock_Force>().curl.x = curl;

        Debug.Log("Curl is " + rock.GetComponent<Rock_Force>().curl.x);
        rb.drag = (rb.drag - (sweepAmt));

        yield return new WaitForSeconds(sweepTime);
    }

    IEnumerator SweepLine()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(sweepAmt);

        rb.drag = (rb.drag - (sweepAmt / 2f));

        float curl = rock.GetComponent<Rock_Force>().curl.x + (9f * sweepAmt);
        rock.GetComponent<Rock_Force>().curl.x = curl;
    }

    IEnumerator SweepCurl()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(sweepAmt);

        rb.drag = (rb.drag - (sweepAmt / 2f));

        float curl = rock.GetComponent<Rock_Force>().curl.x - (4f * sweepAmt);
        rock.GetComponent<Rock_Force>().curl.x = curl;
    }

    IEnumerator Whoa()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(sweepTime);
        rock.GetComponent<Rock_Force>().curl.x = -0.5f;
        
        rb.drag = 0.38f;
        rb.angularDrag = 0.24f;
    }
}
