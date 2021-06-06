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

    private void Update()
    {
        if (rockRB != null)
        {
            Vector3 followSpot = new Vector3((rockRB.position.x), (rockRB.position.y), 0f);
            transform.position = followSpot;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);


            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

                if (hit.collider == sweeperLCol)
                {
                    if (sweeperL.sweep)
                    {
                        Debug.Log("Hard!");
                        sm.SweepLeft();
                    }
                    else if (sweeperL.hard)
                    {
                        Debug.Log("Whoa!");
                        sm.SweepWhoa();
                    }
                    else if (sweeperL.whoa)
                    {
                        Debug.Log("Sweep!");
                        sm.SweepLeft();
                    }

                    Debug.Log(hit.collider.gameObject.name);
                }

                if (hit.collider == sweeperRCol)
                {
                    if (sweeperR.sweep)
                    {
                        Debug.Log("Hard!");
                        sm.SweepRight();
                    }
                    else if (sweeperR.hard)
                    {
                        Debug.Log("Whoa!");
                        sm.SweepWhoa();
                    }
                    else if (sweeperR.whoa)
                    {
                        Debug.Log("Sweep!");
                        sm.SweepRight();
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
