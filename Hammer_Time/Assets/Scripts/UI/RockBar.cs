using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RockBar : MonoBehaviour
{
    public GameManager gm;
    public RockManager rm;

    public Camera uiCam;
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

    public GameObject yellowTurnSelect;
    public TurnAnim yellowTurnAnim;
    public GameObject redTurnSelect;
    public TurnAnim redTurnAnim;
    public Text yellowScoreDisplay;
    public Text redScoreDisplay;
    
    public List<GameObject> rockListUI;

    public float offset = 57f;

    GameObject activeRock;
    Rock_Info activeRockInfo;
    public int rockCurrent;
    int rocksPerTeam;

    public Collider2D col;

    // Update is called once per frame
    void Start()
    {
        rocksPerTeam = gm.rocksPerTeam;

        rockListUI = new List<GameObject>();

        
        yellowTurnSelect.SetActive(false);
        redTurnSelect.SetActive(false);
        //Debug.Log("yellow rocks position is " + yellowRocks.anchoredPosition.x + ", " + yellowRocks.anchoredPosition.y);
    }

    private void Update()
    {
        //for (int i = 0; i <= rockListUI.Count; i++)
        //{
        //        if (gm.rockList != null)
        //        {
        //            if (gm.rockList[i].rock != null)
        //            {
        //            if (gm.rockList[i].rockInfo.outOfPlay)
        //            {
        //                DeadRock(i);
        //            }
        //            else IdleRock(i);
        //            }
        //        }
        //}
    }

    public void ResetBar(bool redHammer)
    {
        

        if (redHammer)
        {
            StartCoroutine(SetupBar(true));
        }
        else
        {
            StartCoroutine(SetupBar(false));
        }
    }

    IEnumerator SetupBar(bool redHammer)
    {
        if (rockListUI.Count != 0)
        {
            foreach (GameObject rock in rockListUI)
            {
                Destroy(rock);
            }

            rockListUI.Clear();
        }
        //redRocks.anchoredPosition = new Vector2(-450f, 5f);
        //yellowRocks.anchoredPosition = new Vector2(450f, 5f);

        //yellowRocks.anchoredPosition = yellowRocks.anchoredPosition + new Vector2(-offset * (rocksPerTeam - 1f), 0f);
        //redRocks.anchoredPosition = redRocks.anchoredPosition + new Vector2(offset * (rocksPerTeam - 1f), 0f);

        yield return new WaitUntil(() => gm.rockList.Count == 16);
        //Debug.Log("Rock List Rock Bar is " + gm.rockList.Count);
        bool redTurn;
        if (redHammer)
        {
            redTurn = false;
            for (int i = 0; i < 8; i++)
            {
                GameObject yellowDot = Instantiate(yellowRockGO, yellowRocks, false);
                yellowDot.transform.position += new Vector3(i * -offset, 0f, 0f);
                yellowDot.name = gm.rockList[2 * i].rock.name;
                rockListUI.Add(yellowDot);

                yield return new WaitForEndOfFrame();

                GameObject redDot = Instantiate(redRockGO, redRocks, false);
                redDot.transform.position += new Vector3((i * offset), 0f, 0f);
                rockListUI.Add(redDot);
                redDot.name = gm.rockList[(2 * i) + 1].rock.name;
            }
        }
        else
        {
            redTurn = true;

            for (int i = 0; i < 8; i++)
            {
                GameObject redDot = Instantiate(redRockGO, redRocks, false);
                redDot.transform.position += new Vector3((i * offset), 0f, 0f);
                rockListUI.Add(redDot);
                redDot.name = gm.rockList[(2 * i) + 1].rock.name;


                GameObject yellowDot = Instantiate(yellowRockGO, yellowRocks, false);
                yellowDot.transform.position += new Vector3(i * -offset, 0f, 0f);
                yellowDot.name = gm.rockList[2 * i].rock.name;
                rockListUI.Add(yellowDot);


            }
        }

        yield return new WaitForEndOfFrame();

        ActiveRock(redTurn, 0);

    }

    public void EndUpdate(int yellowScore, int redScore)
    {
        yellowScoreDisplay.text = yellowScore.ToString();
        redScoreDisplay.text = redScore.ToString();

    }

    public void ShotUpdate(int rockIndex, bool outOfPlay)
    {
        //Debug.Log("Rock Bar Shot Update RockListUI Count is " + rockListUI.Count);
        if (outOfPlay)
        {
            DeadRock(rockIndex);
        }

        if (gm.rockList != null)
        {
            foreach (Rock_List rock in gm.rockList)
            {
                if (rock.rockInfo.outOfPlay)
                {
                    DeadRock(gm.rockList.IndexOf(rock));
                }
                else
                {
                    if (rock.rockInfo.shotTaken)
                    {
                        IdleRock(gm.rockList.IndexOf(rock));
                    }
                }
            }
        }
    }

    public void ActiveRock(bool redTurn, int rockCrnt)
    {
        if (rockListUI.Count != 0 && gm.rockCurrent < rockListUI.Count)
        {
            rockCurrent = rockCrnt;
            rockListUI[rockCurrent].GetComponent<RockBar_Dot>().ActiveRockSprite();
            //Debug.Log("Active Rock is " + rockListUI[rockCurrent].name);
            //Debug.Log("RedTurn is " + redTurn);

            if (redTurn)
            {
                redTurnSelect.SetActive(true);
                yellowTurnSelect.SetActive(false);

                redTurnAnim.SetTurn(rm.inturn);
            }
            else
            {
                yellowTurnSelect.SetActive(true);
                redTurnSelect.SetActive(false);

                yellowTurnAnim.SetTurn(rm.inturn);
            }
        }
    }

    public void DeadRock(int rockIndex)
    {
        rockListUI[rockIndex].GetComponent<RockBar_Dot>().DeadRockSprite();
    }

    public void IdleRock(int rockIndex)
    {
        rockListUI[rockIndex].gameObject.GetComponent<RockBar_Dot>().IdleRockSprite();
    }

    public void Tutorial(int rockIndex)
    {
        for (int i = 0; i < 12; i++)
        {
            if (gm.rockList[i].rockInfo.outOfPlay)
            {
                DeadRock(i);
            }
            else if (gm.rockList[i].rockInfo.inPlay)
            {
                IdleRock(i);
            }
        }
    }
}
