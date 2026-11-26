using UnityEngine;

/// <summary>
/// Times a thrown rock: the SPLIT (back line to near hog line) and the HOG-TO-HOG time (near hog to far hog). It only measures;
/// TimingPanel shows the numbers in the top-left corner.
///
/// Times are game time (physics time), so they follow the pace profile (Real-time shows real-world seconds) and not the frame
/// rate, and each line crossing is interpolated inside the physics step. Works for player and AI rocks alike: RockSlide arms it at launch.
/// </summary>
public class SplitTimer : MonoBehaviour
{
    /// <summary>Tester/shipping switch for the live clocks.</summary>
    public static bool Enabled = true;

    /// <summary>The back line at the throwing end. It is where the shooter lets go (ShooterAnim.DefaultReleasePointY), so from here
    /// to the near hog the rock runs at one steady speed in every pace.</summary>
    public const float BackLineY = ShooterAnim.DefaultReleasePointY;
    public const float NearHogY = -16.15f; // near hog line (same line the rock's friction starts at)
    public const float FarHogY = 0f;       // far hog line

    /// <summary>Last finished times of this rock, seconds (negative = not measured).</summary>
    public float Split { get; private set; } = -1f;
    public float HogToHog { get; private set; } = -1f;

    enum Stage { Idle, WaitingForBackLine, Split, HogToHog }
    Stage stage = Stage.Idle;

    TimingPanel panel;
    Rigidbody2D body;
    RockSlide slide;
    float prevY, prevT;
    bool havePrev;
    float backTime, hogTime;

    /// <summary>Called at launch (RockSlide.BeginFlight): starts the clocks for this throw.</summary>
    public static void Arm(GameObject rock)
    {
        if (!Enabled || rock == null) return;
        var t = rock.GetComponent<SplitTimer>();
        if (t == null) t = rock.AddComponent<SplitTimer>();
        t.Begin();
    }

    void Begin()
    {
        body = GetComponent<Rigidbody2D>();
        slide = GetComponent<RockSlide>();
        Split = HogToHog = -1f;
        havePrev = false;
        stage = Stage.WaitingForBackLine;
        panel = TimingPanel.Instance;
        if (panel != null) panel.BeginThrow(this);
    }

    // ---- pure helpers (tested) ----

    /// <summary>"03:21": whole seconds, then hundredths. Runs out at 99:99.</summary>
    public static string Format(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int hundredths = Mathf.FloorToInt(seconds * 100f + 1e-4f);
        if (hundredths > 9999) hundredths = 9999;
        return (hundredths / 100).ToString("00") + ":" + (hundredths % 100).ToString("00");
    }

    /// <summary>When a straight move from (y0 at t0) to (y1 at t1) crosses a line.</summary>
    public static float CrossingTime(float y0, float t0, float y1, float t1, float line)
    {
        float dy = y1 - y0;
        if (Mathf.Abs(dy) < 1e-6f) return t1;
        return t0 + Mathf.Clamp01((line - y0) / dy) * (t1 - t0);
    }

    // ---- measuring ----

    void Update()
    {
        if (panel == null) return;
        // live count while a clock runs (physics time, so it matches where the rock really is)
        if (stage == Stage.Split) panel.SetSplit(this, Time.fixedTime - backTime, finished: false);
        if (stage == Stage.HogToHog) panel.SetHogToHog(this, Time.fixedTime - hogTime, finished: false);
    }

    void FixedUpdate()
    {
        if (stage == Stage.Idle || body == null) return;

        float y = body.position.y;
        float t = Time.fixedTime;
        if (!havePrev)
        {
            prevY = y; prevT = t; havePrev = true;
            if (y >= BackLineY) { backTime = t; stage = Stage.Split; }   // armed past the back line: count from now
            return;
        }

        if (stage == Stage.WaitingForBackLine && prevY < BackLineY && y >= BackLineY)
        {
            backTime = CrossingTime(prevY, prevT, y, t, BackLineY);
            stage = Stage.Split;
        }

        if (stage == Stage.Split && prevY < NearHogY && y >= NearHogY)
        {
            hogTime = CrossingTime(prevY, prevT, y, t, NearHogY);
            Split = hogTime - backTime;
            if (panel != null) panel.SetSplit(this, Split, finished: true);
            stage = Stage.HogToHog;
        }

        if (stage == Stage.HogToHog && prevY < FarHogY && y >= FarHogY)
        {
            HogToHog = CrossingTime(prevY, prevT, y, t, FarHogY) - hogTime;
            Debug.Log($"[SplitTimer] {name}: split {Format(Split)}  hog to hog {Format(HogToHog)}  ({Split:F3} s, {HogToHog:F3} s)");
            if (panel != null) panel.SetHogToHog(this, HogToHog, finished: true);
            Stop();
        }
        else if (stage != Stage.Idle && slide != null && slide.AtRest)
        {
            Stop();   // stopped short of the next line
        }

        prevY = y;
        prevT = t;
    }

    void Stop()
    {
        stage = Stage.Idle;
        if (panel != null) panel.EndThrow(this);
    }

    void OnDisable()
    {
        if (stage != Stage.Idle) Stop();
    }
}
