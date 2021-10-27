using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TigerForge;

public class CareerSettings : MonoBehaviour
{
    CareerManager cm;

    public string playerName;
    public string teamName;
    public int season;
    public int week;
    public float earnings;
    public Vector2 record;
    public Vector2 tourRecord;

    public GameObject player;
    public GameObject load;
    public InputField playerNameInput;
    public InputField teamNameInput;
    public Text nameLoad;
    public Image colourLoad;
    public Slider teamColourSlider;
    public Image teamHandleSlider;
    public Color teamColour;

    public Text earningsLoad;
    public Text recordLoad;
    GameSettingsPersist gsp;
    Gradient gradient;
    public GameObject tournyInProg;
    public Text drawLoad;
    public Text tournyNameLoad;

    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    EasyFileSave myFile;

    void Start()
    {
        StartCoroutine(LoadFromFile());
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

    }
    // Update is called once per frame
    void Update()
    {
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

    IEnumerator LoadFromFile()
    {
        cm = FindObjectOfType<CareerManager>();
        cm.inProgress = true;
        gsp = FindObjectOfType<GameSettingsPersist>();
        myFile = new EasyFileSave("my_player_data");


        if (myFile.Load())
        {
            playerName = myFile.GetString("Player Name");
            teamName = myFile.GetString("Team Name");
            teamColour = myFile.GetUnityColor("Team Colour");
            earnings = myFile.GetFloat("Career Earnings");
            record = myFile.GetUnityVector2("Career Record");
            gsp.inProgress = myFile.GetBool("Tourny In Progress");
            Debug.Log("Tourny in Progress is " + myFile.GetBool("Tourny In Progress"));
            week = myFile.GetInt("Week");
            season = myFile.GetInt("Season");
            tourRecord = myFile.GetUnityVector2("Tour Record");
            
            //Vector2 tempRecord = myFile.GetUnityVector2("Career Record");
            //record = new Vector2Int((int)tempRecord.x, (int)tempRecord.y);

            if (gsp.inProgress)
            {
                tournyInProg.SetActive(true);
                tournyNameLoad.text = myFile.GetString("Current Tourny Name");
                int draw = myFile.GetInt("Draw");
                int playoffRound = myFile.GetInt("Playoff Round");
                drawLoad.text = "Draw " + draw;
                //cm.currentTourny.id = myFile.GetInt("Current Tourny ID");
                //cm.currentTourny.tour = myFile.GetBool("Current Tourny Tour");
                //cm.currentTourny.qualifier = myFile.GetBool("Current Tourny Qualifier");
                //cm.currentTourny.championship = myFile.GetBool("Current Tourny Championship");
            }
            else
                tournyInProg.SetActive(false);

            myFile.Dispose();
            yield return new WaitForEndOfFrame();

            gsp.careerLoad = true;
            //if (gsp.inProgress)
            //{
            //    drawLoad.text = gsp.draw.ToString();
            //    tournyNameLoad.text = cm.currentTourny.name;
            //}

            load.SetActive(true);
            player.SetActive(false);
            nameLoad.text = playerName + " " + teamName;
            colourLoad.color = teamColour;
            earningsLoad.text = "$" + earnings.ToString();
            recordLoad.text = "Week " + week.ToString() + " | " + record.x.ToString() + " - " + record.y.ToString();
        }
        else
        {
            player.SetActive(true);
            load.SetActive(false);
        }
    }

    public void LoadToCM()
    {
        cm = FindObjectOfType<CareerManager>();
        cm.LoadSettings();

        if (gsp.inProgress)
            SceneManager.LoadScene("Tourny_Home_1");
        else
            SceneManager.LoadScene("Arena_Selector");
    }

    public void Player()
    {
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
    }

    public void New()
    {
        cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();
        ClearPlayer();
        gsp.careerLoad = false;
        earnings = 0f;
        record = Vector2.zero;
        week = 0;
        season = 0;
        gsp.draw = 0;
        gsp.playoffRound = 0;
        gsp.inProgress = false;
        cm.inProgress = false;
        load.SetActive(false);
        player.SetActive(true);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("SplashMenu");
    }

    public void ClearPlayer()
    {
        cm.inProgress = false;
        myFile = new EasyFileSave("my_player_data");
        if (myFile.Load())
            myFile.Delete();
    }
}
