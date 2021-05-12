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

    Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        sr = GetComponent<SpriteRenderer>();
        lr.startWidth = 0.175f;
        lr.endWidth = 0.175f;
    }

    public void ParentToRock(GameObject rock)
    {
        if (rock != null)
        {
            transform.parent = rock.transform;
        }
        
        sr.enabled = true;

    }

    public void UnParentandHide()
    {
        transform.parent = null;
        sr.enabled = false;
        transform.position = new Vector3(0f, -25f, 0f);
    }

    private void Update()
    {
        //ColourChange();
        Vector2 startPoint = launcher.transform.position;
        Vector2 endPoint = transform.position;

        distance = Vector2.Distance(startPoint, endPoint);

        lr.SetPosition(0, new Vector3(hogLinePoint.transform.position.x, hogLinePoint.transform.position.y, 0f));
        lr.SetPosition(1, new Vector3(endPoint.x, endPoint.y, 0f));

        if (sr.enabled)
        {
            lr.startColor = sr.color;
            lr.endColor = sr.color;

            lr.startWidth = Mathf.Lerp(0f, 0.3f, distance / 3.25f);
            lr.endWidth = Mathf.Lerp(0f, 0.1f, distance / 3.25f);

            if (distance > 2.1f)
            {
                float distColor = (distance - 2.1f) / 0.75f;
                sr.color = Color.Lerp(Color.yellow, Color.red, distColor);
                //spriteRenderer.color = new Color(1f, 0f, 0f, 1f);
            }
            else if (distance > 1.9f)
            {
                if (distance < 2.1f)
                {
                    float distColor = (distance - 1.9f) / 0.25f;
                    sr.color = Color.Lerp(Color.green, Color.yellow, distColor);
                    //spriteRenderer.color = new Color(0f, 1f, 0f, 1f);
                }
            }
            else if (distance > 1.6f)
            {
                if (distance < 1.9f)
                {
                    float distColor = (distance - 1.6f) / 0.25f;
                    sr.color = Color.Lerp(Color.yellow, Color.green, distColor);
                    //spriteRenderer.color = new Color(1f, 0.92f, 0.016f, 1f);
                }
            }
            else if (distance <= 1.6f)
            {
                float distColor = (distance - 0.5f);
                sr.color = Color.Lerp(Color.red, Color.yellow, distColor);
                //spriteRenderer.color = new Color(1f, 0f, 0f, 1f);
            }
        }
        
    }

}
