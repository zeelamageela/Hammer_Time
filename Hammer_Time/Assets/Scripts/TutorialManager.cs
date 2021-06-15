using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameManager gm;
    public RockBar rockBar;
    public RockManager rm;

    Rock_Info rockInfo;
    Rock_Flick rockFlick;
    Rigidbody2D rockRB;

    public Vector2 centreGuard;
    public Vector2 cornerGuard;
    public Vector2 twelveFoot;
    public Vector2 fourFoot;
    public Vector2 takeOut;

    public string testing;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetupTutorial());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(PlaceRocks());
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (rm.inturn)
            {
                rm.inturn = false;
            }
            else rm.inturn = true;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();
            rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();

            StartCoroutine(Shot(testing));

        }

        //if (gameState == GameState.YELLOWTURN)
        //{
        //    rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
        //    rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();
        //    rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();

        //    PlaceRocks();
        //    switch (gm.rockCurrent)
        //    {
        //        case 10:
        //            StartCoroutine(Shot("Centre Guard"));
        //            break;
        //        case 12:
        //            StartCoroutine(Shot("Twelve Foot"));
        //            break;
        //        case 14:
        //            StartCoroutine(Shot("Take Out"));
        //            break;
        //        default:
        //            break;
        //    }

        //}

    }

    IEnumerator SetupTutorial()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Set Hammer Tutorial");
        //gm.SetHammerRed();

    }
    IEnumerator PlaceRocks()
    {
        //yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < 4; i++)
        {
            gm.rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
            gm.rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
            gm.rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.18f;
            gm.rockList[i].rock.transform.parent = null;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();

        gm.rockList[0].rock.GetComponent<Rigidbody2D>().position = new Vector2(0.02f, 2.33f);
        gm.rockList[1].rock.GetComponent<Rigidbody2D>().position = new Vector2(-0.97f, 3.26f);
        gm.rockList[2].rock.GetComponent<Rigidbody2D>().position = new Vector2(0.0854f, 6.999f);
        gm.rockList[3].rock.GetComponent<Rigidbody2D>().position = new Vector2(-0.851f, 5.873f);

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < 4; i++)
        {
            gm.rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
            gm.rockList[i].rock.GetComponent<CircleCollider2D>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Release>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Force>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Colliders>().enabled = true;
            gm.rockList[i].rockInfo.moving = false;
            gm.rockList[i].rockInfo.shotTaken = true;
            gm.rockList[i].rockInfo.released = true;
            gm.rockList[i].rockInfo.stopped = true;
            gm.rockList[i].rockInfo.rest = true;
            rockBar.IdleRock(i);
            Debug.Log("i is equal to " + i);
            //rockBar.ShotUpdate(i, gm.rockList[i].rockInfo.outOfPlay);

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);
        gm.rockCurrent = 3;
        //gm.OnYellowTurn();

        yield return new WaitForEndOfFrame();
    }

    IEnumerator Shot(string aiShotType)
    {
        Debug.Log("AI Shot " + aiShotType);

        //yield return new WaitForSeconds(0.5f);

        rockFlick.isPressedAI = true;

        switch (aiShotType)
        {
            case "Centre Guard":
                rockRB.position = centreGuard;
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Corner Guard":
                rockRB.position = cornerGuard;
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Twelve Foot":
                rockRB.position = twelveFoot;
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Four Foot":
                rockRB.position = fourFoot;
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Take Out":
                rockRB.position = takeOut;
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            default:
                break;
        }

    }
}
