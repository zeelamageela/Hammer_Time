using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public GameManager gm;
    public CameraManager cm;

    public GameState state;

    public float timeScale;

    private void Update()
    {
        state = gm.state;
        timeScale = Time.timeScale;
        if (state == GameState.REDTURN)
        {
            StartCoroutine(StoryFlow());
        }
    }
    IEnumerator StoryFlow()
    {
        yield return new WaitUntil(() => state == GameState.YELLOWTURN);

        Time.timeScale = 0.5f;
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
