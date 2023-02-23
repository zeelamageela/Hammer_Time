using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun.UtilityScripts;
using System.Linq;

public class CashGames : MonoBehaviour
{

    public Text budgetTitle;
    public Text highRollTitle;
    public Text skinsTitle;
    public Text selectTitle;
    public GameObject selectGO;

    public Text budgetVS;
    public Text highRollVS;
    public Text skinsVS;
    public Text selectVS;

    public BracketDisplay[] budgetDisplay;
    public BracketDisplay[] highRollDisplay;
    public BracketDisplay[] skinsDisplay;
    public BracketDisplay[] selectDisplay;

    public Tourny[] budgetGames;
    public Tourny[] highRollGames;
    public Tourny[] skinsGames;

    public Button playButton;
    public Button simButton;

    public Text xp;
    public Text costPerWeek;
    public Text cash;

    Vector2 rank1Pos;
    Vector2 name0Pos;
    Vector2 name1Pos;
    Vector2 vsPos;
    Vector2 titlePos;

    int btnSelect;


    // Start is called before the first frame update
    void Start()
    {
        selectGO.SetActive(false);
        playButton.gameObject.SetActive(false);
        simButton.gameObject.SetActive(false);

        name0Pos = budgetDisplay[0].name.GetComponent<RectTransform>().anchoredPosition;
        name1Pos = budgetDisplay[1].name.GetComponent<RectTransform>().anchoredPosition;
        rank1Pos = budgetDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition;

        vsPos = budgetVS.GetComponent<RectTransform>().anchoredPosition;
        titlePos = budgetTitle.GetComponent<RectTransform>().anchoredPosition;
    }

    public void SetUp()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        CareerManager cm = FindObjectOfType<CareerManager>();

        List<Standings_List> tempTeam = new List<Standings_List>();

        Debug.Log("GSP teams " + gsp.teams.Length);

        if (gsp.tournyInProgress)
        {
            budgetDisplay[1].name.text = gsp.teams[1].name;
            budgetDisplay[1].rank.text = gsp.teams[1].rank.ToString();
            budgetTitle.text = "$" + budgetGames[0].prizeMoney.ToString("n0");
            budgetDisplay[0].bg.GetComponent<Button>().interactable = false;

            highRollDisplay[1].name.text = gsp.teams[2].name;
            highRollDisplay[1].rank.text = gsp.teams[2].rank.ToString();
            highRollDisplay[0].name.text = highRollGames[0].format;
            highRollDisplay[0].bg.GetComponent<Button>().interactable = false;

            skinsDisplay[1].name.text = gsp.teams[1].name;
            skinsDisplay[1].rank.text = gsp.teams[1].rank.ToString();
            skinsDisplay[0].name.text = skinsGames[0].format;
            skinsDisplay[0].bg.GetComponent<Button>().interactable = false;

            //btnSelect
            if (gsp.aiRed)
            {
                if (gsp.redScore > gsp.yellowScore)
                    GameScoring(false);
                else
                    GameScoring(true);
            }
            else
            {
                if (gsp.yellowScore > gsp.redScore)
                    GameScoring(false);
                else
                    GameScoring(true);
            }
        }
        else
        {
            budgetDisplay[0].name.text = budgetGames[0].format;
            budgetDisplay[1].name.text = gsp.teams[1].name;
            budgetDisplay[1].rank.text = gsp.teams[1].rank.ToString();

            if (budgetGames[0].prizeMoney > cm.cash)
            {
                budgetDisplay[0].bg.GetComponent<Button>().interactable = false;
                budgetTitle.text = "Not Enough $";
                budgetDisplay[0].name.color -= new Color(0f, 0f, 0f, 0.75f);
                budgetDisplay[1].name.color -= new Color(0f, 0f, 0f, 0.75f);
                budgetDisplay[1].rank.color -= new Color(0f, 0f, 0f, 0.75f);
                
            }
            else
            {
                if (cm.cash > 1000)
                {
                    float d = cm.cash / 1000f;
                    int mult = Mathf.RoundToInt(d);
                    budgetGames[0].prizeMoney *= mult;
                }

                budgetTitle.text = "$" + budgetGames[0].prizeMoney.ToString("n0");
            }


            highRollDisplay[0].name.text = highRollGames[0].format;
            highRollDisplay[1].name.text = gsp.teams[2].name;
            highRollDisplay[1].rank.text = gsp.teams[2].rank.ToString();

            if (highRollGames[0].prizeMoney > cm.cash)
            {
                if (cm.cash < 500)
                {
                    highRollDisplay[0].bg.GetComponent<Button>().interactable = false;
                    highRollTitle.text = "Not Enough $";
                    highRollDisplay[0].name.color -= new Color(0f, 0f, 0f, 0.75f);
                    highRollDisplay[1].name.color -= new Color(0f, 0f, 0f, 0.75f);
                    highRollDisplay[1].rank.color -= new Color(0f, 0f, 0f, 0.75f);
                    
                }
                else
                {
                    highRollGames[0].prizeMoney = 500;
                    highRollTitle.text = "$" + highRollGames[0].prizeMoney.ToString("n0");
                }
            }
            else
            {
                if (cm.cash > 5000)
                {
                    float d = cm.cash / 5000f;
                    int mult = Mathf.RoundToInt(d);
                    highRollGames[0].prizeMoney *= mult;
                }
                highRollTitle.text = "$" + highRollGames[0].prizeMoney.ToString("n0");
            }


            skinsDisplay[0].name.text = skinsGames[0].format;
            skinsDisplay[1].name.text = gsp.teams[3].name;
            skinsDisplay[1].rank.text = gsp.teams[3].rank.ToString();

            if (skinsGames[0].prizeMoney > cm.cash)
            {
                if (cm.cash < 2000)
                {
                    skinsDisplay[0].bg.GetComponent<Button>().interactable = false;
                    skinsTitle.text = "Not Enough $";
                    skinsDisplay[0].name.color -= new Color(0f, 0f, 0f, 0.75f);
                    skinsDisplay[1].name.color -= new Color(0f, 0f, 0f, 0.75f);
                    skinsDisplay[1].rank.color -= new Color(0f, 0f, 0f, 0.75f);
                    
                }
                else
                {
                    skinsGames[0].prizeMoney = 2000;
                    skinsTitle.text = "Skins - $" + skinsGames[0].prizeMoney + " total";
                }
            }
            else
            {
                if (cm.cash > 10000)
                {
                    float d = cm.cash / 10000f;
                    int mult = Mathf.RoundToInt(d);
                    skinsGames[0].prizeMoney *= mult;
                }
                skinsTitle.text = "Skins - $" + skinsGames[0].prizeMoney + " total";
            }


            xp.text = cm.xp.ToString();
            costPerWeek.text = "$" + cm.costPerWeek.ToString();
            cash.text = "$" + cm.cash.ToString("n0");
        }

    }

    public void OffsetText(int select, bool on)
    {
        Vector2 offset = new Vector2(75f, 75f);

        if (on)
        {
            if (select == 0)
            {
                budgetDisplay[0].name.GetComponent<RectTransform>().anchoredPosition -= offset;
                budgetDisplay[1].name.GetComponent<RectTransform>().anchoredPosition -= offset;
                budgetDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition -= offset;

                budgetVS.GetComponent<RectTransform>().anchoredPosition -= offset;
                budgetTitle.GetComponent<RectTransform>().anchoredPosition -= offset;
                //selectTitle.text = budgetTitle.text;


            }
            if (select == 1)
            {
                highRollDisplay[0].name.GetComponent<RectTransform>().anchoredPosition -= offset;
                highRollDisplay[1].name.GetComponent<RectTransform>().anchoredPosition -= offset;
                highRollDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition -= offset;

                highRollVS.GetComponent<RectTransform>().anchoredPosition -= offset;
                highRollTitle.GetComponent<RectTransform>().anchoredPosition -= offset;
                //selectTitle.text = highRollTitle.text;
            }
            if (select == 2)
            {
                skinsDisplay[0].name.GetComponent<RectTransform>().anchoredPosition -= offset;
                skinsDisplay[1].name.GetComponent<RectTransform>().anchoredPosition -= offset;
                skinsDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition -= offset;

                skinsVS.GetComponent<RectTransform>().anchoredPosition -= offset;
                skinsTitle.GetComponent<RectTransform>().anchoredPosition -= offset;

                //selectTitle.text = skinsTitle.text;
            }
        }
        else
        {
            budgetDisplay[0].name.GetComponent<RectTransform>().anchoredPosition = name0Pos;
            budgetDisplay[1].name.GetComponent<RectTransform>().anchoredPosition = name1Pos;
            budgetDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition = rank1Pos;

            budgetVS.GetComponent<RectTransform>().anchoredPosition = vsPos;
            budgetTitle.GetComponent<RectTransform>().anchoredPosition = titlePos;

            highRollDisplay[0].name.GetComponent<RectTransform>().anchoredPosition = name0Pos;
            highRollDisplay[1].name.GetComponent<RectTransform>().anchoredPosition = name1Pos;
            highRollDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition = rank1Pos;

            highRollVS.GetComponent<RectTransform>().anchoredPosition = vsPos;
            highRollTitle.GetComponent<RectTransform>().anchoredPosition = titlePos;

            skinsDisplay[0].name.GetComponent<RectTransform>().anchoredPosition = name0Pos;
            skinsDisplay[1].name.GetComponent<RectTransform>().anchoredPosition = name1Pos;
            skinsDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition = rank1Pos;

            skinsVS.GetComponent<RectTransform>().anchoredPosition = vsPos;
            skinsTitle.GetComponent<RectTransform>().anchoredPosition = titlePos;

        }

    }

    public void OnSelectGame(int select)
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        CareerManager cm = FindObjectOfType<CareerManager>();
        btnSelect = select;
        //Vector2 offset = new Vector2(75f, 75f);
        OffsetText(0, false);

        cm.cash -= gsp.cash;

        if (select == 0)
        {
            gsp.ends = budgetGames[0].teams;
            gsp.bg = budgetGames[0].BG;
            gsp.crowdDensity = budgetGames[0].crowdDensity;
            gsp.rocks = 2;
            gsp.prize = budgetGames[0].prizeMoney;

            selectDisplay[0].name.text = budgetDisplay[0].name.text;

            selectDisplay[1].name.text = budgetDisplay[1].name.text;
            selectDisplay[1].rank.text = budgetDisplay[1].rank.text;

            //selectTitle.text = budgetTitle.text;
        }
        if (select == 1)
        {
            gsp.ends = highRollGames[0].teams;
            gsp.bg = highRollGames[0].BG;
            gsp.crowdDensity = highRollGames[0].crowdDensity;
            gsp.rocks = 2;
            gsp.prize = highRollGames[0].prizeMoney;

            selectDisplay[0].name.text = highRollDisplay[0].name.text;

            selectDisplay[1].name.text = highRollDisplay[1].name.text;
            selectDisplay[1].rank.text = highRollDisplay[1].rank.text;

            //selectTitle.text = highRollTitle.text;
        }
        if (select == 2)
        {
            gsp.ends = skinsGames[0].teams;
            gsp.bg = skinsGames[0].BG;
            gsp.crowdDensity = skinsGames[0].crowdDensity;
            gsp.rocks = 2;
            gsp.skinsGame = true;
            gsp.skins = 0;
            gsp.prize = skinsGames[0].prizeMoney;
            gsp.skinValue = new float[3] { gsp.prize * 0.5f, gsp.prize * 0.5f, gsp.prize };

            selectDisplay[0].name.text = skinsDisplay[0].name.text;

            selectDisplay[1].name.text = skinsDisplay[1].name.text;
            selectDisplay[1].rank.text = skinsDisplay[1].rank.text;

            //selectTitle.text = skinsTitle.text;
        }

        gsp.cash = -gsp.prize;
        OffsetText(select, true);
        cm.cash += gsp.cash;
        xp.text = cm.xp.ToString();
        costPerWeek.text = "$" + cm.costPerWeek.ToString();
        cash.text = "$" + cm.cash.ToString("n0");

        selectGO.SetActive(true);
        simButton.gameObject.SetActive(true);
        playButton.gameObject.SetActive(true);
    }

    public void PlayGame()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();

        gsp.cash -= gsp.prize;
        gsp.TournySetup(btnSelect);
        SceneManager.LoadScene("End_Menu_Tourny_1");
    }

    public void OnSim()
    {
        //playoffRound = pm.playoffRound;
        OffsetText(0, false);
        SimGame();
    }
    
    void SimGame()
    {
        CareerManager cm = FindObjectOfType<CareerManager>();
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        //SetDraw();

        gsp.cash -= gsp.prize;

        float strength = cm.cStats.drawAccuracy
            + cm.cStats.takeOutAccuracy
            + cm.cStats.guardAccuracy
            + cm.cStats.sweepStrength
            + cm.cStats.sweepEndurance
            + cm.cStats.sweepCohesion
            + cm.modStats.drawAccuracy
            + cm.modStats.takeOutAccuracy
            + cm.modStats.guardAccuracy
            + cm.modStats.sweepStrength
            + cm.modStats.sweepEndurance
            + cm.modStats.sweepCohesion;
        gsp.playerTeam.strength = Mathf.RoundToInt(strength / 12f);

        float oppStrength = cm.oppStats.drawAccuracy
            + cm.oppStats.takeOutAccuracy
            + cm.oppStats.guardAccuracy
            + cm.oppStats.sweepStrength
            + cm.oppStats.sweepEndurance
            + cm.oppStats.sweepCohesion;
        oppStrength = Mathf.RoundToInt(oppStrength / 6f);

        if (Random.Range(0, gsp.playerTeam.strength) < Random.Range(0, oppStrength))
        {
            gsp.playerTeam.loss++;
            gsp.record.y++;
            GameScoring(false);
        }
        else
        {
            gsp.playerTeam.wins++;
            gsp.record.x++;
            GameScoring(true);
        }

        Debug.Log("Career Record is " + gsp.record.x + " - " + gsp.record.y);
        //yield return new WaitForEndOfFrame();
    }

    void GameScoring(bool win)
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        TournyManager tm = GetComponent<TournyManager>();
        //Debug.Log("Final End");

        selectGO.SetActive(true);
        selectTitle.text = "Results";
        selectVS.text = "wins";

        if (win)
        {
            float winnings = 0f;
            if (btnSelect == 2)
            {
                winnings += gsp.skinValue[0];

                if (gsp.playerTeam.strength > Random.Range(0, 10))
                {
                    winnings += gsp.skinValue[1];
                }
                if (gsp.playerTeam.strength > Random.Range(0, 10))
                {
                    winnings += gsp.skinValue[2];
                }

                selectDisplay[0].name.text = gsp.teams[tm.playerTeam].name;
                selectDisplay[1].name.text = "$" + winnings.ToString("n0");
                selectDisplay[1].rank.gameObject.SetActive(false);
            }
            else
            {
                tm.heading.text = "You Win!";
                winnings = gsp.prize * 2f;
                //tm.teams[playerTeam].earnings = gsp.prize * 0.075f;
            }
            selectDisplay[1].rank.gameObject.SetActive(false);
            selectDisplay[0].name.text = gsp.teams[tm.playerTeam].name;
            selectDisplay[1].name.text = "$" + winnings.ToString("n0");

            gsp.cash = winnings;
        }
        else
        {
            tm.heading.text = "You Lose";

            selectDisplay[0].name.text = gsp.teams[tm.playerTeam].name;
            selectDisplay[1].name.text = "$0";
            selectDisplay[1].rank.gameObject.SetActive(false);
        }

        
        tm.contButton.gameObject.SetActive(false);
        tm.pm.nextButton.gameObject.SetActive(true);
        
        playButton.gameObject.SetActive(false);
        tm.simButton.gameObject.SetActive(false);
    }

}
