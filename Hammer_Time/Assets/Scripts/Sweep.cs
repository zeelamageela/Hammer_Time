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

    public int sweepTime;
    public float sweepAmt;


    public void Sweeping()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        StartCoroutine(OnSweep());
        sweepButton.gameObject.SetActive(false);
    }

    public IEnumerator OnSweep()
    {
        
        for (int i = 0; i < sweepTime; i++)
        {
            //Debug.Log("Angular Vel is " + rb.angularVelocity);
            rb.angularDrag = rb.angularDrag + (i * (sweepAmt / 2f)); 
            rb.drag = (rb.drag - (i * sweepAmt));
            //Debug.Log("Swept Angular Vel is " + rb.angularVelocity);
            //Debug.Log("Angular Drag is " + rb.angularDrag);
            //Debug.Log(rb.drag + " is current drag");
        }
        yield return new WaitForSeconds(0.75f);
        rb.drag = 0.38f;
        sweepButton.gameObject.SetActive(true);

    }
}
