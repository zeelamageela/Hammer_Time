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
        cm = FindFirstObjectByType<CareerManager>();
        gsp = FindFirstObjectByType<GameSettingsPersist>();
        if (gsp.tourny)
        {
            Shuffle(teamRed);
            Shuffle(teamYellow);

            teamRedColour = gsp.redTeamColour;
            teamYellowColour = gsp.yellowTeamColour;

            //if (gsp.week < 5)
            //    aiStats = 5;
            //else if (gsp.week < 10)
            //    aiStats = 7;
            //else
                aiStats = 10;
            Debug.Log("Ai Stats are " + aiStats + " in Week " + cm.week);

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
        // CRITICAL FIX: Add null checks
        if (gm == null || gm.rockList == null || gm.rockList.Count == 0)
            return;
        
        // Rest of the Update code is commented out anyway, so just return
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
        cm = FindFirstObjectByType<CareerManager>();

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
                sweeperT.name = cm.playerName + " " + cm.teamName;
                sweeperL.sweepStrength.SetBaseValue(cm.activePlayers[1].sweepStrength);
                sweeperR.sweepStrength.SetBaseValue(cm.activePlayers[0].sweepStrength);
                sweeperT.sweepStrength.SetBaseValue(cm.cStats.sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(cm.activePlayers[1].sweepEnduro);
                sweeperR.sweepEndurance.SetBaseValue(cm.activePlayers[0].sweepEnduro);
                sweeperT.sweepEndurance.SetBaseValue(cm.cStats.sweepEndurance);
                sweeperL.sweepCohesion.SetBaseValue(cm.activePlayers[1].sweepCohesion);
                sweeperR.sweepCohesion.SetBaseValue(cm.activePlayers[0].sweepCohesion);
                sweeperT.sweepCohesion.SetBaseValue(cm.cStats.sweepCohesion);
            }
            else if (rockCurrent > 3)
            {
                sweeperL.name = cm.activePlayers[2].name;
                sweeperR.name = cm.activePlayers[0].name;
                sweeperT.name = cm.playerName + " " + cm.teamName;
                sweeperL.sweepStrength.SetBaseValue(cm.activePlayers[2].sweepStrength);
                sweeperR.sweepStrength.SetBaseValue(cm.activePlayers[0].sweepStrength);
                sweeperT.sweepStrength.SetBaseValue(cm.cStats.sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(cm.activePlayers[2].sweepEnduro);
                sweeperR.sweepEndurance.SetBaseValue(cm.activePlayers[0].sweepEnduro);
                sweeperT.sweepEndurance.SetBaseValue(cm.cStats.sweepEndurance);
                sweeperL.sweepCohesion.SetBaseValue(cm.activePlayers[2].sweepCohesion);
                sweeperR.sweepCohesion.SetBaseValue(cm.activePlayers[0].sweepCohesion);
                sweeperT.sweepCohesion.SetBaseValue(cm.cStats.sweepCohesion);
            }
            else
            {
                sweeperL.name = cm.activePlayers[1].name;
                sweeperR.name = cm.activePlayers[2].name;
                sweeperT.name = cm.playerName + " " + cm.teamName;
                sweeperL.sweepStrength.SetBaseValue(cm.activePlayers[1].sweepStrength);
                sweeperR.sweepStrength.SetBaseValue(cm.activePlayers[2].sweepStrength);
                sweeperT.sweepStrength.SetBaseValue(cm.cStats.sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(cm.activePlayers[1].sweepEnduro);
                sweeperR.sweepEndurance.SetBaseValue(cm.activePlayers[2].sweepEnduro);
                sweeperT.sweepEndurance.SetBaseValue(cm.cStats.sweepEndurance);
                sweeperL.sweepCohesion.SetBaseValue(cm.activePlayers[1].sweepCohesion);
                sweeperR.sweepCohesion.SetBaseValue(cm.activePlayers[2].sweepCohesion);
                sweeperT.sweepCohesion.SetBaseValue(cm.cStats.sweepCohesion);
            }
        }
    }
    public void SetCharacter(int rockCurrent, bool redTurn)
    {
        if (redTurn)
        {
            for (int i = 0; i < teamRed.Length; i++)
            {
                teamRed[i].shooter.GetComponent<CharColourChanger>().TeamColour(teamRedColour);
            }

            for (int j = 0; j < teamRed.Length; j++)
            {
                teamRed[j].charStats.drawAccuracy.SetBaseValue(gsp.redTeam.players[j].draw);
                teamRed[j].charStats.takeOutAccuracy.SetBaseValue(gsp.redTeam.players[j].takeOut);
                teamRed[j].charStats.guardAccuracy.SetBaseValue(gsp.redTeam.players[j].guard);
                teamRed[j].charStats.sweepStrength.SetBaseValue(gsp.redTeam.players[j].sweepStrength);
                teamRed[j].charStats.sweepEndurance.SetBaseValue(gsp.redTeam.players[j].sweepEnduro);
                teamRed[j].charStats.sweepCohesion.SetBaseValue(gsp.redTeam.players[j].sweepCohesion);
                //Debug.Log("Red Turn PLAYER stats " + j + " - "
                //+ teamRed[j].charStats.guardAccuracy.GetValue());
            }
        }
        else
        {
            for (int i = 0; i < teamYellow.Length; i++)
            {
                teamYellow[i].shooter.GetComponent<CharColourChanger>().TeamColour(teamYellowColour);
            }

            for (int j = 0; j < teamYellow.Length; j++)
            {
                teamYellow[j].charStats.drawAccuracy.SetBaseValue(gsp.yellowTeam.players[j].draw);
                teamYellow[j].charStats.takeOutAccuracy.SetBaseValue(gsp.yellowTeam.players[j].takeOut);
                teamYellow[j].charStats.guardAccuracy.SetBaseValue(gsp.yellowTeam.players[j].guard);
                teamYellow[j].charStats.sweepStrength.SetBaseValue(gsp.yellowTeam.players[j].sweepStrength);
                teamYellow[j].charStats.sweepEndurance.SetBaseValue(gsp.yellowTeam.players[j].sweepEnduro);
                teamYellow[j].charStats.sweepCohesion.SetBaseValue(gsp.yellowTeam.players[j].sweepCohesion);
                //Debug.Log("Yellow Turn PLAYER stats " + j + " - "
                //+ teamYellow[j].charStats.guardAccuracy.GetValue());
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
            
            // Call OnShoot for the active shooter (AI only)
            if (redTurn && gm.aiTeamRed)
                teamRed[0].charStats.OnShoot();
            else if (!redTurn && gm.aiTeamYellow)
                teamYellow[0].charStats.OnShoot();
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
            
            // Call OnShoot for the active shooter (AI only)
            if (redTurn && gm.aiTeamRed)
                teamRed[1].charStats.OnShoot();
            else if (!redTurn && gm.aiTeamYellow)
                teamYellow[1].charStats.OnShoot();
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
            
            // Call OnShoot for the active shooter (AI only)
            if (redTurn && gm.aiTeamRed)
                teamRed[2].charStats.OnShoot();
            else if (!redTurn && gm.aiTeamYellow)
                teamYellow[2].charStats.OnShoot();
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
            
            // Call OnShoot for the active shooter (AI only)
            if (redTurn && gm.aiTeamRed)
                teamRed[3].charStats.OnShoot();
            else if (!redTurn && gm.aiTeamYellow)
                teamYellow[3].charStats.OnShoot();
        }
    }
}
