using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperSelector : MonoBehaviour
{
    public GameObject halL;
    public GameObject halR;
    public SweeperParent sweeperL;
    public SweeperParent sweeperR;
    public Collider2D sweeperLCol;
    public Collider2D sweeperRCol;
    public Collider2D sweepZoneCol;

    public GameObject panel;
    public RockManager rm;
    public SweeperManager sm;
    public Sweep sweep;
    bool inturn;
    Rigidbody2D rockRB;
    public Transform launcher;

    bool aiTurn;
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

            
            Vector2 moveDirection = rockRB.velocity;

            if (moveDirection != Vector2.zero)
            {
                if (Mathf.Abs(moveDirection.x) > 0.02f | Mathf.Abs(moveDirection.y) > 0.02f)
                {
                    float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis((angle - 90f), Vector3.forward);

                    if (transform.rotation.z > 30f)
                    {
                        sweeperL.yOffset = 1.2f;
                        sweeperR.yOffset = 0.6f;
                    }
                    else if (transform.rotation.z < -30f)
                    {
                        sweeperL.yOffset = 0.6f;
                        sweeperR.yOffset = 1.2f;
                    }
                }

                
            }

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit.collider == sweepZoneCol)
                {
                    //sm.isSweeping = true;
                    sm.SweepTap();
                        
                    Debug.Log(hit.collider.gameObject.name);
                }
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
                    else
                    {
                        Debug.Log("Default Sweep!");
                        sm.SweepRight(false);
                    }

                    Debug.Log(hit.collider.gameObject.name);
                }
            }
        }
    }

    public void PostHitSelect()
    {
        sweeperL.gameObject.SetActive(false);
        sweeperR.gameObject.SetActive(false);
        panel.SetActive(true);

        GameManager gm = FindObjectOfType<GameManager>();
        for (int i = 0; i < gm.rockList.Count; i++)
        {
            if (gm.rockList[i].rockInfo.moving)
            {
                Debug.Log(gm.rockList[i].rockInfo.name + " is moving sweepSel");
            }
            else
                gm.rockList[i].rock.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.3f);
        }
    }

    public void AttachToRock(GameObject rock)
    {
        rockRB = rock.GetComponent<Rigidbody2D>();
        //rj.connectedBody = rockRB;
    }

    public void SetColliders()
    {
        sweeperL = sm.sweeperL;
        sweeperLCol = sweeperL.GetComponent<BoxCollider2D>();
        sweeperR = sm.sweeperR;
        sweeperRCol = sweeperR.GetComponent<BoxCollider2D>();
    }
}
