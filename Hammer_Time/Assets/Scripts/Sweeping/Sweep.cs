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
    
    [Tooltip("Duration in seconds for one sweep tap animation. Set to match your sweep animation clip length.")]
    public float sweepTapDuration = 2.0f; // Default 2 seconds per tap - ADJUST THIS to match your animation!

    float statCalc;
    float statEndur;
    
    // Track original dampings so we can reset after sweep
    private float originalLinearDamping = 0.38f;
    private float originalAngularDamping = 0.32f;
    private Coroutine activeSweepCoroutine = null;

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
        
        // CRITICAL FIX: If stats are 0, use a default value for testing
        if (statCalc <= 0.1f)
        {
            Debug.LogWarning($"[Sweep.OnLeft] swprLStats returned {statCalc}, using default 80");
            statCalc = 80f; // Default sweeper strength for testing
        }
        
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
        
        // CRITICAL FIX: If stats are 0, use a default value for testing
        if (statCalc <= 0.1f)
        {
            Debug.LogWarning($"[Sweep.OnRight] swprRStats returned {statCalc}, using default 80");
            statCalc = 80f; // Default sweeper strength for testing
        }
        
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
        
        // Stop any existing sweep effect
        if (activeSweepCoroutine != null)
        {
            StopCoroutine(activeSweepCoroutine);
        }
        
        // CRITICAL FIX: If stats are 0, use a default value
        if (statCalc <= 0.1f)
        {
            statCalc = 80f;
        }
        
        // LINE SWEEP: GENTLE straightening per tap
        
        float sweepStrength = statCalc / 100f;
        
        // MUCH GENTLER effects
        float angularIncrease = sweepAmt * sweepStrength * 4.0f;  // Was 0.25, now 0.025!
        
        // Apply GENTLE change
        rb.angularDamping += angularIncrease;
        
        Debug.Log($"Line Sweep TAP: angularDamping={rb.angularDamping:F3} (+{angularIncrease:F3}), will reset in {sweepTapDuration}s");

        // Wait for animation duration
        yield return new WaitForSeconds(sweepTapDuration);
        
        // RESET to original after tap completes
        rb.angularDamping = originalAngularDamping;
        
        Debug.Log($"Line Sweep RESET: angularDamping restored to {originalAngularDamping:F3}");
        
        activeSweepCoroutine = null;
    }

    IEnumerator SweepCurl(bool inturn)
    {
        sm.CallOut("Curl");
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        Rock_Force rf = rock.GetComponent<Rock_Force>();
        
        // Stop any existing sweep effect
        if (activeSweepCoroutine != null)
        {
            StopCoroutine(activeSweepCoroutine);
        }
        
        // CRITICAL FIX: If stats are 0, use a default value
        if (statCalc <= 0.1f)
        {
            statCalc = 80f;
        }
        
        // CURL SWEEP: GENTLE, TEMPORARY effect
        // Each tap gives a small burst that lasts for the animation duration
        
        float sweepStrength = statCalc / 100f; // Normalize to 0-1 range
        
        // MUCH GENTLER effects - divide by 10!
        float angularReduction = sweepAmt * sweepStrength * 4.0f;  // Was 0.25, now 0.025 (10x weaker!)
        
        // Apply GENTLE change
        rb.angularDamping -= angularReduction;
        
        Debug.Log($"Curl Sweep TAP: angularDamping={rb.angularDamping:F3} (-{angularReduction:F3}), will reset in {sweepTapDuration}s");

        // Wait for animation duration
        yield return new WaitForSeconds(sweepTapDuration);
        
        // RESET to original after tap completes
        rb.angularDamping = originalAngularDamping;
        
        Debug.Log($"Curl Sweep RESET: angularDamping restored to {originalAngularDamping:F3}");
        
        activeSweepCoroutine = null;
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
