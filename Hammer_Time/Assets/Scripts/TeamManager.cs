using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public GameManager gm;
    public SweeperManager sm;
    public GameSettingsPersist gsp;
    public CareerManager cm;

    public Color teamRedColour;
    public TeamMember[] teamRed;
    public Color teamYellowColour;
    public TeamMember[] teamYellow;

    public int[] activeSweeperL;
    public int[] activeSweeperR;

    int aiStats;
    bool playerRed;
    //public GameObject[] leadGO;
    //public GameObject[] secondGO;
    //public GameObject[] thirdGO;
    //public GameObject[] skipGO;

    // Start is called before the first frame update
    void Start()
    {
        cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();
        if (gsp.tourny)
        {
            Shuffle(teamRed);
            Shuffle(teamYellow);

            teamRedColour = gsp.redTeamColour;
            teamYellowColour = gsp.yellowTeamColour;

            if (gsp.week < 5)
                aiStats = 5;
            else if (gsp.week < 10)
                aiStats = 7;
            else
                aiStats = 10;
            Debug.Log("Ai Stats are " + aiStats + " in Week " + gsp.week);

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

    public void SetSweepers(CharacterStats sweeperL, CharacterStats sweeperR, CharacterStats sweeperT, int rockCurrent, bool aiTurn)
    {
        cm = FindObjectOfType<CareerManager>();

        if (aiTurn)
        {
            sweeperL.name = "AI Sweeper Left";
            sweeperR.name = "AI Sweeper Right";
            sweeperT.name = "AI Sweeper Tee";
            sweeperL.sweepStrength.SetBaseValue(aiStats + gsp.oppStats.sweepStrength);
            sweeperR.sweepStrength.SetBaseValue(aiStats + gsp.oppStats.sweepStrength);
            sweeperT.sweepStrength.SetBaseValue(aiStats + gsp.oppStats.sweepStrength);
            sweeperL.sweepEndurance.SetBaseValue(aiStats + gsp.oppStats.sweepEndurance);
            sweeperR.sweepEndurance.SetBaseValue(aiStats + gsp.oppStats.sweepEndurance);
            sweeperT.sweepEndurance.SetBaseValue(aiStats + gsp.oppStats.sweepEndurance);
            sweeperL.sweepCohesion.SetBaseValue(aiStats + gsp.oppStats.sweepCohesion);
            sweeperR.sweepCohesion.SetBaseValue(aiStats + gsp.oppStats.sweepCohesion);
            sweeperT.sweepCohesion.SetBaseValue(aiStats + gsp.oppStats.sweepCohesion);
        }
        else
        {
            if (rockCurrent > 11)
            {
                sweeperL.name = cm.activePlayers[1].name;
                sweeperR.name = cm.activePlayers[0].name;
                sweeperT.name = cm.activePlayers[2].name;
                sweeperL.sweepStrength.SetBaseValue(cm.activePlayers[1].sweepStrength);
                sweeperR.sweepStrength.SetBaseValue(cm.activePlayers[0].sweepStrength);
                sweeperT.sweepStrength.SetBaseValue(cm.activePlayers[2].sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(cm.activePlayers[1].sweepEnduro);
                sweeperR.sweepEndurance.SetBaseValue(cm.activePlayers[0].sweepEnduro);
                sweeperT.sweepEndurance.SetBaseValue(cm.activePlayers[2].sweepEnduro);
                sweeperL.sweepCohesion.SetBaseValue(cm.activePlayers[1].sweepCohesion);
                sweeperR.sweepCohesion.SetBaseValue(cm.activePlayers[0].sweepCohesion);
                sweeperT.sweepCohesion.SetBaseValue(cm.activePlayers[2].sweepCohesion);
            }
            else if (rockCurrent > 7)
            {
                sweeperL.name = cm.activePlayers[1].name;
                sweeperR.name = cm.activePlayers[0].name;
                sweeperT.name = cm.activePlayers[3].name;
                sweeperL.sweepStrength.SetBaseValue(cm.activePlayers[1].sweepStrength);
                sweeperR.sweepStrength.SetBaseValue(cm.activePlayers[0].sweepStrength);
                sweeperT.sweepStrength.SetBaseValue(cm.activePlayers[3].sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(cm.activePlayers[1].sweepEnduro);
                sweeperR.sweepEndurance.SetBaseValue(cm.activePlayers[0].sweepEnduro);
                sweeperT.sweepEndurance.SetBaseValue(cm.activePlayers[3].sweepEnduro);
                sweeperL.sweepCohesion.SetBaseValue(cm.activePlayers[1].sweepCohesion);
                sweeperR.sweepCohesion.SetBaseValue(cm.activePlayers[0].sweepCohesion);
                sweeperT.sweepCohesion.SetBaseValue(cm.activePlayers[3].sweepCohesion);
            }
            else if (rockCurrent > 3)
            {
                sweeperL.name = cm.activePlayers[2].name;
                sweeperR.name = cm.activePlayers[0].name;
                sweeperT.name = cm.activePlayers[3].name;
                sweeperL.sweepStrength.SetBaseValue(cm.activePlayers[2].sweepStrength);
                sweeperR.sweepStrength.SetBaseValue(cm.activePlayers[0].sweepStrength);
                sweeperT.sweepStrength.SetBaseValue(cm.activePlayers[3].sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(cm.activePlayers[2].sweepEnduro);
                sweeperR.sweepEndurance.SetBaseValue(cm.activePlayers[0].sweepEnduro);
                sweeperT.sweepEndurance.SetBaseValue(cm.activePlayers[3].sweepEnduro);
                sweeperL.sweepCohesion.SetBaseValue(cm.activePlayers[2].sweepCohesion);
                sweeperR.sweepCohesion.SetBaseValue(cm.activePlayers[0].sweepCohesion);
                sweeperT.sweepCohesion.SetBaseValue(cm.activePlayers[3].sweepCohesion);
            }
            else
            {
                sweeperL.name = cm.activePlayers[1].name;
                sweeperR.name = cm.activePlayers[2].name;
                sweeperT.name = cm.activePlayers[3].name;
                sweeperL.sweepStrength.SetBaseValue(cm.activePlayers[1].sweepStrength);
                sweeperR.sweepStrength.SetBaseValue(cm.activePlayers[2].sweepStrength);
                sweeperT.sweepStrength.SetBaseValue(cm.activePlayers[3].sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(cm.activePlayers[1].sweepEnduro);
                sweeperR.sweepEndurance.SetBaseValue(cm.activePlayers[2].sweepEnduro);
                sweeperT.sweepEndurance.SetBaseValue(cm.activePlayers[3].sweepEnduro);
                sweeperL.sweepCohesion.SetBaseValue(cm.activePlayers[1].sweepCohesion);
                sweeperR.sweepCohesion.SetBaseValue(cm.activePlayers[2].sweepCohesion);
                sweeperT.sweepCohesion.SetBaseValue(cm.activePlayers[3].sweepCohesion);
            }
        }
    }
    public void SetCharacter(int rockCurrent, bool redTurn)
    {
        CareerManager cm = FindObjectOfType<CareerManager>();

        if (redTurn)
        {
            for (int i = 0; i < teamRed.Length; i++)
            {
                teamRed[i].shooter.GetComponent<CharColourChanger>().TeamColour(teamRedColour);
            }
            if (gsp.redTeamName == gsp.teamName)
            {
                for (int j = 0; j < teamRed.Length; j++)
                {
                    if (j < 3)
                    {
                        teamRed[j].charStats.drawAccuracy.SetBaseValue(cm.activePlayers[j].draw);
                        teamRed[j].charStats.takeOutAccuracy.SetBaseValue(cm.activePlayers[j].takeOut);
                        teamRed[j].charStats.guardAccuracy.SetBaseValue(cm.activePlayers[j].guard);
                        teamRed[j].charStats.sweepStrength.SetBaseValue(cm.activePlayers[j].sweepStrength);
                        teamRed[j].charStats.sweepEndurance.SetBaseValue(cm.activePlayers[j].sweepEnduro);
                        teamRed[j].charStats.sweepCohesion.SetBaseValue(cm.activePlayers[j].sweepCohesion);
                    }
                    else
                    {
                        teamRed[j].charStats.drawAccuracy.SetBaseValue(gsp.cStats.drawAccuracy);
                        teamRed[j].charStats.takeOutAccuracy.SetBaseValue(gsp.cStats.takeOutAccuracy);
                        teamRed[j].charStats.guardAccuracy.SetBaseValue(gsp.cStats.guardAccuracy);
                        teamRed[j].charStats.sweepStrength.SetBaseValue(gsp.cStats.sweepStrength);
                        teamRed[j].charStats.sweepEndurance.SetBaseValue(gsp.cStats.sweepEndurance);
                        teamRed[j].charStats.sweepCohesion.SetBaseValue(gsp.cStats.sweepCohesion);
                    }
                    //Debug.Log("Red Turn PLAYER stats " + j + " - "
                    //+ teamRed[j].charStats.guardAccuracy.GetValue());
                }
            }
            else
            {
                for (int j = 0; j < teamRed.Length; j++)
                {
                    teamRed[j].charStats.drawAccuracy.SetBaseValue(aiStats + gsp.oppStats.drawAccuracy);
                    teamRed[j].charStats.takeOutAccuracy.SetBaseValue(aiStats + gsp.oppStats.takeOutAccuracy);
                    teamRed[j].charStats.guardAccuracy.SetBaseValue(aiStats + gsp.oppStats.guardAccuracy);
                    teamRed[j].charStats.sweepStrength.SetBaseValue(aiStats + gsp.oppStats.sweepStrength);
                    teamRed[j].charStats.sweepEndurance.SetBaseValue(aiStats + gsp.oppStats.sweepEndurance);
                    teamRed[j].charStats.sweepCohesion.SetBaseValue(aiStats + gsp.oppStats.sweepCohesion);
                    //Debug.Log("Red Turn AI stats " + j + " - "
                    //+ teamRed[j].charStats.guardAccuracy.GetValue());
                }
            }
        }
        else
        {
            for (int i = 0; i < teamYellow.Length; i++)
            {
                teamYellow[i].shooter.GetComponent<CharColourChanger>().TeamColour(teamYellowColour);
            }
            if (gsp.yellowTeamName == gsp.teamName)
            {
                for (int j = 0; j < teamYellow.Length; j++)
                {
                    if (j < 3)
                    {
                        teamYellow[j].charStats.drawAccuracy.SetBaseValue(cm.activePlayers[j].draw);
                        teamYellow[j].charStats.takeOutAccuracy.SetBaseValue(cm.activePlayers[j].takeOut);
                        teamYellow[j].charStats.guardAccuracy.SetBaseValue(cm.activePlayers[j].guard);
                        teamYellow[j].charStats.sweepStrength.SetBaseValue(cm.activePlayers[j].sweepStrength);
                        teamYellow[j].charStats.sweepEndurance.SetBaseValue(cm.activePlayers[j].sweepEnduro);
                        teamYellow[j].charStats.sweepCohesion.SetBaseValue(cm.activePlayers[j].sweepCohesion);
                    }
                    else
                    {
                        teamYellow[j].charStats.drawAccuracy.SetBaseValue(gsp.cStats.drawAccuracy);
                        teamYellow[j].charStats.takeOutAccuracy.SetBaseValue(gsp.cStats.takeOutAccuracy);
                        teamYellow[j].charStats.guardAccuracy.SetBaseValue(gsp.cStats.guardAccuracy);
                        teamYellow[j].charStats.sweepStrength.SetBaseValue(gsp.cStats.sweepStrength);
                        teamYellow[j].charStats.sweepEndurance.SetBaseValue(gsp.cStats.sweepEndurance);
                        teamYellow[j].charStats.sweepCohesion.SetBaseValue(gsp.cStats.sweepCohesion);
                    }
                    //Debug.Log("Yellow Turn PLAYER stats " + j + " - "
                    //+ teamYellow[j].charStats.guardAccuracy.GetValue());
                }
            }
            else
            {
                for (int j = 0; j < teamYellow.Length; j++)
                {
                    teamYellow[j].charStats.drawAccuracy.SetBaseValue(aiStats + gsp.oppStats.drawAccuracy);
                    teamYellow[j].charStats.takeOutAccuracy.SetBaseValue(aiStats + gsp.oppStats.takeOutAccuracy);
                    teamYellow[j].charStats.guardAccuracy.SetBaseValue(aiStats + gsp.oppStats.guardAccuracy);
                    teamYellow[j].charStats.sweepStrength.SetBaseValue(aiStats + gsp.oppStats.sweepStrength);
                    teamYellow[j].charStats.sweepEndurance.SetBaseValue(aiStats + gsp.oppStats.sweepEndurance);
                    teamYellow[j].charStats.sweepCohesion.SetBaseValue(aiStats + gsp.oppStats.sweepCohesion);
                    //Debug.Log("Yellow Turn AI stats " + j + " - "
                    //+ teamYellow[j].charStats.guardAccuracy.GetValue());
                }
            }
        }
        if (rockCurrent < 4)
        {
            sm.sweeperRedL = teamRed[1].sweeperL;
            sm.sweeperRedR = teamRed[2].sweeperR;
            sm.sweeperRedTee = teamRed[3].sweeperL;
            sm.sweeperYellowL = teamYellow[1].sweeperL;
            sm.sweeperYellowR = teamYellow[2].sweeperR;
            sm.sweeperYellowTee = teamYellow[3].sweeperL;
            gm.shooterAnimRed = teamRed[0].shooter;
            gm.shooterAnimYellow = teamYellow[0].shooter;
        }
        else if (rockCurrent < 8)
        {
            sm.sweeperRedL = teamRed[0].sweeperL;
            sm.sweeperRedR = teamRed[2].sweeperR;
            sm.sweeperRedTee = teamRed[3].sweeperL;
            sm.sweeperYellowL = teamYellow[0].sweeperL;
            sm.sweeperYellowR = teamYellow[2].sweeperR;
            sm.sweeperYellowTee = teamYellow[3].sweeperL;
            gm.shooterAnimRed = teamRed[1].shooter;
            gm.shooterAnimYellow = teamYellow[1].shooter;
        }
        else if (rockCurrent < 12)
        {
            sm.sweeperRedL = teamRed[0].sweeperL;
            sm.sweeperRedR = teamRed[1].sweeperR;
            sm.sweeperRedTee = teamRed[3].sweeperL;
            sm.sweeperYellowL = teamYellow[0].sweeperL;
            sm.sweeperYellowR = teamYellow[1].sweeperR;
            sm.sweeperYellowTee = teamYellow[3].sweeperL;
            gm.shooterAnimRed = teamRed[2].shooter;
            gm.shooterAnimYellow = teamYellow[2].shooter;
        }
        else
        {
            sm.sweeperRedL = teamRed[0].sweeperL;
            sm.sweeperRedR = teamRed[1].sweeperR;
            sm.sweeperRedTee = teamRed[2].sweeperL;
            sm.sweeperYellowL = teamYellow[0].sweeperL;
            sm.sweeperYellowR = teamYellow[1].sweeperR;
            sm.sweeperYellowTee = teamYellow[2].sweeperL;
            gm.shooterAnimRed = teamRed[3].shooter;
            gm.shooterAnimYellow = teamYellow[3].shooter;
        }
    }
}
