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

    public Text tournyName;

    public int rocks;
    public int ends;
    public int teams;
    public int prize;
    public int games;
    public float earnings;
    public float entryFee;

    public GameObject expandButton;
    public GameObject settings;
    public Slider gameSlider;
    public Text gameText;
    public Slider rockSlider;
    public Text rockText;
    public Slider endSlider;
    public Text endText;

    public Text[] tournyInfoText;

    public DialogueTrigger coachGreen;
    public GameObject dialogueGO;

    EasyFileSave myFile;

    void Start()
    {
        cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();

        rockSlider.interactable = false;
        if (cm)
        {
            gsp.earnings = 0;

            teams = cm.currentTournyTeams.Length;
            entryFee = cm.currentTourny.entryFee;
            prize = cm.currentTourny.prizeMoney;
            Settings();
        }

        if (gsp.cashGame)
        {
            endSlider.value = gsp.ends;
            rockSlider.value = 2;
            gameSlider.value = 1;
            endSlider.transform.parent.gameObject.SetActive(false);
            rockSlider.interactable = false;
            gameSlider.transform.parent.gameObject.SetActive(false);
        }
        //else
        //    StartCoroutine(LoadFromFile());

    }

    // Update is called once per frame
    void Update()
    {
        if (settings.activeSelf)
        {
            ends = (int)endSlider.value;
            rocks = (int)rockSlider.value;
            games = (int)gameSlider.value;

            endText.text = ends.ToString();
            rockText.text = rocks.ToString();
            gameText.text = games.ToString();

        }
    }

    IEnumerator LoadFromFile()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        myFile = new EasyFileSave("my_player_data");

        //gsp.earnings = cm.earnings;

        if (myFile.Load())
        {
            if (gsp.tournyInProgress)
            {
                prize = myFile.GetInt("Prize");
                teams = myFile.GetInt("Number Of Teams");
                ends = myFile.GetInt("Ends");
                rocks = myFile.GetInt("Rocks");

            }
            //Vector2 tempRecord = myFile.GetUnityVector2("Career Record");
            //record = new Vector2Int((int)tempRecord.x, (int)tempRecord.y);

            myFile.Dispose();
            yield return new WaitForEndOfFrame();
        }

    }

    public void LoadToGSP()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        //if (!gsp.inProgress)
        //    earnings -= entryFee;

        gsp.cash -= entryFee;

        //for (int i = 0; i < gsp.teams.Length; i++)
        //{
        //    gsp.teams[i].earnings -= entryFee;
        //}
        //Debug.Log("Career Earnings after Fee - $" + earnings);
        gsp.LoadTournySettings(this);

        //myFile = new EasyFileSave("my_player_data");

        //myFile.Add("Player Name", playerName);
        //myFile.Add("Team Name", teamName);
        //myFile.Add("Team Colour", teamColour);
        //myFile.Add("Career Earnings", career);
        //myFile.Add("Career Record", new Vector2(0, 0));

        //myFile.Save();
        if (gsp.KO)
            SceneManager.LoadScene("Tourny_Home_3K");
        else if (gsp.cashGame)
            SceneManager.LoadScene("Tourny_Home_2");
        else
            SceneManager.LoadScene("Tourny_Home_1");
    }

    public void Player()
    {

        settings.SetActive(false);
    }

    public void Settings()
    {
        cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();

        //if (cm.week == 1)
        //{
        //    //Help();
        //}

        if (gsp.KO)
        {
            gameText.gameObject.SetActive(false);
            gameSlider.gameObject.SetActive(false);
            gameSlider.transform.parent.gameObject.SetActive(false);

        }
        else
        {
            if (teams > 12)
            {
                gameSlider.minValue = 5;
                games = 5;
            }
            else if (teams > 8)
            {
                gameSlider.minValue = 4;
                games = 4;
            }
            else
            {
                gameSlider.minValue = 3;
                games = 3;
            }
            gameSlider.maxValue = teams - 1;
        }
        tournyName.text = cm.currentTourny.name;
        tournyInfoText[0].text = cm.currentTourny.teams.ToString();
        tournyInfoText[1].text = cm.currentTourny.format;
        tournyInfoText[2].text = "$" + cm.currentTourny.prizeMoney.ToString("n0");
        tournyInfoText[3].text = "$" + cm.currentTourny.entryFee.ToString("n0");
        ends = 2;
        rocks = 2;
        rockSlider.interactable = true;
        endSlider.interactable = true;
    }

    public void Help()
    {
        dialogueGO.SetActive(true);
        coachGreen.TriggerDialogue("Help", 0);
    }

    IEnumerator WaitForClick()
    {
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
        //Debug.Log("Clickeddd");
    }

    public void Expand(bool active)
    {
        Animator expandAnim = expandButton.GetComponent<Animator>();
        //Animator panelAnim = tournyName.transform.parent.gameObject.GetComponent<Animator>();

        if (active)
        {
            expandAnim.SetBool("Expand", true);
            expandButton.transform.GetChild(0).gameObject.SetActive(false);
            StartCoroutine(WaitForTime(0.35f));
        }
        else
        {
            expandAnim.SetBool("Expand", false);
            expandButton.transform.GetChild(0).gameObject.SetActive(true);
            settings.gameObject.SetActive(true);
        }
    }

    IEnumerator WaitForTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        settings.SetActive(true);
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
