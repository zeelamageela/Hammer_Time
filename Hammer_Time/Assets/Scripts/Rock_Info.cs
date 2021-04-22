using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock_Info : MonoBehaviour
{
    public string teamName;
    public int rockNumber;

    public bool stopped = false;
    public bool rest = false;
    public bool inPlay = true;
    public bool outOfPlay = false;
    public bool inHouse = false;
    public bool hit = false;
    public bool moving = false;

    public float distance;

    List<Rock_List> rockList;

    void Awake()
    {
    }

    void FixedUpdate()
    {
        distance = Vector2.Distance(new Vector2(0.02f, 6.5f), gameObject.transform.position);

        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            stopped = false;
            moving = true;
        }
        else moving = false;
        
        if (GetComponent<Rock_Flick>().shotTaken && GetComponent<Rock_Release>().released)
        {
            if (moving == false)
            {
                stopped = true;
            }
        }

        if (stopped == true && rest == false)
        {
            StartCoroutine(Stopped());
        }

        if (hit == true && rest == true)
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                stopped = false;
                moving = true;
            }
            else
            {
                stopped = true;
                hit = false;
            }
        }
    }

    IEnumerator Stopped()
    {
        rest = true;

        if (inPlay != true)
        {
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        if (outOfPlay == true)
        {
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
        
        if (inHouse == true)
        {
            yield return new WaitForSeconds(1f);
        }

        else yield return new WaitForFixedUpdate();


    }

    
}
