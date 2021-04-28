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
        for (int i = 0; i < sweepTime; i++)
        {
            rb.drag = rb.drag - sweepAmt;
            Debug.Log(rb.drag + " is current drag");
            Debug.Log("Sweep Counter - " + i);
            //rb.angularDrag = rb.angularDrag + sweepAmt;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();

    }
}
