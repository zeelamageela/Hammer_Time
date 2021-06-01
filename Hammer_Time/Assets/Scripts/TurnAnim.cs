using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAnim : MonoBehaviour
{
    private Animator anim;
    public GameManager gm;
    public RockManager rm;

    public Camera uiCam;
    public Collider2D col;
    GameObject rock;
    Rock_Force rockForce;
    //bool isPressed = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }



    void Update()
    {
        if (gm.rockList.Count != 0)
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

    
}
