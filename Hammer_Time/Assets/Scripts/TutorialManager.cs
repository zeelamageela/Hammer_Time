using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameManager gm;
    public GameState gameState;

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
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(Shot(testing));

            rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();
            rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();
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

        gm.SetHammerRed();

    }
    IEnumerator PlaceRocks()
    {
        yield return new WaitForSeconds(0.5f);

        gm.rockCurrent = 10;
        
    }

    IEnumerator Shot(string aiShotType)
    {
        yield return new WaitForSeconds(0.5f);

        rockFlick.isPressed = true;

        switch (aiShotType)
        {
            case "Centre Guard":
                rockRB.position = centreGuard;
                yield return new WaitForSeconds(1f);
                rockFlick.mouseUp = true;
                break;

            case "Corner Guard":
                rockRB.position = cornerGuard;
                yield return new WaitForSeconds(1f);
                rockFlick.mouseUp = true;
                break;

            case "TwelveFoot":
                rockRB.position = twelveFoot;
                yield return new WaitForSeconds(1f);
                rockFlick.mouseUp = true;
                break;

            case "FourFoot":
                rockRB.position = fourFoot;
                yield return new WaitForSeconds(1f);
                rockFlick.mouseUp = true;
                break;

            case "Take Out":
                rockRB.position = takeOut;
                yield return new WaitForSeconds(1f);
                rockFlick.mouseUp = true;
                break;

            default:
                break;
        }

    }
}
