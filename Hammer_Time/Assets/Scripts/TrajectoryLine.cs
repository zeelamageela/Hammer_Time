using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    private LineRenderer lr;
    public GameObject launcher;
    public GameManager gm;

    GameObject rock;
    Vector2 velocity;
    Vector2 force;
    public Rock_Flick_Traj circleTraj;



    void Start()
    {
        lr = GetComponent<LineRenderer>();
        //lr.material = new Material(Shader.Find("Sprites/Default"));
        //circleTraj = GameObject.Find("CircleTraj").GetComponent<Rock_Flick_Traj>();
    }

    void Update()
    {
        //rock = gm.rockList[gm.rockCurrent].rock;

        List<Vector3> pos = new List<Vector3>(); 
        
        velocity = circleTraj.velocity;
        force = circleTraj.force;
        lr.startWidth = 0.25f;
        lr.endWidth = 0.25f;
        lr.useWorldSpace = true;
        lr.positionCount = 200;
        //float t = 20f;
        Vector3 B = new Vector3(0, -25, 0);

        pos.Add(new Vector3(launcher.transform.position.x, launcher.transform.position.y, 0f));
        B.y = (velocity.y * 10f) + (force.y * 10f * 10f) / (2f * 145f);
        pos.Add(new Vector3(0f, B.y, 0f));

        //pos.Add(new Vector3(rock.transform.position.x, rock.transform.position.y, 0f));

        //for (int i = 0; i < lr.positionCount; i++)
        //{
        //    //B = (1 - t) * (1 - t) * point0 + 2 * (1 - t) * t * point1 + t * t * point2;
        //    B.y = (velocity.y * t) + (force.y * t * t) / (2f * 145f);
        //    pos.Add(B);
        //    lr.SetPosition(i, B);
        //    t += (1 / (float)lr.positionCount);
        //}

        //position y = circleTraj velocity when it hits the launcher * time + 0.5*time squared / 2*145
        //DrawQuadraticBezierCurve();
    }


}