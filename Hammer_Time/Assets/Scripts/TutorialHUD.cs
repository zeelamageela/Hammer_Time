using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHUD : MonoBehaviour
{
    public GameManager gm;
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

    public GameObject direcSweepPanel;
    public Text sweepLineText1;
    public Text sweepLineText2;
    public Text sweepCurlText1;
    public Text sweepCurlText2;

    public GameObject scoringPanel;
    public Text scoringText;
    public Text failedText;

    public GameObject turnTwoPanel;
    public Text turnTwoText;
    public Text turnSelectText;

    public GameObject finalScoringPanel;
    public Text finalScoringText;
    public Text finalFailedText;

    public GameObject aimCircle1;
    public GameObject aimCircle2;
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

    public void OnRules()
    {
        StartCoroutine(Rules());
    }

    public void OnTurnTwo()
    {
        StartCoroutine(TurnTwo());
    }
    public void OnShoot()
    {
        paused = true;
        StartCoroutine(Shoot());
    }

    public void OnAim()
    {
        StartCoroutine(Aim());
    }

    public void OnSweep()
    {
        paused = true;
        StartCoroutine(Sweep());
    }

    public void OnWhoa()
    {
        paused = true;
        StartCoroutine(Whoa());
    }

    public void OnSweepLine()
    {
        paused = true;
        StartCoroutine(SweepLine());
    }
    public void OnSweepCurl()
    {
        paused = true;
        StartCoroutine(SweepCurl());
    }

    public void OnScoring()
    {
        paused = true;
        StartCoroutine(Scoring());
    }

    public void OnFinalScoring()
    {
        paused = true;
        StartCoroutine(FinalScoring());
    }

IEnumerator Rules()
    {
        rulesPanel.SetActive(true);
        rulesText.enabled = true;

        yield return StartCoroutine(WaitForClick());

        rulesText.enabled = false;
        scenarioText.enabled = true;
        
        gm.gHUD.Scoreboard(10, 0, 0);

        yield return new WaitForFixedUpdate();

        yield return StartCoroutine(WaitForClick());
        gm.gHUD.ScoreboardOff();
        rulesPanel.SetActive(false);
        rulesText.enabled = false;
        paused = false;

        yield return new WaitForSeconds(0.25f);

        OnShoot();
    }

    IEnumerator Shoot()
    {
        shootPanel.SetActive(true);
        pullText.enabled = true;

        yield return StartCoroutine(WaitForClick());

        paused = false;
        pullText.enabled = false;
        releaseText.enabled = true;
        aimCircle1.SetActive(true);

        yield return new WaitForSeconds(1f);

        StartCoroutine(Aim());

        yield return new WaitUntil(() => gm.rockList[13].rock.GetComponent<Rock_Flick>().isPressed == false);

        shootPanel.SetActive(false);
        releaseText.enabled = false;
    }

    IEnumerator Aim()
    {
        aimPanel.SetActive(true);
        aimText.enabled = true;

        yield return new WaitForSeconds(5f);

        aimText.enabled = false;
        guideText.enabled = true;

        yield return new WaitUntil(() => gm.rockList[13].rock.GetComponent<Rock_Flick>().isPressed == false);

        aimPanel.SetActive(false);
        guideText.enabled = false;
        paused = false;
    }

    IEnumerator Sweep()
    {
        sweepPanel.SetActive(true);
        sweepIntroText.enabled = true;

        yield return StartCoroutine(WaitForClick());
        paused = false;

        yield return new WaitUntil(() => gm.rockList[13].rock.transform.position.y >= -5f);

        sweepIntroText.enabled = false;
        sweepButtonText.enabled = true;
        paused = true;

        yield return new WaitUntil(() => gm.sm.sweepButton.activeSelf == false);

        paused = false;
        sweepPanel.SetActive(false);
        sweepButtonText.enabled = false;

    }

    IEnumerator Whoa()
    {
        sweepPanel.SetActive(true);
        whoaButtonText.enabled = true;

        yield return new WaitUntil(() => gm.sm.whoaButton.activeSelf == false);
        paused = false;
        sweepPanel.SetActive(false);
        whoaButtonText.enabled = false;

        //Time.timeScale = 0.2f;
    }

    
    IEnumerator FinalScoring()
    {
        finalScoringPanel.SetActive(true);

        if (gm.houseList.Count == 0)
        {
            finalScoringText.enabled = true;

            yield return StartCoroutine(WaitForClick());
        }
        else if (gm.houseList[0].rockInfo.teamName == gm.rockList[gm.rockCurrent].rockInfo.teamName)
        {
            finalScoringText.enabled = true;
            yield return StartCoroutine(WaitForClick());
        }
        else if (gm.houseList[0].rockInfo.teamName != gm.rockList[gm.rockCurrent].rockInfo.teamName)
        {
            finalFailedText.enabled = true;
            yield return StartCoroutine(WaitForClick());
        }

        paused = false;
        finalScoringPanel.SetActive(false);
        finalScoringText.enabled = false;

    }
    IEnumerator Scoring()
    {
        scoringPanel.SetActive(true);

        if (gm.houseList.Count == 0)
        {
            scoringText.enabled = true;

            yield return StartCoroutine(WaitForClick());
        }
        else if (gm.houseList[0].rockInfo.teamName == gm.rockList[gm.rockCurrent].rockInfo.teamName)
        {
            scoringText.enabled = true;
            yield return StartCoroutine(WaitForClick());
        }
        else if (gm.houseList[0].rockInfo.teamName != gm.rockList[gm.rockCurrent].rockInfo.teamName)
        {
            failedText.enabled = true;
            yield return StartCoroutine(WaitForClick());
        }

        paused = false;
        scoringPanel.SetActive(false);
        scoringText.enabled = false;

    }

    IEnumerator TurnTwo()
    {
        turnTwoPanel.SetActive(true);
        turnTwoText.enabled = true;
        paused = true;

        yield return StartCoroutine(WaitForClick());

        paused = false; 
        turnTwoText.enabled = false;
        turnSelectText.enabled = true;
        aimCircle2.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(WaitForClick());

        turnSelectText.enabled = false;
        turnTwoPanel.SetActive(false);
    }

    IEnumerator SweepLine()
    {
        direcSweepPanel.SetActive(true);
        direcSweepText.enabled = true;

        yield return StartCoroutine(WaitForClick());

        paused = false;

        yield return new WaitUntil(() => gm.rockList[gm.rockCurrent].rock.transform.position.y >= -1.75f);

        direcSweepText.enabled = false;
        sweepLineText1.enabled = true;
        paused = true;

        yield return StartCoroutine(WaitForClick());

        paused = false;
        sweepLineText1.enabled = false;
        sweepLineText2.enabled = true;
        paused = true;

        yield return new WaitUntil(() => gm.sm.sweeperR.sweep == true);
        paused = false;
        direcSweepPanel.SetActive(false);
        sweepLineText2.enabled = false;
    }

    IEnumerator SweepCurl()
    {
        direcSweepPanel.SetActive(true);
        sweepCurlText1.enabled = true;

        yield return StartCoroutine(WaitForClick());

        paused = false;
        sweepCurlText1.enabled = false;
        sweepCurlText2.enabled = true;
        paused = true;

        yield return new WaitUntil(() => gm.sm.sweeperL.sweep == true);

        paused = false;
        direcSweepPanel.SetActive(false);
        sweepCurlText2.enabled = false;
    }
}
