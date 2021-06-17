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
    public Text sweepCurlText;
    public Button contButton;
    public GameObject scoringPanel;
    public Text scoringText;

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

        yield return new WaitForSeconds(2f);

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

        yield return new WaitForSeconds(2f);

        StartCoroutine(Aim());

        yield return new WaitUntil(() => gm.rockList[5].rock.GetComponent<Rock_Flick>().isPressed == false);

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

        yield return new WaitUntil(() => gm.rockList[5].rock.GetComponent<Rock_Flick>().isPressed == false);

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

        yield return new WaitUntil(() => gm.rockList[5].rock.transform.position.y >= -5f);

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

    IEnumerator SweepCurl()
    {
        sweepPanel.SetActive(true);
        direcSweepText.enabled = true;

        yield return StartCoroutine(WaitForClick());

        paused = false;
        direcSweepText.enabled = false;
        sweepCurlText.enabled = true;

        yield return new WaitUntil(() => gm.sm.sweeperR.sweep == true);

        sweepPanel.SetActive(false);
        sweepCurlText.enabled = false;

        //Time.timeScale = 0.2f;
    }

    IEnumerator Scoring()
    {
        scoringPanel.SetActive(true);
        scoringText.enabled = true;

        yield return StartCoroutine(WaitForClick());

        scoringPanel.SetActive(false);
        scoringText.enabled = false;

        paused = false;
    }
}
