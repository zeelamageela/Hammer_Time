using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterAnim : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    public GameObject square;
    Square_Flick squareFlick;
    public GameObject launcher;
    Rigidbody2D launchRB;
    public bool isPressed = false;
    public bool springReleased = false;
    public bool isReleased = false;
    public float pullback;
    public Transform shootRotation;
    int currentRock;
    GameObject rock;

    Vector2 startPoint;
    Vector2 endPoint;
    Vector2 force;
    public float springDistance;
    public Vector2 springDirection;
    float angle;
    public float throwSpeed;
    public bool extend;

    int slowdownTimer;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        squareFlick = square.GetComponent<Square_Flick>();
    }


    void Update()
    {
        isPressed = squareFlick.isPressed;
        springReleased = squareFlick.springReleased;
        isReleased = squareFlick.isReleased;
        extend = squareFlick.extend;
        force = squareFlick.force;

        if (isPressed)
        {
            springDistance = squareFlick.springDistance;
            pullback = springDistance / 1.2f;
            anim.Play("Shooter_BackSwing2", 0, pullback);

            if (pullback > 0.8f)
            {
                springDirection = square.GetComponent<Square_Flick>().springDirection;
                angle = Mathf.Atan2(springDirection.y, springDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
            }
        }

        if (isPressed == false && springReleased == true)
        {
            startPoint = square.transform.position;
            endPoint = launcher.transform.position;
            throwSpeed = Vector2.Distance(startPoint, endPoint);
            Debug.Log(throwSpeed);
            anim.Play("Shooter_Shoot", 0, throwSpeed);

            if (isReleased)
            {
                if (slowdownTimer >= 10)
                {
                    StartCoroutine(Slowdown());
                }
            }
        }

        if (extend)
        {
            rb.velocity = square.GetComponent<Rigidbody2D>().velocity;
            Debug.Log("extend");
        }
    }

    IEnumerator Slowdown()
    {
        slowdownTimer = 1;
        Debug.Log("slowdon " + slowdownTimer);
        rb.velocity = new Vector2(rb.velocity.x / slowdownTimer, rb.velocity.y / slowdownTimer);

        yield return new WaitForSeconds(0.1f);

        slowdownTimer++;
    }

}


