using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterAnim : MonoBehaviour
{
    private Animator anim;
    public GameObject gm;
    public GameObject launcher;
    GameManager gameManager;
    public bool isPressed = false;
    public float pullback;
    int currentRock;
    GameObject rock;

    Vector2 startPoint;
    Vector2 endPoint;
    Vector2 force;
    public float springDistance;
    public Vector2 springDirection;
    float springForce;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        gameManager = gm.GetComponent<GameManager>();
    }

    void OnMouseDown()
    {
        isPressed = true;
    }

    private void OnMouseUp()
    {
        isPressed = false;
    }


    void Update()
    {
        currentRock = gameManager.rockCurrent;
        if (isPressed)
        {
            OnDrag();
        }
        anim.SetBool("mouseDown", isPressed);
        anim.SetFloat("Pullback", pullback);

    }

    void OnDrag()
    {

        GameObject rock = gameManager.rockList[currentRock].rock;
        startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        endPoint = launcher.transform.position;
        springDistance = Vector2.Distance(startPoint, endPoint);
        force = rock.GetComponent<SpringJoint2D>().GetReactionForce(Time.deltaTime);
        springDirection = (Vector2)Vector3.Normalize(endPoint - startPoint);
        springForce = force.magnitude;
    }
}
