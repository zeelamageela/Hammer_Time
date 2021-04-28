using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square_Flick : MonoBehaviour
{
    public Rigidbody2D rb;

    public float releaseTime = .15f;

    public bool isPressed = false;

    public Collider2D hogLine;

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

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == hogLine)
        {
            Debug.Log(collider.gameObject.name);
            isReleased = true;
        }
    }

    IEnumerator Release()
    {
        yield return new WaitForSeconds(releaseTime);

        GetComponent<SpringJoint2D>().enabled = false;
        springReleased = true;


        yield return new WaitUntil(() => transform.position.y >= -24f);

        extend = true;
    }
}
