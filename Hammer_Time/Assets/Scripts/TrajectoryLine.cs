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
    public GameObject circle;
    public Rock_Flick_Traj circleTraj;
    Vector2 startPoint;
    Vector2 endPoint;
    public Vector2 springForce;
    public float springDistance;
    public float yScaler;
    public float xScaler;



    void Start()
    {
        lr = GetComponent<LineRenderer>();
        //lr.material = new Material(Shader.Find("Sprites/Default"));
        //circleTraj = GameObject.Find("CircleTraj").GetComponent<Rock_Flick_Traj>();
    }

    void Update()
    {
        //rock = gm.rockList[gm.rockCurrent].rock;

        //List<Vector3> pos = new List<Vector3>(); 
        
        //velocity = circleTraj.velocity;
        //float t = 5f;

        startPoint = circle.transform.position;
        springForce = circle.GetComponent<SpringJoint2D>().GetReactionForce(Time.deltaTime);
        //springDistance = Vector2.Distance(startPoint, launcher.transform.position);

        //force = circleTraj.force;
        //lr.startWidth = 0.25f;
        //lr.endWidth = 0.25f;
        //pos.Add(new Vector3(launcher.transform.position.x, launcher.transform.position.y, 0f));
        //pos.Add(new Vector3(rock.transform.position.x, rock.transform.position.y, 0f));
        //Vector3 B = new Vector3(0f, 0f, 0f);

        //X Velocity -> vi * t - (vi * t * t)/2t
        //B.x = (velocity.x * t) - ((velocity.x * t) / 2);

        //Y Velocity -> (vi + vf) / 2 * t
        //B.y = ((velocity.y / 2f) * t) - 25f;
        //pos.Add(B);
        //B.y = (springDistance * yScaler) + launcher.transform.position.y;

        //pos.Add(B);
        //Debug.Log(B.y);
        //lr.SetPositions(pos.ToArray());
        Vector3 springForce3 = new Vector3(springForce.x, springForce.y, 0f);
        //DrawQuadraticBezierCurve(launcher.transform.position, springForce, circle.transform.position);
    }

    void DrawQuadraticBezierCurve(Vector3 launcher, Vector3 curlPoint, Vector3 circle)
    {
        lr.positionCount = 20;
        float t = 0f;
        Vector3 B = new Vector3(0, 0, 0);

        for (int i = 0; i < lr.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * -0.0026f * launcher - 0.035f * (1 - t) * t * pos + t * t * 0.33f * new Vector3(;
            lr.SetPosition(i, B);
            t += (1 / (float)lr.positionCount);
        }
    }
}