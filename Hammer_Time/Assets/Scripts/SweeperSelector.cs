using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperSelector : MonoBehaviour
{
    public GameObject halL;
    public GameObject halR;
    public Sweeper sweeperL;
    public Sweeper sweeperR;

    public RockManager rm;
    bool inturn;
    Rigidbody2D rockRB;

    private void Update()
    {
        if (rockRB != null)
        {
            Vector3 followSpot = new Vector3((rockRB.position.x), (rockRB.position.y), 0f);
            transform.position = followSpot;
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
