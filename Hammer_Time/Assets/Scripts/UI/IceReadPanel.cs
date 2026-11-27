using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The player's ice menu (see PlayerRead): an ICE button during their turn that opens speed and curl settings, and the question
/// about last visit's notes. Built in code so no scene has to contain it; move things by the numbers in Build.
/// </summary>
public class IceReadPanel : MonoBehaviour
{
    static IceReadPanel instance;

    static readonly Color Back = new Color(0.07f, 0.1f, 0.17f, 0.95f);
    static readonly Color ButtonColour = new Color(0.16f, 0.28f, 0.44f, 1f);
    static readonly Color Ink = new Color(0.95f, 0.91f, 0.87f, 1f);

    GameManager gm;
    TMP_FontAsset font;
    RectTransform openButton, panel, dialog;
    Slider speedSlider, curlSlider;
    TMP_Text speedText, curlText, notesText;

    /// <summary>Called as each turn starts.</summary>
    public static void OnTurn(GameManager game)
    {
        if (!PlayerRead.Active) return;
        PlayerRead.Prepare();
        if (instance == null)
        {
            instance = new GameObject("Ice Read Panel").AddComponent<IceReadPanel>();
            instance.Build();
        }
        instance.gm = game;
    }

    /// <summary>Whether a touch here belongs to this menu, so it must not pick up the rock.</summary>
    public static bool Blocks(Vector2 screen)
    {
        if (instance == null) return false;
        if (instance.dialog.gameObject.activeInHierarchy) return true;
        return Over(instance.openButton, screen) || Over(instance.panel, screen);
    }

    static bool Over(RectTransform rect, Vector2 screen)
    {
        return rect.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rect, screen, null);
    }

    /// <summary>The player is about to shoot: their turn, the rock still in the hack, not paused.</summary>
    bool PlayersTurn()
    {
        if (gm == null || !PlayerRead.Active || Time.timeScale == 0f) return false;
        bool theirs = (gm.state == GameState.REDTURN && !gm.aiTeamRed) || (gm.state == GameState.YELLOWTURN && !gm.aiTeamYellow);
        if (!theirs || gm.rockList == null || gm.rockCurrent < 0 || gm.rockCurrent >= gm.rockList.Count) return false;
        GameObject rock = gm.rockList[gm.rockCurrent].rock;
        if (rock == null) return false;
        var flick = rock.GetComponent<Rock_Flick>();
        return !gm.rockList[gm.rockCurrent].rockInfo.released && (flick == null || (!flick.isPressed && !flick.springReleased));
    }

    void Update()
    {
        bool turn = PlayersTurn();
        bool asking = turn && PlayerRead.NotesWaiting;
        if (asking && !dialog.gameObject.activeSelf)
        {
            var note = PlayerRead.LastNotes;
            notesText.text = IceConditions.Describe(note.speed, note.curl) + "\n" + Weight(IceUnits.Seconds(note.speed)) + "    "
                + Bend(IceUnits.Feet(note.curl));
        }
        dialog.gameObject.SetActive(asking);
        if (!turn || asking) panel.gameObject.SetActive(false);
        openButton.gameObject.SetActive(turn && !asking && !panel.gameObject.activeSelf);
    }

    static string Weight(float seconds) { return "weight " + seconds.ToString("0.0") + " s"; }

    static string Bend(float feet) { return "curl " + feet.ToString("0.0") + " ft"; }

    void Open()
    {
        // the sliders count tenths of a second and of a foot
        speedSlider.SetValueWithoutNotify(Mathf.Round(IceUnits.Seconds(PlayerRead.Speed) * 10f));
        curlSlider.SetValueWithoutNotify(Mathf.Round(IceUnits.Feet(PlayerRead.Curl) * 10f));
        Relabel();
        panel.gameObject.SetActive(true);
        openButton.gameObject.SetActive(false);
    }

    void Changed(float unused)
    {
        PlayerRead.Set(IceUnits.SpeedFromSeconds(speedSlider.value / 10f), IceUnits.CurlFromFeet(curlSlider.value / 10f));
        Relabel();
    }

    void ResetToAverage()
    {
        PlayerRead.StartFresh();
        Open();
    }

    void Relabel()
    {
        speedText.text = "Weight  " + (speedSlider.value / 10f).ToString("0.0") + " s";
        curlText.text = "Curl  " + (curlSlider.value / 10f).ToString("0.0") + " ft";
    }

    // ---- building ----

    void Build()
    {
        var timing = Resources.Load<TimingPanel>(TimingPanel.ResourceName);   // the game's font, as the timing panel has it
        if (timing != null && timing.weightName != null) font = timing.weightName.font;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        Vector2 middle = new Vector2(0.5f, 0.5f), right = new Vector2(1f, 0.5f);

        openButton = Press(transform, "ICE", right, new Vector2(-110f, 180f), new Vector2(180f, 120f), Open);

        // above the hack, so the rock and the room to pull it back stay clear
        panel = Box(transform, "Read", middle, new Vector2(0f, 380f), new Vector2(920f, 620f));
        Words(panel, "YOUR ICE READ", new Vector2(0f, 260f), new Vector2(860f, 70f), 60f);
        Words(panel, "button draw: hog to hog seconds, and feet of curl", new Vector2(0f, 207f), new Vector2(860f, 45f), 34f);
        speedText = Row(panel, 100f, IceUnits.MinSeconds, IceUnits.MaxSeconds, out speedSlider);
        curlText = Row(panel, -80f, IceUnits.MinFeet, IceUnits.MaxFeet, out curlSlider);
        Press(panel, "Venue average", middle, new Vector2(-215f, -235f), new Vector2(400f, 100f), ResetToAverage);
        Press(panel, "Done", middle, new Vector2(215f, -235f), new Vector2(400f, 100f), () => panel.gameObject.SetActive(false));

        // the whole screen, so nothing else can be touched until it is answered
        dialog = Box(transform, "Notes", middle, Vector2.zero, new Vector2(4000f, 4000f));
        dialog.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        var card = Box(dialog, "Card", middle, new Vector2(0f, 200f), new Vector2(920f, 560f));
        Words(card, "LAST TIME HERE", new Vector2(0f, 190f), new Vector2(860f, 90f), 64f);
        notesText = Words(card, "", new Vector2(0f, 40f), new Vector2(860f, 170f), 50f);
        Press(card, "Use these notes", middle, new Vector2(-215f, -170f), new Vector2(400f, 120f), PlayerRead.UseNotes);
        Press(card, "Start fresh", middle, new Vector2(215f, -170f), new Vector2(400f, 120f), PlayerRead.StartFresh);

        openButton.gameObject.SetActive(false);
        panel.gameObject.SetActive(false);
        dialog.gameObject.SetActive(false);
    }

    /// <summary>A label over a slider with a one-step button at each end.</summary>
    TMP_Text Row(RectTransform parent, float y, float min, float max, out Slider slider)
    {
        var label = Words(parent, "", new Vector2(0f, y + 55f), new Vector2(860f, 70f), 48f);

        var go = DefaultControls.CreateSlider(new DefaultControls.Resources());
        Place((RectTransform)go.transform, parent, new Vector2(0.5f, 0.5f), new Vector2(0f, y - 30f), new Vector2(560f, 50f));
        var made = go.GetComponent<Slider>();
        made.minValue = Mathf.Round(min * 10f);
        made.maxValue = Mathf.Round(max * 10f);
        made.wholeNumbers = true;
        made.onValueChanged.AddListener(Changed);
        go.transform.Find("Background").GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
        made.fillRect.GetComponent<Image>().color = ButtonColour;
        made.handleRect.sizeDelta = new Vector2(60f, 30f);   // big enough for a thumb
        made.handleRect.GetComponent<Image>().color = Ink;

        Press(parent, "-", new Vector2(0.5f, 0.5f), new Vector2(-380f, y - 30f), new Vector2(100f, 100f), () => made.value -= 1f);
        Press(parent, "+", new Vector2(0.5f, 0.5f), new Vector2(380f, y - 30f), new Vector2(100f, 100f), () => made.value += 1f);
        slider = made;
        return label;
    }

    static void Place(RectTransform rect, Transform parent, Vector2 anchor, Vector2 at, Vector2 size)
    {
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = at;
        rect.sizeDelta = size;
    }

    static RectTransform Box(Transform parent, string name, Vector2 anchor, Vector2 at, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Place((RectTransform)go.transform, parent, anchor, at, size);
        go.GetComponent<Image>().color = Back;
        return (RectTransform)go.transform;
    }

    RectTransform Press(Transform parent, string text, Vector2 anchor, Vector2 at, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        var box = Box(parent, text, anchor, at, size);
        box.GetComponent<Image>().color = ButtonColour;
        box.gameObject.AddComponent<Button>().onClick.AddListener(action);
        Words(box, text, Vector2.zero, size, 52f);
        return box;
    }

    TMP_Text Words(Transform parent, string text, Vector2 at, Vector2 size, float fontSize)
    {
        var go = new GameObject("Text", typeof(RectTransform));
        Place((RectTransform)go.transform, parent, new Vector2(0.5f, 0.5f), at, size);
        var label = go.AddComponent<TextMeshProUGUI>();
        if (font != null) label.font = font;
        label.text = text;
        label.fontSize = fontSize;
        label.enableAutoSizing = true;
        label.fontSizeMin = 20f;
        label.fontSizeMax = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Ink;
        label.raycastTarget = false;
        return label;
    }
}
