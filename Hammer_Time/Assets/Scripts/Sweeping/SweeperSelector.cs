using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperSelector : MonoBehaviour
{
    public GameObject halL;
    public GameObject halR;

    public SweeperParent sweeperL;
    public SweeperParent sweeperR;

    public GameObject tSweepParent;

    public SweeperParent sweeperRedTee;
    public SweeperParent sweeperYellowTee;

    public Collider2D sweeperLCol;
    public Collider2D sweeperRCol;
    public Collider2D sweepZoneCol;

    public GameObject panel;
    public RockManager rm;
    public SweeperManager sm;
    public Sweep sweep;
    bool inturn;
    Rigidbody2D rockRB;
    Rigidbody2D rock2RB;
    public Transform launcher;
    bool aiTurn;

    public Vector2 moveDirection;

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

            
            moveDirection = rockRB.velocity;

            if (moveDirection != Vector2.zero)
            {
                if (Mathf.Abs(moveDirection.x) > 0.02f | Mathf.Abs(moveDirection.y) > 0.02f)
                {
                    float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis((angle - 90f), Vector3.forward);
                    //Debug.Log("Angle is " + angle);
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
                    sm.SweepTapLeft();

                    Debug.Log(hit.collider.gameObject.name);
                }

                if (hit.collider == sweeperRCol)
                {
                    sm.SweepTapRight();

                    Debug.Log(hit.collider.gameObject.name);
                }

                if (hit.collider.gameObject.layer == 3)
                {
                    ReAttachToRock(hit.collider.gameObject);
                    Debug.Log(hit.collider.gameObject.name);
                    Debug.Log(hit.collider.gameObject.layer);
                }

            }
        }
    }

    public void ReAttachToRock(GameObject rock)
    {
        //panel.SetActive(true);
        Rock_Info rockInfo = rock.GetComponent<Rock_Info>();
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();

        if (rockInfo.moving)
        {
            if (rockInfo.teamName == gsp.yellowTeamName)
            {
                if (rock.transform.position.y > 6.5f)
                {
                    sweeperRedTee.gameObject.SetActive(true);
                    rock2RB = rock.GetComponent<Rigidbody2D>();
                }
            }
            else if (rockInfo.teamName == gsp.redTeamName)
            {
                if (rock.transform.position.y > 6.5f)
                {
                    sweeperYellowTee.gameObject.SetActive(true);
                    rock2RB = rock.GetComponent<Rigidbody2D>();
                }
            }
            else
            {
                if (rock.transform.position.y > 6.5f)
                {
                    sweeperRedTee.gameObject.SetActive(true);
                    rock2RB = rock.GetComponent<Rigidbody2D>();
                }
            }
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
