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
    public AudioManager am;

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
        am = FindObjectOfType<AudioManager>();
        cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();
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

        if (am != null)
            am.PlayBG(3);

        cm.LoadCareer();
        Player(!cm.gameOver);
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


    public void LoadToCM()
    {
        cm = FindObjectOfType<CareerManager>();
        cm.LoadSettings();
        ginProg = gsp.loadGame;

        if (gsp.tournyInProgress)
        {
            if (ginProg)
            {
                SceneManager.LoadScene("TournyGame");
            }
            else
            {
                if (gsp.KO3)
                    SceneManager.LoadScene("Tourny_Home_3K");
                else if (gsp.KO1)
                    SceneManager.LoadScene("Tourny_Home_SingleK");
                else
                    SceneManager.LoadScene("Tourny_Home_1");
            }
        }
        else
            SceneManager.LoadScene("Arena_Selector");

        
    }

    public void Player(bool loadPlayer)
    {
        if (loadPlayer)
        { 
            playerName = cm.playerName;
            teamName = cm.teamName;
            teamColour = cm.teamColour;
            earnings = cm.earnings;
            //Debug.Log("Earnings are " + earnings);
            record = cm.record;
            Debug.Log("Tourny in Progress is " + gsp.tournyInProgress);
            week = cm.week;
            season = cm.season;
            tourRecord = cm.tourRecord;


            if (gsp.tournyInProgress)
            {
                tournyInProg.SetActive(true);
                tournyNameLoad.text = cm.currentTourny.name;
                int draw = 1 + gsp.draw;
                int playoffRound = gsp.playoffRound;

                if (gsp.KO3)
                    drawLoad.text = "Triple KO - Round " + playoffRound;
                else if (gsp.KO1)
                    drawLoad.text = "Single KO - Round " + playoffRound;
                else if (playoffRound > 0)
                    drawLoad.text = "Playoffs - Round " + playoffRound;
                else
                    drawLoad.text = "Draw " + draw;
            }
            else
                tournyInProg.SetActive(false);

            gsp.careerLoad = true;
            nameLoad.text = playerName + " " + teamName;
            colourLoad.color = teamColour;
            weekLoad.text = "Week " + week.ToString();
            earningsLoad.text = "$" + earnings.ToString("n0");
            recordLoad.text = record.x.ToString() + " - " + record.y.ToString();
            load.SetActive(true);

            player.SetActive(false); 
            if (cm.gameOver)
            {
                weekLoad.text = "Game Over!";
                nextButton.transform.parent.gameObject.SetActive(false);
            }

            player.SetActive(false);
            load.SetActive(true);
            nextButton.text = "Continue>";
        }
        else
        {
            New();
            //load.SetActive(false);
            //player.SetActive(true);
            //nextButton.text = "Start>";
            newButton.gameObject.SetActive(false);
        }
    }

    public void New()
    {
        ClearPlayer();
        cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();

        cm.gameOver = false;
        nextButton.transform.parent.gameObject.SetActive(true);

        gsp.careerLoad = false;
        record = Vector2.zero;
        week = 0;
        season = 0;
        cm.cash = 1000f;
        gsp.draw = 0;
        gsp.playoffRound = 0;
        gsp.tournyInProgress = false;
        cm.inProgress = false;

        cm.activeEquipID = null;
        cm.inventoryID = null;

        load.SetActive(false);
        player.SetActive(true);
        nextButton.text = "Start>";

        playerNameInput.text = cm.playerName;
        teamNameInput.text = cm.teamName;

        //NameGenerator();
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
        {
            cm.playerName = myFile.GetString("Player Name");
            cm.teamName = myFile.GetString("Team Name");
            myFile.Delete();
        }
    }

    public void NameGenerator()
    {
        int first = Random.Range(0, 11);
        string[] name1 = {"JJ", "Scrap", "Trabbitha", "Greezy", "Treep", "Cherp", "Glimp", "Jam", "Craw", "Stint", "Arugula" };

        int syllables = Random.Range(0, 3);

        int pref = Random.Range(0, 14);
        string[] name2pref = { "O'", "de ", "de la ", "Mc" };

        int lasta = Random.Range(0, 14);
        string[] name2a = { "Griff", "Stamp", "Gloob", "Frist", "Jum", "Stoff", "Wel", "Tank", "Gus", "Stoob", "Lol", "Sen", "Thun", "Hel", "Cleev" };

        int lastb = Random.Range(0, 11);
        string[] name2b = { "il", "ity", "on", "son", "len", "ler", "lun", "in", "or", "le", "ly" };

        int lastc = Random.Range(0, 13);
        string[] name2c = { "", "ty", "sen", "rov", "werk", "lova", "ter", "vun", "brun", "son", "bing", "ich", "eux" };

        Debug.Log("Syllables" + syllables + " - pref " + pref);

        if (syllables == 0)
        {
            if (pref <= 3)
                teamNameInput.text = name2pref[pref] + name2a[lasta];
            else 
                teamNameInput.text = name2a[lasta];
        }
        else if (syllables == 1)
        {
            if (pref <= 3)
                teamNameInput.text = name2pref[pref] + name2a[lasta] + name2b[lastb];
            else
                teamNameInput.text = name2a[lasta] + name2b[lastb];
        }
        else if (syllables == 2)
        {
            if (pref <= 3)
                teamNameInput.text = name2pref[pref] + name2a[lasta] + name2b[lastb] + name2c[lastc];
            else
                teamNameInput.text = name2a[lasta] + name2b[lastb] + name2c[lastc];
        }

        playerNameInput.text = name1[first];
    }
}
