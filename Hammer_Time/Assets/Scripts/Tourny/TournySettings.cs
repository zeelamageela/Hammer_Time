using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TigerForge;

public class TournySettings : MonoBehaviour
{
    GameSettingsPersist gsp;
    CareerManager cm;

    public string playerName;
    public string teamName;
    public int rocks;
    public int ends;
    public int teams;
    public int prize;
    public float earnings;
    public float entryFee;

    public GameObject settings;
    public GameObject player;
    public GameObject load;
    public InputField playerNameInput;
    public InputField teamNameInput;
    public Text nameLoad;
    public Image colourLoad;
    public Slider teamsSlider;
    public Text teamsText;
    public Slider prizeSlider;
    public Text prizeText;
    public Slider rockSlider;
    public Text rockText;
    public Slider endSlider;
    public Text endText;
    public Text entryFeeText;
    public Slider teamColourSlider;
    public Image teamHandleSlider;
    public Color teamColour;

    public Text earningsLoad;
    public Text recordLoad;
    public Vector2 record;
    Gradient gradient;
    public GameObject tournyInProg;
    public Text drawLoad;
    public Text rankLoad;

    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    EasyFileSave myFile;

    void Start()
    {
        cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();

        if (cm && cm.inProgress)
        {
            StartCoroutine(LoadCareer());
            Settings();
        }
        else
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
        if (settings.activeSelf)
        {
            ends = (int)endSlider.value;
            rocks = (int)rockSlider.value;
            teams = (int)teamsSlider.value * 2;
            prize = (int)prizeSlider.value * 10000;

            endText.text = ends.ToString();
            rockText.text = rocks.ToString();
            teamsText.text = teams.ToString();
            prizeText.text = "$" + prize.ToString();

            if (!gsp.inProgress)
            {
                //entryFee = Mathf.RoundToInt((prize)/ (10f * teams));
                entryFeeText.fontSize = 100;
                entryFeeText.text = "Entry Fee - $" + entryFee.ToString();
            }
            else
            {
                entryFeeText.fontSize = 200;

                entryFeeText.text = "Load Tourny";
            }
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

    IEnumerator LoadCareer()
    {
        playerName = cm.playerName;
        teamName = cm.teamName;
        teamColour = cm.teamColour;
        record = cm.record;
        earnings = cm.earnings;
        teams = cm.currentTournyTeams.Length;
        entryFee = cm.currentTourny.entryFee;
        prize = cm.currentTourny.prizeMoney;

        if (gsp.inProgress)
        {
            //StartCoroutine(LoadFromFile());
        }
        else
        {
            load.SetActive(false);
            player.SetActive(false);
            settings.SetActive(true);
            
        }
        yield break;
    }

    IEnumerator LoadFromFile()
    {
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

            if (gsp.inProgress)
            {
                //tournyInProg.SetActive(true);
                //if (myFile.GetInt("Playoff Round") > 0)
                //{
                //    drawLoad.text = "Playoffs";
                //}
                //else
                //    drawLoad.text = "Draw " + (myFile.GetInt("Draw") + 1).ToString();

                prize = myFile.GetInt("Prize");
                teams = myFile.GetInt("Number Of Teams");
                ends = myFile.GetInt("Ends");
                rocks = myFile.GetInt("Rocks");

                //int[] rankList = myFile.GetArray<int>("Teams Rank");
                //int playerTeam = myFile.GetInt("Player Team");
                //if (rankList[playerTeam] == 1)
                //    rankLoad.text = "1st Place";
                //else if (rankList[playerTeam] == 2)
                //    rankLoad.text = "2nd Place";
                //else if (rankList[playerTeam] == 3)
                //    rankLoad.text = "3rd Place";
                //else
                //    rankLoad.text = rankList[playerTeam] + "th Place";

            }
            else
            {
                tournyInProg.SetActive(false);

            }
            //Vector2 tempRecord = myFile.GetUnityVector2("Career Record");
            //record = new Vector2Int((int)tempRecord.x, (int)tempRecord.y);

            myFile.Dispose();
            yield return new WaitForEndOfFrame();
            //gsp.careerLoad = true;
            load.SetActive(true);
            player.SetActive(false);
            nameLoad.text = playerName + " " + teamName;
            colourLoad.color = teamColour;
            earningsLoad.text = "$" + earnings.ToString();
            recordLoad.text = record.x.ToString() + " - " + record.y.ToString();
        }
        else
        {
            player.SetActive(true);
            load.SetActive(false);
        }
        tournyInProg.SetActive(false);

    }
    public void LoadToGSP()
    {
        gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
        //if (!gsp.inProgress)
        //    earnings -= entryFee;
        for (int i = 0; i < gsp.teams.Length; i++)
        {
            gsp.teams[i].earnings -= entryFee;
        }
        Debug.Log("Career Earnings after Fee - $" + earnings);
        gsp.LoadTournySettings();

        //myFile = new EasyFileSave("my_player_data");

        //myFile.Add("Player Name", playerName);
        //myFile.Add("Team Name", teamName);
        //myFile.Add("Team Colour", teamColour);
        //myFile.Add("Career Earnings", career);
        //myFile.Add("Career Record", new Vector2(0, 0));

        //myFile.Save();
        SceneManager.LoadScene("Tourny_Home_1");
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

        settings.SetActive(false);
    }

    public void Settings()
    {
        cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();
        if (cm && cm.inProgress)
        {
            teamsSlider.value = cm.currentTournyTeams.Length / 2f;
            teamsSlider.interactable = false;
            prizeSlider.value = cm.currentTourny.prizeMoney / 10000f;
            prizeSlider.interactable = false;
        }
        else if (gsp.inProgress)
        {
            Debug.Log("Settings In Progress teams is " + rocks);
            rockSlider.value = rocks;
            rockSlider.interactable = false;
            teamsSlider.value = teams / 2f;
            teamsSlider.interactable = false;
            endSlider.value = ends;
            endSlider.interactable = false;
            prizeSlider.value = prize / 10000f;
            prizeSlider.interactable = false;
        }
        else
        {
            teamsSlider.value = Random.Range(3, 9);
            prizeSlider.value = Random.Range(1, 16);
            rockSlider.interactable = true;
            teamsSlider.interactable = true;
            endSlider.interactable = true;
            prizeSlider.interactable = true;
            Debug.Log("Settings not In Progress teams is " + teamsSlider.value);
            Debug.Log("Settings not In Progress prize is " + prizeSlider.value);
        }
        load.SetActive(false);
        settings.SetActive(true);
        player.SetActive(false);

        
    }

    public void New()
    {
        cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();

        ClearPlayer();
        gsp.careerLoad = false;
        earnings = 0f;
        
        record = Vector2.zero;
        gsp.draw = 0;
        gsp.playoffRound = 0;
        gsp.careerLoad = false;
        gsp.inProgress = false;
        load.SetActive(false);
        settings.SetActive(false);
        player.SetActive(true);
    }

    public void Back()
    {
        SceneManager.LoadScene("Arena_Selector");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("SplashMenu");
    }

    public void ClearPlayer()
    {
        myFile.Delete();
    }
}
