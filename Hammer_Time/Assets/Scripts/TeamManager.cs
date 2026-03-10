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
            // AI's turn - they're shooting, so we need AI stats for their sweepers
            // Find opponent team's stats
            Team opponentTeam = null;
            for (int i = 0; i < cm.currentTournyTeams.Length; i++)
            {
                if (cm.currentTournyTeams[i].name == cm.playerTeam.nextOpp)
                {
                    opponentTeam = cm.currentTournyTeams[i];
                    // Use team average sweep strength as base AI stats
                    aiStats = opponentTeam.sweepStrength;
                    break;
                }
            }
            
            // NULL CHECK: If opponent team not found, use fallback
            if (opponentTeam == null || opponentTeam.players == null || opponentTeam.players.Count < 4)
            {
                Debug.LogWarning("[TeamManager] Opponent team or players not found - using fallback stats");
                sweeperL.name = "AI Sweeper Left";
                sweeperR.name = "AI Sweeper Right";
                sweeperT.name = "AI Skip";
                sweeperL.sweepStrength.SetBaseValue(aiStats + gsp.oppStats.sweepStrength);
                sweeperR.sweepStrength.SetBaseValue(aiStats + gsp.oppStats.sweepStrength);
                sweeperT.sweepStrength.SetBaseValue(aiStats + gsp.oppStats.sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(aiStats + gsp.oppStats.sweepEndurance);
                sweeperR.sweepEndurance.SetBaseValue(aiStats + gsp.oppStats.sweepEndurance);
                sweeperT.sweepEndurance.SetBaseValue(aiStats + gsp.oppStats.sweepEndurance);
                sweeperL.sweepCohesion.SetBaseValue(aiStats + gsp.oppStats.sweepCohesion);
                sweeperR.sweepCohesion.SetBaseValue(aiStats + gsp.oppStats.sweepCohesion);
                sweeperT.sweepCohesion.SetBaseValue(aiStats + gsp.oppStats.sweepCohesion);
                return;
            }

            // Set sweeper names and stats based on rock position
            if (rockCurrent > 11)
            {
                // Skip shooting (13-16) → Lead + Second sweep, Third sweeps T-line
                sweeperL.name = opponentTeam.players[0].name;
                sweeperL.sweepStrength.SetBaseValue(opponentTeam.players[0].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(opponentTeam.players[0].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperL.sweepCohesion.SetBaseValue(opponentTeam.players[0].sweepCohesion + gsp.oppStats.sweepCohesion);
                
                sweeperR.name = opponentTeam.players[1].name;
                sweeperR.sweepStrength.SetBaseValue(opponentTeam.players[1].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperR.sweepEndurance.SetBaseValue(opponentTeam.players[1].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperR.sweepCohesion.SetBaseValue(opponentTeam.players[1].sweepCohesion + gsp.oppStats.sweepCohesion);
                
                sweeperT.name = opponentTeam.players[2].name;  // Third sweeps T-line when skip is shooting
                sweeperT.sweepStrength.SetBaseValue(opponentTeam.players[2].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperT.sweepEndurance.SetBaseValue(opponentTeam.players[2].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperT.sweepCohesion.SetBaseValue(opponentTeam.players[2].sweepCohesion + gsp.oppStats.sweepCohesion);
            }
            else if (rockCurrent > 7)
            {
                // Third shooting (9-12) → Lead + Second sweep, Skip sweeps T-line
                sweeperL.name = opponentTeam.players[0].name;
                sweeperL.sweepStrength.SetBaseValue(opponentTeam.players[0].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(opponentTeam.players[0].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperL.sweepCohesion.SetBaseValue(opponentTeam.players[0].sweepCohesion + gsp.oppStats.sweepCohesion);
                
                sweeperR.name = opponentTeam.players[1].name;
                sweeperR.sweepStrength.SetBaseValue(opponentTeam.players[1].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperR.sweepEndurance.SetBaseValue(opponentTeam.players[1].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperR.sweepCohesion.SetBaseValue(opponentTeam.players[1].sweepCohesion + gsp.oppStats.sweepCohesion);
                
                sweeperT.name = opponentTeam.players[3].name;  // Skip sweeps T-line
                sweeperT.sweepStrength.SetBaseValue(opponentTeam.players[3].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperT.sweepEndurance.SetBaseValue(opponentTeam.players[3].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperT.sweepCohesion.SetBaseValue(opponentTeam.players[3].sweepCohesion + gsp.oppStats.sweepCohesion);
            }
            else if (rockCurrent > 3)
            {
                // Second shooting (5-8) → Lead + Third sweep, Skip sweeps T-line
                sweeperL.name = opponentTeam.players[0].name;
                sweeperL.sweepStrength.SetBaseValue(opponentTeam.players[0].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(opponentTeam.players[0].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperL.sweepCohesion.SetBaseValue(opponentTeam.players[0].sweepCohesion + gsp.oppStats.sweepCohesion);
                
                sweeperR.name = opponentTeam.players[2].name;  // Third sweeps right
                sweeperR.sweepStrength.SetBaseValue(opponentTeam.players[2].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperR.sweepEndurance.SetBaseValue(opponentTeam.players[2].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperR.sweepCohesion.SetBaseValue(opponentTeam.players[2].sweepCohesion + gsp.oppStats.sweepCohesion);
                
                sweeperT.name = opponentTeam.players[3].name;  // Skip sweeps T-line
                sweeperT.sweepStrength.SetBaseValue(opponentTeam.players[3].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperT.sweepEndurance.SetBaseValue(opponentTeam.players[3].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperT.sweepCohesion.SetBaseValue(opponentTeam.players[3].sweepCohesion + gsp.oppStats.sweepCohesion);
            }
            else
            {
                // Lead shooting (1-4) → Second + Third sweep, Skip sweeps T-line
                sweeperL.name = opponentTeam.players[1].name;  // Second sweeps left
                sweeperL.sweepStrength.SetBaseValue(opponentTeam.players[1].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperL.sweepEndurance.SetBaseValue(opponentTeam.players[1].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperL.sweepCohesion.SetBaseValue(opponentTeam.players[1].sweepCohesion + gsp.oppStats.sweepCohesion);
                
                sweeperR.name = opponentTeam.players[2].name;  // Third sweeps right
                sweeperR.sweepStrength.SetBaseValue(opponentTeam.players[2].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperR.sweepEndurance.SetBaseValue(opponentTeam.players[2].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperR.sweepCohesion.SetBaseValue(opponentTeam.players[2].sweepCohesion + gsp.oppStats.sweepCohesion);
                
                sweeperT.name = opponentTeam.players[3].name;  // Skip sweeps T-line
                sweeperT.sweepStrength.SetBaseValue(opponentTeam.players[3].sweepStrength + gsp.oppStats.sweepStrength);
                sweeperT.sweepEndurance.SetBaseValue(opponentTeam.players[3].sweepEnduro + gsp.oppStats.sweepEndurance);
                sweeperT.sweepCohesion.SetBaseValue(opponentTeam.players[3].sweepCohesion + gsp.oppStats.sweepCohesion);
            }
            
            Debug.Log($"[TeamManager] AI Sweepers - L: {sweeperL.name}, R: {sweeperR.name}, T: {sweeperT.name}");
        }
        else
        {
            // Player's turn - determine which players are sweeping based on who's shooting
            // T-line sweeper is ALWAYS the skip (for player team, that's cm.playerCharacter at index 3)

            if (rockCurrent > 11)
            {
                // Skip is shooting (rocks 13-16) → Lead + Second sweep
                sweeperL.name = cm.activePlayers[1].name;
                sweeperR.name = cm.activePlayers[0].name;
                sweeperT.name = cm.activePlayers[2].name;  // Third sweeps behind T-line
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
                // Third is shooting (rocks 9-12) → Lead + Second sweep, Skip sweeps T-line
                sweeperL.name = cm.activePlayers[1].name;
                sweeperR.name = cm.activePlayers[0].name;
                sweeperT.name = cm.playerName + " " + cm.teamName;  // Skip (player) sweeps T-line
                sweeperL.sweepStrength.SetBaseValue(cm.activePlayers[1].sweepStrength);
                sweeperR.sweepStrength.SetBaseValue(cm.activePlayers[0].sweepStrength);
                sweeperT.sweepStrength.SetBaseValue(cm.cStats.sweepStrength);  // Skip's stats
                sweeperL.sweepEndurance.SetBaseValue(cm.activePlayers[1].sweepEnduro);
                sweeperR.sweepEndurance.SetBaseValue(cm.activePlayers[0].sweepEnduro);
                sweeperT.sweepEndurance.SetBaseValue(cm.cStats.sweepEndurance);  // Skip's stats
                sweeperL.sweepCohesion.SetBaseValue(cm.activePlayers[1].sweepCohesion);
                sweeperR.sweepCohesion.SetBaseValue(cm.activePlayers[0].sweepCohesion);
                sweeperT.sweepCohesion.SetBaseValue(cm.cStats.sweepCohesion);  // Skip's stats
                Debug.Log($"[TeamManager] Player Skip T-line sweeper: {sweeperT.name} (Strength: {cm.cStats.sweepStrength}, Endurance: {cm.cStats.sweepEndurance})");
            }
            else if (rockCurrent > 3)
            {
                // Second is shooting (rocks 5-8) → Third + Lead sweep, Skip sweeps T-line
                sweeperL.name = cm.activePlayers[2].name;
                sweeperR.name = cm.activePlayers[0].name;
                sweeperT.name = cm.playerName + " " + cm.teamName;  // Skip (player) sweeps T-line
                sweeperL.sweepStrength.SetBaseValue(cm.activePlayers[2].sweepStrength);
                sweeperR.sweepStrength.SetBaseValue(cm.activePlayers[0].sweepStrength);
                sweeperT.sweepStrength.SetBaseValue(cm.cStats.sweepStrength);  // Skip's stats
                sweeperL.sweepEndurance.SetBaseValue(cm.activePlayers[2].sweepEnduro);
                sweeperR.sweepEndurance.SetBaseValue(cm.activePlayers[0].sweepEnduro);
                sweeperT.sweepEndurance.SetBaseValue(cm.cStats.sweepEndurance);  // Skip's stats
                sweeperL.sweepCohesion.SetBaseValue(cm.activePlayers[2].sweepCohesion);
                sweeperR.sweepCohesion.SetBaseValue(cm.activePlayers[0].sweepCohesion);
                sweeperT.sweepCohesion.SetBaseValue(cm.cStats.sweepCohesion);  // Skip's stats
                Debug.Log($"[TeamManager] Player Skip T-line sweeper: {sweeperT.name} (Strength: {cm.cStats.sweepStrength}, Endurance: {cm.cStats.sweepEndurance})");
            }
            else
            {
                // Lead is shooting (rocks 1-4) → Second + Third sweep, Skip sweeps T-line
                sweeperL.name = cm.activePlayers[1].name;
                sweeperR.name = cm.activePlayers[2].name;
                sweeperT.name = cm.playerName + " " + cm.teamName;  // Skip (player) sweeps T-line
                sweeperL.sweepStrength.SetBaseValue(cm.activePlayers[1].sweepStrength);
                sweeperR.sweepStrength.SetBaseValue(cm.activePlayers[2].sweepStrength);
                sweeperT.sweepStrength.SetBaseValue(cm.cStats.sweepStrength);  // Skip's stats
                sweeperL.sweepEndurance.SetBaseValue(cm.activePlayers[1].sweepEnduro);
                sweeperR.sweepEndurance.SetBaseValue(cm.activePlayers[2].sweepEnduro);
                sweeperT.sweepEndurance.SetBaseValue(cm.cStats.sweepEndurance);  // Skip's stats
                sweeperL.sweepCohesion.SetBaseValue(cm.activePlayers[1].sweepCohesion);
                sweeperR.sweepCohesion.SetBaseValue(cm.activePlayers[2].sweepCohesion);
                sweeperT.sweepCohesion.SetBaseValue(cm.cStats.sweepCohesion);  // Skip's stats
                Debug.Log($"[TeamManager] Player Skip T-line sweeper: {sweeperT.name} (Strength: {cm.cStats.sweepStrength}, Endurance: {cm.cStats.sweepEndurance})");
            }
        }
    }

    public void SetCharacter(int rockCurrent, bool redTurn)
    {
        // CRITICAL: Null safety checks for loaded games
        if (gsp == null)
        {
            Debug.LogError("[TeamManager] gsp is null!");
            return;
        }
        
        if (redTurn)
        {
            // Null check for team arrays
            if (teamRed == null || teamRed.Length == 0)
            {
                Debug.LogError("[TeamManager] teamRed is null or empty!");
                return;
            }
            
            if (gsp.redTeam == null || gsp.redTeam.players == null || gsp.redTeam.players.Count == 0)
            {
                Debug.LogError("[TeamManager] gsp.redTeam or players is null/empty!");
                return;
            }
            
            for (int i = 0; i < teamRed.Length; i++)
            {
                if (teamRed[i] != null && teamRed[i].shooter != null)
                {
                    CharColourChanger colourChanger = teamRed[i].shooter.GetComponent<CharColourChanger>();
                    if (colourChanger != null)
                        colourChanger.TeamColour(teamRedColour);
                }
            }

            for (int j = 0; j < teamRed.Length && j < gsp.redTeam.players.Count; j++)
            {
                if (teamRed[j] != null && teamRed[j].charStats != null && gsp.redTeam.players[j] != null)
                {
                    teamRed[j].charStats.weightAccuracy.SetBaseValue(gsp.redTeam.players[j].weight);
                    teamRed[j].charStats.aimAccuracy.SetBaseValue(gsp.redTeam.players[j].aim);
                    teamRed[j].charStats.finesseAccuracy.SetBaseValue(gsp.redTeam.players[j].finesse);
                    teamRed[j].charStats.sweepStrength.SetBaseValue(gsp.redTeam.players[j].sweepStrength);
                    teamRed[j].charStats.sweepEndurance.SetBaseValue(gsp.redTeam.players[j].sweepEnduro);
                    teamRed[j].charStats.sweepCohesion.SetBaseValue(gsp.redTeam.players[j].sweepCohesion);
                }
            }
        }
        else
        {
            // Null check for team arrays
            if (teamYellow == null || teamYellow.Length == 0)
            {
                Debug.LogError("[TeamManager] teamYellow is null or empty!");
                return;
            }
            
            if (gsp.yellowTeam == null || gsp.yellowTeam.players == null || gsp.yellowTeam.players.Count == 0)
            {
                Debug.LogError("[TeamManager] gsp.yellowTeam or players is null/empty!");
                return;
            }
            
            for (int i = 0; i < teamYellow.Length; i++)
            {
                if (teamYellow[i] != null && teamYellow[i].shooter != null)
                {
                    CharColourChanger colourChanger = teamYellow[i].shooter.GetComponent<CharColourChanger>();
                    if (colourChanger != null)
                        colourChanger.TeamColour(teamYellowColour);
                }
            }

            for (int j = 0; j < teamYellow.Length && j < gsp.yellowTeam.players.Count; j++)
            {
                if (teamYellow[j] != null && teamYellow[j].charStats != null && gsp.yellowTeam.players[j] != null)
                {
                    teamYellow[j].charStats.weightAccuracy.SetBaseValue(gsp.yellowTeam.players[j].weight);
                    teamYellow[j].charStats.aimAccuracy.SetBaseValue(gsp.yellowTeam.players[j].aim);
                    teamYellow[j].charStats.finesseAccuracy.SetBaseValue(gsp.yellowTeam.players[j].finesse);
                    teamYellow[j].charStats.sweepStrength.SetBaseValue(gsp.yellowTeam.players[j].sweepStrength);
                    teamYellow[j].charStats.sweepEndurance.SetBaseValue(gsp.yellowTeam.players[j].sweepEnduro);
                    teamYellow[j].charStats.sweepCohesion.SetBaseValue(gsp.yellowTeam.players[j].sweepCohesion);
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
