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

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 rockPos = Camera.main.WorldToScreenPoint(sweeperParent.position);
        sweep.transform.position = new Vector2 (rockPos.x, rockPos.y - 200f);
        hard.transform.position = new Vector2(rockPos.x, rockPos.y - 200f);
        whoa.transform.position = new Vector2(rockPos.x, rockPos.y - 400f);

        Vector2 launchPos = new Vector2(launcher.position.x, launcher.position.y);
        Vector2 rockDirection = (Vector2)Vector3.Normalize(rockPos - launchPos);
        float angle = Mathf.Atan2(rockDirection.y, rockDirection.x) * Mathf.Rad2Deg;
        sweeperParent.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 45f));
    }
}
