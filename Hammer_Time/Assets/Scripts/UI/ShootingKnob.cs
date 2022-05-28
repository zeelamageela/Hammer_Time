using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    float distColor;
    
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
            Vector2 colourStartPoint = aimCircle.transform.position;
            Vector2 colourEndPoint = target.transform.position;

            distColor = Vector2.Distance(colourStartPoint, colourEndPoint);

            distance = Vector2.Distance(startPoint, endPoint);

            lr.SetPosition(0, new Vector3(hogLinePoint.transform.position.x, hogLinePoint.transform.position.y, 0f));
            lr.SetPosition(1, new Vector3(endPoint.x, endPoint.y, 0f));

            lr.startColor = sr.color;
            lr.endColor = sr.color;

            lr.startWidth = Mathf.Lerp(0f, 0.3f, distance / 3.25f);
            lr.endWidth = Mathf.Lerp(0f, 0.1f, distance / 3.25f);


            if (distColor > 10f)
            {
                distColor = (distColor - 10f) / 7.5f;
                sr.color = Color.Lerp(red, redTrans, distColor);
                //spriteRenderer.color = new Color(1f, 0f, 0f, 1f);
            }
            else if (distColor > 5f)
            {
                distColor = (distColor - 5f) / 5f;
                sr.color = Color.Lerp(yellow, red, distColor);
                //spriteRenderer.color = new Color(0f, 1f, 0f, 1f);
            }
            else if (distColor > 1f)
            {
                distColor = (distColor - 1f) / 4f;
                sr.color = Color.Lerp(green, yellow, distColor);
                //spriteRenderer.color = new Color(1f, 0.92f, 0.016f, 1f);
            }
            else if (distColor <= 1f)
            {
                //distColor = (distColor - 1f) / 0.45f;
                sr.color = Color.Lerp(brightGreen, green, distColor);

                //spriteRenderer.color = new Color(1f, 0f, 0f, 1f);
            }
        }
        
    }

}
