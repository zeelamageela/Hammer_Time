using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperParent : MonoBehaviour
{
    public Sweeper[] sweeperLayers;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        sweeperParent = transform.parent;

        Vector3 followSpot = new Vector3((xOffset + sweeperParent.position.x), (sweeperParent.position.y + yOffset), 0f);
        transform.position = followSpot;
    }
}
