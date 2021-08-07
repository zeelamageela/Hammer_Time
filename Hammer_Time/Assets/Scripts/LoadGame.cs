using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGame : MonoBehaviour
{
    public GameManager gm;
    public RockBar rockBar;
    public RockManager rm;

    public bool rocksPlaced;


    void OnEnable()
    {
        StartCoroutine(PlaceRocks());
    }

    IEnumerator PlaceRocks()
    {
        //yield return new WaitForSeconds(3.5f);

        for (int i = 0; i < gm.rockCurrent; i++)
        {
            gm.rockList[i].rockInfo.placed = true;
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < gm.rockCurrent; i++)
        {
            gm.rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.18f;
            gm.rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
            gm.rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
            gm.rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
            gm.rockList[i].rock.transform.parent = null;
            rockBar.DeadRock(i);
            yield return new WaitForEndOfFrame();
            if (gm.rockList[i].rockInfo.inPlay)
            {
                //gm.rockList[i].rock.transform;
            }
            else
            {
                gm.rockList[i].rock.SetActive(false);
                gm.rockList[i].rockInfo.inPlay = true;
                gm.rockList[i].rockInfo.outOfPlay = true;
            }
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < gm.rockCurrent; i++)
        {
            if (gm.rockList[i].rockInfo.inPlay)
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
            }
            yield return new WaitForEndOfFrame();
        }

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
