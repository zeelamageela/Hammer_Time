using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sweep : MonoBehaviour
{
    public GameManager gm;

    GameObject rock;
    Rigidbody2D rb;

    public int sweepTime;
    public float sweepAmt;


    public void Sweeping()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        StartCoroutine(OnSweep());
    }

    public IEnumerator OnSweep()
    {
        rb.drag = rb.drag - sweepAmt;
        Debug.Log(rb.drag + " is current drag");
        rb.angularDrag = rb.angularDrag + sweepAmt / 2f;
        yield return new WaitForSeconds(0.25f);
        rb.drag = 0.38f;

    }
}
