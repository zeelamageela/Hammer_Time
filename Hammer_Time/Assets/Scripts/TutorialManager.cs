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
        StartCoroutine(SetupTutorial());
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

    IEnumerator SetupTutorial()
    {
        //yield return new WaitForSeconds(0.5f);
        Debug.Log("Set Hammer Tutorial");
        //gm.SetHammerRed();

        yield return StartCoroutine(PlaceRocks());
        //OnAIShot();

        //rm.inturn = false;
        //StartCoroutine(Shot("Take Out"));

        yield return new WaitUntil(() => gm.rockList[5].rock.GetComponent<CircleCollider2D>().enabled == true);
        yield return new WaitForSeconds(2.5f);
        tHUD.OnRules();

        yield return new WaitUntil(() => gm.rockList[5].rockInfo.released == true);

        tHUD.OnSweep();

        yield return new WaitUntil(() => sm.sweeperL.sweep == true);

        tHUD.Resume();

        yield return new WaitUntil(() => gm.rockList[5].rock.transform.position.y >= 1f);

        tHUD.OnWhoa();

        yield return new WaitUntil(() => sm.sweeperL.whoa == true);

        tHUD.Resume();

        yield return new WaitUntil(() => gm.rockList[5].rock.transform.position.y >= 2.75f);

        tHUD.OnSweepCurl();

        yield return new WaitUntil(() => sm.sweeperR.sweep == true);

        tHUD.Resume();

        yield return new WaitUntil(() => gm.rockList[5].rockInfo.rest == true);

        tHUD.OnScoring();
    }

    IEnumerator PlaceRocks()
    {
        //yield return new WaitForSeconds(3.5f);

        for (int i = 0; i < 4; i++)
        {
            gm.rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.18f;
            gm.rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
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
