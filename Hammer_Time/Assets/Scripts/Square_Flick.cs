using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square_Flick : MonoBehaviour
{
    public Rigidbody2D rb;

    public float releaseTime = .15f;

    public bool isPressed = false;

    Vector2 startPoint;
    Vector2 endPoint;
    public Vector2 springDirection;
    public Vector2 force;
    public float springDistance;
    public float springForce;
    public bool springReleased;
    public bool isReleased;
    public bool extend;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    void Update()
    {
        if (isPressed)
        {
            rb.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            OnDrag();
        }

    }

    public void OnMouseDown()
    {
        isPressed = true;
        rb.isKinematic = true;
    }

    void OnDrag()
    {
        startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        endPoint = GetComponent<SpringJoint2D>().connectedBody.transform.position;

        springDistance = Vector2.Distance(startPoint, endPoint);
        Vector2 force = GetComponent<SpringJoint2D>().GetReactionForce(Time.deltaTime);

        springDirection = (Vector2)Vector3.Normalize(endPoint - startPoint);
        springForce = force.sqrMagnitude;
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
        yield return new WaitUntil(() => transform.position.y >= -25.6f);

        springReleased = true;

        yield return new WaitUntil(() => transform.position.y >= -24.15f);

        extend = true;
        Debug.Log(transform.position.y);

        yield return new WaitUntil(() => transform.position.y >= -16f);

        Debug.Log(transform.position.y);
        isReleased = true;

        //gameObject.SetActive(false);
    }
}
