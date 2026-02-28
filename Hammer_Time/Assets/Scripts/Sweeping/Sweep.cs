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
    [Tooltip("Base sweep effect strength (0-1 range). Higher = stronger sweep effects. Default: 0.5")]
    public float sweepAmt = 0.5f; // DEFAULT VALUE: 0.5 for moderate effect

    float statCalc;
    float statEndur;

    void Start()
    {
        rm = GetComponent<RockManager>();
        
        // Safety check: Ensure sweepAmt is never 0 (for old scene files)
        if (sweepAmt <= 0.01f)
        {
            Debug.LogWarning($"[Sweep] sweepAmt was {sweepAmt}, resetting to default 0.5");
            sweepAmt = 0.5f;
        }
        
        // DIAGNOSTIC: Log the actual value being used
        Debug.Log($"[Sweep] sweepAmt initialized to {sweepAmt}");
        
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
        
        // WEIGHT SWEEP: SIMPLE AND DIRECT
        // Reduce both friction AND curl (makes rock go farther and straighter)
        
        float sweepStrength = statCalc / 100f; // Normalize to 0-2 range
        
        // Apply PERMANENT changes IMMEDIATELY
        float linearReduction = sweepAmt * sweepStrength * 0.2f;  // Rock goes farther
        rb.linearDamping -= linearReduction;
        
        float angularIncrease = sweepAmt * sweepStrength * 0.15f;  // Rock goes straighter (spin dies faster)
        rb.angularDamping += angularIncrease;
        
        Debug.Log($"Weight Sweep: linearDamping={rb.linearDamping:F3} (-{linearReduction:F3}), angularDamping={rb.angularDamping:F3} (+{angularIncrease:F3})");

        yield return new WaitForSeconds(sweepTime);
    }

    IEnumerator SweepHard()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        Rock_Force rf = rock.GetComponent<Rock_Force>();
        
        // HARD SWEEP: Maximum push!
        // Both sweepers working hard = big distance + straightening
        
        float sweepStrength = statCalc / 100f; // Both sweepers
        
        // Apply PERMANENT changes IMMEDIATELY
        float linearReduction = sweepAmt * sweepStrength * 0.3f;  // MAX distance boost
        rb.linearDamping -= linearReduction;
        
        float angularIncrease = sweepAmt * sweepStrength * 0.2f;  // Strong straightening
        rb.angularDamping += angularIncrease;

        Debug.Log($"Hard Sweep: linearDamping={rb.linearDamping:F3} (-{linearReduction:F3}), angularDamping={rb.angularDamping:F3} (+{angularIncrease:F3})");

        yield return new WaitForSeconds(sweepTime);
    }

    IEnumerator SweepLine(bool inturn)
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        Rock_Force rf = rock.GetComponent<Rock_Force>();
        sm.CallOut("Line");
        
        // LINE SWEEP: STRAIGHTEN the rock
        // Main effect: Kill the spin faster (rock goes straighter)
        
        float sweepStrength = statCalc / 100f; // One sweeper
        
        // Apply PERMANENT changes IMMEDIATELY
        float linearReduction = sweepAmt * sweepStrength * 0.15f;  // Moderate distance boost
        rb.linearDamping -= linearReduction;
        
        float angularIncrease = sweepAmt * sweepStrength * 0.25f;  // STRONG straightening (more than weight!)
        rb.angularDamping += angularIncrease;
        
        Debug.Log($"Line Sweep: linearDamping={rb.linearDamping:F3} (-{linearReduction:F3}), angularDamping={rb.angularDamping:F3} (+{angularIncrease:F3} = STRAIGHTER)");

        yield return new WaitForSeconds(sweepTime);
    }

    IEnumerator SweepCurl(bool inturn)
    {
        sm.CallOut("Curl");
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        Rock_Force rf = rock.GetComponent<Rock_Force>();
        
        // CURL SWEEP: Make it curl MORE!
        // Main effect: Preserve spin longer (rock curls more)
        
        float sweepStrength = statCalc / 100f; // One sweeper
        
        // DIAGNOSTIC: Log all the values
        Debug.Log($"[SweepCurl DIAGNOSTIC] sweepAmt={sweepAmt}, statCalc={statCalc}, sweepStrength={sweepStrength}");
        
        // Apply PERMANENT changes IMMEDIATELY
        float linearReduction = sweepAmt * sweepStrength * 0.15f;  // Moderate distance boost
        rb.linearDamping -= linearReduction;
        
        float angularReduction = sweepAmt * sweepStrength * 0.25f;  // PRESERVE spin (less damping = more curl!)
        rb.angularDamping -= angularReduction;  // SUBTRACT = spin lasts longer = MORE CURL!
        
        Debug.Log($"Curl Sweep: linearDamping={rb.linearDamping:F3} (-{linearReduction:F3}), angularDamping={rb.angularDamping:F3} (-{angularReduction:F3} = MORE CURL!)");

        yield return new WaitForSeconds(sweepTime);
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
