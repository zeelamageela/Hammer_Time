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
    public Toggle aiTog;
    public Toggle dbTog;

    public int ends;
    public int rocks;
    public float volume;
    public bool ai;
    public bool debug;
    public bool mixed;
    public bool loadGame;
    GameSettingsPersist gsp;
    public static GameSettingsPersist instance;
    // Start is called before the first frame update

    private void Start()
    {
        gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
        //ends = gsp.ends;
        //rocks = gsp.rocks;
        
        gsp.LoadSettings();
    }
    private void Update()
    {
        ends = (int)endSlider.value;
        rocks = (int)rockSlider.value;

        if (aiTog.isOn == false)
        {
            ai = false;
        }
        else ai = true;

        if (dbTog.isOn == false)
        {
            debug = false;
        }
        else debug = true;

        endText.text = ends.ToString();
        rockText.text = rocks.ToString();
        volume = om.volume;
    }

    public void SetHammerRed()
    {
        redHammer = true;
        GameSettingsPersist gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
        gsp.ends = ends;
        gsp.rocks = rocks;
        gsp.aiYellow = ai;
        gsp.redHammer = redHammer;
        gsp.mixed = mixed;

        SceneManager.LoadScene("AIGame_2");
    }

    public void SetHammerYellow()
    {
        redHammer = false;
        GameSettingsPersist gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
        gsp.ends = ends;
        gsp.rocks = rocks;
        gsp.aiRed = ai;
        gsp.redHammer = redHammer;

        SceneManager.LoadScene("AIGame_2");
    }
}
