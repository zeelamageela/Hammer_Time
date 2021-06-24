using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Rock_Flick : MonoBehaviour
{
    public Rigidbody2D rb;

    public float releaseTime = .15f;

    public bool isPressed = false;
    public bool mouseUp = false;
    //public bool shotTaken = false;
    public bool isPressedAI = false;

    GameObject launcher;
    Rigidbody2D launcher_rb;

    AudioManager am;
    Vector2 startPoint;
    Vector2 endPoint;
    public Vector2 springDirection;
    public float springDistance;
    public bool springReleased;

    Transform tFollowTarget;
    public GameObject vcam_go;
    public CinemachineVirtualCamera vcam;

    public GameManager gm;

    GameObject trajLineGO;
    TrajectoryLine trajLine;
    GameObject shootKnobGO;
    ShootingKnob shootKnob;

    Vector3 lastMouseCoordinate = Vector3.zero;

    Vector2 posScale = new Vector2(1f / 3f, 1f);
    void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        GetComponent<SpriteRenderer>().enabled = false;


        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();

        trajLineGO = GameObject.Find("TrajectoryLine");
        trajLine = trajLineGO.GetComponent<TrajectoryLine>();
        //trajLine.DrawTrajectory();

        launcher = GameObject.FindWithTag("Launcher");
        launcher_rb = launcher.GetComponent<Rigidbody2D>();

        shootKnobGO = GameObject.Find("ShootingKnob");
        shootKnob = shootKnobGO.GetComponent<ShootingKnob>();

        GetComponent<SpringJoint2D>().connectedBody = launcher_rb;
        GetComponent<Rock_Colliders>().enabled = false;

        vcam_go = GameObject.Find("CM vcam1");
        vcam = vcam_go.GetComponent<CinemachineVirtualCamera>();

        gameObject.transform.parent = null;
        gameObject.transform.position = launcher.transform.position;

        GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<CircleCollider2D>().radius = 1.5f;
        mouseUp = false;
    }

    void Update()
    {
        if (isPressed)
        {
            rb.position = Vector2.Scale(Camera.main.ScreenToWorldPoint(Input.mousePosition), posScale);

            trajLine.DrawTrajectory();

            shootKnob.ParentToRock();
            //if (Input.GetKeyDown(KeyCode.O))
            //{
            //    GetComponent<Rock_Force>().flipAxis = true;
            //}
            //if (Input.GetKeyDown(KeyCode.I))
            //{
            //    GetComponent<Rock_Force>().flipAxis = false;
            //}

            OnDrag();
        }

        if (isPressedAI)
        {
            OnDrag();
        }

        if (mouseUp)
        {
            isPressed = false;
            rb.isKinematic = false;

            GetComponent<CircleCollider2D>().radius = 0.18f;
            StartCoroutine(Release());
            Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
            trajLine.Release();
            shootKnob.UnParentandHide();
        }
    }

    void OnMouseDown()
    {
        //if red has hammer
        if (gm.redHammer)
        {
            //if the rock is red
            if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
            {
                //if the ai team is not red
                if (!gm.aiTeamRed)
                {
                    GetComponent<SpringJoint2D>().dampingRatio = 0.2f;
                    GetComponent<SpringJoint2D>().frequency = 1.5f;
                    isPressed = true;
                    rb.isKinematic = true;
                }
            }
            //if the rock is yellow
            else if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
            {
                //if the ai team is not yellow
                if (!gm.aiTeamYellow)
                {
                    GetComponent<SpringJoint2D>().dampingRatio = 0.2f;
                    GetComponent<SpringJoint2D>().frequency = 1.5f;
                    isPressed = true;
                    rb.isKinematic = true;
                }
            }
        }
        else if (!gm.redHammer)
        {
            //if the rock is yellow
            if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
            {
                //if the ai team is not yellow
                if (!gm.aiTeamRed)
                {
                    GetComponent<SpringJoint2D>().dampingRatio = 0.2f;
                    GetComponent<SpringJoint2D>().frequency = 1.5f;
                    isPressed = true;
                    rb.isKinematic = true;
                }
            }
            //if the rock is red
            else if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
            {
                //if the ai team is not red
                if (!gm.aiTeamYellow)
                {
                    GetComponent<SpringJoint2D>().dampingRatio = 0.2f;
                    GetComponent<SpringJoint2D>().frequency = 1.5f;
                    isPressed = true;
                    rb.isKinematic = true;
                }
            }
        }
        //GetComponent<SpringJoint2D>().dampingRatio = 0.2f;
        //GetComponent<SpringJoint2D>().frequency = 1.5f;
        //isPressed = true;
        //rb.isKinematic = true;
    }

    void OnDrag()
    {
        startPoint = rb.position;

        GetComponent<SpriteRenderer>().enabled = false;
        endPoint = GetComponent<SpringJoint2D>().connectedBody.transform.position;
        springDistance = Vector2.Distance(startPoint, endPoint);

        springDirection = (Vector2)Vector3.Normalize(endPoint - startPoint);
    }

    public void OnMouseUp()
    {
        //if red has hammer
        if (gm.redHammer)
        {
            //if the rock is red
            if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
            {
                //if the ai team is not red
                if (!gm.aiTeamRed)
                {
                    isPressed = false;
                    rb.isKinematic = false;

                    if (rb.position.y >= -24f)
                    {
                        RockReset();
                    }
                    else if (Mathf.Abs(rb.position.x) >= 0.34f)
                    {
                        RockReset();
                    }
                    else if (springDistance <= 1.5f)
                    {
                        RockReset();
                    }
                    else
                    {
                        GetComponent<CircleCollider2D>().radius = 0.18f;
                        StartCoroutine(Release());
                        Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
                        trajLine.Release();
                        shootKnob.UnParentandHide();
                    }
                }
            }
            //if the rock is yellow
            else if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
            {
                //if the ai team is not yellow
                if (!gm.aiTeamYellow)
                {
                    isPressed = false;
                    rb.isKinematic = false;

                    if (rb.position.y >= -24f)
                    {
                        RockReset();
                    }
                    else if (Mathf.Abs(rb.position.x) >= 0.34f)
                    {
                        RockReset();
                    }
                    else if (springDistance <= 1.5f)
                    {
                        RockReset();
                    }
                    else
                    {
                        GetComponent<CircleCollider2D>().radius = 0.18f;
                        StartCoroutine(Release());
                        Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
                        trajLine.Release();
                        shootKnob.UnParentandHide();
                    }
                }
            }
        }
        //if yellow has the hammer
        else if (!gm.redHammer)
        {
            //if the rock is yellow
            if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
            {
                //if the ai team is not yellow
                if (!gm.aiTeamRed)
                {
                    isPressed = false;
                    rb.isKinematic = false;

                    if (rb.position.y >= -24f)
                    {
                        RockReset();
                    }
                    else if (Mathf.Abs(rb.position.x) >= 0.34f)
                    {
                        RockReset();
                    }
                    else if (springDistance <= 1.5f)
                    {
                        RockReset();
                    }
                    else
                    {
                        GetComponent<CircleCollider2D>().radius = 0.18f;
                        StartCoroutine(Release());
                        Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
                        trajLine.Release();
                        shootKnob.UnParentandHide();
                    }
                }
                //if the rock is red
                else if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
                {
                    //if the ai team is not red
                    if (!gm.aiTeamYellow)
                    {
                        isPressed = false;
                        rb.isKinematic = false;

                        if (rb.position.y >= -24f)
                        {
                            RockReset();
                        }
                        else if (Mathf.Abs(rb.position.x) >= 0.34f)
                        {
                            RockReset();
                        }
                        else if (springDistance <= 1.5f)
                        {
                            RockReset();
                        }
                        else
                        {
                            GetComponent<CircleCollider2D>().radius = 0.18f;
                            StartCoroutine(Release());
                            Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
                            trajLine.Release();
                            shootKnob.UnParentandHide();
                        }
                    }
                }
            }
        }
        //isPressed = false;
        //rb.isKinematic = false;

        //if (springDistance <= 1.5f)
        //{
        //    RockReset();
        //}
        //else
        //{
        //    GetComponent<CircleCollider2D>().radius = 0.18f;
        //    StartCoroutine(Release());
        //    Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
        //    trajLine.Release();
        //    shootKnob.UnParentandHide();
        //}

        //trajLineGO.SetActive(false);
    }

    void RockReset()
    {
        transform.position = launcher_rb.position;
        GetComponent<SpringJoint2D>().dampingRatio = 1f;
        GetComponent<SpringJoint2D>().frequency = 10000f;
    }

    IEnumerator Release()
    {
        mouseUp = false;
        springReleased = true;

        launcher.GetComponent<Collider2D>().enabled = true;
        yield return new WaitForSeconds(releaseTime);

        GetComponent<SpringJoint2D>().enabled = false;
        this.enabled = false;

        yield return new WaitForFixedUpdate();

        //tFollowTarget = gameObject.transform;
        //vcam.LookAt = tFollowTarget;
        //vcam.Follow = tFollowTarget;
        //vcam.enabled = true;


        //yield return new WaitUntil(() => rb.position == launcher_rb.position);

        //shotTaken = true;
    }
}