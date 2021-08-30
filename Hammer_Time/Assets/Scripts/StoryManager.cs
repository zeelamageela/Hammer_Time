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
    public GameObject targetPullback;
    public GameObject targetPlayer;
    public GameObject targetAi;

    public Vector2[] rockPos;
    public Vector2[] targetStoryPos;
    public Vector2[] targetAiPos;
    public Vector2[] targetPullbackPos;

    Rock_Flick rockFlick;
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
        
        if (rockFlick && rockFlick.springReleased)
        {
        }
    }
    IEnumerator StoryFlow()
    {
        yield return new WaitUntil(() => state == GameState.YELLOWTURN);
        gm.gHUD.mainDisplay.enabled = false;

        #region Shot 8
        yield return new WaitUntil(() => gm.rockCurrent == 7);

        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(0);

        yield return new WaitUntil(() => dialogueGO.activeSelf == false);

        targetAi.transform.position = targetAiPos[0];
        gm.rm.inturn = true;
        aiTarg.OnTarget("Manual Draw", gm.rockCurrent, 0);

        yield return new WaitUntil(() => gm.rockList[7].rock.transform.position.y >= -7f);
        sm.SweepWeight(true);

        yield return new WaitUntil(() => gm.rockList[7].rock.transform.position.y >= 1.5f);
        sm.SweepWhoa(true);

        yield return new WaitUntil(() => gm.rockList[7].rockInfo.rest == true);

        yield return new WaitUntil(() => state == GameState.CHECKSCORE);

        #endregion

        #region Shot 9
        yield return new WaitUntil(() => gm.rockCurrent == 8);
        rockFlick = gm.rockList[8].rock.GetComponent<Rock_Flick>();
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(1);


        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        cm.RockZoom(gm.rockList[5].rock.transform);
        dialogueGO.SetActive(true);
        skipDialogue.gameObject.SetActive(true);
        skipDialogue.TriggerDialogue(0);
        targetStory.SetActive(true);
        targetStory.transform.position = gm.rockList[5].rock.transform.position;
        targetPullback.transform.position = targetPullbackPos[0];
        targetPullback.SetActive(true);

        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        cm.ShotSetup();

        yield return new WaitUntil(() => targetPullback.gameObject.GetComponent<TutorialTrajectory>().distance <= 0.15f);
        
            dialogueGO.SetActive(true);
            skipDialogue.gameObject.SetActive(true);
            skipDialogue.TriggerDialogue(1);
            dm.contButton.SetActive(false);

        rockFlick.story = true;
        yield return new WaitUntil(() => rockFlick.isPressed == true);

        yield return new WaitUntil(() => rockFlick.GetComponent<SpringJoint2D>().dampingRatio == 1f);
        //rockFlick.enabled = false;
        //yield return new WaitForSeconds(0.1f);
        //rockFlick.enabled = true;

        rockFlick.story = false;
        //yield return new WaitForSeconds(0.5f);
        targetAi.transform.position = targetPullbackPos[0];
        aiTarg.OnTarget("Manual Draw", gm.rockCurrent, 0);
        yield return new WaitUntil(() => gm.rockList[8].rockInfo.shotTaken == true);

        //yield return new WaitUntil(() => gm.rockList[8].rock.GetComponent<Rock_Flick>().isPressed = true);
        //Debug.Log("mouse pressed baybay");

        //yield return new WaitUntil(() => gm.rockList[8].rock.GetComponent<Rock_Flick>().isPressed = false);


        //gm.rockList[8].rock.GetComponent<Rock_Flick>().enabled = false;

        dm.DisplayNextSentence();
        dm.contButton.SetActive(true);
        dialogueGO.SetActive(false);
        targetStory.SetActive(false);
        targetPullback.SetActive(false);

        yield return new WaitUntil(() => gm.rockList[8].rockInfo.rest);
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        #endregion

        #region Shot 10
        yield return new WaitUntil(() => gm.rockCurrent == 9);

        targetAi.transform.position = targetAiPos[1];
        gm.rm.inturn = true;
        //aiShoot.OnShot("Top Four Foot", gm.rockCurrent);
        aiTarg.OnTarget("Manual Draw", gm.rockCurrent, 0);

        yield return new WaitUntil(() => gm.rockList[9].rock.transform.position.y >= -8f);
        sm.SweepRight(true);

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
        //cm.RockZoom(gm.rockList[5].rock.transform);
        targetStory.SetActive(true);
        targetStory.transform.position = gm.rockList[5].rock.transform.position;
        targetPullback.transform.position = targetPullbackPos[1];
        targetPullback.SetActive(true);
        #endregion

        #region Shot 12
        yield return new WaitUntil(() => gm.rockCurrent == 11);

        targetAi.transform.position = targetAiPos[2];
        gm.rm.inturn = true;
        //aiShoot.OnShot("Top Four Foot", gm.rockCurrent);
        aiTarg.OnTarget("Manual Draw", gm.rockCurrent, 0);

        yield return new WaitUntil(() => gm.rockList[11].rock.transform.position.y >= 1.5f);
        sm.SweepLeft(true);

        yield return new WaitUntil(() => gm.rockList[11].rock.transform.position.y >= 3.5f);
        sm.SweepWhoa(true);

        yield return new WaitUntil(() => gm.rockList[11].rockInfo.rest);
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
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
