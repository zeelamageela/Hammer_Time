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
    public TournySelector tSel;

    public Text tournyName;

    public int rocks;
    public int ends;
    public int teams;
    public int prize;
    public string format;
    public string location;
    public Image trophy;
    public int games;
    public float earnings;
    public float entryFee;

    public GameObject tournyGO;
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


    void Start()
    {
        cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();

        rockSlider.interactable = false;
        
        //else
        //    StartCoroutine(LoadFromFile());

    }

    // Update is called once per frame
    void Update()
    {
        if (settings.activeSelf)
        {
            if (endSlider.value <= 2)
                endSlider.value = 2;
            ends = (int)endSlider.value;

            if (rockSlider.value <= 2)
                gameSlider.value = 2;
            rocks = (int)rockSlider.value;

            if (gameSlider.value <= (gameSlider.minValue + 1))
                gameSlider.value = gameSlider.minValue + 1;
            games = (int)gameSlider.value;

            endText.text = ends.ToString();
            rockText.text = rocks.ToString();
            gameText.text = games.ToString();

        }
    }

    public void LoadToGSP()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        //if (!gsp.inProgress)
        //    earnings -= entryFee;

        cm.cash -= entryFee;

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
        if (gsp.KO3)
            SceneManager.LoadScene("Tourny_Home_3K");
        else if (gsp.KO1)
            SceneManager.LoadScene("Tourny_Home_SingleK");
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
        tournyGO.SetActive(true);
        if (cm)
        {
            gsp.tournyEarnings = 0;

            teams = cm.currentTournyTeams.Length;
            format = cm.currentTourny.format;
            entryFee = cm.currentTourny.entryFee;
            prize = cm.currentTourny.prizeMoney;
            trophy.sprite = cm.currentTourny.image;
            location = cm.currentTourny.location;
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
        //if (cm.week == 1)
        //{
        //    //Help();
        //}

        if (gsp.KO3 || gsp.KO1)
        {
            gameText.gameObject.SetActive(false);
            gameSlider.gameObject.SetActive(false);
            gameSlider.transform.parent.gameObject.SetActive(false);

        }
        else
        {
            if (teams > 12)
            {
                gameSlider.minValue = 4;
                games = 5;
            }
            else if (teams > 8)
            {
                gameSlider.minValue = 3;
                games = 4;
            }
            else
            {
                gameSlider.minValue = 2;
                games = 3;
            }
            gameSlider.maxValue = teams - 1;
        }
        tournyName.text = cm.currentTourny.name;
        tournyInfoText[0].text = teams.ToString();
        tournyInfoText[1].text = format;
        tournyInfoText[2].text = "$" + prize.ToString("n0");
        tournyInfoText[3].text = "$" + entryFee.ToString("n0");
        tournyInfoText[4].text = location;
        if (gsp.ends <= 0)
            ends = 2;
        else
            ends = gsp.ends;
        rocks = gsp.rocks;
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
            expandButton.gameObject.SetActive(false);
            settings.gameObject.SetActive(true);
        }
        else
        {
            expandAnim.SetBool("Expand", false);
            expandButton.gameObject.SetActive(true);
        }
    }

    IEnumerator WaitForTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        settings.SetActive(true);
    }
    public void Back()
    {
        cm.cash += cm.costPerWeek;
        for (int i = 0; i < cm.activeTournies.Length; i++)
        {
            cm.activeTournies[i].complete = false;

            if (cm.activeTournies[i].tour)
            {
                for (int j = 0; j < cm.tour.Length; j++)
                {
                    if (cm.activeTournies[i].id == cm.tour[j].id)
                        cm.tour[j].complete = false;
                }
            }
            else if (cm.activeTournies[i].qualifier)
            {
                for (int j = 0; j < cm.prov.Length; j++)
                {
                    if (cm.activeTournies[i].id == cm.prov[j].id)
                        cm.prov[j].complete = false;
                }
            }
            else if (cm.activeTournies[i].championship)
            {
                for (int j = 0; j < cm.champ.Length; j++)
                {
                    if (cm.activeTournies[i].id == cm.champ[j].id)
                        cm.champ[j].complete = false;
                }
            }
            else
            {
                for (int j = 0; j < cm.tournies.Length; j++)
                {
                    if (cm.activeTournies[i].id == cm.tournies[j].id)
                        cm.tournies[j].complete = false;
                }
            }
        }

        cm.currentTourny.complete = false;
        
        cm.SaveCareer();
        tournyGO.SetActive(false);

        tSel.SetActiveTournies();
        //SceneManager.LoadScene("Arena_Selector");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("SplashMenu");
    }
}
