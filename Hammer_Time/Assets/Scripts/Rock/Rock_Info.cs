using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock_Info : MonoBehaviour
{
    public string teamName;
    public int rockNumber;

    public bool stopped = false;
    public bool rest = false;
    public bool released = false;
    public bool inPlay = true;
    public bool outOfPlay = false;
    public bool inHouse = false;
    public bool hit = false;
    public bool moving = false;
    public bool shotTaken = false;
    public bool placed = false;
    public bool freeGuard = false;
    public float velocity;

    public float distance;

    void FixedUpdate()
    {
        if (!placed)
        {
            distance = Vector2.Distance(new Vector2(0.02f, 6.5f), gameObject.transform.position);
            shotTaken = GetComponent<Rock_Colliders>().shotTaken;
        }
        else
        {

        }

        inPlay = GetComponent<Rock_Colliders>().inPlay;
        outOfPlay = GetComponent<Rock_Colliders>().outOfPlay;
        hit = GetComponent<Rock_Colliders>().hit;
        inHouse = GetComponent<Rock_Colliders>().inHouse;

        //if (transform.hasChanged)
        //{
        //    transform.hasChanged = false;
        //    stopped = false;
        //    moving = true;
        //}
        //else moving = false;


        if (transform.hasChanged)
        {
            //Debug.Log("Hit RockInfo!");
            transform.hasChanged = false;
            stopped = false;
            moving = true;
        }
        else
        {
            moving = false;
            stopped = true;
        }

        if (shotTaken && released)
        {
            if (moving == false)
            {
                stopped = true;
            }
        }

        if (stopped == true && rest == false)
        {
            rest = true;

            if (!inPlay)
            {
                outOfPlay = true;
            }
        }

        if (hit == true)
        {
            if (transform.hasChanged)
            {
                //Debug.Log("Hit RockInfo!");
                transform.hasChanged = false;
                stopped = false;
                moving = true;
            }
            else
            {
                stopped = true;
                hit = false;
            }

            if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) > 0.01f && Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) > 0.01f)
            {
                stopped = false;
                moving = true;

            }
            else
            {
                stopped = true;
                moving = false;
            }

        }

        if (!shotTaken)
        {
            stopped = false;
            rest = false;
        }
    }   
}
