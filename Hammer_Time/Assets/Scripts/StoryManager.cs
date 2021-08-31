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
    Rigidbody2D rockRB;

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

        #region Shot 8 AI
        yield return new WaitUntil(() => gm.rockCurrent == 7);
        rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(0);

        yield return new WaitUntil(() => dialogueGO.activeSelf == false);

        targetAi.transform.position = targetAiPos[0];
        gm.rm.inturn = true;
        aiTarg.OnTarget("Manual Draw", gm.rockCurrent, 0);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= -7f);
        sm.SweepWeight(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= 0f);
        if (rockRB.velocity.y >= 3)
            sm.SweepWhoa(true);

        //if (rockRB.velocity.x <= -0.025)
        //    sm.SweepRight(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= 1.5f);
        sm.SweepWhoa(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.rest == true);

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

        rockFlick.story = false;
        targetAi.transform.position = targetPullbackPos[0];
        aiTarg.OnTarget("Manual Tap Back", gm.rockCurrent, 0);
        yield return new WaitUntil(() => gm.rockList[8].rockInfo.shotTaken == true);

        dm.DisplayNextSentence();
        dm.contButton.SetActive(true);
        dialogueGO.SetActive(false);
        targetStory.SetActive(false);
        targetPullback.SetActive(false);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= -3f);
        sm.SweepWeight(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= 0f);
        if (rockRB.velocity.y >= 3.2f)
            sm.SweepRight(true);

        //if (rockRB.velocity.x <= -0.025)
        //    sm.SweepRight(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= 3f);
        sm.SweepWhoa(true);

        yield return new WaitUntil(() => gm.rockList[8].rockInfo.rest);
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        #endregion

        #region Shot 10 AI
        yield return new WaitUntil(() => gm.rockCurrent == 9);

        targetAi.transform.position = targetAiPos[1];
        gm.rm.inturn = true;
        //aiShoot.OnShot("Top Four Foot", gm.rockCurrent);
        aiTarg.OnTarget("Manual Draw", gm.rockCurrent, 0);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= -7f);
        sm.SweepWeight(true);

        //yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= -3f);
        //sm.SweepHard(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= -1f);
        sm.SweepWhoa(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.rest);
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        #endregion

        #region Shot 11
        yield return new WaitUntil(() => gm.rockCurrent == 10);
        rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);


        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        cm.RockZoom(gm.rockList[8].rock.transform);
        dialogueGO.SetActive(true);
        skipDialogue.gameObject.SetActive(true);
        skipDialogue.TriggerDialogue(2);
        targetStory.SetActive(true);
        targetStory.transform.position = gm.rockList[8].rock.transform.position;
        targetPullback.transform.position = new Vector2(targetPullbackPos[1].x + gm.rockList[8].rock.transform.position.x, targetPullbackPos[1].y);
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

        rockFlick.story = false;
        targetPlayer.transform.position = targetPullback.transform.position;
        aiTarg.OnTarget("Manual Take Out", gm.rockCurrent, 0);
        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.shotTaken == true);

        dm.DisplayNextSentence();
        dm.contButton.SetActive(true);
        dialogueGO.SetActive(false);
        targetStory.SetActive(false);
        targetPullback.SetActive(false);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.rest);
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        #endregion

        #region Shot 12 AI
        yield return new WaitUntil(() => gm.rockCurrent == 11);

        targetAi.transform.position = targetAiPos[2];
        gm.rm.inturn = true;
        //aiShoot.OnShot("Top Four Foot", gm.rockCurrent);
        aiTarg.OnTarget("Manual Draw", gm.rockCurrent, 0);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= -7f);
        sm.SweepWeight(true);

        //yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= -3f);
        //sm.SweepHard(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= 0f);
        sm.SweepWhoa(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= 1f);
        sm.SweepLeft(true);


        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.rest);
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        #endregion

        #region Shot 13
        yield return new WaitUntil(() => gm.rockCurrent == 12);
        rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);

        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        cm.RockZoom(gm.rockList[2].rock.transform);
        dialogueGO.SetActive(true);
        skipDialogue.gameObject.SetActive(true);
        skipDialogue.TriggerDialogue(2);
        targetStory.SetActive(true);
        targetStory.transform.position = gm.rockList[2].rock.transform.position;
        targetPullback.transform.position = new Vector2(targetPullbackPos[2].x + gm.rockList[2].rock.transform.position.x, targetPullbackPos[2].y);
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
        rockFlick.story = false;
        targetPlayer.transform.position = targetPullback.transform.position;
        aiTarg.OnTarget("Manual Take Out", gm.rockCurrent, 0);
        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.shotTaken == true);

        dm.DisplayNextSentence();
        dm.contButton.SetActive(true);
        dialogueGO.SetActive(false);
        targetStory.SetActive(false);
        targetPullback.SetActive(false);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.rest);
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);

        #endregion

        #region Shot 14 AI
        yield return new WaitUntil(() => gm.rockCurrent == 13);

        targetAi.transform.position = targetAiPos[3];
        gm.rm.inturn = true;
        aiTarg.OnTarget("Manual Draw", gm.rockCurrent, 0);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= 1.5f);
        sm.SweepLeft(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= 3.5f);
        sm.SweepWhoa(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.rest);
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);

        #endregion

        #region Shot 15
        yield return new WaitUntil(() => gm.rockCurrent == 14);
        rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();

        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);

        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        cm.RockZoom(gm.rockList[13].rock.transform);
        dialogueGO.SetActive(true);
        skipDialogue.gameObject.SetActive(true);
        skipDialogue.TriggerDialogue(2);
        targetStory.SetActive(true);
        targetStory.transform.position = gm.rockList[13].rock.transform.position;
        targetPullback.transform.position = targetPullbackPos[3];
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
        
        rockFlick.story = false;
        targetAi.transform.position = targetPullbackPos[3];
        aiTarg.OnTarget("Manual Draw", gm.rockCurrent, 0);
        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.shotTaken == true);

        dm.DisplayNextSentence();
        dm.contButton.SetActive(true);
        dialogueGO.SetActive(false);
        targetStory.SetActive(false);
        targetPullback.SetActive(false);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.rest);
        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(2);
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);

        #endregion

        #region Shot 16 AI
        yield return new WaitUntil(() => gm.rockCurrent == 15);

        targetAi.transform.position = targetAiPos[4];
        gm.rm.inturn = true;
        aiTarg.OnTarget("Manual Draw", gm.rockCurrent, 0);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= 1.5f);
        sm.SweepLeft(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= 3.5f);
        sm.SweepWhoa(true);

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rockInfo.rest);
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
