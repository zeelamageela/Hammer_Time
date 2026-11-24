using System.Collections;
using System.Collections.Generic;
using Lofelt.NiceVibrations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameHUD gHUD;
    AudioManager am;

    public Slider volumeSlider;
    public float volume;
    public Toggle debugToggle;

    // Update is called once per frame
    private void Start()
    {
        // Static, so it survives scene loads: leaving via Pause > Main Menu used to leave it true in the next scene.
        GameIsPaused = false;
        am = FindObjectOfType<AudioManager>();
        am.PlayBG(3);

        if (debugToggle != null)
        {
            debugToggle.isOn = GameSettingsPersist.instance != null && GameSettingsPersist.instance.debug;
            debugToggle.onValueChanged.AddListener(OnDebugToggleChanged);
        }

        FindMenus();
        if (Modern)
        {
            WireAdvanced();
        }
        else
        {
            // older pause menus (no PlayerOptions / AdvancedOptions panels): generate the tester buttons
            BuildTesterToggle();
            BuildPhysicsButton();
            BuildStopCurveButton();
        }
        TidyMenu();
    }

    // ---- the two-panel pause menu: PlayerOptions (what a player sees) and AdvancedOptions (testers only, behind the Advanced button) ----

    Transform playerOptions, advancedOptions;
    Button advancedButton;
    Slider physicsSlider, stopSlider;

    bool Modern { get { return playerOptions != null && advancedOptions != null; } }

    void FindMenus()
    {
        if (pauseMenuUI == null) return;
        playerOptions = pauseMenuUI.transform.Find("PlayerOptions");
        advancedOptions = pauseMenuUI.transform.Find("AdvancedOptions");
    }

    /// <summary>
    /// Makes a button's click do only what is added in code: the scene's saved clicks (a duplicated Resume button still carries
    /// "Resume") are switched off, which is possible at runtime even though they cannot be removed.
    /// </summary>
    static void ReplaceClick(Button b, UnityEngine.Events.UnityAction action)
    {
        if (b == null) return;
        for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
            b.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(action);
    }

    static Button ButtonIn(Transform root, string name)
    {
        var t = root != null ? root.Find(name) : null;
        return t != null ? t.GetComponent<Button>() : null;
    }

    /// <summary>A duplicated Resume button keeps the word "Resume" until someone renames it.</summary>
    static void FixLabels(Button b, string wanted)
    {
        if (b == null) return;
        foreach (var t in b.GetComponentsInChildren<Text>(true))
            if (t.text.Trim().ToLowerInvariant() == "resume") t.text = wanted;
    }

    void WireAdvanced()
    {
        advancedButton = ButtonIn(playerOptions, "AdvancedSettings");
        var back = ButtonIn(advancedOptions, "BackButton");
        ReplaceClick(advancedButton, () => ShowAdvanced(true));
        ReplaceClick(back, () => ShowAdvanced(false));
        FixLabels(advancedButton, "Advanced");
        FixLabels(back, "Back");

        // the physics pace has three settings and the Arcade stop curve four: whole-number sliders
        physicsSlider = SliderIn("Physics Slider");
        stopSlider = SliderIn("Stop Slider");
        SetupSlider(physicsSlider, 2, OnPhysicsSlider);
        SetupSlider(stopSlider, RockPhysicsBootstrap.StopPowers.Length - 1, OnStopSlider);

        if (debugToggle == null)
        {
            var t = advancedOptions.Find("Debug Toggle");
            if (t != null)
            {
                debugToggle = t.GetComponent<Toggle>();
                debugToggle.isOn = GameSettingsPersist.instance != null && GameSettingsPersist.instance.debug;
                debugToggle.onValueChanged.AddListener(OnDebugToggleChanged);
            }
        }

        RefreshAdvanced();
        ShowAdvanced(false);
    }

    Slider SliderIn(string name)
    {
        var t = advancedOptions.Find(name);
        return t != null ? t.GetComponent<Slider>() : null;
    }

    static void SetupSlider(Slider s, int max, UnityEngine.Events.UnityAction<float> onChange)
    {
        if (s == null) return;
        s.minValue = 0;
        s.maxValue = max;
        s.wholeNumbers = true;
        s.onValueChanged.RemoveAllListeners();
        s.onValueChanged.AddListener(onChange);
    }

    void OnPhysicsSlider(float v)
    {
        RockPhysicsBootstrap.Choose((PhysicsChoice)Mathf.RoundToInt(v));
        RefreshAdvanced();
    }

    void OnStopSlider(float v)
    {
        RockPhysicsBootstrap.SetStopIndex(Mathf.RoundToInt(v));
        RefreshAdvanced();
    }

    /// <summary>Puts the sliders and their labels on the settings in force.</summary>
    void RefreshAdvanced()
    {
        if (physicsSlider != null)
        {
            physicsSlider.SetValueWithoutNotify((int)RockPhysicsBootstrap.Current);
            SetSliderLabel(physicsSlider, RockPhysicsBootstrap.Label(RockPhysicsBootstrap.Current));
        }
        if (stopSlider != null)
        {
            int i = RockPhysicsBootstrap.StopIndex();
            stopSlider.SetValueWithoutNotify(i);
            SetSliderLabel(stopSlider, RockPhysicsBootstrap.StopNames[i] + " - " + RockPhysicsBootstrap.StopPowers[i].ToString("0.0"));
        }
    }

    static void SetSliderLabel(Slider s, string text)
    {
        var label = s.GetComponentInChildren<Text>(true);
        if (label != null) label.text = text;
    }

    public void ShowAdvanced(bool on)
    {
        if (!Modern) return;
        on = on && DevOptions.Enabled;
        playerOptions.gameObject.SetActive(!on);
        advancedOptions.gameObject.SetActive(on);
        if (on) RefreshAdvanced();
    }

    // ---- menu layout: player aids that training has earned, and one collapsed "Tester options" group (dev builds only) -------------

    /// <summary>The vertical list the toggles live in (older pause menus without one use the menu root).</summary>
    Transform ListRoot
    {
        get
        {
            if (pauseMenuUI == null) return null;
            if (Modern) return playerOptions;
            var panel = pauseMenuUI.transform.Find("Panel");
            return panel != null ? panel : pauseMenuUI.transform;
        }
    }

    // Everything a player should never see: AI tuning, debug switches and the physics pace buttons. Names are the scene's own.
    static readonly string[] TesterItemNames = { "AI Settings", "EV Optimization Toggle", "EV Influence Slider", "Show EV Logs", "Debug Toggle" };

    bool testerOpen;
    Text testerLabel;
    readonly List<GameObject> testerItems = new List<GameObject>();

    void BuildTesterToggle()
    {
        testerLabel = BuildDevButton("TesterToggleButton", 0f, ToggleTester);
        RefreshTesterLabel();
    }

    public void ToggleTester()
    {
        testerOpen = !testerOpen;
        RefreshTesterLabel();
        ApplyTesterVisibility();
    }

    void RefreshTesterLabel()
    {
        if (testerLabel != null) testerLabel.text = "TESTER OPTIONS " + (testerOpen ? "-" : "+");
    }

    void ApplyTesterVisibility()
    {
        bool show = DevOptions.Enabled && testerOpen;
        foreach (var go in testerItems)
            if (go != null) go.SetActive(show);
    }

    /// <summary>Shows only the aim aids the player has earned (see TrainingAids) and files the tester items under the Tester button.</summary>
    void TidyMenu()
    {
        var root = ListRoot;
        if (root == null) return;

        SetShown(root, "Traj Toggle", true);
        SetShown(root, "Collision Toggle", TrainingAids.CollisionIndicatorUnlocked);
        SetShown(root, "Collision Line Toggle", TrainingAids.CollisionWarningUnlocked);
        SetShown(root, "Aim Circle Toggle", TrainingAids.AimCircleUnlocked);
        SetShown(root, "Curl Toggle", true);
        SetShown(root, "Guidelines Toggle", true);
        bool flick = GameVisualizationSettings.FlickModeAvailable;
        var flickRoot = Modern ? advancedOptions : root;
        SetShown(flickRoot, "Shot Type", flick);
        SetShown(flickRoot, "Flick Shooting Toggle", flick);

        // new names, but only where the scene still has the old default text (so anything renamed by hand is left alone)
        Relabel(root, "Trajectory", "Trajectory", "Aim Aids");
        RelabelToggle(root, "Collision Toggle", "Collision Trajectory", "Hit Arrows");
        RelabelToggle(root, "Collision Line Toggle", "Collision Line", "Collision Warning");
        RelabelToggle(root, "Curl Toggle", "Curl Fade", "Curl Line");

        if (Modern)
        {
            if (advancedButton != null) advancedButton.gameObject.SetActive(DevOptions.Enabled);   // release builds: no way in
            return;
        }

        if (testerItems.Count == 0)
        {
            var button = root.Find("TesterToggleButton");
            if (button != null) button.SetAsLastSibling();
            foreach (var n in TesterItemNames)
            {
                var t = root.Find(n);
                if (t == null) continue;
                t.SetAsLastSibling();
                testerItems.Add(t.gameObject);
            }
            foreach (var n in new[] { "PhysicsPaceButton", "StopCurveButton" })
            {
                var t = root.Find(n);
                if (t == null) continue;
                t.SetAsLastSibling();
                testerItems.Add(t.gameObject);
            }
        }
        ApplyTesterVisibility();
        if (testerLabel == null) { /* release build: no button, items stay hidden */ }
    }

    static void SetShown(Transform root, string name, bool shown)
    {
        var t = root.Find(name);
        if (t != null && t.gameObject.activeSelf != shown) t.gameObject.SetActive(shown);
    }

    static void Relabel(Transform root, string name, string oldText, string text)
    {
        var t = root.Find(name);
        var label = t != null ? t.GetComponent<Text>() : null;
        if (label != null && label.text.Trim().ToLowerInvariant() == oldText.ToLowerInvariant()) label.text = text;
    }

    static void RelabelToggle(Transform root, string name, string oldText, string text)
    {
        var t = root.Find(name);
        var label = t != null ? t.GetComponentInChildren<Text>(true) : null;
        if (label != null && label.text.Trim().ToLowerInvariant() == oldText.ToLowerInvariant()) label.text = text;
    }

    // ---- tester option: rock physics pace (hidden in release builds via DevOptions) -------------------------------------------
    Text physicsLabel;
    Text stopCurveLabel;

    void BuildPhysicsButton()
    {
        physicsLabel = BuildDevButton("PhysicsPaceButton", 30f, CyclePhysics);
        RefreshPhysicsLabel();
    }

    void BuildStopCurveButton()
    {
        stopCurveLabel = BuildDevButton("StopCurveButton", 120f, CycleStopCurve);
        RefreshStopCurveLabel();
    }

    public void CycleStopCurve()
    {
        RockPhysicsBootstrap.NextStopCurve();
        RefreshStopCurveLabel();
    }

    void RefreshStopCurveLabel()
    {
        if (stopCurveLabel != null)
            stopCurveLabel.text = "ARCADE STOP: " + RockPhysicsBootstrap.StopLabel();
    }

    // A tester-only button in the pause menu's list (hidden in release builds via DevOptions).
    Text BuildDevButton(string name, float unused, UnityEngine.Events.UnityAction onClick)
    {
        if (!DevOptions.Enabled || pauseMenuUI == null) return null;

        var parent = ListRoot;
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var le = go.GetComponent<LayoutElement>();
        // the menu's layout group does not stretch its children, so without a width these collapse to a sliver
        le.minHeight = le.preferredHeight = 100f;
        le.minWidth = le.preferredWidth = 760f;
        go.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f, 0.92f);

        var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelGO.transform.SetParent(go.transform, false);
        var lrt = (RectTransform)labelGO.transform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var label = labelGO.GetComponent<Text>();
        // look like the rest of the menu: borrow the toggle labels' font
        var sample = parent.Find("Debug Toggle") != null ? parent.Find("Debug Toggle").GetComponentInChildren<Text>(true) : null;
        label.font = sample != null && sample.font != null ? sample.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 48;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 20;
        label.resizeTextMaxSize = 48;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;

        go.GetComponent<Button>().onClick.AddListener(onClick);
        return label;
    }

    public void CyclePhysics()
    {
        RockPhysicsBootstrap.Choose(RockPhysicsBootstrap.Next(RockPhysicsBootstrap.Current));
        RefreshPhysicsLabel();
    }

    void RefreshPhysicsLabel()
    {
        if (physicsLabel != null)
            physicsLabel.text = "PHYSICS: " + RockPhysicsBootstrap.Label(RockPhysicsBootstrap.Current);
    }

    public void OnDebugToggleChanged(bool on)
    {
        if (GameSettingsPersist.instance != null)
            GameSettingsPersist.instance.debug = on;
    }
    void Update()
    {
        if (GameIsPaused && am != null)
        {
            volume = volumeSlider.value;
            am.Volume(volume);

            am.maxVol = volume;

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
                
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        GameIsPaused = false;
        AudioManager am = FindObjectOfType<AudioManager>();
        am.PlayBG(0);
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        HapticController.Stop();
        pauseMenuUI.SetActive(true);
        ShowAdvanced(false);
        TidyMenu();
        GameIsPaused = true;
        gHUD.scoreCheck = true;
        volumeSlider.value = am.maxVol;
        am.PlayBG(3);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        AudioManager am = FindObjectOfType<AudioManager>();
        am.Stop("Ambience");
        SceneManager.LoadScene("SplashMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
