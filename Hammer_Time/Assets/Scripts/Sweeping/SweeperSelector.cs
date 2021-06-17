using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperSelector : MonoBehaviour
{
    public GameObject halL;
    public GameObject halR;
    public Sweeper sweeperL;
    public Sweeper sweeperR;
    public Collider2D sweeperLCol;
    public Collider2D sweeperRCol;

    public RockManager rm;
    public SweeperManager sm;
    public Sweep sweep;
    bool inturn;
    Rigidbody2D rockRB;
    public Transform launcher;
    private void Update()
    {
        if (rockRB != null)
        {
            Vector3 followSpot = new Vector3((rockRB.position.x), (rockRB.position.y), 0f);
            transform.position = followSpot;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            Vector2 rockPos = new Vector2(rockRB.position.x, rockRB.position.y);
            Vector2 launchPos = new Vector2(launcher.position.x, launcher.position.y);
            Vector2 rockDirection = (Vector2)Vector3.Normalize(rockPos - launchPos);

            //float angle = Mathf.Atan2(rockDirection.x, rockDirection.y) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 45f));
            //if (angle <= 45f)
            //{
            //    sweeperParent.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            //}

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

                if (hit.collider == sweeperLCol)
                {
                    if (sweeperL.sweep)
                    {
                        Debug.Log("Hard!");
                        sm.SweepLeft(false);
                    }
                    else if (sweeperL.hard)
                    {
                        Debug.Log("Whoa!");
                        sm.SweepWhoa(false);
                    }
                    else if (sweeperL.whoa)
                    {
                        Debug.Log("Sweep!");
                        sm.SweepLeft(false);
                    }

                    Debug.Log(hit.collider.gameObject.name);
                }

                if (hit.collider == sweeperRCol)
                {
                    if (sweeperR.sweep)
                    {
                        Debug.Log("Hard!");
                        sm.SweepRight(false);
                    }
                    else if (sweeperR.hard)
                    {
                        Debug.Log("Whoa!");
                        sm.SweepWhoa(false);
                    }
                    else if (sweeperR.whoa)
                    {
                        Debug.Log("Sweep!");
                        sm.SweepRight(false);
                    }

                    Debug.Log(hit.collider.gameObject.name);
                }
            }
        }
    }


    public void AttachToRock(GameObject rock)
    {
        rockRB = rock.GetComponent<Rigidbody2D>();
        //rj.connectedBody = rockRB;
    }

    public void AssignSweepers()
    {

    }
}
