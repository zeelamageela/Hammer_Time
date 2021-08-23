using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public GameManager gm;
    public CameraManager cm;
    public SweeperManager sm;
    public DialogueManager dm;

    public DialogueTrigger skipDialogue;
    public DialogueTrigger annDialogue;
    public AI_Target aiTarg;
    public AI_Shooter aiShoot;
    public GameObject dialogueGO;

    public GameState state;

    public bool redHammer;
    public int ends;
    public int rocks;
    public float volume;
    public bool tutorial;
    public bool loadGame;
    public bool aiYellow;
    public bool aiRed;
    public bool debug;
    public bool mixed;
    public int rockCurrent;
    public int endCurrent;
    public int yellowScore;
    public int redScore;
    public bool third;
    public bool skip;

    public float timeScale;

    public GameObject targetStory;
    public GameObject targetStory2;
    public GameObject targetPlayer;
    public GameObject targetAi;

    private void Start()
    {
        StartCoroutine(StoryFlow());
      
    }
    private void Update()
    {
        state = gm.state;
        timeScale = Time.timeScale;
        //if (state == GameState.START)
        //{
        //    StartCoroutine(StoryFlow());
        //}
        

    }
    IEnumerator StoryFlow()
    {
        yield return new WaitUntil(() => state == GameState.YELLOWTURN);
        #region Shot 8
        yield return new WaitUntil(() => gm.rockCurrent == 7);
        gm.gHUD.mainDisplay.enabled = false;
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(0);

        yield return new WaitUntil(() => dialogueGO.activeSelf == false);

        gm.rm.inturn = true;
        aiTarg.OnTarget("Manual Tap Back", gm.rockCurrent, 0);

        yield return new WaitUntil(() => gm.rockList[7].rock.transform.position.y >= -7f);
        sm.SweepWeight(true);

        yield return new WaitUntil(() => gm.rockList[7].rock.transform.position.y >= 1.25f);
        sm.SweepWhoa(true);
        #endregion
        yield return new WaitUntil(() => state == GameState.CHECKSCORE);

        #region Shot 9
        yield return new WaitUntil(() => gm.rockCurrent == 8);

        dialogueGO.SetActive(true);
        skipDialogue.gameObject.SetActive(true);
        skipDialogue.TriggerDialogue(0);
        cm.RockZoom(gm.rockList[5].rock.transform);
        targetStory.SetActive(true);
        targetStory.transform.position = gm.rockList[5].rock.transform.position;
        targetStory2.SetActive(true);

        yield return new WaitUntil(() => dm.sentenceCount < 1);
        cm.ShotSetup();

        yield return new WaitUntil(() => dialogueGO.activeSelf == false);

        yield return new WaitUntil(() => targetStory2.gameObject.GetComponent<TutorialTrajectory>().distance <= 0.1f);

        dialogueGO.SetActive(true);
        skipDialogue.gameObject.SetActive(true);
        skipDialogue.TriggerDialogue(1);
        dm.contButton.gameObject.SetActive(false);

        yield return new WaitUntil(() => gm.rockList[8].rockInfo.shotTaken);

        dm.DisplayNextSentence();
        dialogueGO.SetActive(false);
        targetStory.SetActive(false);
        targetStory2.SetActive(false);

        yield return new WaitUntil(() => gm.rockList[8].rockInfo.rest);
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(1);
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        #endregion

        #region Shot 10
        yield return new WaitUntil(() => gm.rockCurrent == 9);

        gm.rm.inturn = true;
        aiShoot.OnShot("Top Four Foot", gm.rockCurrent);

        yield return new WaitUntil(() => gm.rockList[9].rock.transform.position.y >= -8f);
        sm.SweepLeft(true);

        yield return new WaitUntil(() => gm.rockList[9].rock.transform.position.y >= 3.5f);
        sm.SweepWhoa(true);
        yield return new WaitUntil(() => gm.rockList[9].rockInfo.rest);
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        #endregion

        #region Shot 11
        yield return new WaitUntil(() => gm.rockCurrent == 10);

        dialogueGO.SetActive(true);
        skipDialogue.gameObject.SetActive(true);
        skipDialogue.TriggerDialogue(3);
        cm.RockZoom(gm.rockList[5].rock.transform);
        targetStory.SetActive(true);
        targetStory.transform.position = gm.rockList[5].rock.transform.position;

        #endregion

    }

    IEnumerator WaitForClick()
    {
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
        Debug.Log("Story Manager - Clickeddd");
    }
}
