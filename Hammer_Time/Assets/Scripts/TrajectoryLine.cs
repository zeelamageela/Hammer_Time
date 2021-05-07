using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    private LineRenderer lr;
    public GameObject launcher;
    public Traj_Transform trajTransform;
    public GameManager gm;

    GameObject rock;
    public float springDistance;
    public Vector3 springDirection;
    public float angle;

    public GameObject curlPointGO;
    public Vector3 curlPoint;
    public GameObject targetPointGO;
    public Vector3 targetPoint;
    public GameObject hogLinePointGO;
    public Vector3 hogLinePoint;


    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    //void Update()
    //{
    //    //rock = gm.rockList[gm.rockCurrent].rock;

    //    //springDistance = trajTransform.springDistance;
    //    //springDirection = Vector3.Normalize(launcher.transform.position - circle.transform.position);
    //    //targetPoint = targetPointGO.transform.position;
    //    //curlPoint = curlPointGO.transform.position;

    //    //Vector3 circlePos = circle.transform.position;
    //    Vector3 curlPos = curlPointGO.transform.position;
    //    Vector3 hogLinePos = hogLinePointGO.transform.position;
    //    Vector3 targetPos = targetPointGO.transform.position;

    //    hogLinePoint = hogLinePos;
    //    curlPoint = curlPos;
    //    targetPoint = targetPos;

    //    //hogLinePointGO.transform.RotateAround(launcher.transform.position, new Vector3(0, 0, 1), -springDirection.x);
    //    //List<Vector3> pos = new List<Vector3>();

    //    //pos.Add(new Vector3(launcher.transform.position.x, launcher.transform.position.y, 0f));
    //    //pos.Add(new Vector3(rock.transform.position.x, rock.transform.position.y, 0f));

    //    //lr.SetPositions(pos.ToArray());

    //    DrawQuadraticBezierCurve(hogLinePoint, curlPoint, targetPoint);
    //}

    public void DrawTrajectory()
    {
        hogLinePoint = new Vector3(hogLinePointGO.transform.position.x, -15.75f, 0f);
        curlPoint = curlPointGO.transform.position;
        targetPoint = targetPointGO.transform.position;

        lr.positionCount = 20;
        float t = 0f;
        Vector3 B = new Vector3(0, -25, 0);

        lr.SetPosition(0, launcher.transform.position);

        for (int i = 1; i < lr.positionCount; i++)
        {
            B = ((1 - t) * (1 - t) * hogLinePoint) + (2 * (1 - t) * t * curlPoint) + (t * t * targetPoint);
            lr.SetPosition(i, B);
            t += (1 / (float)lr.positionCount);
        }

        //DrawQuadraticBezierCurve(hogLinePoint, curlPoint, targetPoint);
    }

    void DrawQuadraticBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        lr.positionCount = 200;
        float t = 0f;
        Vector3 B = new Vector3(0, -25, 0);

        lr.SetPosition(0, launcher.transform.position);

        for (int i = 1; i<lr.positionCount; i++)
        {
            B = ((1 - t) * (1 - t) * point0) + (2 * (1 - t) * t * point1) + (t * t * point2);
            lr.SetPosition(i, B);
            t += (1 / (float)lr.positionCount);
        }
    }
}