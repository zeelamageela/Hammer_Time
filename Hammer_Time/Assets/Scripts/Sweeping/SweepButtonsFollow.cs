using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SweepButtonsFollow : MonoBehaviour
{
    public Transform sweeperParent;
    public Transform launcher;
    public Button sweep;
    public Button hard;
    public Button whoa;

    public float angle;
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 rockPos = Camera.main.WorldToScreenPoint(sweeperParent.position);
        sweep.transform.position = new Vector2 (rockPos.x, rockPos.y - 200f);
        hard.transform.position = new Vector2(rockPos.x, rockPos.y - 200f);
        whoa.transform.position = new Vector2(rockPos.x, rockPos.y - 400f);

        
    }
}
