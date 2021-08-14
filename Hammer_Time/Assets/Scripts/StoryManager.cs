using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public GameManager gm;
    public CameraManager cm;
    public DialogueTrigger skipDialogue;

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

    private void Update()
    {
        state = gm.state;
        timeScale = Time.timeScale;
        if (state == GameState.START)
        {
            StartCoroutine(StoryFlow());
        }
    }
    IEnumerator StoryFlow()
    {
        yield return new WaitUntil(() => state == GameState.YELLOWTURN);

        yield return new WaitUntil(() => gm.rockCurrent == 1);

        dialogueGO.SetActive(true);
        skipDialogue.gameObject.SetActive(true);
        skipDialogue.TriggerDialogue();
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
