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
    public Vector2 springDirection;
    public Vector3 curlPoint;
    public float yScaler;
    public float xScaler;
    public Vector3 targetPoint;


    void Start()
    {
        lr = GetComponent<LineRenderer>();
        //lr.material = new Material(Shader.Find("Sprites/Default"));
        //circleTraj = GameObject.Find("CircleTraj").GetComponent<Rock_Flick_Traj>();
    }

    void Update()
    {
        //rock = gm.rockList[gm.rockCurrent].rock;

        springDistance = Vector2.Distance(circle.transform.position, launcher.transform.position);
        springDirection = (Vector2)Vector3.Normalize(launcher.transform.position - circle.transform.position);
        Vector3 circlePos = circle.transform.position;
        targetPoint.x = targetPoint.x * xScaler * springDirection.x * springDistance;
        targetPoint.y = targetPoint.y * yScaler * springDirection.y * springDistance - 25f;
        //curlPoint.x = curlPoint.x * springDistance;
        //curlPoint.y = curlPoint.y * springDistance;

        //List<Vector3> pos = new List<Vector3>();


        //pos.Add(new Vector3(launcher.transform.position.x, launcher.transform.position.y, 0f));
        //pos.Add(new Vector3(rock.transform.position.x, rock.transform.position.y, 0f));

        //lr.SetPositions(pos.ToArray());
        DrawQuadraticBezierCurve(launcher.transform.position, curlPoint, targetPoint);
    }
    void DrawQuadraticBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        lr.positionCount = 200;
        float t = 0f;
        Vector3 B = new Vector3(0, -25, 0);
        for (int i = 0; i < lr.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * point0 + 2 * (1 - t) * t * point1 + t * t * point2;
            lr.SetPosition(i, B);
            t += (1 / (float)lr.positionCount);
        }
    }
}