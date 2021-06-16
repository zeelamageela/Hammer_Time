using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    public bool redHammer;
    public Slider endSlider;
    public Slider rockSlider;
    public Text endText;
    public Text rockText;
    public OptionsMenu om;

    public int ends;
    public int rocks;
    public float volume;

    public static GameSettingsPersist instance;
    // Start is called before the first frame update

    private void Start()
    {
        GameSettingsPersist gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
        ends = gsp.ends;
        rocks = gsp.rocks;
        gsp.LoadSettings();
    }
    private void Update()
    {
        ends = (int)endSlider.value;
        rocks = (int)rockSlider.value;

        endText.text = ends.ToString();
        rockText.text = rocks.ToString();
        volume = om.volume;
    }
    public void SetHammerRed()
    {
        redHammer = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        GameSettingsPersist gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
        gsp.ends = ends;
        gsp.rocks = rocks;
    }

    public void SetHammerYellow()
    {
        redHammer = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        GameSettingsPersist gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
        gsp.ends = ends;
        gsp.rocks = rocks;
    }
}