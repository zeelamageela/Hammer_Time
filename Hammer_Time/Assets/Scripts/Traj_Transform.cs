using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Traj_Transform : MonoBehaviour
{
    public GameManager gm;
    public RockManager rm;
    GameObject rock;
    public GameObject launcher;
    public TrajectoryLine trajLine;
    public float springDistance;
    public Vector2 springDirection;
    public float weightScale;
    public float weight;
    float angle;
    //bool flipAxis = false;

    
    void Update()
    {
        //if the rock list has rocks in it
        if (gm.rockList.Count != 0)
        {
            //fetch the current rock from gm
            rock = gm.rockList[gm.rockCurrent].rock;

            //compute the distance and direction
            springDistance = rock.GetComponent<Rock_Flick>().springDistance;
            springDirection = rock.GetComponent<Rock_Flick>().springDirection;

            //convert the direction to an angle and rotate the trajectory
            angle = Mathf.Atan2(springDirection.y, springDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));

            //scale the line based on the weight of the shot
            weight = (weightScale * springDistance) / 4f;
            //weight = (weightScale);

            if (!rm.inturn)
            {
                transform.localScale = new Vector3(-1f, weight, 1f);
            }
            else
            {
                transform.localScale = new Vector3(1f, weight, 1f);
            }
            

            
        }


    }
}
