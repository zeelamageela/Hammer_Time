using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    private LineRenderer lr;
    private EdgeCollider2D edgeCol;
    public GameObject launcher;
    public Traj_Transform trajTransform;
    public GameManager gm;
    public CameraManager cm;

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

    public GameObject aimCircle;
    public int dotCount;
    public GameObject dot;

    public GameObject shootKnob;
    Color knobColour;
    List<GameObject> dots;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        edgeCol = GetComponent<EdgeCollider2D>();

        dots = new List<GameObject>();
        aimCircle.GetComponent<SpriteRenderer>().enabled = false;

        lr.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log(collider.name);

        if (collider == rock.GetComponent<Collider2D>())
        {
            Debug.Log(collider.name);
        }
        //else trajCollision = true;
    }

    private void Update()
    {
        if (gm.rockList.Count != 0 && gm.rockList.Count == 16)
        {
            rock = gm.rockList[gm.rockCurrent].rock;
        }

        knobColour = shootKnob.GetComponent<SpriteRenderer>().color;
    }

    public void DrawTrajectory()
    {
        lr.enabled = false;

        if (dots.Count != 0)
        {
            foreach (GameObject dot in dots)
            {
                Destroy(dot);
            }
            dots.Clear();
        }

        hogLinePoint = new Vector3(hogLinePointGO.transform.position.x, -15.75f, 0f);
        curlPoint = curlPointGO.transform.position;
        targetPoint = targetPointGO.transform.position;
        springDistance = trajTransform.springDistance;
        List<Vector3> pos = new List<Vector3>();

        if (springDistance < 1)
        {
            lr.positionCount = 2;
        }
        else if (springDistance >= 1f)
        {
            if (springDistance < 1.25)
            {
                lr.positionCount = Mathf.RoundToInt(Mathf.Lerp(3f, 5f, springDistance));
            }
            else if (springDistance < 1.5)
            {
                lr.positionCount = Mathf.RoundToInt(Mathf.Lerp(5f, 100f, springDistance));
            }
            else
            {
                lr.positionCount = 100;
            }
        }

        lr.startWidth = Mathf.Lerp(0f, 0.3f, springDistance / 3.25f);
        lr.endWidth = Mathf.Lerp(0f, 0.1f, springDistance / 3.25f);

        float t = 0f;
        Vector3 B = new Vector3(0, -25, 0);

        lr.SetPosition(0, launcher.transform.position);

        for (int i = 1; i < lr.positionCount; i++)
        {
            B = ((1 - t) * (1 - t) * hogLinePoint) + (2 * (1 - t) * t * curlPoint) + (t * t * targetPoint);
            lr.SetPosition(i, B);
            pos.Add(B);
            t += (1 / (float)lr.positionCount);
        }

        int counter = lr.positionCount / 20;

        for (int i = 1; i < dotCount; i++)
        {
            Vector2 dotPos = lr.GetPosition(i * counter);
            GameObject dotPlace = Instantiate(dot, dotPos, Quaternion.identity);
            dotPlace.transform.parent = transform;
            dotPlace.GetComponent<SpriteRenderer>().color = knobColour;
            dots.Add(dotPlace);
        }

        aimCircle.GetComponent<SpriteRenderer>().enabled = true;

        aimCircle.transform.position = lr.GetPosition(lr.positionCount - 1);
        aimCircle.GetComponent<SpriteRenderer>().color = knobColour;

        //lr.SetPositions(lr.GetPositions());
        //DrawQuadraticBezierCurve(hogLinePoint, curlPoint, targetPoint);
        //edgeCol.SetPoints(pos2D);
    }

    public void Release()
    {
        aimCircle.GetComponent<SpriteRenderer>().enabled = false;
        lr.enabled = false;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;

        if (dots.Count != 0)
        {
            foreach (GameObject dot in dots)
            {
                Destroy(dot);
            }
            dots.Clear();
        }
    }


    //void DrawQuadraticBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2)
    //{
    //    lr.positionCount = 200;
    //    float t = 0f;
    //    Vector3 B = new Vector3(0, -25, 0);

    //    lr.SetPosition(0, launcher.transform.position);

    //    for (int i = 1; i<lr.positionCount; i++)
    //    {
    //        B = ((1 - t) * (1 - t) * point0) + (2 * (1 - t) * t * point1) + (t * t * point2);
    //        lr.SetPosition(i, B);
    //        t += (1 / (float)lr.positionCount);
    //    }
    //}
}