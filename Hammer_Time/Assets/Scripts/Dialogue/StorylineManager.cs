using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorylineManager: MonoBehaviour
{
    public static StorylineManager instance;
    
    CareerManager cm;
    TournySelector tSel;
    TeamMenu tMenu;
    PowerUpManager pm;
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

    void FindDialogueParts(int trgSwitch = 0)
    {
        dm = FindObjectOfType<DialogueManager>();
        dialogueGO = dm.dialogueCanvas;
        dialogueGO.SetActive(true);
        GameObject tempGO = GameObject.Find("DialogueTriggers").transform.GetChild(trgSwitch).gameObject;
        skipTrg = tempGO.GetComponent<DialogueTrigger>();
    }

    void TriggerDialogue(GameObject go, DialogueTrigger dlg, Dialogue[] dt, StoryBlock currentStoryBlock, int trgIndex = 0)
    {
        dlg.dialogue = new Dialogue[dt.Length];
        dlg.dialogue[0] = dt[trgIndex];

        dlg.TriggerDialogue();
    }

    IEnumerator FirstFiveWeeks()
    {
        //First Week
        yield return new WaitUntil(() => cm.week == 1);
        FindDialogueParts();
        TriggerDialogue(skip, skipTrg, storyBlocks[blockIndex].triggers, storyBlocks[blockIndex]);
        blockIndex++;


        yield return new WaitUntil(() => cm.week == 2);

        //Week 2
        yield return new WaitUntil(() => cm.week == 2);

        //Week 3
        yield return new WaitUntil(() => cm.week == 3);

        yield break;
    }

}
