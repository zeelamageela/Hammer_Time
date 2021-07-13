using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock_Release : MonoBehaviour
{
    GameObject hogline;
    Collider2D hogline_collider;
    private Rigidbody2D body;

    public bool released = false;


    void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        hogline = GameObject.Find("Hog_Line");
        hogline_collider = hogline.GetComponent<Collider2D>();
    }


    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider == hogline_collider)
        {

            //Debug.Log("Released");
            //Debug.Log("Hog Line");

            released = true;
            GetComponent<Rock_Info>().released = true;
            GetComponent<Rock_Force>().Release();
        }

    }
}
