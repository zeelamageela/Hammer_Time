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

    public Text yellowScoreDisplay;
    public Text redScoreDisplay;
    
    public List<GameObject> rockListUI;

    public float offset = 60f;

    GameObject activeRock;
    Rock_Info activeRockInfo;
    public int rockCurrent;
    int rocksPerTeam;

    // Update is called once per frame
    void Start()
    {
        rocksPerTeam = gm.rocksPerTeam;

        rockListUI = new List<GameObject>();

        yellowRocks.anchoredPosition = yellowRocks.anchoredPosition + new Vector2(-offset * (rocksPerTeam - 1f), 0f);
        redRocks.anchoredPosition = redRocks.anchoredPosition + new Vector2(offset * (rocksPerTeam - 1f), 0f);
    }

    public void ResetBar(bool redHammer)
    {
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
        if (redHammer)
        {
            offset = -offset;
        }

        for (int i = 0; i < rocksPerTeam; i++)
        {
            GameObject notHammer = Instantiate(notHammerRock);
            notHammer.transform.SetParent(notHammerRockPos, false);
            notHammer.transform.position += new Vector3((i * -offset) / 125f, 0f, 0f);

            notHammer.name = gm.rockList[2 * i].rock.name;
            rockListUI.Add(notHammer);

            yield return new WaitForSeconds(i * 0.02f);

            GameObject hammer = Instantiate(hammerRock);
            hammer.transform.SetParent(hammerRockPos, false);

            hammer.transform.position += new Vector3((i * offset) / 125f, 0f, 0f);

            hammer.name = gm.rockList[(2 * i) + 1].rock.name;
            rockListUI.Add(hammer);
        }

        foreach (GameObject rock in rockListUI)
        {
            Debug.Log(rock.name);
        }

        yield return new WaitForFixedUpdate();

        ActiveRock();
    }

    public void EndUpdate(int yellowScore, int redScore)
    {
        yellowScoreDisplay.text = yellowScore.ToString();
        redScoreDisplay.text = redScore.ToString();

        if (rockListUI.Count != 0)
        {
            foreach (GameObject rock in rockListUI)
            {
                Destroy(rock);
            }

            rockListUI.Clear();
        }
    }

    public void ShotUpdate(int rockIndex, bool outOfPlay)
    {
        if (outOfPlay)
        {
            DeadRock(rockIndex);
        }
    }

    public void ActiveRock()
    {
        if (rockListUI.Count != 0)
        {
            rockCurrent = gm.rockCurrent;
            rockListUI[rockCurrent].GetComponent<RockBar_Dot>().ActiveRockSprite();
            Debug.Log("Active Rock is " + rockListUI[rockCurrent].name);
        }
    }

    public void DeadRock(int rockIndex)
    {
        rockCurrent = gm.rockCurrent;
        rockListUI[rockCurrent].GetComponent<RockBar_Dot>().DeadRockSprite();
    }

    public void IdleRock()
    {
        rockCurrent = gm.rockCurrent;
        rockListUI[rockCurrent].gameObject.GetComponent<RockBar_Dot>().IdleRockSprite();
    }
}
