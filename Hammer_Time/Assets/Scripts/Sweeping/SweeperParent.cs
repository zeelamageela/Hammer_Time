using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperParent : MonoBehaviour
{
    public Sweeper[] sweeperLayers;

    public bool sweep;
    public bool hard;
    public bool whoa;

    public float xOffset;
    public float yOffset;

    Vector3 currentEulerAngles;
    Quaternion currentRotation;

    float x;
    float y;
    float z;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //sweeperParent = transform.parent;

        //Vector3 followSpot = new Vector3((xOffset + transform.parent.position.x), (transform.parent.position.y + yOffset), 0f);
        //transform.position = followSpot;

        float angle = 2 * transform.parent.rotation.z * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.AngleAxis(-angle, Vector3.forward);

        float absAngle = Mathf.Abs(angle);
        //Debug.Log("Parent Rotation is " + transform.parent.rotation.z);

        if (absAngle > 0f)
            transform.localPosition = new Vector3(0f, ((yOffset - 0.3f) * ((absAngle - 90f) / -90f)) + 0.3f, 0f);
        else if (absAngle < 0f)
            transform.localPosition = new Vector3(0f, ((yOffset - 0.3f) * ((absAngle - 90f) / -90f)) + 0.3f, 0f);
        else
            transform.localPosition = new Vector3(0f, yOffset, 0f);

    }

    public void Sweep()
    {
        sweep = true;
        hard = false;
        whoa = false;

        for (int i = 0; i < sweeperLayers.Length; i++)
        {
            sweeperLayers[i].Sweep();
        }
    }

    public void Hard()
    {
        sweep = false;
        hard = true;
        whoa = false;

        for (int i = 0; i < sweeperLayers.Length; i++)
        {
            sweeperLayers[i].Hard();
        }
    }

    public void Whoa()
    {
        sweep = false;
        hard = false;
        whoa = true;

        for (int i = 0; i < sweeperLayers.Length; i++)
        {
            sweeperLayers[i].Whoa();
        }
    }
}
