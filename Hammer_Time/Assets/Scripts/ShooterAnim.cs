using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterAnim : MonoBehaviour
{
    private Animator anim;
    public GameObject square;
    public bool isPressed = false;
    public float pullback;
    int currentRock;
    GameObject rock;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void OnMouseDown()
    {

    }
    // Update is called once per frame
    void Update()
    {
        
        
        anim.SetBool("mouseDown", isPressed);
        anim.SetFloat("Pullback", pullback);

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

        shooterForce = springDirection.y;
    }
}
