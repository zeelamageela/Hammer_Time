using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrawFormatList : MonoBehaviour
{
    public DrawFormat[] team5;
    public DrawFormat[] team6;
    public DrawFormat[] team7;
    public DrawFormat[] team8;
    public DrawFormat[] team9;
    public DrawFormat[] team10;
    public DrawFormat[] team11;
    public DrawFormat[] team12;
    public DrawFormat[] team13;
    public DrawFormat[] team14;
    public DrawFormat[] team15;
    public DrawFormat[] team16;

    public DrawFormat[] team16_2Pools;
    public DrawFormat[] team18_2Pools;
    public DrawFormat[] team20_2Pools;

    public DrawFormat[] currentFormat;

    public void DrawSelector(int numberOfTeams, int gameLength, int games)
    {
        Debug.Log($"[DrawFormatList] DrawSelector called - numberOfTeams={numberOfTeams}, gameLength={gameLength}, games={games}");
        
        // CRITICAL: Verify that the team arrays are populated in the Inspector
        if (numberOfTeams == 8 && (team8 == null || team8.Length == 0))
        {
            Debug.LogError("[DrawFormatList] CRITICAL: team8 array is null or empty! This must be set in the Unity Inspector!");
            currentFormat = null;
            return;
        }
        
        DrawFormat[] shorterFormat;
        //DrawFormat[] shortestFormat;

        #region Full Sim
        if (gameLength == 0)
        {
            switch (numberOfTeams)
            {
                case 5:
                    currentFormat = team5;
                    break;

                case 6:
                    currentFormat = team6;
                    break;

                case 7:
                    currentFormat = team7;
                    break;

                case 8:
                    currentFormat = team8;
                    break;

                case 9:
                    currentFormat = team9;
                    break;

                case 10:
                    currentFormat = team10;
                    break;

                case 11:
                    currentFormat = team11;
                    break;

                case 12:
                    currentFormat = team12;
                    break;

                case 13:
                    currentFormat = team13;
                    break;

                case 14:
                    currentFormat = team14;
                    break;

                case 15:
                    currentFormat = team15;
                    break;

                case 16:
                    currentFormat = team16;
                    break;

                default:
                    currentFormat = null;
                    Debug.Log("Need between 5 and 16 teams for a tourny");
                    break;
            }
        }
        #endregion

        else if (gameLength >= 1)
        {
            Debug.Log($"[DrawFormatList] gameLength >= 1 branch - about to create shorterFormat with games={games}");
            
            switch (numberOfTeams)
            {
                case 5:
                    currentFormat = team5;
                    shorterFormat = new DrawFormat[games];
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    break;

                case 6:
                    currentFormat = team6;
                    shorterFormat = new DrawFormat[games];
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    break;

                case 7:
                    currentFormat = team7;
                    shorterFormat = new DrawFormat[games];
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    break;

                case 8:
                    currentFormat = team8;
                    Debug.Log($"[DrawFormatList] team8 assigned - team8.Length={team8?.Length ?? 0}");
                    
                    if (games <= 0)
                    {
                        Debug.LogError($"[DrawFormatList] CRITICAL: games parameter is {games}! Cannot create shorterFormat with length <= 0!");
                        Debug.LogError("[DrawFormatList] This will result in drawFormat.Length = 0. Check gsp.games value!");
                        currentFormat = null;
                        break;
                    }
                    
                    shorterFormat = new DrawFormat[games];
                    Debug.Log($"[DrawFormatList] Created shorterFormat with length={games}");
                    
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    Debug.Log($"[DrawFormatList] currentFormat set to shorterFormat - length={currentFormat.Length}");
                    break;

                case 9:
                    currentFormat = team9;
                    shorterFormat = new DrawFormat[games];
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    break;

                case 10:
                    currentFormat = team10;
                    shorterFormat = new DrawFormat[games];
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    break;

                case 11:
                    currentFormat = team11;
                    shorterFormat = new DrawFormat[games];
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    break;

                case 12:
                    currentFormat = team12;
                    shorterFormat = new DrawFormat[games];
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    break;

                case 13:
                    currentFormat = team13;
                    shorterFormat = new DrawFormat[games];
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    break;

                case 14:
                    currentFormat = team14;
                    shorterFormat = new DrawFormat[games];
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    break;

                case 15:
                    currentFormat = team15;
                    shorterFormat = new DrawFormat[games];
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    break;

                case 16:
                    currentFormat = team16;
                    shorterFormat = new DrawFormat[games];
                    for (int i = 0; i < shorterFormat.Length; i++)
                    {
                        shorterFormat[i] = currentFormat[i];
                    }
                    currentFormat = shorterFormat;
                    break;

                default:
                    currentFormat = null;
                    Debug.Log("Need between 5 and 16 teams for a tourny");
                    break;
            }
        }

        Debug.Log("currentFormat Length is " + (currentFormat?.Length ?? 0));
        
        if (currentFormat == null || currentFormat.Length == 0)
        {
            Debug.LogError($"[DrawFormatList] CRITICAL ERROR: currentFormat is null or empty after DrawSelector!");
            Debug.LogError($"[DrawFormatList] This usually means the team{numberOfTeams} array is not set up in the Unity Inspector!");
            Debug.LogError($"[DrawFormatList] Please check the DrawFormatList component in the Tourny_Home scene!");
        }
    }
    
    /// <summary>
    /// Generate two separate 8-team round robin schedules for a two-pool tournament.
    /// Pool A: teams 0-7, Pool B: teams 8-15
    /// Returns combined format where both pools play simultaneously each draw.
    /// </summary>
    public void DrawSelector_TwoPool(int numberOfTeams, int gameLength, int games)
    {
        Debug.Log($"[DrawFormatList] DrawSelector_TwoPool called - numberOfTeams={numberOfTeams}, games={games}");
        
        if (numberOfTeams != 16)
        {
            Debug.LogError($"[DrawFormatList] Two-pool mode only supports 16 teams! Got {numberOfTeams}");
            currentFormat = null;
            return;
        }
        
        // Verify that team8 is populated (we'll use it for each pool)
        if (team8 == null || team8.Length == 0)
        {
            Debug.LogError("[DrawFormatList] CRITICAL: team8 array is null or empty! Cannot generate two-pool format!");
            currentFormat = null;
            return;
        }
        
        // Create combined format: each draw has 8 games (4 from Pool A, 4 from Pool B)
        int numDraws = team8.Length; // Number of draws in a full 8-team round robin
        if (games > 0 && games < numDraws)
        {
            numDraws = games; // Use shortened format if requested
        }
        
        currentFormat = new DrawFormat[numDraws];
        
        for (int draw = 0; draw < numDraws; draw++)
        {
            // Each draw has 8 games total (4 from each pool)
            DrawFormat combinedDraw = new DrawFormat();
            combinedDraw.game = new Vector2Int[8];
            
            // Pool A: Copy games from team8, teams are indices 0-7
            for (int game = 0; game < 4; game++)
            {
                combinedDraw.game[game] = team8[draw].game[game];
            }
            
            // Pool B: Copy games from team8, but offset team indices by 8
            // So team 0 in the template becomes team 8, team 1 becomes 9, etc.
            for (int game = 0; game < 4; game++)
            {
                Vector2Int poolBGame = team8[draw].game[game];
                combinedDraw.game[game + 4] = new Vector2Int(poolBGame.x + 8, poolBGame.y + 8);
            }
            
            currentFormat[draw] = combinedDraw;
        }
        
        Debug.Log($"[DrawFormatList] Two-pool format generated - {currentFormat.Length} draws with 8 games each");
    }
}


