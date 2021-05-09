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
    public GameObject yellowRocks;

    public Sprite redRock;
    public GameObject redRockGO;
    public Sprite redRockMouseOver;
    public Sprite redRockActive;
    public Sprite redRockDead;
    public GameObject redRocks;

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

        if (redHammer)
        {
            StartCoroutine(SetupBar(true, redRockGO, yellowRockGO, redRocks, yellowRocks));
        }
        else
        {
            StartCoroutine(SetupBar(false, yellowRockGO, redRockGO, yellowRocks, redRocks));
        }
    }

    IEnumerator SetupBar(bool redHammer, GameObject hammerRock, GameObject notHammerRock, GameObject hammerRockPos, GameObject notHammerRockPos)
    {
        int totalRocks = rocksPerTeam * 2;
        float hammerOffset;
        Vector3 hammerPos;
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
            hammerRockSpread = (i * hammerOffset) / rocksPerTeam;
            hammerPos = notHammerRockPos.transform.position + new Vector3(0f, -hammerRockSpread, 0f);
            GameObject notHammer = Instantiate(notHammerRock, hammerPos, Quaternion.identity);
            rockList.Add(notHammer);

            hammerPos = hammerRockPos.transform.position + new Vector3(0f, hammerRockSpread, 0f);
            GameObject hammer = Instantiate(hammerRock, hammerPos, Quaternion.identity);
            rockList.Add(hammer);
        }
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
