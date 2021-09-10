using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockManager : MonoBehaviour
{
    public GameManager gm;
    public CameraManager cm;
    public RandomRockPlacerment rrp;
    public RockBar rb;
    public GameObject rock;
    public Rock_Info rockInfo;
    public ShootingKnob shootKnob;
    public Transform house;
    public bool inturn;
    bool isPressed;


    void OnEnable()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.rockList.Count != 0 && gm.rockList.Count == 16)
        {
            rock = gm.rockList[gm.rockCurrent].rock;
            rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            if (inturn)
            {
                rock.GetComponent<Rock_Force>().flipAxis = true;
            }
            else
            {
                rock.GetComponent<Rock_Force>().flipAxis = false;
            }

            //if (rock.transform.position.y >= -4f)
            //{
            //    cm.InPlayZoom(rock.transform.position.y);
            //    cm.top.depth = -1;
            //}

            if (rockInfo.inPlay)
            {
                //Debug.Log("in House");
                float dist = Vector2.Distance(rock.transform.position, house.position);
                cm.top.depth = -1;
                cm.InPlayZoom(dist);
            } 
        }
    }
}
