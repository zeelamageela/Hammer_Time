using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameManager gm;
    public RockBar rockBar;
    public RockManager rm;
    public AIManager aim;
    public TutorialHUD tHUD;

    public SweeperManager sm;


    public float tScale;
    public bool rocksPlaced;
    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(RunTutorial());
    }

    // Update is called once per frame
    void Update()
    {
        tScale = Time.timeScale;

        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(PlaceRocks());
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

    public void OnAIShot()
    {
        //aim.Shot();
    }

    IEnumerator RunTutorial()
    {
        //yield return new WaitForSeconds(0.5f);
        Debug.Log("Set Hammer Tutorial");
        //gm.SetHammerRed();

        yield return StartCoroutine(PlaceRocks());
        //OnAIShot();

        //rm.inturn = false;
        //StartCoroutine(Shot("Take Out"));

        yield return new WaitUntil(() => gm.rockList[13].rock.GetComponent<CircleCollider2D>().enabled == true);
        yield return new WaitForSeconds(0.25f);

        tHUD.OnRules();
        //tHUD.OnTurnTwo();
        yield return new WaitUntil(() => gm.rockList[13].rockInfo.released == true);
        tHUD.aimCircle1.SetActive(false);
        tHUD.OnSweep();
        //tHUD.OnSweepLine();

        yield return new WaitUntil(() => sm.sweeperL.sweep == true);

        tHUD.Resume();

        yield return new WaitUntil(() => gm.rockList[13].rock.transform.position.y >= 1f);

        tHUD.OnWhoa();

        yield return new WaitUntil(() => gm.rockList[13].rock.transform.position.y >= 2.75f);

        tHUD.Resume();

        yield return new WaitUntil(() => gm.rockList[13].rockInfo.rest == true);
        yield return new WaitUntil(() => gm.state == GameState.CHECKSCORE);
        tHUD.OnScoring();

        yield return new WaitUntil(() => gm.rockList[15].rock.GetComponent<CircleCollider2D>().enabled == true);

        yield return new WaitForSeconds(0.25f);

        tHUD.OnTurnTwo();

        yield return new WaitUntil(() => gm.rockList[15].rockInfo.released == true);
        tHUD.aimCircle1.SetActive(false);

        tHUD.OnSweepLine();

        yield return new WaitUntil(() => gm.rockList[15].rock.transform.position.y >= 1.5f);

        tHUD.OnWhoa();

        yield return new WaitUntil(() => gm.rockList[15].rock.transform.position.y >= 2.5f);

        tHUD.OnSweepCurl();

        yield return new WaitUntil(() => gm.rockList[15].rockInfo.rest == true);
        yield return new WaitUntil(() => gm.state == GameState.END);
        tHUD.OnFinalScoring();
        //tHUD.OnSweepCurl();

        //yield return new WaitUntil(() => sm.sweeperR.sweep == true);
    }

    IEnumerator PlaceRocks()
    {
        //yield return new WaitForSeconds(3.5f);

        for (int i = 0; i < 12; i++)
        {
            gm.rockList[i].rockInfo.placed = true;
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < 12; i++)
        {
            gm.rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.18f;
            gm.rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
            gm.rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
            gm.rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
            gm.rockList[i].rock.transform.parent = null;
            rockBar.DeadRock(i);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < 8; i++)
        {
            gm.rockList[i].rock.SetActive(false);
            gm.rockList[i].rockInfo.inPlay = true;
            gm.rockList[i].rockInfo.outOfPlay = true;
        }

        gm.rockList[8].rock.GetComponent<Rigidbody2D>().position = new Vector2(0.957f, 6.266f);
        gm.rockList[8].rockInfo.inHouse = true;
        rockBar.IdleRock(8);
        //rockBar.ShotUpdate(8, false);
        gm.rockList[9].rock.GetComponent<Rigidbody2D>().position = new Vector2(1.65f, 3.32f);
        //rockBar.IdleRock(9);
        //rockBar.ShotUpdate(9, false);
        gm.rockList[10].rock.GetComponent<Rigidbody2D>().position = new Vector2(-1.018f, 2.34f);
        //rockBar.IdleRock(10);
        //rockBar.ShotUpdate(10, false);

        yield return new WaitForEndOfFrame();

        for (int i = 8; i < 12; i++)
        {
            gm.rockList[i].rock.GetComponent<CircleCollider2D>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Release>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Force>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Colliders>().enabled = true;
            gm.rockList[i].rockInfo.inPlay = true;
            gm.rockList[i].rockInfo.outOfPlay = false;
            gm.rockList[i].rockInfo.moving = false;
            gm.rockList[i].rockInfo.shotTaken = true;
            gm.rockList[i].rockInfo.released = true;
            gm.rockList[i].rockInfo.stopped = true;
            gm.rockList[i].rockInfo.rest = true;
            Debug.Log("i is equal to " + i);

            //rockBar.ShotUpdate(i, gm.rockList[i].rockInfo.outOfPlay);

            yield return new WaitForEndOfFrame();
        }

        gm.rockList[11].rock.SetActive(false);
        //rockBar.DeadRock(11);
        //rockBar.ShotUpdate(11, true);
        //rockBar.ShotUpdate(11, gm.rockList[11].rockInfo.outOfPlay);
        rocksPlaced = true;

        yield return new WaitForEndOfFrame();

        //gm.rockCurrent = 3;
        //sm.SetupSweepers();
        //gm.NextTurn();

        //gm.OnRedTurn();
        //gm.OnYellowTurn();

        //yield return new WaitForEndOfFrame();
    }

    
}
