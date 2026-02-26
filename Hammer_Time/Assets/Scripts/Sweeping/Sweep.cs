using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

public class Sweep : MonoBehaviour
{
    public GameManager gm;
    public SweeperManager sm;
    public MMF_Player fltFdbk;
    public MMF_FloatingText fltText;

    RockManager rm;
    GameObject rock;
    Rigidbody2D rb;

    public GameObject sweeperGO;
    public Sweeper sweeper;
    //public SweepSelector sweepSel;
    public Button sweepButton;
    public Button hardButton;
    public Button whoaButton;
    public Button leftButton;
    public Button rightButton;

    public int sweepTime;
    public float sweepAmt;

    float statCalc;
    float statEndur;

    void Start()
    {
        rm = GetComponent<RockManager>();
        //fltText.Direction = sm.sweepSel.moveDirection;
    }

    private void Update()
    {
        //fltText.Direction = sm.sweepSel.moveDirection;
    }

    public void EnterSweepZone()
    {
        //sweepButton.gameObject.SetActive(true);
        //sweeper.gameObject.SetActive(true);

        //sweepSel.SetupSweepers();
    }

    public void ExitSweepZone()
    {
        OnWhoa();
        sm.ResetSweepers();
    }

    public void OnSweep()
    {
        //// ? QUICK TEST MODE: Disable sweeping for perfect determinism
        //if (PlayerPrefs.GetInt("DisableSweeping", 0) == 1)
        //{
        //    Debug.Log("[Sweep] ? QUICK TEST MODE: Sweeping disabled for deterministic testing");
        //    return;
        //}
        
        statCalc = sm.swprLStats.sweepStrength.GetValue() + sm.swprRStats.sweepStrength.GetValue();
        statEndur = sm.swprLStats.sweepEndurance.GetValue() + sm.swprRStats.sweepEndurance.GetValue();
        //fltText.Direction = sm.sweepSel.moveDirection;
        Debug.Log("statCalc is " + statCalc);
        //Time.timeScale = 0.25f;
        StartCoroutine(SweepWeight());
    }

    public void OnHard()
    {
        //// ? QUICK TEST MODE: Disable sweeping for perfect determinism
        //if (PlayerPrefs.GetInt("DisableSweeping", 0) == 1)
        //{
        //    Debug.Log("[Sweep] ? QUICK TEST MODE: Hard sweep disabled");
        //    return;
        //}

        statCalc = sm.swprLStats.sweepStrength.GetValue() + sm.swprRStats.sweepStrength.GetValue();
        StartCoroutine(SweepHard());
    }

    public void OnLeft()
    {
        //// ? QUICK TEST MODE: Disable sweeping for perfect determinism
        //if (PlayerPrefs.GetInt("DisableSweeping", 0) == 1)
        //{
        //    Debug.Log("[Sweep] ? QUICK TEST MODE: Left sweep disabled");
        //    return;
        //}
        
        statCalc = sm.swprLStats.sweepStrength.GetValue();
        if (!rm.inturn)
        {
            StartCoroutine(SweepLine(true));
        }
        else
        {
            StartCoroutine(SweepCurl(false));
        }
        
    }

    public void OnRight()
    {
        //// ? QUICK TEST MODE: Disable sweeping for perfect determinism
        //if (PlayerPrefs.GetInt("DisableSweeping", 0) == 1)
        //{
        //    Debug.Log("[Sweep] ? QUICK TEST MODE: Right sweep disabled");
        //    return;
        //}
        
        statCalc = sm.swprRStats.sweepStrength.GetValue();
        if (!rm.inturn)
        {
            StartCoroutine(SweepCurl(true));
        }
        else
        {
            StartCoroutine(SweepLine(false));
        }
    }

    public void OnWhoa()
    {
        //Time.timeScale = 1f;
        StartCoroutine(Whoa());
    }

    IEnumerator SweepWeight()
    {
        Debug.Log("Rock being swept - " + gm.rockList[gm.rockCurrent].rock.name);
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        sweepTime = Mathf.Clamp(sweepTime - (int)(statEndur / 200f), 0, sweepTime);

        Debug.Log("Sweep Time is " + sweepTime);

        yield return new WaitForSeconds(sweepTime);
        
        // WEIGHT SWEEP (both sweepers): Reduces BOTH linear and angular damping
        // Effect: Rock goes FARTHER and stays STRAIGHTER
        Debug.Log("Curl before sweep is " + rock.GetComponent<Rock_Force>().curl.x);
        float curl = rock.GetComponent<Rock_Force>().curl.x + (statCalc * sweepAmt);
        rock.GetComponent<Rock_Force>().curl.x = curl;
        Debug.Log("Curl after sweep is " + rock.GetComponent<Rock_Force>().curl.x);

        Debug.Log("Sweep Amount is " + (statCalc * sweepAmt));

        // DOUBLED SWEEPING STRENGTH: 2x multiplier on all damping reductions
        // Reduce linear damping (rock goes farther) - 2X STRENGTH
        rb.linearDamping -= (statCalc * sweepAmt * 2f);
        
        // Reduce angular damping (rock stays straighter - spin decay slowed) - 2X STRENGTH
        rb.angularDamping -= (statCalc * sweepAmt * 0.8f * 2f); // 80% effect on angular, doubled
        
        Debug.Log($"Weight Sweep (2X STRENGTH): linearDamping={rb.linearDamping:F3}, angularDamping={rb.angularDamping:F3}");
    }

    IEnumerator SweepHard()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rock.GetComponent<Rock_Force>().curl.x = -0.5f;

        rb.linearDamping = 0.38f;
        rb.angularDamping = 0.32f;

        yield return new WaitForSeconds(sweepTime);
        
        // HARD SWEEP: Aggressive reduction of BOTH dampings - 2X STRENGTH
        // Effect: Rock goes MUCH farther and stays straighter
        rb.linearDamping = (rb.linearDamping - (1.5f * sweepAmt * 2f));
        rb.angularDamping -= (1.2f * sweepAmt * 2f); // Significant angular reduction too, doubled

        float curl = rock.GetComponent<Rock_Force>().curl.x + ((statCalc / 2f) * sweepAmt);
        rock.GetComponent<Rock_Force>().curl.x = curl;

        Debug.Log($"Hard Sweep (2X STRENGTH): linearDamping={rb.linearDamping:F3}, angularDamping={rb.angularDamping:F3}, curl={rock.GetComponent<Rock_Force>().curl.x:F3}");
    }

    IEnumerator SweepLine(bool inturn)
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        sm.CallOut("Line");
        rock = gm.rockList[gm.rockCurrent].rock;
        //fltText.Value = "LINE!";
        //fltText.TargetTransform = gm.rockList[gm.rockCurrent].rock.transform;
        //fltText.Direction = sm.sweepSel.moveDirection;
        //fltText.Play(gm.rockList[gm.rockCurrent].rock.transform.position);
        //rb = rock.GetComponent<Rigidbody2D>();
        rock.GetComponent<Rock_Force>().curl.x = -0.5f;

        rb.linearDamping = 0.38f;
        rb.angularDamping = 0.32f;

        yield return new WaitForSeconds(sweepTime);

        // LINE SWEEP (one sweeper): Reduces LINEAR damping ONLY - 2X STRENGTH
        // Effect: Rock goes FARTHER but curl is unaffected
        rb.linearDamping -= sweepAmt * statCalc / 4f * 2f; // Doubled strength
        // NO angular damping change!

        float curl = rock.GetComponent<Rock_Force>().curl.x + (statCalc * sweepAmt * 5f);
        rock.GetComponent<Rock_Force>().curl.x = curl;
        
        Debug.Log($"Line Sweep (2X STRENGTH): linearDamping={rb.linearDamping:F3}, angularDamping={rb.angularDamping:F3} (unchanged), curl={rock.GetComponent<Rock_Force>().curl.x:F3}");

    }

    IEnumerator SweepCurl(bool inturn)
    {
        sm.CallOut("Curl");
        rock = gm.rockList[gm.rockCurrent].rock;
        //fltText.Value = "CURL!";
        //fltText.TargetTransform = gm.rockList[gm.rockCurrent].rock.transform;
        //fltText.Direction = sm.sweepSel.moveDirection;
        //fltText.Play(gm.rockList[gm.rockCurrent].rock.transform.position);
        rb = rock.GetComponent<Rigidbody2D>();
        rock.GetComponent<Rock_Force>().curl.x = -0.5f;

        rb.linearDamping = 0.38f;
        rb.angularDamping = 0.32f;

        yield return new WaitForSeconds(sweepAmt);

        // CURL SWEEP (one sweeper): Reduces ANGULAR damping ONLY - 2X STRENGTH
        // Effect: Rock CURLS MORE (spin maintained longer)
        rb.angularDamping -= sweepAmt * statCalc / 4f * 2f; // Doubled strength
        // NO linear damping change!

        float curl = rock.GetComponent<Rock_Force>().curl.x - (sweepAmt * statCalc * 5f);
        rock.GetComponent<Rock_Force>().curl.x = curl;
        
        Debug.Log($"Curl Sweep (2X STRENGTH): linearDamping={rb.linearDamping:F3} (unchanged), angularDamping={rb.angularDamping:F3}, curl={rock.GetComponent<Rock_Force>().curl.x:F3}");

    }

    IEnumerator Whoa()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(sweepTime);
        //sm.SweepWhoa();
        //sweepSel.SweepWhoa();

        rock.GetComponent<Rock_Force>().curl.x = -0.5f;

        //Time.timeScale = 1f;
        rb.linearDamping = 0.38f;
        rb.angularDamping = 0.32f;
        //Debug.Log("Curl is " + rock.GetComponent<Rock_Force>().curl.x);

    }
}
