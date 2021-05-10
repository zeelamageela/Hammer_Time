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
    public RectTransform yellowRocks;

    public Sprite redRock;
    public GameObject redRockGO;
    public Sprite redRockMouseOver;
    public Sprite redRockActive;
    public Sprite redRockDead;
    public RectTransform redRocks;

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

    IEnumerator SetupBar(bool redHammer, GameObject hammerRock, GameObject notHammerRock, RectTransform hammerRockPos, RectTransform notHammerRockPos)
    {
        float hammerOffset;
        Vector3 newHammerRockPos;
        Vector3 newNotHammerRockPos;

        if (redHammer)
        {
            hammerOffset = -10f;
        }
        else
        {
            hammerOffset = 10f;
        }

        for (int i = 0; i < rocksPerTeam; i++)
        {

            //GameObject notHammer = Instantiate(notHammerRock, notHammerRockPos);
            //newNotHammerRockPos = new Vector3((i / rocksPerTeam) * -hammerOffset, 0f, 0f);
            //notHammer.transform.position = newNotHammerRockPos;
            ////rockList.Add(notHammer);
            //yield return new WaitForEndOfFrame();
            //Debug.Log("notHammer RockPos is " + newNotHammerRockPos.x + ", " + newNotHammerRockPos.y);

            GameObject hammer = Instantiate(hammerRock, new Vector3(i * hammerOffset, 0f, 0f), Quaternion.identity, hammerRockPos);
            //hammer.transform.position = new Vector3(hammerRockPos.pivot.x + hammerOffset, 0f, 0f);
            //newHammerRockPos = new Vector3((i / rocksPerTeam) * hammerOffset, 0f, 0f);
            //hammer.transform.position = newHammerRockPos;
            //Debug.Log("hammerRock's Pos is " + newHammerRockPos.x + ", " + newHammerRockPos.y);
            //rockList.Add(hammer);
        }

        yield return new WaitForFixedUpdate();
    }

    public void BarUpdate(bool redHammer, int rockCurrent, int yellowScore, int redScore)
    {
        if (redHammer)
        {

        }
    }

    public void ActiveRock(bool redHammer, int rockCurrent)
    {

    }

    public void DeadRock(int rockCurrent, int redHammer)
    {

    }
}
