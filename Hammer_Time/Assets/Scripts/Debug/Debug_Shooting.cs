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

    public Vector2 houseForce;
    public Vector2 buttonForce;
    public Vector2 guardForce;
    
    public CinemachineVirtualCamera vcam;
    Transform tFollowTarget;


    void Update()
    {

        if (gm.rockList != null)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                rock = gm.rockList[gm.rockCurrent].rock;
                rb = rock.GetComponent<Rigidbody2D>();
                rockFlick = rock.GetComponent<Rock_Flick>();
                rockInfo = rock.GetComponent<Rock_Info>();
                rockCols = rock.GetComponent<Rock_Colliders>();

                StartCoroutine(HouseShot());
            }
        }
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

    public void OnButton()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();
        rockCols = rock.GetComponent<Rock_Colliders>();

        StartCoroutine(ButtonShot());
    }

    public void OnGuard()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();
        rockCols = rock.GetComponent<Rock_Colliders>();

        StartCoroutine(GuardShot());
    }

    IEnumerator HouseShot()
    {
        yield return new WaitForFixedUpdate();

        rock.GetComponent<SpringJoint2D>().enabled = false;
        rockFlick.enabled = false;

        yield return new WaitForSeconds(0.1f);

        rb.AddForce(houseForce * 1000f);
        Debug.Log(rb.velocity.x + ", " + rb.velocity.y);
        rockCols.shotTaken = true;

        tFollowTarget = rock.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;

        this.enabled = false;
    }

    IEnumerator ButtonShot()
    {
        Debug.Log("gonna shoot");

        yield return new WaitForFixedUpdate();

        rock.GetComponent<SpringJoint2D>().enabled = false;
        rockFlick.enabled = false;

        yield return new WaitForSeconds(0.1f);

        rb.AddForce(buttonForce * 1000f);
        Debug.Log(rb.velocity.x + ", " + rb.velocity.y);
        rockCols.shotTaken = true;

        tFollowTarget = rock.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;

        this.enabled = false;
    }

    IEnumerator GuardShot()
    {
        Debug.Log("gonna shoot");

        yield return new WaitForFixedUpdate();

        rock.GetComponent<SpringJoint2D>().enabled = false;
        rockFlick.enabled = false;

        yield return new WaitForSeconds(0.1f);

        rb.AddForce(guardForce * 1000f);
        Debug.Log(rb.velocity.x + ", " + rb.velocity.y);
        rockCols.shotTaken = true;

        tFollowTarget = rock.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;

        this.enabled = false;
    }
}
