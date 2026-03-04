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
    public float sweepAmt = 0.1f; // DEFAULT VALUE: 0.5 for moderate effect
    
    [Tooltip("Duration in seconds for one sweep tap animation. Set to match your sweep animation clip length.")]
    public float sweepTapDuration = 0.25f; // Default 0.25 seconds per tap
    
    [Tooltip("If true, each sweep tap extends the duration instead of resetting it. Allows continuous sweeping.")]
    public bool cumulativeSweeping = true; // NEW: Allow taps to stack duration!

    float statCalc;
    float statEndur;
    
    // Track original dampings so we can reset after sweep
    private float originalLinearDamping = 0.38f;
    private float originalAngularDamping = 0.32f;
    private Coroutine activeSweepCoroutine = null;
    
    // CRITICAL: Real-time curl force multiplier (read by Rock_Force every frame!)
    public float activeCurlMultiplier = 1.0f; // 1.0 = normal, <1.0 = straighter, >1.0 = more curl
    public bool isSweeping = false;
    private float sweepEndTime = 0f; // When current sweep effect expires

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
        
        // WEIGHT SWEEP: NON-CUMULATIVE!
        // Reset dampings to original first, then apply changes (prevents stacking)
        rb.linearDamping = originalLinearDamping;
        rb.angularDamping = originalAngularDamping;

        //If stats are 0, use a default value (prevents divide by zero and ensures some effect for testing)
        if (statCalc <= 0.1f)
        {
            statCalc = 99f;
        }

        float sweepStrength = statCalc / 100f; // Normalize to 0-2 range
        
        // Apply changes (will be reset on next sweep) - VERY GENTLE!
        float linearReduction = sweepAmt * sweepStrength * 0.035f;  // Rock goes farther (was 0.05, now 30% weaker)
        rb.linearDamping -= linearReduction;
        
        float angularIncrease = sweepAmt * sweepStrength * 0.25f;  // Rock goes straighter (was 0.5, now 30% weaker)
        rb.angularDamping += angularIncrease;
        
        // ALSO add lateral straightening (like line sweep, but gentler)
        Vector2 currentVel = rb.linearVelocity;
        float forwardSpeed = Mathf.Abs(currentVel.y);
        float speedMultiplier = Mathf.Clamp01(forwardSpeed / 1.0f);
        
        float lateralVelocityBoost = sweepAmt * sweepStrength * 0.005f * speedMultiplier; // Gentle straightening (was 0.01, now 50% weaker)
        float straightenDirection = rf.curl.x < 0 ? 1f : -1f; // OPPOSITE to curl
        rb.linearVelocity = new Vector2(currentVel.x + (straightenDirection * lateralVelocityBoost), currentVel.y);
        
        Debug.Log($"Weight Sweep: linearDamping={rb.linearDamping:F3}, angularDamping={rb.angularDamping:F3}, lateral boost={lateralVelocityBoost:F3}");

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
        
        // CRITICAL FIX: If stats are 0, use a default value
        if (statCalc <= 0.1f)
        {
            statCalc = 99f;
        }
        
        // LINE SWEEP: DIRECTLY modify velocity + add damping!
        // Add lateral velocity OPPOSITE to curl direction (straightening), SCALED by forward speed
        
        float sweepStrength = statCalc / 100f;
        
        // Get current velocity FIRST (need it for speed calculation)
        Vector2 currentVel = rb.linearVelocity;
        
        // Add damping effects (straightening focus - GENTLE angular, slight linear)
        float linearReduction = sweepAmt * sweepStrength * 0.015f;  // Slight distance (was 0.05, reduced 40%)
        rb.linearDamping -= linearReduction;
        
        float angularIncrease = sweepAmt * sweepStrength * 0.10f;  // Gentle spin kill (was 1.4, now 75% weaker!)
        rb.angularDamping += angularIncrease;
        
        // Safety clamp: Never let angular damping get too high (rock must keep some spin!)
        rb.angularDamping = Mathf.Min(rb.angularDamping, 1.0f); // Cap at 1.0
        
        // Scale the sweep effect by the rock's FORWARD velocity (Y-axis)
        float forwardSpeed = Mathf.Abs(currentVel.y);
        float speedMultiplier = Mathf.Clamp01(forwardSpeed / 2.5f); // Normalize: 1.0 m/s = 100% effect
        
        // Calculate lateral velocity to add, scaled by forward speed!
        float lateralVelocityBoost = sweepAmt * sweepStrength * 0.015f * speedMultiplier; // Scaled!
        float straightenDirection = rf.curl.x < 0 ? 1f : -1f; // OPPOSITE to curl
        
        // DIRECTLY add to the rock's velocity
        rb.linearVelocity = new Vector2(currentVel.x + (straightenDirection * lateralVelocityBoost), currentVel.y);
        
        Debug.Log($"[Line Sweep] linearDamping={rb.linearDamping:F3} (-{linearReduction:F3}), angularDamping={rb.angularDamping:F3} (+{angularIncrease:F3}), lateral={lateralVelocityBoost:F3}");

        yield return new WaitForSeconds(sweepTapDuration);
        
        activeSweepCoroutine = null;
    }

    IEnumerator SweepCurl(bool inturn)
    {
        sm.CallOut("Curl");
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        Rock_Force rf = rock.GetComponent<Rock_Force>();
        
        // CRITICAL FIX: If stats are 0, use a default value
        if (statCalc <= 0.1f)
        {
            statCalc = 99f;
        }
        
        // CURL SWEEP: DIRECTLY modify velocity + add damping!
        // Add lateral velocity in the curl direction, SCALED by forward speed
        
        float sweepStrength = statCalc / 100f;
        
        // Get current velocity FIRST (need it for speed calculation)
        Vector2 currentVel = rb.linearVelocity;
        
        // Add damping effects (curl focus - VERY GENTLE angular reduction, slight linear)
        float linearReduction = sweepAmt * sweepStrength * 0.015f;  // Slight distance (was 0.04, reduced 40%)
        rb.linearDamping -= linearReduction;
        
        float angularReduction = sweepAmt * sweepStrength * 0.10f;  // Gentle spin preservation (was 0.7, now 60% weaker!)
        rb.angularDamping -= angularReduction;  // LESS angular damping = spin lasts longer = MORE CURL!
        
        // Safety clamp: NEVER let angular damping go below 0.1 (rock must have some damping!)
        rb.angularDamping = Mathf.Max(rb.angularDamping, 0.1f); // Minimum 0.1
        
        // Scale the sweep effect by the rock's FORWARD velocity (Y-axis)
        // When rock is fast (3 m/s), full effect. When slow (0.5 m/s), much weaker effect.
        float forwardSpeed = Mathf.Abs(currentVel.y);
        float speedMultiplier = Mathf.Clamp01(forwardSpeed / 1.0f); // Normalize: 2.0 m/s = 100% effect, <2.0 = proportionally less
        
        // Calculate lateral velocity to add, scaled by forward speed!
        float lateralVelocityBoost = sweepAmt * sweepStrength * 0.02f * speedMultiplier; // Scaled!
        float curlDirection = rf.curl.x < 0 ? -1f : 1f; // Direction rock is curling
        
        // DIRECTLY add to the rock's velocity (not force!)
        rb.linearVelocity = new Vector2(currentVel.x + (curlDirection * lateralVelocityBoost), currentVel.y);
        
        Debug.Log($"[Curl Sweep] linearDamping={rb.linearDamping:F3} (-{linearReduction:F3}), angularDamping={rb.angularDamping:F3} (-{angularReduction:F3} = spin preserved!), lateral={lateralVelocityBoost:F3}");

        yield return new WaitForSeconds(sweepTapDuration);
        
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
