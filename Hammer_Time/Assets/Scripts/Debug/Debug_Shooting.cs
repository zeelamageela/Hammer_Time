using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Debug_Shooting : MonoBehaviour
{
    public GameManager gm;
    GameObject rock;
    Rigidbody2D rb;
    Rock_Flick rockFlick;
    Rock_Info rockInfo;
    Rock_Colliders rockCols;
    public Transform houseMarker;
    public Transform buttonMarker;
    public Transform guardMarker;
    public Vector2 velocity;
    bool charge;

    //public GameObject vcamGO;
    public CinemachineVirtualCamera vcam;
    Transform tFollowTarget;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            rock = gm.rockList[gm.rockCurrent].rock;
            rb = rock.GetComponent<Rigidbody2D>();
            rockFlick = rock.GetComponent<Rock_Flick>();
            rockInfo = rock.GetComponent<Rock_Info>();

            StartCoroutine(HouseShot());
        }

        if (rockCols.shotTaken = true)
    }

    public void OnHouse()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();
        rockCols = rock.GetComponent<Rock_Colliders>();

        StartCoroutine(HouseShot());
    }


    IEnumerator HouseShot()
    {
        Debug.Log("gonna shoot");

        yield return new WaitForFixedUpdate();

        rock.GetComponent<SpringJoint2D>().enabled = false;

        yield return new WaitForSeconds(0.1f);

        rb.AddForce(velocity * 1000f);
        Debug.Log(rb.velocity.x + ", " + rb.velocity.y);
        rockCols.shotTaken = true;


        tFollowTarget = rock.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;
    }
}
