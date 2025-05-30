using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterAnim_Testing : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private RelativeJoint2D rj;
    public GameObject square;
    Square_Flick squareFlick;
    public GameObject launcher;
    Rigidbody2D launchRB;
    public GameManager gm;
    public GameObject gmGO;
    public GameObject hogLine;
    public bool isPressed = false;
    public bool springReleased = false;
    public bool isReleased = false;
    public float pullback;
    public Transform shootRotation;
    int currentRock;
    GameObject rock;

    public float releasePoint;
    public float backSwingPoint;
    public float throwDistance;

    Vector2 startPoint;
    Vector2 endPoint;
    Vector2 force;
    public float springDistance;
    public Vector2 springDirection;
    float angle;
    public float throwSpeed;
    public bool extend;

    int slowdownTimer = 0;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        squareFlick = square.GetComponent<Square_Flick>();

        rj = GetComponent<RelativeJoint2D>();
        rj.enabled = false;

        hogLine = GameObject.Find("Hog_Line");
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

            if(springDistance > 0.1f)
            {
                pullback = springDistance / 3f;
                pullback = Mathf.Clamp(pullback, 0f, 1f);
                anim.SetBool("mouseDown", false);
                anim.Play("Shooter_2_Backswing", 0, pullback);
            }
            

            if (pullback > 0.6f)
            {
                springDirection = square.GetComponent<Square_Flick>().springDirection;
                angle = Mathf.Atan2(springDirection.y, springDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
            }
        }

        if (isPressed == false && springReleased == true)
        {
            throwDistance = square.transform.position.y;
            endPoint = launcher.transform.position;
            throwDistance = Mathf.Clamp(throwDistance, backSwingPoint, releasePoint);

            throwSpeed = (throwDistance - backSwingPoint) / (releasePoint - backSwingPoint);
            throwSpeed = (1f - pullback) + throwSpeed;
            anim.Play("Shooter_2_Kick", 0, throwSpeed);

            if (extend)
            {
                float slidePos = square.transform.position.y;
                float slideSpeed = (slidePos - releasePoint) / (hogLine.transform.position.y - releasePoint);
                rj.enabled = true;
                anim.SetBool("extend", true);
                anim.Play("Shooter_2_Slide", 0, 0f);
            }
            
            if (isReleased)
            {
                extend = false;
                anim.SetBool("isReleased", true);
                anim.Play("Shooter_2_Release", 0, 0f);
                rj.enabled = false;
                if (slowdownTimer <= 10)
                {
                    StartCoroutine(Slowdown());

                }

            }
        }
    }

    IEnumerator Slowdown()
    {
        Debug.Log("slowdon " + slowdownTimer);
        rb.linearDamping = 0.1f * slowdownTimer;

        yield return new WaitForSeconds(0.1f * slowdownTimer);

        slowdownTimer++;
    }

}


