using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TigerForge;

public class TournySettings : MonoBehaviour
{
    public string playerName;
    public string teamName;
    public int rocks;
    public int ends;
    public float earnings;

    public GameObject settings;
    public GameObject player;
    public GameObject load;
    public InputField playerNameInput;
    public InputField teamNameInput;
    public Text nameLoad;
    public Image colourLoad;
    public Slider rockSlider;
    public Text rockText;
    public Slider endSlider;
    public Text endText;
    public Slider teamColourSlider;
    public Image teamHandleSlider;
    public Color teamColour;

    public Text earningsLoad;
    public Text recordLoad;
    public Vector2Int record;
    GameSettingsPersist gsp;
    Gradient gradient;

    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    EasyFileSave myFile;

    void Start()
    {
        myFile = new EasyFileSave("my_player_data");

        if (myFile.Load())
        {
            playerName = myFile.GetString("Player Name");
            teamName = myFile.GetString("Team Name");
            teamColour = myFile.GetUnityColor("Team Colour");
            earnings = myFile.GetFloat("Career Earnings");
            //Vector2 tempRecord = myFile.GetUnityVector2("Career Record");
            //record = new Vector2Int((int)tempRecord.x, (int)tempRecord.y);

            myFile.Dispose();

            load.SetActive(true);
            player.SetActive(false);
            nameLoad.text = playerName + " " + teamName;
            colourLoad.color = teamColour;
            earningsLoad.text = earnings.ToString();
            recordLoad.text = record.x.ToString() + " - " + record.y.ToString();
        }
        else
        {
            player.SetActive(true);
            load.SetActive(false);
        }


        gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[5];
        colorKey[0].color = Color.red;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.blue;
        colorKey[1].time = 0.25f;
        colorKey[2].color = Color.green;
        colorKey[2].time = 0.5f;
        colorKey[3].color = Color.yellow;
        colorKey[3].time = 0.75f;
        colorKey[4].color = Color.red;
        colorKey[4].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);

        // What's the color at the relative time 0.25 (25 %) ?
        //Debug.Log(gradient.Evaluate(0.25f));
    }
    // Update is called once per frame
    void Update()
    {
        if (settings.activeSelf)
        {
            ends = (int)endSlider.value;
            rocks = (int)rockSlider.value;

            endText.text = ends.ToString();
            rockText.text = rocks.ToString();

        }

        if (player.activeSelf)
        {
            teamName = teamNameInput.text;
            playerName = playerNameInput.text;

            teamHandleSlider.color = teamColour;

        }
    }

    public void TeamColour()
    {
        teamColour = gradient.Evaluate(teamColourSlider.value);
    }

    public void LoadToGSP()
    {
        gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
        gsp.LoadTournySettings();

        myFile = new EasyFileSave("my_player_data");

        myFile.Add("Player Name", playerName);
        myFile.Add("Team Name", teamName);
        myFile.Add("Team Colour", teamColour);
        myFile.Add("Career Earnings", 0f);
        myFile.Add("Career Record", new Vector2Int(0, 0));

        myFile.Save();
        SceneManager.LoadScene("Tourny_Home_1");
    }

    public void Player()
    {
        myFile = new EasyFileSave("my_player_data");
        if (myFile.Load())
        {
            player.SetActive(false);
            load.SetActive(true);
        }
        else
        {
            load.SetActive(false);
            player.SetActive(true);
        }

        settings.SetActive(false);
    }

    public void Settings()
    {
        load.SetActive(false);
        settings.SetActive(true);
        player.SetActive(false);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("SplashMenu");
    }

    public void ClearPlayer()
    {
        myFile = new EasyFileSave("my_player_data");

        myFile.Delete();
    }
}
