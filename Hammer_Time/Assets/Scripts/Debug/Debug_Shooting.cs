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

    public Vector2 buttonForce;
    public Vector2 houseForce;
    public Vector2 guardForce;
    public Vector2 takeoutForce;
    public Vector2 customForce;

    public CinemachineVirtualCamera vcam;
    Transform tFollowTarget;

    GameObject trajLineGO;
    TrajectoryLine trajLine;

    void Update()
    {
        if (gm.rockList != null)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                gm.OnDebugReset();
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                OnHouse();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                OnGuard();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                OnButton();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                OnTakeout();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                OnCustom();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                rock = gm.rockList[gm.rockCurrent].rock;
                rb = rock.GetComponent<Rigidbody2D>();
                rockFlick = rock.GetComponent<Rock_Flick>();
                rockInfo = rock.GetComponent<Rock_Info>();
                rockCols = rock.GetComponent<Rock_Colliders>();

                Debug.Log("Debug Stop");

                rb.velocity = Vector2.zero;
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

        trajLineGO = GameObject.Find("TrajectoryLine");
        trajLine = trajLineGO.GetComponent<TrajectoryLine>();
        trajLine.DrawTrajectory();


        StartCoroutine(HouseShot());
    }

    public void OnButton()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();
        rockCols = rock.GetComponent<Rock_Colliders>();

        trajLineGO = GameObject.Find("TrajectoryLine");
        trajLine = trajLineGO.GetComponent<TrajectoryLine>();
        trajLine.DrawTrajectory();


        StartCoroutine(ButtonShot());
    }

    public void OnGuard()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();
        rockCols = rock.GetComponent<Rock_Colliders>();

        trajLineGO = GameObject.Find("TrajectoryLine");
        trajLine = trajLineGO.GetComponent<TrajectoryLine>();
        trajLine.DrawTrajectory();


        StartCoroutine(GuardShot());
    }

    public void OnTakeout()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();
        rockCols = rock.GetComponent<Rock_Colliders>();

        trajLineGO = GameObject.Find("TrajectoryLine");
        trajLine = trajLineGO.GetComponent<TrajectoryLine>();
        trajLine.DrawTrajectory();


        StartCoroutine(TakeoutShot());
    }

    public void OnCustom()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();
        rockCols = rock.GetComponent<Rock_Colliders>();

        trajLineGO = GameObject.Find("TrajectoryLine");
        trajLine = trajLineGO.GetComponent<TrajectoryLine>();
        trajLine.DrawTrajectory();

        StartCoroutine(CustomShot());
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

        //this.enabled = false;
    }

    IEnumerator ButtonShot()
    {

        yield return new WaitForFixedUpdate();

        rock.GetComponent<SpringJoint2D>().enabled = false;
        rockFlick.enabled = false;


        rb.AddForce(buttonForce * 1000f);
        yield return new WaitForSeconds(0.1f);
        Debug.Log(rb.velocity.x + ", " + rb.velocity.y);
        rockCols.shotTaken = true;

        tFollowTarget = rock.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;

        //this.enabled = false;
    }

    IEnumerator GuardShot()
    {

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

        //this.enabled = false;
    }
    IEnumerator TakeoutShot()
    {
        yield return new WaitForFixedUpdate();

        rock.GetComponent<SpringJoint2D>().enabled = false;
        rockFlick.enabled = false;

        yield return new WaitForSeconds(0.1f);

        rb.AddForce(takeoutForce * 1000f);
        Debug.Log(rb.velocity.x + ", " + rb.velocity.y);
        rockCols.shotTaken = true;

        tFollowTarget = rock.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;

        //this.enabled = false;
    }

    IEnumerator CustomShot()
    {
        yield return new WaitForFixedUpdate();

        rock.GetComponent<SpringJoint2D>().enabled = false;
        rockFlick.enabled = false;

        yield return new WaitForSeconds(0.1f);

        rb.AddForce(customForce * 1000f);
        Debug.Log(rb.velocity.x + ", " + rb.velocity.y);
        rockCols.shotTaken = true;

        tFollowTarget = rock.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;

        //this.enabled = false;
    }
}
