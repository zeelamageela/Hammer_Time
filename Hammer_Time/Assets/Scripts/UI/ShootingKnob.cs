using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class ShootingKnob : MonoBehaviour
{
    public GameManager gm;
    SpriteRenderer sr;
    LineRenderer lr;
    public GameObject launcher;
    public GameObject hogLinePoint;
    public float distance;
    public TrajectoryLine trajLine;
    public GameObject aimCircle;
    public GameObject target;
    public GameObject mouseCircle;
    float distColor;

    public CameraManager cm;
    
    Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    public Color redTrans;
    public Color red;
    public Color yellow;
    public Color green;
    public Color brightGreen;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        sr = GetComponent<SpriteRenderer>();
        lr.startWidth = 0f;
        lr.endWidth = 0f;
        lr.enabled = false;
    }

    public void ParentToRock()
    {
        if (gm.rockList.Count != 0)
        {
            transform.parent = gm.rockList[gm.rockCurrent].rock.transform;
        }
        sr.enabled = true;
        lr.enabled = true;
    }

    public void UnParentandHide()
    {
        sr.enabled = false;
        lr.enabled = false;
        transform.parent = null;
        transform.position = new Vector3(0f, -25f, 0f);
    }


    private void Update()
    {
        if (sr.enabled)
        {
            Vector2 startPoint = launcher.transform.position;
            Vector2 endPoint = transform.position;

            // Get Y position of aim circle for color calculation
            float aimCircleY = aimCircle.transform.position.y;

            distance = Vector2.Distance(startPoint, endPoint);
            
            if (trajLine.aimCircle.GetComponent<SpriteRenderer>().enabled)
            {
                // Project line from endPoint through launcher to y = 8
                // Calculate the direction vector from endPoint to launcher
                Vector2 direction = (startPoint - endPoint).normalized;

                float targetY = -15.6f;
                float deltaY = targetY - endPoint.y;

                // Calculate corresponding X using the direction ratio
                float projectedX;
                if (Mathf.Abs(direction.y) > 0.001f)
                {
                    float t = deltaY / direction.y;
                    projectedX = endPoint.x + (direction.x * t);
                }
                else
                {
                    projectedX = endPoint.x;
                }

                lr.SetPosition(0, new Vector3(projectedX, targetY, 0f));
                lr.SetPosition(1, new Vector3(endPoint.x, endPoint.y, 0f));
            }
            else
            {
                // Project line from endPoint through launcher to y = 8
                // Calculate the direction vector from endPoint to launcher
                Vector2 direction = (startPoint - endPoint).normalized;
                
                float targetY = -18f;
                float deltaY = targetY - endPoint.y;
                
                // Calculate corresponding X using the direction ratio
                float projectedX;
                if (Mathf.Abs(direction.y) > 0.001f)
                {
                    float t = deltaY / direction.y;
                    projectedX = endPoint.x + (direction.x * t);
                }
                else
                {
                    projectedX = endPoint.x;
                }
                
                lr.SetPosition(0, new Vector3(projectedX, targetY, 0f));
                lr.SetPosition(1, new Vector3(endPoint.x, endPoint.y, 0f));
            }

            lr.startColor = sr.color;
            lr.endColor = sr.color;

            lr.startWidth = Mathf.Lerp(0f, 0.3f, distance / 3.25f);
            lr.endWidth = Mathf.Lerp(0f, 0.1f, distance / 3.25f);

            // Camera zoom based on Y position (optional - can keep or adjust)
            if (cm.aim.enabled)
            {
                float zoomDist = Mathf.Abs(aimCircleY);
                cm.aim.orthographicSize = Mathf.Lerp(1.75f, 2f, zoomDist / 8f);
            }

            // Color based on Y-position zones:
            // Y < 6.5 = Guard zone (bright green)
            // 6.5 <= Y < 8 = House zone (green)
            // 8 <= Y < 13 = Yellow zone (transition yellow to orange)
            // Y >= 13 = Red zone (too far)
            if (aimCircleY < 0f)
            {
                // Out of play - red transparent
                sr.color = redTrans;
            }
            if (aimCircleY < 4.5f)
            {
                // Guard zone - bright green
                sr.color = brightGreen;
            }
            else if (aimCircleY < 5f)
            {
                // Front of house zone - transition from bright green to green
                float frontHouseBlend = (aimCircleY - 4.5f) / 0.5f; // 0 to 1 over 0.5 units
                sr.color = Color.Lerp(brightGreen, green, frontHouseBlend);
            }
            else if (aimCircleY < 8f)
            {
                // House zone - green
                float houseBlend = (aimCircleY - 5f) / 3f; // 0 to 1 over 3 units
                sr.color = green;
            }
            else if (aimCircleY < 8.5f)
            {
                // Back of house zone - transition from green to yellow
                float backHouseBlend = (aimCircleY - 8f) / 0.5f; // 0 to 1 over 0.5 units
                sr.color = Color.Lerp(green, yellow, backHouseBlend);
            }
            else if (aimCircleY < 13f)
            {
                // Yellow zone - yellow
                float yellowBlend = (aimCircleY - 8.5f) / 4.5f; // 0 to 1 over 4.5 units
                sr.color = yellow;
            }
            else if (aimCircleY < 17f)
            {
                // Red zone - transition from yellow to red
                float yellowBlend = (aimCircleY - 13f) / 4.0f; // 0 to 1 over 4.0 units
                sr.color = Color.Lerp(yellow, red, yellowBlend);
            }
            else
            {
                // Red zone - too far (danger/out of bounds)
                sr.color = redTrans;
            }


            mouseCircle.GetComponent<SpriteRenderer>().enabled = true;
            mouseCircle.GetComponent<SpriteRenderer>().color = sr.color;
        }

    }

}
