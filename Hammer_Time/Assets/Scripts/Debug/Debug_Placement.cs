using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Debug_Placement : MonoBehaviour
{
    public GameManager gm;
    GameObject rock;
    Rigidbody2D rb;

    Rock_Flick rockFlick;
    Rock_Info rockInfo;
    Rock_Colliders rockCols;

    public Vector2 buttonPosition;

    public CinemachineVirtualCamera vcam;
    Transform tFollowTarget;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            //Start Coroutine OnPlaceRock(position)
            StartCoroutine(OnPlaceRock("Button"));
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(OnPlaceRock("Top Four Foot"));
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(OnPlaceRock("Back Four Foot"));
        }
    }

    IEnumerator OnPlaceRock(string rockPlacement)
    {
        Debug.Log("Placing a rock at " + rockPlacement);
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();
        rockCols = rock.GetComponent<Rock_Colliders>();

        rock.GetComponent<SpringJoint2D>().enabled = false;
        rockFlick.enabled = false;

        yield return new WaitForFixedUpdate();

        switch (rockPlacement)
        {
            case "Button":
                //position = 0
                rock.transform.position = buttonPosition;

                break;
            case "Back Four Foot":
                //position = 1
                Vector2 rockPos = buttonPosition + (Random.insideUnitCircle * 1.5f);
                rock.transform.position = rockPos;

                break;
            case "Top Four Foot":
                //position = 2
                rock.transform.position = new Vector2(Random.Range(-1.4f, 1.4f), Random.Range(0.05f, 5f));
                break;
        }

        yield return new WaitForFixedUpdate();

        tFollowTarget = rock.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;


        rockCols.shotTaken = true;
        rockInfo.released = true;
        rockInfo.inPlay = true;
        rockInfo.stopped = true;
        rockInfo.rest = true;
        rockInfo.inHouse = true;
        rockInfo.outOfPlay = false;

    }
}
