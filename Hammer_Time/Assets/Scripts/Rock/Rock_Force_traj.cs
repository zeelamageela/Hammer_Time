using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock_Force_traj : MonoBehaviour
{
    public Collider2D hogLine;
    private Rigidbody2D body;

    public float turnValue = 60f;
    public Vector2 curl;
    public float scaleFactor;

    float velX = 0f;
    float velY = 0f;
    bool turnStart;
    bool forceStart;
    public bool flipAxis = false;
    //public bool moving;
    int dirMult = 1;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        hogLine = GameObject.Find("Hog_Line").GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == hogLine)
        {
            Release();
        }
    }
    public void Release()
    {
        if (flipAxis)
        {
            dirMult = -1;
        }

        GetComponent<SpriteRenderer>().enabled = true;
        turnStart = true;
        forceStart = true;

        return;
    }


    void FixedUpdate()
    {
        velX = body.angularVelocity;

        Vector2 vel = new Vector2(velX * scaleFactor, velY);

        if (turnStart == true)
        {
            body.AddTorque(dirMult * turnValue * Mathf.Deg2Rad, ForceMode2D.Impulse);
            Debug.Log("Rotate");
            turnStart = false;
        }

        if (forceStart == true)
        {
            //Debug.Log("Curl Force");
            body.AddForce(curl * vel, ForceMode2D.Force);

            
        }
    }
}