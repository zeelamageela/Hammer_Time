using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAnim : MonoBehaviour
{
    public Animator anim;
    public GameManager gm;
    public RockManager rm;

    public Camera uiCam;
    public Collider2D col;
    GameObject rock;
    Rock_Force rockForce;
    //bool isPressed = false;
    public bool turnAI;
    AudioManager am;

    void Start()
    {
        //anim = GetComponent<Animator>();

        am = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

    }

    private void OnEnable()
    {
        SetTurn(rm.inturn);
        //Debug.Log("rm.inturn");
    }

    void Update()
    {
        if (gm.rockList.Count != 0 && gm.rockCurrent < gm.rockList.Count)
        {
            rock = gm.rockList[gm.rockCurrent].rock;
            bool inturn = rm.inturn;

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = uiCam.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

                if (hit.collider == col)
                {
                    am.Play("Button");

                    if (inturn)
                    {
                        rm.inturn = false;
                        StartCoroutine(IsPressed(inturn));
                    }
                    else
                    {
                        rm.inturn = true;
                        StartCoroutine(IsPressed(inturn));
                    }

                    Debug.Log(hit.collider.gameObject.name);
                }
            }

            if (turnAI)
            {
                if (inturn)
                {
                    rm.inturn = false;
                    StartCoroutine(IsPressed(false));
                }
                else
                {
                    rm.inturn = true;
                    StartCoroutine(IsPressed(true));
                }
                turnAI = false;
            }
            //if (rockForce.flipAxis)
            //{
            //    //inturn = false;
            //    SetTurn(false);
            //}
            //else
            //{
            //    //inturn = true;
            //    SetTurn(true);
            //}
        }
    }

    IEnumerator IsPressed(bool inturn)
    {
        col.enabled = false;

        if (inturn)
        {
            anim.SetBool("inturn", false);
        }
        else
        {
            anim.SetBool("inturn", true);
        }

        yield return new WaitForSeconds(0.25f);
        col.enabled = true;
    }

    public void SetTurn(bool inturn)
    {
        if (inturn)
        {
            anim.SetBool("inturn", true);
        }
        else
        {
            anim.SetBool("inturn", false);
        }
    }
}
