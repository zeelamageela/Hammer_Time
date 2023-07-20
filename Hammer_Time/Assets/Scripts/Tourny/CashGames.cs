using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun.UtilityScripts;
using System.Linq;

[System.Serializable]
public class GameDisplay
{
    public Text topBar;

    public Text btnText;
    public Button btn;

    public Text oppName;
    public Text ends;
    public Text rank;
    public Image img;
}

[System.Serializable]
public class CashGamePlayers
{
    public string name;
    public int id;
    public int rank;
    public Sprite img;
}
public class CashGames : MonoBehaviour
{
    public GameObject selectGO;

    public CashGamePlayers[] opponents;

    public Text budgetVS;
    public Text highRollVS;
    public Text skinsVS;
    public Text selectVS;

    public GameDisplay[] budgetDisplay;
    public GameDisplay[] highRollDisplay;
    public GameDisplay[] skinsDisplay;
    public GameDisplay[] selectDisplay;

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

        vsPos = budgetVS.GetComponent<RectTransform>().anchoredPosition;
        //titlePos = budgetTitle.GetComponent<RectTransform>().anchoredPosition;
    }

    public void SetUp()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        CareerManager cm = FindObjectOfType<CareerManager>();

        List<Standings_List> tempTeam = new List<Standings_List>();

        Debug.Log("GSP teams " + gsp.teams.Length);

        if (gsp.tournyInProgress)
        {
            List<CashGamePlayers> tempCGP = new List<CashGamePlayers>();

            for (int i = 0; i < opponents.Length; i++)
            {
                for (int j = 0; j < gsp.cgp.Length; j++)
                {
                    if (gsp.cgp[j].id == opponents[i].id)
                    {
                        tempCGP.Add(opponents[i]);
                        break;
                    }
                }
            }

            for (int i = 0; i < opponents.Length; i++)
            {
                opponents[i] = tempCGP[i];
            }

            budgetDisplay[0].oppName.text = opponents[0].name;
            budgetDisplay[0].rank.text = opponents[0].rank.ToString();
            budgetDisplay[0].img.sprite = opponents[0].img;

            budgetDisplay[0].ends.text = budgetGames[0].format.ToString();
            budgetDisplay[0].topBar.text = "$" + budgetGames[0].prizeMoney.ToString("n0");

            highRollDisplay[0].oppName.text = opponents[1].name;
            highRollDisplay[0].rank.text = opponents[1].rank.ToString();
            highRollDisplay[0].img.sprite = opponents[1].img;

            highRollDisplay[0].ends.text = highRollGames[0].format.ToString();
            highRollDisplay[0].topBar.text = "$" + highRollGames[0].prizeMoney.ToString("n0");

            skinsDisplay[0].oppName.text = opponents[2].name;
            skinsDisplay[0].rank.text = opponents[2].rank.ToString();
            skinsDisplay[0].img.sprite = opponents[2].img;

            skinsDisplay[0].ends.text = skinsGames[0].format.ToString();
            skinsDisplay[0].topBar.text = "$" + skinsGames[0].prizeMoney.ToString("n0");

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
            Shuffle(opponents);
            budgetDisplay[0].oppName.text = opponents[0].name;
            budgetDisplay[0].rank.text = opponents[0].rank.ToString();
            budgetDisplay[0].img.sprite = opponents[0].img;
            budgetDisplay[0].btnText.text = "Cash Game";

            budgetDisplay[0].ends.text = budgetGames[0].format.ToString();

            if (budgetGames[0].prizeMoney > cm.cash)
            {
                budgetDisplay[0].btn.interactable = false;
                budgetDisplay[0].topBar.text = "Not Enough $";
                budgetDisplay[0].btnText.color -= new Color(0f, 0f, 0f, 0.75f);
                budgetDisplay[0].btn.image.color -= new Color(0f, 0f, 0f, 0.75f);
                budgetDisplay[0].img.color -= new Color(0f, 0f, 0f, 0.75f);
                
            }
            else
            {
                if (cm.cash > 1000)
                {
                    float d = cm.cash / 1000f;
                    int mult = Mathf.RoundToInt(d);
                    budgetGames[0].prizeMoney *= mult;
                }

                budgetDisplay[0].topBar.text = "$" + budgetGames[0].prizeMoney.ToString("n0");
            }

            highRollDisplay[0].oppName.text = opponents[1].name;
            highRollDisplay[0].rank.text = opponents[1].rank.ToString();
            highRollDisplay[0].img.sprite = opponents[1].img;
            highRollDisplay[0].btnText.text = "Cash Game";

            highRollDisplay[0].ends.text = highRollGames[0].format.ToString();

            if (highRollGames[0].prizeMoney > cm.cash)
            {
                if (cm.cash < 500)
                {
                    highRollDisplay[0].btn.interactable = false;
                    highRollDisplay[0].topBar.text = "Not Enough $";
                    highRollDisplay[0].btnText.color -= new Color(0f, 0f, 0f, 0.75f);
                    highRollDisplay[0].btn.image.color -= new Color(0f, 0f, 0f, 0.75f);
                    highRollDisplay[0].img.color -= new Color(0f, 0f, 0f, 0.75f);
                    
                }
                else
                {
                    highRollGames[0].prizeMoney = 500;
                    highRollDisplay[0].topBar.text = "$" + highRollGames[0].prizeMoney.ToString("n0");
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
                highRollDisplay[0].topBar.text = "$" + highRollGames[0].prizeMoney.ToString("n0");
            }

            skinsDisplay[0].oppName.text = opponents[2].name;
            skinsDisplay[0].rank.text = opponents[2].rank.ToString();
            skinsDisplay[0].img.sprite = opponents[2].img;
            skinsDisplay[0].btnText.text = "Skin Game";

            if (skinsGames[0].prizeMoney > cm.cash)
            {
                if (cm.cash < 2000)
                {
                    skinsDisplay[0].btn.interactable = false;
                    skinsDisplay[0].topBar.text = "Not Enough $";
                    skinsDisplay[0].btnText.color -= new Color(0f, 0f, 0f, 0.75f);
                    skinsDisplay[0].btn.image.color -= new Color(0f, 0f, 0f, 0.75f);
                    skinsDisplay[0].img.color -= new Color(0f, 0f, 0f, 0.75f);
                    
                }
                else
                {
                    skinsGames[0].prizeMoney = 2000;
                    skinsDisplay[0].topBar.text = "Skins - $" + skinsGames[0].prizeMoney + " total";
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
                skinsDisplay[0].topBar.text = "Skins - $" + skinsGames[0].prizeMoney + " total";
            }


            xp.text = cm.xp.ToString();
            costPerWeek.text = "$" + cm.costPerWeek.ToString();
            cash.text = "$" + cm.cash.ToString("n0");
        }

    }


    //public void OffsetText(int select, bool on)
    //{
    //    Vector2 offset = new Vector2(75f, 75f);

    //    if (on)
    //    {
    //        if (select == 0)
    //        {
    //            budgetDisplay[0].name.GetComponent<RectTransform>().anchoredPosition -= offset;
    //            budgetDisplay[1].name.GetComponent<RectTransform>().anchoredPosition -= offset;
    //            budgetDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition -= offset;

    //            budgetVS.GetComponent<RectTransform>().anchoredPosition -= offset;
    //            budgetTitle.GetComponent<RectTransform>().anchoredPosition -= offset;
    //            //selectTitle.text = budgetTitle.text;


    //        }
    //        if (select == 1)
    //        {
    //            highRollDisplay[0].name.GetComponent<RectTransform>().anchoredPosition -= offset;
    //            highRollDisplay[1].name.GetComponent<RectTransform>().anchoredPosition -= offset;
    //            highRollDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition -= offset;

    //            highRollVS.GetComponent<RectTransform>().anchoredPosition -= offset;
    //            highRollTitle.GetComponent<RectTransform>().anchoredPosition -= offset;
    //            //selectTitle.text = highRollTitle.text;
    //        }
    //        if (select == 2)
    //        {
    //            skinsDisplay[0].name.GetComponent<RectTransform>().anchoredPosition -= offset;
    //            skinsDisplay[1].name.GetComponent<RectTransform>().anchoredPosition -= offset;
    //            skinsDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition -= offset;

    //            skinsVS.GetComponent<RectTransform>().anchoredPosition -= offset;
    //            skinsTitle.GetComponent<RectTransform>().anchoredPosition -= offset;

    //            //selectTitle.text = skinsTitle.text;
    //        }
    //    }
    //    else
    //    {
    //        budgetDisplay[0].name.GetComponent<RectTransform>().anchoredPosition = name0Pos;
    //        budgetDisplay[1].name.GetComponent<RectTransform>().anchoredPosition = name1Pos;
    //        budgetDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition = rank1Pos;

    //        budgetVS.GetComponent<RectTransform>().anchoredPosition = vsPos;
    //        budgetTitle.GetComponent<RectTransform>().anchoredPosition = titlePos;

    //        highRollDisplay[0].name.GetComponent<RectTransform>().anchoredPosition = name0Pos;
    //        highRollDisplay[1].name.GetComponent<RectTransform>().anchoredPosition = name1Pos;
    //        highRollDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition = rank1Pos;

    //        highRollVS.GetComponent<RectTransform>().anchoredPosition = vsPos;
    //        highRollTitle.GetComponent<RectTransform>().anchoredPosition = titlePos;

    //        skinsDisplay[0].name.GetComponent<RectTransform>().anchoredPosition = name0Pos;
    //        skinsDisplay[1].name.GetComponent<RectTransform>().anchoredPosition = name1Pos;
    //        skinsDisplay[1].rank.GetComponent<RectTransform>().anchoredPosition = rank1Pos;

    //        skinsVS.GetComponent<RectTransform>().anchoredPosition = vsPos;
    //        skinsTitle.GetComponent<RectTransform>().anchoredPosition = titlePos;

    //    }

    //}

    public void OnSelectGame(int select)
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        CareerManager cm = FindObjectOfType<CareerManager>();
        btnSelect = select;
        //Vector2 offset = new Vector2(75f, 75f);
        //OffsetText(0, false);

        cm.cash -= gsp.cash;

        if (select == 0)
        {
            gsp.ends = budgetGames[0].teams;
            gsp.bg = budgetGames[0].BG;
            gsp.crowdDensity = budgetGames[0].crowdDensity;
            gsp.rocks = 2;
            gsp.prize = budgetGames[0].prizeMoney;

            selectDisplay[0].topBar.text = "$" + budgetGames[0].prizeMoney.ToString() + " Cash Game";
            selectDisplay[0].btnText.text = gsp.teamName;
            selectDisplay[0].oppName.text = budgetDisplay[0].oppName.text;
            selectDisplay[0].rank.text = "VS";

            //selectTitle.text = budgetTitle.text;
        }
        if (select == 1)
        {
            gsp.ends = highRollGames[0].teams;
            gsp.bg = highRollGames[0].BG;
            gsp.crowdDensity = highRollGames[0].crowdDensity;
            gsp.rocks = 2;
            gsp.prize = highRollGames[0].prizeMoney;

            selectDisplay[0].topBar.text = "$" + highRollGames[0].prizeMoney.ToString() + " Cash Game";
            selectDisplay[0].btnText.text = gsp.teamName;
            selectDisplay[0].oppName.text = highRollDisplay[0].oppName.text;
            selectDisplay[0].rank.text = "VS";

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

            selectDisplay[0].topBar.text = "$" + skinsGames[0].prizeMoney.ToString("n0") + " Skins Game";
            selectDisplay[0].btnText.text = gsp.teamName;
            selectDisplay[0].oppName.text = skinsDisplay[0].oppName.text;
            selectDisplay[0].rank.text = "VS";

            //selectTitle.text = skinsTitle.text;
        }

        gsp.cash = -gsp.prize;
        //OffsetText(select, true);
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
        //OffsetText(0, false);
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

        budgetDisplay[0].btn.interactable = false;
        highRollDisplay[0].btn.interactable = false;
        skinsDisplay[0].btn.interactable = false;

        selectGO.SetActive(true);
        selectDisplay[0].topBar.text = "Results";
        selectDisplay[0].rank.text = "wins";

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

                selectDisplay[0].oppName.text = gsp.teams[tm.playerTeam].name;
                selectDisplay[0].oppName.text = "$" + winnings.ToString("n0");
            }
            else
            {
                tm.heading.text = "You Win!";
                winnings = gsp.prize * 2f;
                //tm.teams[playerTeam].earnings = gsp.prize * 0.075f;
            }
            selectDisplay[0].btnText.text = gsp.teams[tm.playerTeam].name;
            selectDisplay[0].oppName.text = "$" + winnings.ToString("n0");

            gsp.cash = winnings;
        }
        else
        {
            tm.heading.text = "You Lose";

            selectDisplay[0].btnText.text = gsp.teams[tm.playerTeam].name;
            selectDisplay[0].oppName.text = "$0";
        }

        cash.text = "$" + FindObjectOfType<CareerManager>().cash.ToString("n0");

        tm.contButton.gameObject.SetActive(false);
        tm.pm.nextButton.gameObject.SetActive(true);
        
        playButton.gameObject.SetActive(false);
        tm.simButton.gameObject.SetActive(false);
    }

    void Shuffle(CashGamePlayers[] a)
    {
        // Loops through array
        for (int i = a.Length - 1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            CashGamePlayers temp = a[i];

            // Swap the new and old values
            a[i] = a[rnd];
            a[rnd] = temp;
        }

        // Print
        //PrintRows(a);
        //for (int i = 0; i < a.Length; i++)
        //{
        //	Print;
        //}
    }
}
