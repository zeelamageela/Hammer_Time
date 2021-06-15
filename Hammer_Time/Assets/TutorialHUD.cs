using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHUD : MonoBehaviour
{
    public GameObject rulesPanel;
    public Text rulesText;
    public Text scenarioText;

    public GameObject shootPanel;
    public Text pullText;
    public Text releaseText;

    public GameObject aimPanel;
    public Text aimText;
    public Text guideText;

    public GameObject sweepPanel;
    public Text sweepIntroText;
    public Text sweepButtonText;
    public Text whoaButtonText;
    public Text direcSweepText;
    public Text sweepCurlText;

    public bool paused;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (paused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    IEnumerator WaitForClick()
    {
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
        Debug.Log("ClickedUI");
    }

    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
    }

    public void Rules()
    {
        paused = true;
        rulesPanel.SetActive(true);
        rulesText.enabled = true;

    }
}
