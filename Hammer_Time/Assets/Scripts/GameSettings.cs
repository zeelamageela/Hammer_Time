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
    public Toggle aiRedTog;
    public Toggle aiYellowTog;
    public Toggle redHammerTog;
    public Toggle yellowHammerTog;
    public Toggle dbTog;

    public int ends;
    public int rocks;
    public float volume;
    public bool aiRed;
    public bool aiYellow;
    public bool team;
    public bool debug;
    public bool mixed;
    public bool loadGame;
    GameSettingsPersist gsp;
    public static GameSettingsPersist instance;
    // Start is called before the first frame update

    private void Start()
    {
        //ends = gsp.ends;
        //rocks = gsp.rocks;

        gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
        gsp.LoadSettings();
    }
    private void Update()
    {
        ends = (int)endSlider.value;
        rocks = (int)rockSlider.value;

        if (redHammerTog.isOn == true)
            redHammer = true;
        else
            redHammer = false;
        if (aiRedTog.isOn == false)
        {
            aiRed = false;
        }
        else aiRed = true;


        if (aiYellowTog.isOn == false)
            aiYellow = false;
        else
            aiYellow = true;

        endText.text = ends.ToString();
        rockText.text = rocks.ToString();
        volume = om.volume;

        if (dbTog.isOn == false)
        {
            debug = false;
        }
        else debug = true;

        
    }

    public void PlayGame()
    {
        GameSettingsPersist gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
        gsp.LoadSettings();

        SceneManager.LoadScene("AiGame");
    }
}
