using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    int btnSelect;
    // Start is called before the first frame update
    void Start()
    {
        selectGO.SetActive(false);
        playButton.gameObject.SetActive(false);
        simButton.gameObject.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUp()
    {
        budgetDisplay[0].name.text = budgetGames[0].format;

        budgetDisplay[1].name.text = budgetGames[0].name;
        budgetDisplay[1].rank.text = "-";

        budgetTitle.text = "$" + budgetGames[0].prizeMoney.ToString("n0");

        highRollDisplay[0].name.text = highRollGames[0].format;

        highRollDisplay[1].name.text = highRollGames[0].name;
        highRollDisplay[1].rank.text = "-";

        highRollTitle.text = "$" + highRollGames[0].prizeMoney.ToString("n0");


        skinsDisplay[0].name.text = skinsGames[0].format;

        skinsDisplay[1].name.text = skinsGames[0].name;
        skinsDisplay[1].rank.text = "-";

        skinsTitle.text = "Skins - $" + skinsGames[0].prizeMoney.ToString("n0");
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
            budgetDisplay[0].name.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            budgetDisplay[1].name.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            budgetDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            budgetVS.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            budgetTitle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            highRollDisplay[0].name.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            highRollDisplay[1].name.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            highRollDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            highRollVS.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            highRollTitle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            skinsDisplay[0].name.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            skinsDisplay[1].name.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            skinsDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            skinsVS.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            skinsTitle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        }

    }

    public void OnSelectGame(int select)
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        btnSelect = select;
        Vector2 offset = new Vector2(75f, 75f);
        OffsetText(0, false);
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

            OffsetText(select, true);
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
            OffsetText(select, true);

        }
        if (select == 2)
        {
            gsp.ends = skinsGames[0].teams;
            gsp.bg = skinsGames[0].BG;
            gsp.crowdDensity = skinsGames[0].crowdDensity;
            gsp.rocks = 2;
            gsp.prize = skinsGames[0].prizeMoney;

            selectDisplay[0].name.text = skinsDisplay[0].name.text;

            selectDisplay[1].name.text = skinsDisplay[1].name.text;
            selectDisplay[1].rank.text = skinsDisplay[1].rank.text;

            OffsetText(select, true);
            //selectTitle.text = skinsTitle.text;
        }

        selectGO.SetActive(true);
        simButton.gameObject.SetActive(true);
        playButton.gameObject.SetActive(true);
    }

    public void PlayDraw()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();

        gsp.TournySetup();
        SceneManager.LoadScene("End_Menu_Tourny_1");
    }

    public void OnSim()
    {
        //playoffRound = pm.playoffRound;

        SimGame();
    }

    void SimGame()
    {
        CareerManager cm = FindObjectOfType<CareerManager>();
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        //SetDraw();

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
        gsp.playerTeam.strength = Mathf.RoundToInt(strength / 24f);

        if (Random.Range(0, gsp.playerTeam.strength) < Random.Range(0, 10))
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
                winnings += 1000f;

                if (gsp.playerTeam.strength > Random.Range(0, 10))
                {
                    winnings += 1000f;
                }
                if (gsp.playerTeam.strength > Random.Range(0, 10))
                {
                    winnings += 3000f;
                }

                selectDisplay[0].name.text = gsp.teams[tm.playerTeam].name;
                selectDisplay[1].name.text = "$" + winnings.ToString("n0");
                selectDisplay[1].rank.gameObject.SetActive(false);
            }
            else
            {
                tm.heading.text = "Win!";
                gsp.cash += gsp.prize * 2f;
                //tm.teams[playerTeam].earnings = gsp.prize * 0.075f;

                selectDisplay[0].name.text = gsp.teams[tm.playerTeam].name;
                selectDisplay[1].name.text = "$" + (gsp.prize * 2f).ToString("n0");
                selectDisplay[1].rank.gameObject.SetActive(false);
            }
        }
        else
        {
            tm.heading.text = "Lose";
            gsp.cash -= gsp.prize;

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
