using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TournySettings : MonoBehaviour
{
    public string teamName;
    public int rocks;
    public int ends;

    public InputField teamNameInput;
    public Slider rockSlider;
    public Text rockText;
    public Slider endSlider;
    public Text endText;

    GameSettingsPersist gsp;

    // Update is called once per frame
    void Update()
    {
        ends = (int)endSlider.value;
        rocks = (int)rockSlider.value;

        endText.text = ends.ToString();
        rockText.text = rocks.ToString();

        teamName = teamNameInput.text;
    }

    public void LoadToGSP()
    {
        gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
        gsp.LoadTournySettings();

        SceneManager.LoadScene("Tourny_Home_1");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("SplashMenu");
    }
}
