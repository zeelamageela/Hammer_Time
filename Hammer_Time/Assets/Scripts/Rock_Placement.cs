using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class Rock_Placement : MonoBehaviour
{
    public GameManager gm;
    public CameraManager cm;
    GameObject rock;
    Rigidbody2D rb;

    Rock_Flick rockFlick;
    Rock_Info rockInfo;
    Rock_Colliders rockCols;

    public CinemachineVirtualCamera vcam;
    Transform tFollowTarget;

    public string testing;

    public Vector2 centreGuard;
    public Vector2 tightCentreGuard;
    public Vector2 highCentreGuard;

    public Vector2 leftHighCornerGuard;
    public Vector2 leftTightCornerGuard;
    public Vector2 leftCornerGuard;
    public Vector2 rightHighCornerGuard;
    public Vector2 rightTightCornerGuard;
    public Vector2 rightCornerGuard;

    public Vector2 topTwelveFoot;
    public Vector2 backTwelveFoot;
    public Vector2 leftTwelveFoot;
    public Vector2 rightTwelveFoot;

    public Vector2 backFourFoot;
    public Vector2 topFourFoot;
    public Vector2 leftFourFoot;
    public Vector2 rightFourFoot;
    public Vector2 button;

    public Vector2 guardAccu;
    public Vector2 drawAccu;
    public Vector2 toAccu;
    public Vector2 tickAccu;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            StartCoroutine(OnPlaceRock(testing));
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            //Start Coroutine OnPlaceRock(position)
            StartCoroutine(OnPlaceRock("Four Foot"));
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(OnPlaceRock("Button"));
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(OnPlaceRock("Centre Guard"));
        }
    }

    public void OnPlace(string rockPlacement)
    {
        StartCoroutine(OnPlaceRock(rockPlacement));
    }

    IEnumerator OnPlaceRock(string rockPlacement)
    {
        Debug.Log("Placing a rock on " + rockPlacement);
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();
        rockCols = rock.GetComponent<Rock_Colliders>();

        rock.GetComponent<SpringJoint2D>().enabled = false;
        rockFlick.enabled = false;

        rock.GetComponent<CircleCollider2D>().radius = 0.14f;

        yield return new WaitForFixedUpdate();

        switch (rockPlacement)
        {
            case "Button":
                rock.transform.position = new Vector2(button.x, button.y);
                Debug.Log(rock.transform.position.x + ", " + rock.transform.position.y);
                break;

            case "Four Foot":
                rock.transform.position = button + (Random.insideUnitCircle * 1.5f);
                break;

            case "Back Four Foot":
                rock.transform.position = new Vector2(backFourFoot.x, backFourFoot.y);
                break;

            case "Top Four Foot":
                rock.transform.position = new Vector2(Random.Range(topFourFoot.x + drawAccu.x, topFourFoot.x - drawAccu.x), Random.Range(topFourFoot.y + drawAccu.y, topFourFoot.y - drawAccu.y));
                break;

            case "Left Four Foot":
                rock.transform.position = new Vector2(Random.Range(leftFourFoot.x + drawAccu.x, leftFourFoot.x - drawAccu.x), Random.Range(leftFourFoot.y + drawAccu.y, leftFourFoot.y - drawAccu.y));
                break;

            case "Right Four Foot":
                rock.transform.position = new Vector2(Random.Range(rightFourFoot.x + drawAccu.x, rightFourFoot.x - drawAccu.x), Random.Range(rightFourFoot.y + drawAccu.y, rightFourFoot.y - drawAccu.y));
                break;

            case "Back Twelve Foot":
                rock.transform.position = new Vector2(Random.Range(backTwelveFoot.x + drawAccu.x, backTwelveFoot.x - drawAccu.x), Random.Range(backTwelveFoot.y + drawAccu.y, backTwelveFoot.y - drawAccu.y));
                break;

            case "Top Twelve Foot":
                rock.transform.position = new Vector2(Random.Range(topTwelveFoot.x + drawAccu.x, topTwelveFoot.x - drawAccu.x), Random.Range(topTwelveFoot.y + drawAccu.y, rightTwelveFoot.y - drawAccu.y));
                break;

            case "Left Twelve Foot":
                rock.transform.position = new Vector2(Random.Range(leftTwelveFoot.x + drawAccu.x, leftTwelveFoot.x - drawAccu.x), Random.Range(leftTwelveFoot.y + drawAccu.y, leftTwelveFoot.y - drawAccu.y));
                break;

            case "Right Twelve Foot":
                rock.transform.position = new Vector2(Random.Range(rightTwelveFoot.x + drawAccu.x, rightTwelveFoot.x - drawAccu.x), Random.Range(rightTwelveFoot.y + drawAccu.y, rightTwelveFoot.y - drawAccu.y));
                break;

            case "Tight Centre Guard":
                rock.transform.position = new Vector2(Random.Range(tightCentreGuard.x + guardAccu.x, tightCentreGuard.x - guardAccu.x), Random.Range(tightCentreGuard.y + guardAccu.y, tightCentreGuard.y - guardAccu.y));
                break;

            case "Centre Guard":
                rock.transform.position = new Vector2(centreGuard.x, centreGuard.y); 
                break;

            case "High Centre Guard":
                rock.transform.position = new Vector2(highCentreGuard.x, highCentreGuard.y);
                break;

            case "Right Corner Guard":
                rock.transform.position = new Vector2(Random.Range(rightCornerGuard.x + guardAccu.x, rightCornerGuard.x - guardAccu.x), Random.Range(rightCornerGuard.y + guardAccu.y, rightCornerGuard.y - guardAccu.y));
                break;

            case "Right Tight Corner Guard":
                rock.transform.position = new Vector2(Random.Range(rightTightCornerGuard.x + guardAccu.x, rightTightCornerGuard.x - guardAccu.x), Random.Range(rightTightCornerGuard.y + guardAccu.y, rightTightCornerGuard.y - guardAccu.y));
                break;

            case "Right High Corner Guard":
                rock.transform.position = new Vector2(Random.Range(rightHighCornerGuard.x + guardAccu.x, rightHighCornerGuard.x - guardAccu.x), Random.Range(rightHighCornerGuard.y + guardAccu.y, rightHighCornerGuard.y - guardAccu.y));
                break;

            case "Left Corner Guard":
                rock.transform.position = new Vector2(Random.Range(leftCornerGuard.x + guardAccu.x, leftCornerGuard.x - guardAccu.x), Random.Range(leftCornerGuard.y + guardAccu.y, leftCornerGuard.y - guardAccu.y));
                break;

            case "Left Tight Corner Guard":
                rock.transform.position = new Vector2(Random.Range(leftTightCornerGuard.x + guardAccu.x, leftTightCornerGuard.x - guardAccu.x), Random.Range(leftTightCornerGuard.y + guardAccu.y, leftTightCornerGuard.y - guardAccu.y));
                break;

            case "Left High Corner Guard":
                rock.transform.position = new Vector2(Random.Range(leftHighCornerGuard.x + guardAccu.x, leftHighCornerGuard.x - guardAccu.x), Random.Range(leftHighCornerGuard.y + guardAccu.y, leftHighCornerGuard.y - guardAccu.y));
                break;

            case "Out of Bounds":
                rock.transform.position = new Vector2(0f, 9f);
                break;
        }

        yield return new WaitForFixedUpdate();

        //tFollowTarget = rock.transform;
        //vcam.LookAt = tFollowTarget;
        //vcam.Follow = tFollowTarget;
        //vcam.enabled = true;

        cm.RockFollow(rock.transform);

        rockFlick.mouseUp = true;


        rock.GetComponent<SpriteRenderer>().enabled = true;
        rockCols.shotTaken = true;
        //rockInfo.placed = true;
        rockInfo.released = true;
        rockInfo.inPlay = true;
        rockInfo.outOfPlay = false;
        rockFlick.mouseUp = true;

        rockInfo.stopped = true;
        rockInfo.rest = true;
        //rockInfo.inHouse = true;

    }
}
