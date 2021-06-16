using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public GameManager gm;
    public TutorialManager tm;
    public RockManager rm;

    Rock_Info rockInfo;
    Rock_Flick rockFlick;
    Rigidbody2D rockRB;

    public Vector2 centreGuard;
    public Vector2 cornerGuard;
    public Vector2 twelveFoot;
    public Vector2 fourFoot;
    public Vector2 takeOut;
    public Vector2 raise;

    public string testing;


    // OnEnable is called when the Game Object is enabled
    void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();
            rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();

            rm.inturn = false;
            //StartCoroutine(Shot(testing));
            StartCoroutine(Shot("Take Out"));
        }
    }

    public void OnShot(int rockCurrent)
    {
        rockInfo = gm.rockList[rockCurrent].rockInfo;
        rockFlick = gm.rockList[rockCurrent].rock.GetComponent<Rock_Flick>();
        rockRB = gm.rockList[rockCurrent].rock.GetComponent<Rigidbody2D>();

        WithoutHammer(rockCurrent);
    }

    public void WithHammer(int rockCurrent)
    {

    }

    public void WithoutHammer(int rockCurrent)
    {
        switch (rockCurrent)
        { 
            case 0:
                StartCoroutine(Shot("Centre Guard"));
                break;
            case 2:
                StartCoroutine(Shot("Four Foot"));
                break;
            case 4:
                rm.inturn = false;
                StartCoroutine(Shot("Take Out"));
                break;
            case 6:
                StartCoroutine(Shot("Corner Guard"));
                break;
            case 8:
                StartCoroutine(Shot("Take Out"));
                break;
            case 10:
                StartCoroutine(Shot("Four Foot"));
                break;
            case 12:
                StartCoroutine(Shot("Twelve Foot"));
                break;
            case 14:
                StartCoroutine(Shot("Take Out"));
                break;
            default:
                break;
        }
    }

    IEnumerator Shot(string aiShotType)
    {
        Debug.Log("AI Shot " + aiShotType);

        rockFlick.isPressedAI = true;

        yield return new WaitForSeconds(0.5f);


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
            case "Raise":
                rockRB.position = raise;
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            default:
                break;
        }

    }
}
