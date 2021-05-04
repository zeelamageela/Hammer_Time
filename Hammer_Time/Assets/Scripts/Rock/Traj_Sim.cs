using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Traj_Sim : MonoBehaviour
{
    public Vector2 velocity;
    public float releaseTime;
    Rigidbody2D rb;
    Rigidbody2D launcherRb;
    Collider2D launcherCol;
    GameObject launcher;
    GameObject circleTraj;
    Rock_Flick_Traj rockFlickTraj;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        launcher = GameObject.FindWithTag("Launcher");
        launcherCol = launcher.GetComponent<Collider2D>();
        launcherRb = launcher.GetComponent<Rigidbody2D>();

        GetComponent<SpringJoint2D>().connectedBody = launcherRb;
        GetComponent<SpringJoint2D>().distance = 0.005f;
        circleTraj = GameObject.Find("CircleTraj");
        rockFlickTraj = circleTraj.GetComponent<Rock_Flick_Traj>();
    }

    private void Start()
    {
        rb.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        rb.isKinematic = true;
        StartCoroutine(Release());
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == launcherCol)
        {
            Debug.Log("collider launcher");
            velocity = rb.velocity;
            Debug.Log("velocity is " + velocity.x + ", " + velocity.y);
            rockFlickTraj.velocity = velocity;
            Destroy(gameObject);
            //gameObject.SetActive(false);
        }
    }

    public IEnumerator Release()
    {
        //yield return new WaitForSeconds(0.15f);
        rb.isKinematic = false;
        launcherCol.enabled = true;
        Debug.Log("Release the traj");
        //yield return new WaitForSeconds(releaseTime);
        //GetComponent<SpringJoint2D>().enabled = false;
        //this.enabled = false;

        yield return new WaitUntil(() => launcherRb.position == rb.position);
        //Debug.Log("origin");
        //gameObject.SetActive(false);
    }
}
