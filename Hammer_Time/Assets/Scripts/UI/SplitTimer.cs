using TMPro;
using UnityEngine;

/// <summary>
/// The split clocks above a thrown rock. A clock "00:00" (seconds:hundredths) counts up from the hack to the near hog line; when the
/// rock crosses it the clock bumps up a row, flashes three times and slowly fades, and a new hog-to-hog clock starts from zero where
/// it was; when the rock crosses the far hog line that one flashes three times and fades.
///
/// Times are game time (physics time), so the display follows the pace profile (Real-time shows real-world seconds) and not the frame
/// rate, and each line crossing is interpolated inside the physics step. Works for player and AI rocks alike: RockSlide arms it at launch.
/// A rock that stops before the far hog line just lets its second clock fade out.
/// </summary>
public class SplitTimer : MonoBehaviour
{
    /// <summary>Tester/shipping switch for the whole display.</summary>
    public static bool Enabled = true;

    public const float HackY = -25f;       // launcher / hack
    public const float NearHogY = -16.15f; // near hog line (same line the rock's friction starts at)
    public const float FarHogY = 0f;       // far hog line

    // Look and feel
    const float FirstRowHeight = 0.65f;    // above the rock centre
    const float RowGap = 0.5f;
    const float BumpSeconds = 0.25f;
    const int Flashes = 3;
    const float FlashPeriod = 0.3f;
    const float FadeSeconds = 2.5f;
    const float FontSize = 4f;
    static readonly Color TextColour = new Color(0.07f, 0.12f, 0.2f, 1f);

    /// <summary>Last finished splits of this rock, seconds (negative = not measured).</summary>
    public float HackToHog { get; private set; } = -1f;
    public float HogToHog { get; private set; } = -1f;

    enum Stage { Idle, WaitingForHack, HackToHog, HogToHog, Done }
    Stage stage = Stage.Idle;

    class Readout
    {
        public TextMeshPro text;
        public float row, targetRow;
        public float finishedAt = -1f;   // Time.time when it stopped counting and began to flash/fade
        public bool flash;
        public float alpha = 1f;
    }

    Readout first, second;
    Rigidbody2D body;
    RockSlide slide;
    float prevY, prevT;
    bool havePrev;
    float hackTime, hogTime;

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
        Clear();
        body = GetComponent<Rigidbody2D>();
        slide = GetComponent<RockSlide>();
        HackToHog = HogToHog = -1f;
        havePrev = false;
        stage = Stage.WaitingForHack;
        first = NewReadout(0f);
        SetText(first, 0f);
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

    // ---- display ----

    Readout NewReadout(float row)
    {
        var go = new GameObject("SplitTimerText");
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.fontSize = FontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = TextColour;
        tmp.outlineWidth = 0.22f;
        tmp.outlineColor = new Color32(255, 255, 255, 255);
        tmp.raycastTarget = false;
        var rect = tmp.rectTransform;
        rect.sizeDelta = new Vector2(6f, 1.5f);

        // draw in front of the ice and the rocks
        var rockSprite = GetComponent<SpriteRenderer>();
        var mr = tmp.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            if (rockSprite != null) mr.sortingLayerID = rockSprite.sortingLayerID;
            mr.sortingOrder = (rockSprite != null ? rockSprite.sortingOrder : 0) + 500;
        }

        var r = new Readout { text = tmp, row = row, targetRow = row };
        Place(r);
        return r;
    }

    static void SetText(Readout r, float seconds)
    {
        if (r != null && r.text != null) r.text.text = "<mspace=0.55em>" + Format(seconds) + "</mspace>";
    }

    void Place(Readout r)
    {
        if (r == null || r.text == null) return;
        r.text.transform.position = transform.position + Vector3.up * (FirstRowHeight + r.row * RowGap);
    }

    /// <summary>Stop counting this readout at the given time and begin the flash-then-fade. Quiet = just fade (no flashing).</summary>
    void Finish(Readout r, float seconds, bool quiet)
    {
        if (r == null) return;
        if (seconds >= 0f) SetText(r, seconds);
        r.finishedAt = Time.time;
        r.flash = !quiet;
    }

    void Animate(Readout r)
    {
        if (r == null || r.text == null) return;
        r.row = Mathf.MoveTowards(r.row, r.targetRow, Time.deltaTime / BumpSeconds);

        if (r.finishedAt >= 0f)
        {
            float e = Time.time - r.finishedAt;
            float flashSpan = r.flash ? Flashes * FlashPeriod : 0f;
            if (e < flashSpan) r.alpha = (e % FlashPeriod) < FlashPeriod * 0.5f ? 1f : 0.1f;
            else r.alpha = 1f - Mathf.Clamp01((e - flashSpan) / FadeSeconds);
        }
        var c = r.text.color;
        c.a = r.alpha;
        r.text.color = c;
        if (r.alpha <= 0f) { Destroy(r.text.gameObject); r.text = null; }
    }

    void Update()
    {
        if (stage == Stage.Idle) return;
        Animate(first);
        Animate(second);

        // live count while a clock runs (physics time, so it matches where the rock really is)
        if (stage == Stage.HackToHog && first != null && first.finishedAt < 0f) SetText(first, Time.fixedTime - hackTime);
        if (stage == Stage.HogToHog && second != null && second.finishedAt < 0f) SetText(second, Time.fixedTime - hogTime);

        if ((first == null || first.text == null) && (second == null || second.text == null) && stage == Stage.Done) stage = Stage.Idle;
    }

    void LateUpdate()
    {
        Place(first);
        Place(second);
    }

    void FixedUpdate()
    {
        if (stage == Stage.Idle || stage == Stage.Done || body == null) return;

        float y = body.position.y;
        float t = Time.fixedTime;
        if (!havePrev)
        {
            prevY = y; prevT = t; havePrev = true;
            if (y >= HackY) { hackTime = t; stage = Stage.HackToHog; }   // armed after the hack: count from now
            return;
        }

        if (stage == Stage.WaitingForHack && prevY < HackY && y >= HackY)
        {
            hackTime = CrossingTime(prevY, prevT, y, t, HackY);
            stage = Stage.HackToHog;
        }

        if (stage == Stage.HackToHog && prevY < NearHogY && y >= NearHogY)
        {
            hogTime = CrossingTime(prevY, prevT, y, t, NearHogY);
            HackToHog = hogTime - hackTime;
            // the first clock bumps up a row and flashes away; the hog-to-hog clock starts from zero where it was
            first.targetRow = 1f;
            Finish(first, HackToHog, quiet: false);
            second = NewReadout(0f);
            SetText(second, 0f);
            stage = Stage.HogToHog;
        }

        if ((stage == Stage.WaitingForHack || stage == Stage.HackToHog) && slide != null && slide.AtRest)
        {
            Finish(first, -1f, quiet: true);        // stopped before the near hog
            stage = Stage.Done;
        }

        if (stage == Stage.HogToHog)
        {
            if (prevY < FarHogY && y >= FarHogY)
            {
                HogToHog = CrossingTime(prevY, prevT, y, t, FarHogY) - hogTime;
                Debug.Log($"[SplitTimer] {name}: hack to hog {Format(HackToHog)}  hog to hog {Format(HogToHog)}  ({HackToHog:F3} s, {HogToHog:F3} s)");
                Finish(second, HogToHog, quiet: false);
                stage = Stage.Done;
            }
            else if (slide != null && slide.AtRest)
            {
                Finish(second, -1f, quiet: true);   // never reached the far hog: let it fade
                stage = Stage.Done;
            }
        }

        prevY = y;
        prevT = t;
    }

    void Clear()
    {
        DestroyReadout(ref first);
        DestroyReadout(ref second);
        stage = Stage.Idle;
    }

    static void DestroyReadout(ref Readout r)
    {
        if (r != null && r.text != null) Destroy(r.text.gameObject);
        r = null;
    }

    void OnDisable()
    {
        Clear();
    }
}
