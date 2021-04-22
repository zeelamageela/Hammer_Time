using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock_Force : MonoBehaviour
{
    private Rigidbody2D body;

    public float turnValue = 60f;
    public Vector2 curl;

    float velX = 0f;
    float velY = 0f;
    Vector2 vel;
    bool turnStart;
    bool forceStart;
    //public bool moving;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Release()
    {
        Transform go = gameObject.transform;

        if (go.position.x <= 0f)
        {
            turnValue = -turnValue;
            curl = new Vector2 (-curl.x , curl.y);
        }

        turnStart = true;
        forceStart = true;

        return;
    }


    void FixedUpdate() 
    {
        velX = body.angularVelocity;
        vel = new Vector2 (velX, velY);

        Vector2 start = transform.position;
        
        if (turnStart == true)
        {
            body.AddTorque(turnValue * Mathf.Deg2Rad, ForceMode2D.Impulse);
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
