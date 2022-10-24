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

        if (am != null)
            am.PlayBG(3);

        cm.SetUpCareer();
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
            //Debug.Log("Earnings are " + earnings);
            record = myFile.GetUnityVector2("Career Record");
            gsp.inProgress = myFile.GetBool("Tourny In Progress");
            //Debug.Log("Tourny in Progress is " + myFile.GetBool("Tourny In Progress"));
            week = myFile.GetInt("Week");
            season = myFile.GetInt("Season");
            tourRecord = myFile.GetUnityVector2("Tour Record");

            cm.xp = myFile.GetFloat("XP");
            cm.totalXp = myFile.GetFloat("Total XP");
            //Vector2 tempRecord = myFile.GetUnityVector2("Career Record");
            //record = new Vector2Int((int)tempRecord.x, (int)tempRecord.y);

            int[] activePlayersId = myFile.GetArray<int>("Active Players ID List");
            string[] activePlayersName = myFile.GetArray<string>("Active Players Name List");

            int[] playerDrawList = myFile.GetArray<int>("Active Players Draw List");
            int[] playerGuardList = myFile.GetArray<int>("Active Players Guard List");
            int[] playerTakeoutList = myFile.GetArray<int>("Active Players Takeout List");
            int[] playerStrengthList = myFile.GetArray<int>("Active Players Strength List");
            int[] playerEnduroList = myFile.GetArray<int>("Active Players Endurance List");
            int[] playerCohesionList = myFile.GetArray<int>("Active Players Cohesion List");

            int[] playerOppDrawList = myFile.GetArray<int>("Active Players Opp Draw List");
            int[] playerOppGuardList = myFile.GetArray<int>("Active Players Opp Guard List");
            int[] playerOppTakeoutList = myFile.GetArray<int>("Active Players Opp Takeout List");
            int[] playerOppStrengthList = myFile.GetArray<int>("Active Players Opp Strength List");
            int[] playerOppEnduroList = myFile.GetArray<int>("Active Players Opp Endurance List");
            int[] playerOppCohesionList = myFile.GetArray<int>("Active Players Opp Cohesion List");

            //Debug.Log("Player Id List Length - " + activePlayersId.Length);
            //Debug.Log("Player Name 3 - " + activePlayersName[2]);

            //cm.activePlayers = new Player[3];

            for (int i = 0; i < cm.activePlayers.Length; i++)
            {
                //Debug.Log("Setting Active Player " + i);
                cm.activePlayers[i].name = activePlayersName[i];
                cm.activePlayers[i].id = activePlayersId[i];


                cm.activePlayers[i].draw = playerDrawList[i];
                cm.activePlayers[i].guard = playerGuardList[i];
                cm.activePlayers[i].takeOut = playerTakeoutList[i];
                cm.activePlayers[i].sweepStrength = playerStrengthList[i];
                cm.activePlayers[i].sweepEnduro = playerEnduroList[i];
                cm.activePlayers[i].sweepCohesion = playerCohesionList[i];

                cm.activePlayers[i].oppDraw = playerOppDrawList[i];
                cm.activePlayers[i].oppGuard = playerOppGuardList[i];
                cm.activePlayers[i].oppTakeOut = playerOppTakeoutList[i];
                cm.activePlayers[i].oppStrength = playerOppStrengthList[i];
                cm.activePlayers[i].oppEnduro = playerOppEnduroList[i];
                cm.activePlayers[i].oppCohesion = playerOppCohesionList[i];
            }

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
        
        gsp.careerLoad = false;
        record = Vector2.zero;
        week = 0;
        season = 0;
        cm.cash = 1000f;
        gsp.draw = 0;
        gsp.playoffRound = 0;
        gsp.inProgress = false;
        cm.inProgress = false;

        load.SetActive(false);
        player.SetActive(true);
        nextButton.text = "Start>";

        NameGenerator();
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

    void NameGenerator()
    {
        int first = Random.Range(0, 10);
        string[] name1 = {"JJ", "Scrap", "Trabbitha", "Greecy", "Treep", "Cherp", "Glimp", "Jam", "Cray", "Stint" };

        int syllables = Random.Range(0, 3);

        int lasta = Random.Range(0, 10);
        string[] name2a = { "Griff", "Stamp", "Gloob", "Frist", "Jum", "Stoff", "Well", "Trink", "Gust", "Stoob" };

        int lastb = Random.Range(0, 10);
        string[] name2b = { "", "on", "son", "len", "ler", "lun", "in", "or", "le", "ly" };

        int lastc = Random.Range(0, 10);
        string[] name2c = { "", "sen", "rov", "witz", "vich", "ter", "vun", "brun", "son", "bing" };

        if (syllables == 0)
        {
            lastb = 0;
            lastc = 0;
        }
        else if (syllables == 1)
        {
            lastc = 0;
        }

        playerNameInput.text = name1[first];
        teamNameInput.text = name2a[lasta] + name2b[lastb] + name2c[lastc];
    }
}
