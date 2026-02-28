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
        Rock_Force rf = rock.GetComponent<Rock_Force>();
        sweepTime = Mathf.Clamp(sweepTime - (int)(statEndur / 200f), 0, sweepTime);

        Debug.Log("Sweep Time is " + sweepTime);

        yield return new WaitForSeconds(sweepTime);
        
        // WEIGHT SWEEP (both sweepers): Polishes ice in front of rock
        // Effect 1: LESS FRICTION ? Rock goes FARTHER
        // Effect 2: LESS CURL ? Rock goes STRAIGHTER (reduced lateral force)
        // This is the "standard" sweep - distance + straightness
        
        float sweepStrength = statCalc / 100f; // Normalize to 0-2 range (two sweepers, each 0-100)
        
        // Reduce friction (rock goes farther)
        float linearReduction = sweepAmt * sweepStrength * 0.15f;
        rb.linearDamping -= linearReduction;
        
        // Reduce curl force (rock goes straighter) - KEY FIX!
        float curlReduction = sweepAmt * sweepStrength * 0.3f; // Stronger curl reduction
        rf.curl.x *= (1f - curlReduction); // Multiply curl by 0.7-0.4 depending on strength
        
        Debug.Log($"Weight Sweep: linearDamping={rb.linearDamping:F3} (-{linearReduction:F3}), curl={rf.curl.x:F3} (reduced by {curlReduction:F2}x), strength={sweepStrength:F2}");
    }

    IEnumerator SweepHard()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        Rock_Force rf = rock.GetComponent<Rock_Force>();

        yield return new WaitForSeconds(sweepTime);
        
        // HARD SWEEP: Emergency "push it through!" sweep
        // Effect: Maximum friction reduction + curl reduction
        // Both sweepers working HARD to polish the ice
        
        float sweepStrength = statCalc / 100f; // Normalize (both sweepers, 0-200 range)
        
        // Very strong friction reduction (aggressive push)
        float linearReduction = sweepAmt * sweepStrength * 0.22f;
        rb.linearDamping -= linearReduction;
        
        // Strong curl reduction (maximum straightening)
        float curlReduction = sweepAmt * sweepStrength * 0.4f; // Even stronger than weight
        rf.curl.x *= (1f - curlReduction);

        Debug.Log($"Hard Sweep: linearDamping={rb.linearDamping:F3} (-{linearReduction:F3}), curl={rf.curl.x:F3} (reduced by {curlReduction:F2}x), strength={sweepStrength:F2}");
    }

    IEnumerator SweepLine(bool inturn)
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        Rock_Force rf = rock.GetComponent<Rock_Force>();
        sm.CallOut("Line");

        yield return new WaitForSeconds(sweepTime);

        // LINE SWEEP (one sweeper, outside path): Sweeps to STRAIGHTEN the rock
        // Effect 1: Rock goes STRAIGHTER (reduced curl - main effect!)
        // Effect 2: Slight distance boost (less than weight sweep)
        // Use case: "I want it to go straight, not curl as much"
        
        float sweepStrength = statCalc / 100f; // Normalize (one sweeper, 0-100 range)
        
        // Moderate friction reduction (some distance boost)
        float linearReduction = sweepAmt * sweepStrength * 0.12f; // Less than weight
        rb.linearDamping -= linearReduction;
        
        // STRONG curl reduction (main purpose!) - MORE than weight sweep
        float curlReduction = sweepAmt * sweepStrength * 0.4f; // Stronger curl reduction than weight
        rf.curl.x *= (1f - curlReduction);
        
        Debug.Log($"Line Sweep: linearDamping={rb.linearDamping:F3} (-{linearReduction:F3}), curl={rf.curl.x:F3} (STRAIGHTENED by {curlReduction:F2}x), strength={sweepStrength:F2}");
    }

    IEnumerator SweepCurl(bool inturn)
    {
        sm.CallOut("Curl");
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        Rock_Force rf = rock.GetComponent<Rock_Force>();

        yield return new WaitForSeconds(sweepAmt);

        // CURL SWEEP (one sweeper, inside path): DON'T sweep the curl path!
        // Effect 1: Rock CURLS MORE (increased curl force - main effect!)
        // Effect 2: Slight distance boost (from sweeping one side)
        // Use case: "I want it to curl hard into the house"
        
        float sweepStrength = statCalc / 100f; // Normalize (one sweeper, 0-100 range)
        
        // Slight friction reduction (some distance boost)
        float linearReduction = sweepAmt * sweepStrength * 0.12f; // Same as line
        rb.linearDamping -= linearReduction;
        
        // INCREASE curl! (opposite of line sweep!)
        float curlIncrease = sweepAmt * sweepStrength * 0.4f; // Amplify curl
        rf.curl.x *= (1f + curlIncrease); // Multiply curl by 1.4-1.7 depending on strength
        
        Debug.Log($"Curl Sweep: linearDamping={rb.linearDamping:F3} (-{linearReduction:F3}), curl={rf.curl.x:F3} (INCREASED by {curlIncrease:F2}x), strength={sweepStrength:F2}");
    }

    IEnumerator Whoa()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(sweepTime);
        
        // WHOA: Stop sweeping - dampings return to default in Rock_Force physics
        // No manual reset needed - just stop modifying damping
        Debug.Log($"Whoa: Sweeping stopped - damping will return to default via physics");
    }
}
