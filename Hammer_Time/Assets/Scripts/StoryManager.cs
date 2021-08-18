using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public GameManager gm;
    public CameraManager cm;
    public SweeperManager sm;

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

    public float timeScale;

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

        yield return new WaitUntil(() => gm.rockCurrent == 7);

        dialogueGO.SetActive(true);
        annDialogue.gameObject.SetActive(true);
        annDialogue.TriggerDialogue(0);

        yield return new WaitUntil(() => dialogueGO.activeSelf == false);

        gm.rm.inturn = true;
        aiTarg.OnTarget("Manual Tap Back", gm.rockCurrent, 0);

        yield return new WaitUntil(() => gm.rockList[7].rock.transform.position.y >= -11f);
        sm.SweepWeight(true);

        yield return new WaitUntil(() => gm.rockList[7].rock.transform.position.y >= 2.25f);
        sm.SweepWhoa(true);

        yield return new WaitUntil(() => state == GameState.CHECKSCORE);




        yield return new WaitUntil(() => gm.rockCurrent == 8);

        dialogueGO.SetActive(true);
        skipDialogue.gameObject.SetActive(true);
        skipDialogue.TriggerDialogue(0);
        cm.RockZoom(gm.rockList[5].rock.transform);

        yield return new WaitUntil(() => dialogueGO.activeSelf == false);

        yield return new WaitForSeconds(0.5f);
        cm.ShotSetup();

        yield return new WaitUntil(() => gm.rockCurrent == 9);


        gm.rm.inturn = true;
        aiShoot.OnShot("Top Four Foot", gm.rockCurrent);

        yield return new WaitUntil(() => gm.rockList[7].rock.transform.position.y >= -8f);
        sm.SweepLeft(true);

        yield return new WaitUntil(() => gm.rockList[7].rock.transform.position.y >= 3.5f);
        sm.SweepWhoa(true);

        yield return new WaitUntil(() => gm.rockCurrent == 9);

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
