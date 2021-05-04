using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Rock_Flick: MonoBehaviour
{
    public Rigidbody2D rb;

    public float releaseTime = .15f;

    public bool isPressed = false;
    //public bool shotTaken = false;

    public Rock_Traj trajectory;

    GameObject launcher;
    Rigidbody2D launcher_rb;

    Vector2 startPoint;
    Vector2 endPoint;
    public Vector2 springDirection;
    public Vector2 force;
    Vector2 pos;
    public float springDistance;
    public float springForce;
    public bool springReleased;

    Transform tFollowTarget;
    public GameObject vcam_go;
    public CinemachineVirtualCamera vcam;

    void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject traj_go = GameObject.Find("Trajectory");
        trajectory = traj_go.GetComponent<Rock_Traj>();

        launcher = GameObject.FindWithTag("Launcher");
        launcher_rb = launcher.GetComponent<Rigidbody2D>();

        GetComponent<SpringJoint2D>().connectedBody = launcher_rb;
        GetComponent<Rock_Colliders>().enabled = false;

        vcam_go = GameObject.Find("CM vcam1");
        vcam = vcam_go.GetComponent<CinemachineVirtualCamera>();

        gameObject.transform.parent = null;
        gameObject.transform.position = launcher.transform.position;
    }

    void Update()
    {
        if (isPressed)
        {
            rb.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (springDirection.x < 0f)
            {
                GetComponent<Rock_Force>().flipAxis = true;
            }
            else if (springDirection.y > 0f)
            {
                GetComponent<Rock_Force>().flipAxis = false;
            }

            OnDrag();
        }
    }

    void OnMouseDown()
    {
        isPressed = true;
        rb.isKinematic = true;
        //trajectory.Show();
        GetComponent<SpriteRenderer>().enabled = true;
    }

    void OnDrag()
    {
        GameObject rock = gameObject;
        startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        endPoint = GetComponent<SpringJoint2D>().connectedBody.transform.position;
        springDistance = Vector2.Distance(startPoint, endPoint);
        force = GetComponent<SpringJoint2D>().GetReactionForce(Time.deltaTime);
        springDirection = (Vector2)Vector3.Normalize(endPoint - startPoint);
        springForce = force.magnitude;
    }

    public void OnMouseUp()
    {
        isPressed = false;
        rb.isKinematic = false;
        StartCoroutine(Release());
    }

    public IEnumerator Release()
    {
        yield return new WaitForSeconds(releaseTime);

        GetComponent<SpringJoint2D>().enabled = false;
        springReleased = true;
        this.enabled = false;

        yield return new WaitForFixedUpdate();

        tFollowTarget = gameObject.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;

        launcher.GetComponent<Collider2D>().enabled = true;

        //yield return new WaitUntil(() => rb.position == launcher_rb.position);

        //shotTaken = true;
    }

}