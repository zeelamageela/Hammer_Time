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
        // CRITICAL FIX: Add comprehensive null checks
        if (gm == null || gm.rockList == null || gm.rockList.Count == 0)
            return;
        
        if (gm.rockCurrent >= gm.rockList.Count)
            return;
        
        if (gm.rockList[gm.rockCurrent] == null || gm.rockList[gm.rockCurrent].rock == null)
            return;
        
        rock = gm.rockList[gm.rockCurrent].rock;
        
        if (rock == null)
            return;
        
        Rock_Flick rockFlick = rock.GetComponent<Rock_Flick>();
        if (rockFlick == null)
            return;

        springDistance = rockFlick.springDistance;
        springDirection = rockFlick.springDirection;

        angle = Mathf.Atan2(springDirection.y, springDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));

        weight = (weightScale * springDistance) / 4f;

        // CORRECT: Match ACTUAL simulator convention
        // TrajectorySimulator line 245: int dirMult = isInTurn ? -1 : 1;
        //   In-turn (true) ? dirMult=-1 ? force = -0.323 * -1 = +0.323 ? RIGHT curl
        //   Out-turn (false) ? dirMult=1 ? force = -0.323 * 1 = -0.323 ? LEFT curl
        // Visual convention: negative X scale = flipped
        if (rm.inturn)
        {
            transform.localScale = new Vector3(-1f, weight, 1f);  // In-turn ? RIGHT ? flip to show right
        }
        else
        {
            transform.localScale = new Vector3(1f, weight, 1f);   // Out-turn ? LEFT ? no flip
        }
        
        if (trajLine == null || trajLine.aimCircle == null || trajLine.shootKnob == null || trajLine.curlPointGO == null)
            return;
        
        SpriteRenderer aimCircleRenderer = trajLine.aimCircle.GetComponent<SpriteRenderer>();
        if (aimCircleRenderer == null)
            return;
        
        SpriteRenderer shootKnobRenderer = trajLine.shootKnob.GetComponent<SpriteRenderer>();
        if (shootKnobRenderer == null)
            return;

        if (aimCircleRenderer.enabled)
        {
            lr.enabled = true;
            lrY.enabled = true;

            point1.localPosition = new Vector3(trajLine.aimCircle.transform.position.x, 0f, 0f);
            point2.localPosition = new Vector3(trajLine.curlPointGO.transform.position.x, 0f, 0f);
            lr.startColor = shootKnobRenderer.color;
            lr.endColor = shootKnobRenderer.color;

            pointY1.localPosition = new Vector3(0f, trajLine.aimCircle.transform.position.y, 0f);
            pointY2.localPosition = new Vector3(0f, 0f, 0f);
            lrY.startColor = shootKnobRenderer.color;
            lrY.endColor = shootKnobRenderer.color;

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
