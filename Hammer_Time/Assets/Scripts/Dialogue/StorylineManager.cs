using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StorylineManager: MonoBehaviour
{
    public static StorylineManager instance;
    
    CareerManager cm;
    TournySelector tSel;
    TeamMenu tMenu;
    PowerUpManager pm;
    TournySettings ts;
    GameManager gm;

    public DialogueManager dm;

    public GameObject dialogueGO;
    public GameObject skip;
    public GameObject announcer;

    public DialogueTrigger skipTrg;
    public DialogueTrigger annTrg;

    public StoryBlock[] storyBlocks;

    public int blockIndex;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        cm = FindObjectOfType<CareerManager>();
        StartCoroutine(FirstFiveWeeks());
    }

    private void Update()
    {
        if (cm != null)
            cm.storyBlock = blockIndex;
    }
    void FindDialogueParts(int trgSwitch = 0)
    {
        dm = FindObjectOfType<DialogueManager>();
        dialogueGO = dm.dialogueCanvas;
        dialogueGO.SetActive(true);

        skip = dm.skipHead;
        skipTrg = GameObject.Find("DialogueTriggers").transform.GetChild(0).gameObject.GetComponent<DialogueTrigger>();

        announcer = dm.announcerHead;
        annTrg = GameObject.Find("DialogueTriggers").transform.GetChild(1).gameObject.GetComponent<DialogueTrigger>();

        if (trgSwitch == 1)
        {
            skip.SetActive(false);
            skipTrg.gameObject.SetActive(false);
            announcer.SetActive(true);
            annTrg.gameObject.SetActive(true);
        }
        else
        {
            skip.SetActive(true);
            skipTrg.gameObject.SetActive(true);
            announcer.SetActive(false);
            annTrg.gameObject.SetActive(false);
        }
    }

    void TriggerDialogue(GameObject go, DialogueTrigger dlg, Dialogue[] dt, int trgIndex = 0)
    {
        dlg.dialogue = new Dialogue[dt.Length];
        dlg.dialogue[0] = dt[trgIndex];

        dlg.TriggerDialogue();
    }

    IEnumerator FirstFiveWeeks()
    {
        if (cm.week <= 1)
        {
            if (blockIndex == 0)
            {
                //First Week
                yield return new WaitUntil(() => cm.week == 1);
                FindDialogueParts();
                TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers);
                yield return new WaitForSeconds(0.5f);
                blockIndex++;
            }

            if (blockIndex == 1)
            {
                bool[] playedDialogue = new bool[storyBlocks[blockIndex].triggers.Length];
                tSel = FindObjectOfType<TournySelector>();

                for (int i = 0; i < playedDialogue.Length; i++)
                {
                    if (SceneManager.GetActiveScene().name == "Arena_Selector")
                    {
                        yield return new WaitUntil(() => !tSel.menuButtons[tSel.menuBut_Select].gameObject.activeSelf);

                        //Debug.Log("tSel.menuBut_Select is " + tSel.menuBut_Select);
                        if (playedDialogue[tSel.menuBut_Select] == false)
                        {
                            FindDialogueParts();
                            TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers, tSel.menuBut_Select);
                            playedDialogue[tSel.menuBut_Select] = true;
                        }
                        else
                            i--;

                        if (dm != null)
                            yield return new WaitUntil(() => !dialogueGO.activeSelf);
                    }
                }
                blockIndex++;

                yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Tourny_Menu_1");

            }

            if (blockIndex == 2)
            {
                yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "TournyGame");
                gm = FindObjectOfType<GameManager>();

                FindDialogueParts(1);
                TriggerDialogue(announcer, annTrg, storyBlocks[blockIndex].triggers);

                yield return new WaitUntil(() => dm.dialogueCanvas.activeSelf);
                blockIndex++;
            }

            if (blockIndex == 3)
            {
                gm = FindObjectOfType<GameManager>();
                yield return new WaitUntil(() => gm.redTurn == gm.aiTeamYellow);

                FindDialogueParts();
                TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers);

                yield return new WaitUntil(() => dm.dialogueCanvas.activeSelf);

                FindDialogueParts();
                TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers, 1);

                yield return new WaitUntil(() => dm.dialogueCanvas.activeSelf);

                yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>().isPressed == true);
                FindDialogueParts();
                TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers, 2);

                yield return new WaitForSeconds(3f);
                dm.dialogueCanvas.SetActive(false);

                yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>().isPressed == false);


                yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.released == true);
                FindDialogueParts();
                TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers, 3);
                Time.timeScale = 0.1f;

                yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.rest == true);

                if (gm.houseList.Count != 0)
                {
                    int houseScore = 0;
                    string winningTeamName = gm.houseList[0].rockInfo.teamName;
                    bool stopCounting = false;

                    // lets loop the list
                    for (int i = 0; i < gm.houseList.Count; i++)
                    {
                        if (!stopCounting)
                        {
                            // lets only count until the team changes
                            if (gm.houseList[i].rockInfo.teamName == winningTeamName)
                            {
                                houseScore++;
                            }
                            else if (gm.houseList[i].rockInfo.teamName != winningTeamName)
                            {
                                stopCounting = true;
                            }
                        }
                    }

                    if (winningTeamName == gm.gsp.teamName)
                    {
                        FindDialogueParts();
                        TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers, 4);
                    }
                    else
                    {
                        FindDialogueParts();
                        TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers, 5);
                    }
                }
                // if the house is empty move along to next turn
                else if (gm.houseList.Count == 0)
                {
                    FindDialogueParts();
                    TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers, 6);
                }


                yield return new WaitUntil(() => gm.rockList[15].rockInfo.rest == true);

                if (gm.houseList.Count != 0)
                {
                    int houseScore = 0;
                    string winningTeamName = gm.houseList[0].rockInfo.teamName;
                    bool stopCounting = false;

                    // lets loop the list
                    for (int i = 0; i < gm.houseList.Count; i++)
                    {
                        if (!stopCounting)
                        {
                            // lets only count until the team changes
                            if (gm.houseList[i].rockInfo.teamName == winningTeamName)
                            {
                                houseScore++;
                            }
                            else if (gm.houseList[i].rockInfo.teamName != winningTeamName)
                            {
                                stopCounting = true;
                            }
                        }
                    }

                    if (winningTeamName == gm.gsp.teamName)
                    {
                        FindDialogueParts();
                        TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers, 7);
                    }
                    else
                    {
                        FindDialogueParts();
                        TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers, 8);
                    }
                }
                // if the house is empty move along to next turn
                else if (gm.houseList.Count == 0)
                {
                    FindDialogueParts();
                    TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers, 8);
                }

                blockIndex++;
            }


            //yield return new WaitUntil(() => gm.rockCurrent ==  )

            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Arena_Selector");
            StartCoroutine(FirstFiveWeeks());
        }

        //Week 2
        if (cm.week == 2)
        {
            if (blockIndex < 4)
                blockIndex = 4;

            if (blockIndex == 4)
            {
                yield return new WaitUntil(() => cm.week == 2);

                FindDialogueParts();
                TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers);
                blockIndex++;
            }
        }

        //Week 3
        yield return new WaitUntil(() => cm.week == 3);

        yield break;
    }

}
