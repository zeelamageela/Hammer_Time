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
