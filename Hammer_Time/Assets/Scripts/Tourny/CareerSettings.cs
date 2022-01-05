using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TigerForge;

public class CareerSettings : MonoBehaviour
{
    CareerManager cm;
    GameSettingsPersist gsp;

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
    public Text nextButton;

    public Text nameLoad;
    public Image colourLoad;
    public Slider teamColourSlider;
    public Image teamHandleSlider;
    public Color teamColour;

    public Text earningsLoad;
    public Text recordLoad;
    public Text weekLoad;

    public GameObject gameInProg;
    public Text gameStats;
    bool ginProg;
    public GameObject tournyInProg;
    public Text drawLoad;
    public Text tournyNameLoad;

    public GameObject newButton;

    Gradient gradient;

    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    EasyFileSave myFile;
    EasyFileSave myFileGame;

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

        //cm.LoadCareer();
        gsp = FindObjectOfType<GameSettingsPersist>();
        myFile = new EasyFileSave("my_player_data");
        myFileGame = new EasyFileSave("my_game_data");
        //if (myFileGame.Load())
        //{
        //    ginProg = myFileGame.GetBool("In Progress");
        //    if (ginProg)
        //    {
        //        int currentEnd = myFileGame.GetInt("Current End");
        //        int endTotal = myFileGame.GetInt("End Total");

        //        gameInProg.SetActive(true);
        //        gameStats.text = "End " + currentEnd.ToString() + "/" + endTotal.ToString();
        //    }
        //    else
        //        gameInProg.SetActive(false);

        //    myFileGame.Dispose();
        //}
        //else
        //    gameInProg.SetActive(false);

        if (myFile.Load())
        {
            playerName = myFile.GetString("Player Name");
            teamName = myFile.GetString("Team Name");
            teamColour = myFile.GetUnityColor("Team Colour");
            cm.earnings = myFile.GetFloat("Career Earnings");
            earnings = cm.earnings;
            record = myFile.GetUnityVector2("Career Record");
            gsp.inProgress = myFile.GetBool("Tourny In Progress");
            Debug.Log("Tourny in Progress is " + myFile.GetBool("Tourny In Progress"));
            week = myFile.GetInt("Week");
            season = myFile.GetInt("Season");
            tourRecord = myFile.GetUnityVector2("Tour Record");

            cm.xp = myFile.GetFloat("XP");
            cm.totalXp = myFile.GetFloat("Total XP");
            cm.cStats.drawAccuracy = myFile.GetInt("Draw Accuracy");
            cm.cStats.takeOutAccuracy = myFile.GetInt("Take Out Accuracy");
            cm.cStats.guardAccuracy = myFile.GetInt("Guard Accuracy");
            cm.cStats.sweepStrength = myFile.GetInt("Sweep Strength");
            cm.cStats.sweepEndurance = myFile.GetInt("Sweep Endurance");
            cm.cStats.sweepHealth = myFile.GetFloat("Sweep Health");
            cm.cardIDList = myFile.GetArray<int>("Card ID List");
            //Vector2 tempRecord = myFile.GetUnityVector2("Career Record");
            //record = new Vector2Int((int)tempRecord.x, (int)tempRecord.y);

            if (gsp.inProgress)
            {
                tournyInProg.SetActive(true);
                gsp.KO = myFile.GetBool("Knockout Tourny");
                tournyNameLoad.text = myFile.GetString("Current Tourny Name");
                int draw = 1 + myFile.GetInt("Draw");
                int playoffRound = myFile.GetInt("Playoff Round");
                if (playoffRound > 0)
                    drawLoad.text = "Playoff Round " + playoffRound;
                else
                    drawLoad.text = "Draw " + draw;
            }
            else
                tournyInProg.SetActive(false);

            myFile.Dispose();
            yield return new WaitForEndOfFrame();

            gsp.careerLoad = true;
            nameLoad.text = playerName + " " + teamName;
            colourLoad.color = teamColour;
            weekLoad.text = "Week " + week.ToString();
            earningsLoad.text = "$" + cm.earnings.ToString("n0");
            recordLoad.text = record.x.ToString() + " - " + record.y.ToString();
            load.SetActive(true);
            player.SetActive(false);
            
        }
        else
        {
            Player();
        }
    }

    public void LoadToCM()
    {
        cm = FindObjectOfType<CareerManager>();
        cm.LoadSettings();

        if (gsp.inProgress)
        {
            if (ginProg)
            {
                SceneManager.LoadScene("End_Menu_Tourny_1");
            }
            else
            {
                if (gsp.KO)
                    SceneManager.LoadScene("Tourny_Home_3K");
                else
                    SceneManager.LoadScene("Tourny_Home_1");
            }
        }
        else
            SceneManager.LoadScene("Arena_Selector");

        
    }

    public void Player()
    {
        if (myFile.Load())
        {
            player.SetActive(false);
            load.SetActive(true);
            nextButton.text = "Continue>";
        }
        else
        {
            load.SetActive(false);
            player.SetActive(true);
            nextButton.text = "Start>";
            newButton.gameObject.SetActive(false);
        }
    }

    public void New()
    {
        ClearPlayer();
        cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();
        
        gsp.careerLoad = false;
        earnings = 0f;
        cm.earnings = 0f;
        record = Vector2.zero;
        week = 0;
        season = 0;
        gsp.draw = 0;
        gsp.playoffRound = 0;
        gsp.inProgress = false;
        cm.inProgress = false;

        load.SetActive(false);
        player.SetActive(true);
        nextButton.text = "Start>";
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
