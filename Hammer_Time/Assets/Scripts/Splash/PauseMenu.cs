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

        BuildPhysicsButton();
        BuildStopCurveButton();
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
            stopCurveLabel.text = "Arcade stop: " + RockPhysicsBootstrap.StopLabel() + "  (tap to change)";
    }

    // A tester-only button along the bottom of the pause menu (hidden in release builds via DevOptions).
    Text BuildDevButton(string name, float fromBottom, UnityEngine.Events.UnityAction onClick)
    {
        if (!DevOptions.Enabled || pauseMenuUI == null) return null;

        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(pauseMenuUI.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, fromBottom);
        rt.sizeDelta = new Vector2(520f, 80f);
        go.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f, 0.92f);

        var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelGO.transform.SetParent(go.transform, false);
        var lrt = (RectTransform)labelGO.transform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var label = labelGO.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 30;
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
            physicsLabel.text = "Physics: " + RockPhysicsBootstrap.Label(RockPhysicsBootstrap.Current) + "  (tap to change)";
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
