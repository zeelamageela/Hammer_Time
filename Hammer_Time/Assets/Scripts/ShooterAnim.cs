using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterAnim : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private RelativeJoint2D rj;

    public GameManager gm;
    public GameObject gmGO;

    public bool isPressed = false;
    public bool springReleased = false;
    public bool isReleased = false;
    public float pullback;
    GameObject rock;
    Rock_Flick rockFlick;
    Rock_Info rockInfo;

    public float releasePoint;
    public float backSwingPoint;
    public float throwDistance;

    Vector2 startPoint;
    Vector2 endPoint;
    public float springDistance;
    public Vector2 springDirection;
    float angle;
    public float throwSpeed;
    public bool extend;

    int slowdownTimer;


    // Start is called before the first frame update
    void Start()
    {
        slowdownTimer = 0;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rj = GetComponent<RelativeJoint2D>();

        gmGO = GameObject.Find("GameManager");
        gm = gmGO.GetComponent<GameManager>();

        rock = gm.rockList[gm.rockCurrent].rock;
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();

        rj.connectedBody = rock.GetComponent<Rigidbody2D>();
        rj.enabled = false;
    }


    void Update()
    {

        isPressed = rockFlick.isPressed;
        springReleased = rockFlick.springReleased;
        isReleased = rockInfo.released;
        springDistance = rockFlick.springDistance;
        springDirection = rockFlick.springDirection;

        if (isPressed)
        {

            if (springDistance > 0.1f && rock.transform.position.y < -25f)
            {
                pullback = springDistance / 3f;
                pullback = Mathf.Clamp(pullback, 0f, 1f);
                anim.SetBool("mouseDown", false);
                anim.Play("Shooter_2_Backswing", 0, pullback);
            }


            if (pullback > 0.6f)
            {
                angle = Mathf.Atan2(springDirection.y, springDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
            }
        }

        if (isPressed == false && springReleased == true)
        {
            throwDistance = rock.transform.position.y;
            throwDistance = Mathf.Clamp(throwDistance, backSwingPoint, releasePoint);

            throwSpeed = (throwDistance - backSwingPoint) / (releasePoint - backSwingPoint);
            throwSpeed = (1f - pullback) + throwSpeed;
            anim.Play("Shooter_2_Kick", 0, throwSpeed);

            if (rock.transform.position.y >= releasePoint)
            {
                float slidePos = rock.transform.position.y;
                float slideSpeed = (slidePos - releasePoint) / (-16.03f - releasePoint);
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
        rb.drag = 0.25f * slowdownTimer;

        yield return new WaitForFixedUpdate();

        slowdownTimer++;
    }

}


