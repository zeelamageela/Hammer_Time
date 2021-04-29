using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_Random : MonoBehaviour
{
    public GameManager gm;
    GameObject rock;

    public int rockTotal;
    public int rocksPerTeam;
    public bool redHammer;
    // Start is called before the first frame update
    public void DebugStart()
    {
        gm.redButton.gameObject.SetActive(false);
        gm.yellowButton.gameObject.SetActive(false);

        rockTotal = 8;
        gm.rockTotal = rockTotal;
        gm.rockCurrent = rockTotal - 1;
        rocksPerTeam = 1;
        gm.rocksPerTeam = rocksPerTeam;
        redHammer = true;
        gm.redHammer = redHammer;

        gm.mainDisplay.enabled = false;
        gm.state = GameState.DEBUG;

        StartCoroutine(DebugMode());
    }

    IEnumerator DebugMode()
    {
        for (int i = 1; i < gm.rockCurrent; i++)
        {
            Vector2 rockPlace = new Vector2(Random.Range(-2f, 2f), Random.Range(0f, 8f));

            if (i % 2 == 1)
            {
                GameObject yellowRock_go = Instantiate(gm.yellowShooter, gm.yellowRocksInactive);

                float yRocks = gm.rocksPerTeam * 0.5f;
                int k = (i / 2) + 1;
                yellowRock_go.transform.position = new Vector2(rockPlace.x, rockPlace.y);

                Rock_Info yellowRock_info = yellowRock_go.GetComponent<Rock_Info>();
                yellowRock_info.rockNumber = k;
                yellowRock_go.name = yellowRock_info.teamName + " " + yellowRock_info.rockNumber;
                yellowRock_go.GetComponent<SpringJoint2D>().enabled = false;
                yellowRock_go.GetComponent<Rock_Colliders>().enabled = true;

                yellowRock_info.moving = false;
                yellowRock_info.stopped = true;
                yellowRock_info.rest = true;
                gm.rockList.Add(new Rock_List(yellowRock_go, yellowRock_info));
                yield return new WaitForSeconds(0.1f);
            }

            if (i % 2 == 0)
            {
                GameObject redRock_go = Instantiate(gm.redShooter, gm.redRocksInactive);

                float yRocks = gm.rocksPerTeam * 0.5f;
                int k = (i / 2);

                Rock_Info redRock_info = redRock_go.GetComponent<Rock_Info>();
                redRock_info.rockNumber = k;
                redRock_go.name = redRock_info.teamName + " " + redRock_info.rockNumber;
                redRock_go.GetComponent<SpringJoint2D>().enabled = false;
                redRock_go.GetComponent<Rock_Colliders>().enabled = true;

                redRock_go.transform.position = new Vector2(rockPlace.x, rockPlace.y);

                redRock_info.moving = false;
                redRock_info.stopped = true;
                redRock_info.rest = true;

                gm.rockList.Add(new Rock_List(redRock_go, redRock_info));
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield return new WaitForFixedUpdate();

        gm.rockList.Sort();

        yield return new WaitForFixedUpdate();

        StartCoroutine(gm.CheckScore());

        yield return new WaitForFixedUpdate();

        StartCoroutine(SetupRocksDebug());

        yield return new WaitForFixedUpdate();

        gm.rockCurrent--;

        yield return new WaitForFixedUpdate();

        gm.OnYellowTurn();

    }

    IEnumerator SetupRocksDebug()
    {
        for (int i = gm.rockCurrent; i <= gm.rockTotal; i++)
        {
            if (i % 2 == 1)
            {
                GameObject yellowRock_go = Instantiate(gm.yellowShooter, gm.yellowRocksInactive);

                float yRocks = gm.rocksPerTeam * 0.5f;
                int k = (i / 2) + 1;

                yellowRock_go.transform.position = new Vector2(yellowRock_go.transform.position.x, yellowRock_go.transform.position.y - (0.4f));
                Rock_Info yellowRock_info = yellowRock_go.GetComponent<Rock_Info>();
                yellowRock_info.rockNumber = k;
                yellowRock_go.name = yellowRock_info.teamName + " " + yellowRock_info.rockNumber;
                yellowRock_go.GetComponent<Rock_Flick>().enabled = false;
                yellowRock_go.GetComponent<Rock_Release>().enabled = false;
                yellowRock_go.GetComponent<Rock_Force>().enabled = false;
                gm.rockList.Add(new Rock_List(yellowRock_go, yellowRock_info));
                yield return new WaitForSeconds(0.1f);
            }
            if (i % 2 == 0)
            {
                GameObject redRock_go = Instantiate(gm.redShooter, gm.redRocksInactive);
                float yRocks = gm.rocksPerTeam / 2f;
                int k = (i / 2);

                redRock_go.transform.position = new Vector2(redRock_go.transform.position.x, redRock_go.transform.position.y - (0.4f));

                Rock_Info redRock_info = redRock_go.GetComponent<Rock_Info>();
                redRock_info.rockNumber = k;
                redRock_go.name = redRock_info.teamName + " " + redRock_info.rockNumber;
                redRock_go.GetComponent<Rock_Flick>().enabled = false;
                redRock_go.GetComponent<Rock_Release>().enabled = false;
                redRock_go.GetComponent<Rock_Force>().enabled = false;
                gm.rockList.Add(new Rock_List(redRock_go, redRock_info));
                yield return new WaitForSeconds(0.1f);
            }
        }

        gm.rockList.Sort();
        foreach (Rock_List rock in gm.rockList)
        {
            Debug.Log(gm.rockList.IndexOf(rock) + " " + rock.rockInfo.teamName);
        }

        gm.CheckScore();
    }

}