using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public GameManager gm;
    public GameState state;

    public bool redHammer;
    public int ends;
    public int rocks;
    public bool aiYellow;
    public bool aiRed;
    public int rockCurrent;
    public int endCurrent;
    public int yellowScore;
    public int redScore;
    public bool third;
    public bool skip;

    public Vector2[] rockPos;
    public Vector2[] targetStoryPos;
    public Vector2[] targetAiPos;
    public Vector2[] targetPullbackPos;

    public float timeScale;

    private void Start()
    {
        //StartCoroutine(StoryFlow());
      
    }
    private void Update()
    {
        state = gm.state;
        Time.timeScale = timeScale;

    }

    public void SyncToGm()
    {
        redHammer = gm.redHammer;
        ends = gm.endTotal;
        rocks = gm.rockTotal;
        aiYellow = gm.aiTeamYellow;
        aiRed = gm.aiTeamRed;
        rockCurrent = gm.rockCurrent;
        endCurrent = gm.endCurrent;
        yellowScore = gm.yellowScore;
        redScore = gm.redScore;
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
