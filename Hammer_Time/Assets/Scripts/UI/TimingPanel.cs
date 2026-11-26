using TMPro;
using UnityEngine;

/// <summary>
/// The shot readout in the top-left corner: the WEIGHT call of the aim, then SPLIT and HOG TO HOG, each with the expected range
/// (On-Ice Training 2) and the live clock of the rock that was just thrown.
///
/// Everything about the look is in the prefab, Assets/Resources/TimingPanel.prefab: move, resize, re-font and re-colour it there.
/// This script only fills the texts, hides what there is nothing to show in, and fades the panel in and out.
/// It loads itself the first time it is needed, so no scene has to contain it.
/// </summary>
public class TimingPanel : MonoBehaviour
{
    public const string ResourceName = "TimingPanel";

    [Header("Wiring")]
    public CanvasGroup group;
    [Tooltip("Kept inside the phone's safe area (notch, rounded corners). Leave empty to turn that off.")]
    public RectTransform safeArea;
    [Tooltip("The weight call (\"7 - Button\"). Its row is hidden when nobody is aiming.")]
    public TMP_Text weightName;
    public TMP_Text splitRange;
    public TMP_Text splitLive;
    public TMP_Text hogRange;
    public TMP_Text hogLive;

    [Header("Text")]
    [Tooltip("Shown for a time that has not been measured.")]
    public string emptyText = "--:--";
    [Tooltip("Each digit gets the same width (in em) so a running clock does not jitter. 0 = the font's own spacing.")]
    public float digitWidth = 0.55f;

    [Header("Timing")]
    [Tooltip("How long the finished times stay up before fading.")]
    public float holdSeconds = 3f;
    public float fadeSeconds = 1f;
    [Tooltip("A time flashes this many times when its line is crossed.")]
    public int flashes = 3;
    public float flashPeriod = 0.3f;

    static TimingPanel instance;
    static bool missing;

    /// <summary>The panel for this scene, made from the prefab when first asked for (null if there is no prefab).</summary>
    public static TimingPanel Instance
    {
        get
        {
            if (instance != null || missing || !Application.isPlaying) return instance;
            var prefab = Resources.Load<TimingPanel>(ResourceName);
            if (prefab == null)
            {
                missing = true;
                Debug.LogWarning("[TimingPanel] No Resources/TimingPanel prefab. Use the menu Hammer Time > Create Timing Panel Prefab.");
                return null;
            }
            instance = Instantiate(prefab);
            instance.ResetTexts();
            return instance;
        }
    }

    /// <summary>Hides the aim's weight and ranges if a panel exists (does not create one).</summary>
    public static void ClearAim()
    {
        if (instance != null) instance.HideAim();
    }

    class Clock
    {
        public TMP_Text text;
        public float flashStart = -1f;
    }

    readonly Clock split = new Clock(), hog = new Clock();
    SplitTimer owner;      // the rock whose clocks are showing
    bool running;          // its clocks are still going
    bool aimShown;         // an aim is being drawn
    float endedAt = -100f; // when the last throw finished

    void ResetTexts()
    {
        split.text = splitLive;
        hog.text = hogLive;
        if (group != null) group.alpha = 0f;
        SetAimActive(false, false);
        ResetClock(split);
        ResetClock(hog);
    }

    // ---- the aim (weight call and ranges) ----

    /// <summary>The aim being drawn: its weight call, and the expected times (null = the player has not earned the ranges).</summary>
    public void ShowAim(string weight, string splitText, string hogText)
    {
        // a new aim: the last throw's times are old news
        if (!aimShown && !running) { owner = null; ResetClock(split); ResetClock(hog); }
        aimShown = true;
        SetAimActive(true, splitText != null);
        if (weightName != null) weightName.text = weight;
        if (splitText != null) Write(splitRange, splitText);
        if (hogText != null) Write(hogRange, hogText);
    }

    public void HideAim()
    {
        aimShown = false;
        SetAimActive(false, false);
    }

    void SetAimActive(bool weight, bool ranges)
    {
        if (weightName != null) weightName.transform.parent.gameObject.SetActive(weight);
        if (splitRange != null) splitRange.gameObject.SetActive(ranges);
        if (hogRange != null) hogRange.gameObject.SetActive(ranges);
    }

    // ---- live clocks (SplitTimer) ----

    /// <summary>A rock has been thrown. The weight and ranges already up for this aim stay, to compare against.</summary>
    public void BeginThrow(SplitTimer timer)
    {
        owner = timer;
        running = true;
        if (!aimShown) SetAimActive(false, false);
        ResetClock(split);
        ResetClock(hog);
    }

    public void SetSplit(SplitTimer timer, float seconds, bool finished) { Set(timer, split, seconds, finished); }

    public void SetHogToHog(SplitTimer timer, float seconds, bool finished) { Set(timer, hog, seconds, finished); }

    public void EndThrow(SplitTimer timer)
    {
        if (timer != owner || !running) return;
        running = false;
        aimShown = false;      // the call and ranges stay on screen and fade with the times
        endedAt = Time.time;
    }

    void Set(SplitTimer timer, Clock clock, float seconds, bool finished)
    {
        if (timer != owner) return;
        Write(clock.text, SplitTimer.Format(seconds));
        if (finished) clock.flashStart = Time.time;
    }

    void ResetClock(Clock clock)
    {
        clock.flashStart = -1f;
        Write(clock.text, emptyText);
        if (clock.text != null) clock.text.alpha = 1f;
    }

    void Write(TMP_Text text, string value)
    {
        if (text == null) return;
        text.text = digitWidth > 0f ? "<mspace=" + digitWidth.ToString(System.Globalization.CultureInfo.InvariantCulture) + "em>" + value + "</mspace>" : value;
    }

    // ---- showing ----

    void Update()
    {
        Flash(split);
        Flash(hog);
        if (group == null) return;

        float alpha = 0f;
        if (running || aimShown) alpha = 1f;
        else if (owner != null)
        {
            float since = Time.time - endedAt - holdSeconds;
            alpha = since <= 0f ? 1f : 1f - Mathf.Clamp01(since / Mathf.Max(0.01f, fadeSeconds));
            if (alpha <= 0f) owner = null;
        }
        // out of the way of the pause menu
        group.alpha = Time.timeScale == 0f ? 0f : alpha;
    }

    void Flash(Clock clock)
    {
        if (clock.text == null || clock.flashStart < 0f) return;
        float e = Time.time - clock.flashStart;
        if (e >= flashes * flashPeriod) { clock.text.alpha = 1f; clock.flashStart = -1f; return; }
        clock.text.alpha = (e % flashPeriod) < flashPeriod * 0.5f ? 1f : 0.15f;
    }

    void LateUpdate()
    {
        if (safeArea == null || Screen.width <= 0 || Screen.height <= 0) return;
        Rect r = Screen.safeArea;
        safeArea.anchorMin = new Vector2(r.xMin / Screen.width, r.yMin / Screen.height);
        safeArea.anchorMax = new Vector2(r.xMax / Screen.width, r.yMax / Screen.height);
    }
}
