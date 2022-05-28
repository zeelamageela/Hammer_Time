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

    public GameObject aimUI;
    public Transform point1;
    public Transform point2;
    public LineRenderer lr;

    public GameObject aimUIY;
    public Transform pointY1;
    public Transform pointY2;
    public LineRenderer lrY;

    void Update()
    {
        //if the rock list has rocks in it
        if (gm.rockList.Count != 0 && gm.rockCurrent < gm.rockList.Count)
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

            //if the shot is not an inturn, flip the trajectory
            if (rm.inturn)
            {
                transform.localScale = new Vector3(-1f, weight, 1f);
            }
            else
            {
                transform.localScale = new Vector3(1f, weight, 1f);
            }
            

            
        }

        if (trajLine.aimCircle.GetComponent<SpriteRenderer>().enabled)
        {
            lr.enabled = true;
            lrY.enabled = true;

            point1.localPosition = new Vector3(trajLine.aimCircle.transform.position.x, 0f, 0f);
            point2.localPosition = new Vector3(trajLine.curlPointGO.transform.position.x, 0f, 0f);
            lr.startColor = trajLine.shootKnob.GetComponent<SpriteRenderer>().color;
            lr.endColor = trajLine.shootKnob.GetComponent<SpriteRenderer>().color;

            pointY1.localPosition = new Vector3(0f, trajLine.aimCircle.transform.position.y, 0f);
            pointY2.localPosition = new Vector3(0f, 0f, 0f);
            lrY.startColor = trajLine.shootKnob.GetComponent<SpriteRenderer>().color;
            lrY.endColor = trajLine.shootKnob.GetComponent<SpriteRenderer>().color;

            Vector3[] aimX = new Vector3[2] { point1.transform.position, point2.transform.position };
            lr.SetPositions(aimX);
            Vector3[] aimY = new Vector3[2] { pointY1.transform.position, pointY2.transform.position };
            lrY.SetPositions(aimY);
        }
        else
        {
            lr.enabled = false;
            lrY.enabled = false;
        }
    }
}
