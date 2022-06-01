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

    public bool story;
    AudioSource[] rockSounds;

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

        rockSounds = GetComponents<AudioSource>();
        //Debug.Log(rockSounds[0].clip.name + " - " + rockSounds[1].clip.name);
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
            Debug.Log("Release Pullback is " + rb.position.x + ", " + rb.position.y);
            GetComponent<CircleCollider2D>().radius = 0.14f;
            StartCoroutine(Release());
            Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
            trajLine.Release();
            shootKnob.UnParentandHide();
        }

        if (springReleased)
        {
            isPressed = false;
        }
    }

    void OnMouseDown()
    {
        if (!GetComponent<Rock_Info>().released)
        {//if red has hammer
            Debug.Log("Clicked on Rock");
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
            else
            {
                //if the rock is red
                if (GetComponent<Rock_Info>().teamName == gm.rockList[0].rockInfo.teamName)
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
                else if (GetComponent<Rock_Info>().teamName == gm.rockList[1].rockInfo.teamName)
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
        }
        
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
        if (!GetComponent<Rock_Info>().released)
        {//if red has hammer
            if (!story && gm.redHammer && !GetComponent<Rock_Info>().released)
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
                            GetComponent<CircleCollider2D>().radius = 0.14f;
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

                        Debug.Log("What the fuckkkk");
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
                            GetComponent<CircleCollider2D>().radius = 0.14f;
                            StartCoroutine(Release());
                            Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
                            trajLine.Release();
                            shootKnob.UnParentandHide();
                        }
                    }
                }
            }
            //if yellow has the hammer
            else if (!story && !gm.redHammer && !GetComponent<Rock_Info>().released)
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
                            GetComponent<CircleCollider2D>().radius = 0.14f;
                            StartCoroutine(Release());
                            //Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
                            trajLine.Release();
                            shootKnob.UnParentandHide();
                        }
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
                            GetComponent<CircleCollider2D>().radius = 0.14f;
                            StartCoroutine(Release());
                            //Debug.Log("Pullback is " + transform.position.x + ", " + transform.position.y);
                            trajLine.Release();
                            shootKnob.UnParentandHide();
                        }
                    }
                }
            }
            else
            {
                StartCoroutine(RockResetStory());
            }
        }
        
    }

    public void RockReset()
    {
        transform.position = launcher_rb.position;
        GetComponent<SpringJoint2D>().dampingRatio = 1f;
        GetComponent<SpringJoint2D>().frequency = 10000f;
    }

    IEnumerator RockResetStory()
    {
        GetComponent<SpringJoint2D>().dampingRatio = 1f;
        GetComponent<SpringJoint2D>().frequency = 10000f;
        transform.position = launcher_rb.position;

        isPressed = false;
        yield return new WaitForEndOfFrame();
        GetComponent<SpringJoint2D>().dampingRatio = 0.2f;
        GetComponent<SpringJoint2D>().frequency = 1.5f;
    }

    IEnumerator Release()
    {
        mouseUp = false;
        springReleased = true;


        //ShotLocation();
        //GetComponent<SpringJoint2D>().enabled = true;
        //Debug.Log("Waht is happening in here. is the spring enabled " + GetComponent<SpringJoint2D>().enabled);
        launcher.GetComponent<Collider2D>().enabled = true;
        yield return new WaitForSeconds(releaseTime);
        rockSounds[1].enabled = true;
        GetComponent<SpringJoint2D>().enabled = false;
        //Debug.Log("Waht is happening in HERE. is the spring enabled " + GetComponent<SpringJoint2D>().enabled);
        this.enabled = false;

        yield return new WaitForFixedUpdate();

        //tFollowTarget = gameObject.transform;
        //vcam.LookAt = tFollowTarget;
        //vcam.Follow = tFollowTarget;
        //vcam.enabled = true;


        //yield return new WaitUntil(() => rb.position == launcher_rb.position);

        //shotTaken = true;
    }

    void ShotLocation()
    {
        AI_Sweeper aiSweep = FindObjectOfType<AI_Sweeper>();
        AI_Shooter aiShoot = FindObjectOfType<AI_Shooter>();
        RockManager rm = FindObjectOfType<RockManager>();
        Vector3 aimPos = trajLine.aimCircle.transform.position;

        string shotType;

        //aim circle is in the house
        if (Vector2.Distance(new Vector2(0f, 6.5f), new Vector2(aimPos.x, aimPos.y)) < 1.5f)
        {
            //Button
            if (Vector2.Distance(new Vector2(0f, 6.5f), new Vector2(aimPos.x, aimPos.y)) < 0.25f)
            {
                shotType = "Button";
            }
            //in the centre
            else if (Mathf.Abs(aimPos.x) < 0.25f)
            {
                //in the front of the house
                if (aimPos.y < 6.5f)
                {
                    //in the four foot
                    if (aimPos.y < 6f)
                    {
                        shotType = "Top Four Foot";
                    }
                    else
                    {
                        shotType = "Top Twelve Foot";
                    }
                }
                else
                {
                    //in the four foot
                    if (aimPos.y < 7f)
                    {
                        shotType = "Back Four Foot";
                    }
                    else
                    {
                        shotType = "Back Twelve Foot";
                    }
                }
            }
            //on the left
            else if (aimPos.x < 0f)
            {
                //in the four foot
                if (Vector2.Distance(new Vector2(0f, 6.5f), new Vector2(aimPos.x, aimPos.y)) < 0.75f)
                {
                    shotType = "Left Four Foot";
                }
                else
                {
                    shotType = "Left Twelve Foot";
                }
            }
            //on the right
            else if (aimPos.x > 0f)
            {
                //in the four foot
                if (Vector2.Distance(new Vector2(0f, 6.5f), new Vector2(aimPos.x, aimPos.y)) < 0.75f)
                {
                    shotType = "Right Four Foot";
                }
                else
                {
                    shotType = "Right Twelve Foot";
                }
            }
            //something has gone wrong
            else
            {
                shotType = "Default - No Shot";
            }
        }
        //outside the house
        else
        {
            //in the centre
            if (Mathf.Abs(aimPos.x) < 0.35f)
            {
                if (aimPos.y < 2f)
                {
                    if (aimPos.y < 4f)
                    {
                        shotType = "High Centre Guard";
                    }
                    else
                    {
                        shotType = "Centre Guard";
                    }
                }
                else
                {
                    shotType = "Tight Centre Guard";
                }
            }
            //on the left
            else if (aimPos.x < 0f)
            {
                if (aimPos.y < 2f)
                {
                    if (aimPos.y < 4f)
                    {
                        shotType = "Left High Corner Guard";
                    }
                    else
                    {
                        shotType = "Left Corner Guard";
                    }
                }
                else
                {
                    shotType = "Left Tight Corner Guard";
                }
            }
            //on the right
            else if (aimPos.x > 0f)
            {
                if (aimPos.y < 2f)
                {
                    if (aimPos.y < 4f)
                    {
                        shotType = "Right High Corner Guard";
                    }
                    else
                    {
                        shotType = "Right Corner Guard";
                    }
                }
                else
                {
                    shotType = "Right Tight Corner Guard";
                }
            }
            else
            {
                shotType = "Default - No Shot";
            }
        }


        aiSweep.OnSweep(false, "Draw To Target", new Vector2(aimPos.x, aimPos.y), rm.inturn);
    }
}