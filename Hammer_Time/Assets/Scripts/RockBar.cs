using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RockBar : MonoBehaviour
{
    public GameManager gm;
    public Sprite yellowRock;
    public GameObject yellowRockGO;
    public Sprite yellowRockMouseOver;
    public Sprite yellowRockActive;
    public Sprite yellowRockDead;
    public Transform yellowRocks;

    public Sprite redRock;
    public GameObject redRockGO;
    public Sprite redRockMouseOver;
    public Sprite redRockActive;
    public Sprite redRockDead;
    public Transform redRocks;

    List<GameObject> rockList;

    int activeRock;
    int rocksPerTeam;
    int redRocksLeft;
    int yellowRocksLeft;

    // Update is called once per frame
    void Start()
    {
        rocksPerTeam = gm.rocksPerTeam;

    }

    public void ResetBar(bool redHammer, int rocksPerTeam, int yellowScore, int redScore)
    {
        activeRock = 0;
        rockList = new List<GameObject>();

        if (redHammer)
        {
            StartCoroutine(SetupBar(true, redRockGO, yellowRockGO, redRocks, yellowRocks));
        }
        else
        {
            StartCoroutine(SetupBar(false, yellowRockGO, redRockGO, yellowRocks, redRocks));
        }
    }

    IEnumerator SetupBar(bool redHammer, GameObject hammerRock, GameObject notHammerRock, Transform hammerRockPos, Transform notHammerRockPos)
    {
        int totalRocks = rocksPerTeam * 2;
        float hammerOffset;
        Vector3 rockPos;
        float hammerRockSpread;

        if (redHammer)
        {
            hammerOffset = 350f;
        }
        else
        {
            hammerOffset = -350f;
        }

        for (int i = 0; i < totalRocks; i++)
        {
            hammerRockSpread = (i / rocksPerTeam) * hammerOffset;

            GameObject notHammer = Instantiate(notHammerRock, notHammerRockPos);
            notHammer.transform.position = new Vector3(notHammer.transform.position.x - hammerRockSpread, 0f, 0f);
            rockList.Add(notHammer);

            rockPos = hammerRockPos.transform.position + new Vector3(0f, hammerRockSpread, 0f);
            GameObject hammer = Instantiate(hammerRock, hammerRockPos);
            rockList.Add(hammer);
        }

        yield return new WaitForFixedUpdate();
    }

    public void BarUpdate(bool redHammer, int rockCurrent, int yellowScore, int redScore)
    {

    }

    public void ActiveRock(bool redHammer, int rockCurrent)
    {

    }

    public void DeadRock(int rockCurrent, int redHammer)
    {

    }
}
