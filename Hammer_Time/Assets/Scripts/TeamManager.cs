using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public GameManager gm;
    public SweeperManager sm;
    public GameSettingsPersist gsp;

    public Color teamRedColour;
    public TeamMember[] teamRed;
    public Color teamYellowColour;
    public TeamMember[] teamYellow;

    bool playerRed;
    //public GameObject[] leadGO;
    //public GameObject[] secondGO;
    //public GameObject[] thirdGO;
    //public GameObject[] skipGO;

    // Start is called before the first frame update
    void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        if (gsp.tourny)
        {
            Shuffle(teamRed);
            Shuffle(teamYellow);
            for (int i = 0; i < teamRed.Length; i++)
            {
                //teamRed[i].charStats.drawAccuracy = gsp.cStats.drawAccuracy;
            }
            teamRedColour = gsp.redTeamColour;
            teamYellowColour = gsp.yellowTeamColour;

            

        }
        else
        {
            teamRedColour = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            teamYellowColour = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            Shuffle(teamRed);
            Shuffle(teamYellow);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.rockList.Count != 0)
        {
            #region Target
            //if (gm.rockCurrent <= 11)
            //{
            //    if (gm.redHammer && gm.rockList[gm.rockCurrent].rockInfo.teamName == gm.rockList[1].rockInfo.teamName)
            //    {
            //        if (!gm.aiTeamRed)
            //            gm.target = true;
            //        else
            //            gm.target = false;
            //    }
            //    else if (gm.redHammer && gm.rockList[gm.rockCurrent].rockInfo.teamName == gm.rockList[0].rockInfo.teamName)
            //    {
            //        if (!gm.aiTeamYellow)
            //            gm.target = true;
            //        else
            //            gm.target = false;
            //    }
            //    else if (!gm.redHammer && gm.rockList[gm.rockCurrent].rockInfo.teamName == gm.rockList[0].rockInfo.teamName)
            //    {
            //        if (!gm.aiTeamRed)
            //            gm.target = true;
            //        else
            //            gm.target = false;
            //    }
            //    else if (!gm.redHammer && gm.rockList[gm.rockCurrent].rockInfo.teamName == gm.rockList[1].rockInfo.teamName)
            //    {
            //        if (!gm.aiTeamYellow)
            //            gm.target = true;
            //        else
            //            gm.target = false;
            //    }
            //    else
            //        gm.target = false;
            //}
            //else
            //    gm.target = false;

            //if (gm.rockCurrent <= 3)
            //{
            //    gm.shooterAnimRed = leadGO[0];
            //    gm.shooterAnimYellow = leadGO[1];

            //}
            //else if (gm.rockCurrent <= 7)
            //{
            //    gm.shooterAnimRed = secondGO[0];
            //    gm.shooterAnimYellow = secondGO[1];
            //}
            //else if (gm.rockCurrent <= 11)
            //{
            //    gm.shooterAnimRed = thirdGO[0];
            //    gm.shooterAnimYellow = thirdGO[1];
            //}
            //else
            //{
            //    gm.shooterAnimRed = skipGO[0];
            //    gm.shooterAnimYellow = skipGO[1];
            //}
            #endregion
        }


    }

    void Shuffle(TeamMember[] a)
    {
        // Loops through array
        for (int i = a.Length - 1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            TeamMember temp = a[i];

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

    public void SetCharacter(int rockCurrent, bool redTurn)
    {
        
        if (redTurn)
        {
            for (int i = 0; i < teamRed.Length; i++)
            {
                teamRed[i].shooter.GetComponent<CharColourChanger>().TeamColour(teamRedColour);
            }
            if (teamRedColour == gsp.teamColour)
            {
                for (int j = 0; j < teamRed.Length; j++)
                {
                    teamRed[j].charStats.drawAccuracy.SetBaseValue(gsp.cStats.drawAccuracy);
                    teamRed[j].charStats.takeOutAccuracy.SetBaseValue(gsp.cStats.takeOutAccuracy);
                    teamRed[j].charStats.guardAccuracy.SetBaseValue(gsp.cStats.guardAccuracy);
                    teamRed[j].charStats.sweepStrength.SetBaseValue(gsp.cStats.sweepStrength);
                    teamRed[j].charStats.sweepEndurance.SetBaseValue(gsp.cStats.sweepEndurance);
                    teamRed[j].charStats.sweepHealth = gsp.cStats.sweepHealth;
                    Debug.Log("Red Turn PLAYER stats - "
                        + teamRed[j].charStats.guardAccuracy.GetValue());
                }
            }
            else
            {
                for (int j = 0; j < teamRed.Length; j++)
                {
                    teamRed[j].charStats.drawAccuracy.SetBaseValue(10);
                    teamRed[j].charStats.takeOutAccuracy.SetBaseValue(10);
                    teamRed[j].charStats.guardAccuracy.SetBaseValue(10);
                    teamRed[j].charStats.sweepStrength.SetBaseValue(10);
                    teamRed[j].charStats.sweepEndurance.SetBaseValue(10);
                    teamRed[j].charStats.sweepHealth = 100;
                    Debug.Log("Red Turn AI stats - "
                        + teamRed[j].charStats.guardAccuracy.GetValue());
                }
            }
        }
        else
        {
            for (int i = 0; i < teamYellow.Length; i++)
            {
                teamYellow[i].shooter.GetComponent<CharColourChanger>().TeamColour(teamYellowColour);

            }
            if (teamYellowColour == gsp.teamColour)
            {
                for (int j = 0; j < teamYellow.Length; j++)
                {
                    teamYellow[j].charStats.drawAccuracy.SetBaseValue(gsp.cStats.drawAccuracy);
                    teamYellow[j].charStats.takeOutAccuracy.SetBaseValue(gsp.cStats.takeOutAccuracy);
                    teamYellow[j].charStats.guardAccuracy.SetBaseValue(gsp.cStats.guardAccuracy);
                    teamYellow[j].charStats.sweepStrength.SetBaseValue(gsp.cStats.sweepStrength);
                    teamYellow[j].charStats.sweepEndurance.SetBaseValue(gsp.cStats.sweepEndurance);
                    teamYellow[j].charStats.sweepHealth = gsp.cStats.sweepHealth;
                    Debug.Log("Yellow Turn Player stats - "
                        + teamYellow[j].charStats.guardAccuracy.GetValue());
                }
            }
            else
            {
                for (int j = 0; j < teamYellow.Length; j++)
                {
                    teamYellow[j].charStats.drawAccuracy.SetBaseValue(10);
                    teamYellow[j].charStats.takeOutAccuracy.SetBaseValue(10);
                    teamYellow[j].charStats.guardAccuracy.SetBaseValue(10);
                    teamYellow[j].charStats.sweepStrength.SetBaseValue(10);
                    teamYellow[j].charStats.sweepEndurance.SetBaseValue(10);
                    teamYellow[j].charStats.sweepHealth = 100;
                    Debug.Log("Yellow Turn AI stats - "
                         + teamYellow[j].charStats.guardAccuracy.GetValue());
                }
            }
        }
        if (rockCurrent < 4)
        {
            sm.sweeperRedL = teamRed[1].sweeperL;
            sm.sweeperRedR = teamRed[2].sweeperR;
            sm.sweeperYellowL = teamYellow[1].sweeperL;
            sm.sweeperYellowR = teamYellow[2].sweeperR;
            gm.shooterAnimRed = teamRed[0].shooter;
            gm.shooterAnimYellow = teamYellow[0].shooter;
        }
        else if (rockCurrent < 8)
        {
            sm.sweeperRedL = teamRed[0].sweeperL;
            sm.sweeperRedR = teamRed[2].sweeperR;
            sm.sweeperYellowL = teamYellow[0].sweeperL;
            sm.sweeperYellowR = teamYellow[2].sweeperR;
            gm.shooterAnimRed = teamRed[1].shooter;
            gm.shooterAnimYellow = teamYellow[1].shooter;
        }
        else if (rockCurrent < 12)
        {
            sm.sweeperRedL = teamRed[0].sweeperL;
            sm.sweeperRedR = teamRed[1].sweeperR;
            sm.sweeperYellowL = teamYellow[0].sweeperL;
            sm.sweeperYellowR = teamYellow[1].sweeperR;
            gm.shooterAnimRed = teamRed[2].shooter;
            gm.shooterAnimYellow = teamYellow[2].shooter;
        }
        else
        {
            sm.sweeperRedL = teamRed[0].sweeperL;
            sm.sweeperRedR = teamRed[1].sweeperR;
            sm.sweeperYellowL = teamYellow[0].sweeperL;
            sm.sweeperYellowR = teamYellow[1].sweeperR;
            gm.shooterAnimRed = teamRed[3].shooter;
            gm.shooterAnimYellow = teamYellow[3].shooter;
        }
    }
}
