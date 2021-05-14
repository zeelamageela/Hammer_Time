using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAnim : MonoBehaviour
{
    private Animator anim;
    public GameManager gm;

    GameObject rock;
    Rock_Force rockForce;
    //bool inturn = true;


    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (gm.rockList.Count != 0)
        {
            rock = gm.rockList[gm.rockCurrent].rock;
            rockForce = rock.GetComponent<Rock_Force>();

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

    public void SetTurn(bool inturn)
    {
        if (gm.rockList.Count != 0)
        {
            if (inturn)
            {
                rockForce.flipAxis = false;
                anim.SetBool("inturn", true);
            }
            else
            {
                rockForce.flipAxis = true;
                anim.SetBool("inturn", false);
            }
        }

    }
    
}
