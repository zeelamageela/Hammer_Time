using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Rock_Flick: MonoBehaviour
{
    public Rigidbody2D rb;

    public float releaseTime = .15f;

    public bool isPressed = false;
    public bool shotTaken = false;

    public Rock_Traj trajectory;

    GameObject launcher;
    Rigidbody2D launcher_rb;

    Vector2 startPoint;
    Vector2 endPoint;
    Vector2 direction;
    Vector2 force;
    Vector2 pos;
    float distance;

    Transform tFollowTarget;
    public GameObject vcam_go;
    public CinemachineVirtualCamera vcam;

    void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject traj = GameObject.Find("Trajectory");
        trajectory = traj.GetComponent<Rock_Traj>();

        launcher = GameObject.FindWithTag("Launcher");
        launcher_rb = launcher.GetComponent<Rigidbody2D>();

        GetComponent<SpringJoint2D>().connectedBody = launcher_rb;
        GetComponent<Rock_Colliders>().enabled = false;

        vcam_go = GameObject.Find("CM vcam1");
        vcam = vcam_go.GetComponent<CinemachineVirtualCamera>();

        gameObject.transform.position = launcher.transform.position;
    }

    void Update()
    {
        if (isPressed)
        {
            rb.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    void OnMouseDown()
    {

        OnDrag();
        isPressed = true;
        rb.isKinematic = true;
        startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        trajectory.Show();
    }

    void OnDrag()
    {

        GameObject rock = gameObject;
        endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        distance = Vector2.Distance(startPoint, endPoint);
        direction = (startPoint - endPoint).normalized;
        force = direction * distance;

        //just for debug
        Debug.DrawLine(startPoint, endPoint);


        trajectory.UpdateDots(transform.position, force);
    }

    void OnMouseUp()
    {
        isPressed = false;
        rb.isKinematic = false;

        StartCoroutine(Release());
    }

    IEnumerator Release()
    {
        yield return new WaitForSeconds(releaseTime);

        GetComponent<SpringJoint2D>().enabled = false;
        this.enabled = false;

        yield return new WaitForFixedUpdate();

        tFollowTarget = gameObject.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;
        GetComponent<Rock_Colliders>().enabled = true;

        yield return new WaitForSeconds(2f);

        shotTaken = true;
    }
}