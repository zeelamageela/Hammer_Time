using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrajectory : MonoBehaviour
{
    public Transform aimCircle;
    public float distance;
    public SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector2.Distance(aimCircle.position, transform.position);
        float distColor = (distance - 0.5f) / 0.5f;

        if (distColor <= 0.5f)
        {
            sr.color = Color.Lerp(Color.green, Color.red, distColor);
        }
    }
}
