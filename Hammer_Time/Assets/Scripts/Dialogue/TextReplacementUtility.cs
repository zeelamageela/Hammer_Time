using UnityEngine;
using System.Linq;

/// <summary>
/// Utility for replacing text tokens in dialogue with dynamic game values
/// Supports tokens like {PLAYER_NAME}, {TEAM_NAME}, {EARNINGS}, {PROV_RANK}, etc.
/// Also supports game state tokens like {RED_SCORE}, {PLAYER_IN_SCORING}, {HAMMER}, etc.
/// </summary>
public static class TextReplacementUtility
{
    public static string Replace(string text, CareerManager cm, GameManager gm = null)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        // If no CareerManager, only process game state
        if (cm == null && gm != null)
        {
            return ReplaceGameState(text, gm);
        }
        
        if (cm == null)
            return text;
        
        // Player info
        text = text.Replace("{PLAYER_NAME}", cm.playerName);
        text = text.Replace("{TEAM_NAME}", cm.teamName);
        text = text.Replace("{EARNINGS}", FormatCurrency(cm.earnings));
        
        // Provincial rank
        if (cm.provRankList != null && cm.provRankList.Count > 0)
        {
            cm.provRankList.Sort();
            
            for (int i = 0; i < cm.provRankList.Count; i++)
            {
                if (cm.playerTeamIndex == cm.provRankList[i].team.id)
                {
                    int rank = cm.provRankList[i].team.rank;
                    text = text.Replace("{PROV_RANK}", FormatRank(rank));
                    break;
                }
            }
        }
        
        // Tournament rank and points
        if (cm.tourRankList != null && cm.tourRankList.Count > 0)
        {
            cm.tourRankList.Sort();
            
            for (int i = 0; i < cm.tourRankList.Count; i++)
            {
                if (cm.playerTeamIndex == cm.tourRankList[i].team.id)
                {
                    int rank = cm.tourRankList[i].team.rank;
                    //float points = cm.tourRankList[i].team.points;
                    text = text.Replace("{TOUR_RANK}", FormatRank(rank));
                    //text = text.Replace("{TOUR_POINTS}", points.ToString("F1"));
                    break;
                }
            }
        }
        
        // Week info
        text = text.Replace("{WEEK}", cm.week.ToString());
        
        // Player team skills (top 2)
        if (cm.activePlayers != null && cm.activePlayers.Length >= 3)
        {
            var skills = new[]
            {
                ("Weight", cm.cStats.weightAccuracy + cm.activePlayers.Sum(p => p.weight) + cm.modStats.weightAccuracy),
                ("Finesse", cm.cStats.finesseAccuracy + cm.activePlayers.Sum(p => p.finesse) + cm.modStats.finesseAccuracy),
                ("Aim", cm.cStats.aimAccuracy + cm.activePlayers.Sum(p => p.aim) + cm.modStats.aimAccuracy),
                ("Sweep Strength", cm.cStats.sweepStrength + cm.activePlayers.Sum(p => p.sweepStrength) + cm.modStats.sweepStrength),
                ("Sweep Endurance", cm.cStats.sweepEndurance + cm.activePlayers.Sum(p => p.sweepEnduro) + cm.modStats.sweepEndurance),
                ("Sweep Cohesion", cm.cStats.sweepCohesion + cm.activePlayers.Sum(p => p.sweepCohesion) + cm.modStats.sweepCohesion)
            }.OrderByDescending(s => s.Item2).ToArray();
            
            text = text.Replace("{PLAYER_TOP_SKILL_1}", skills[0].Item1);
            text = text.Replace("{PLAYER_TOP_SKILL_2}", skills[1].Item1);
            text = text.Replace("{PLAYER_TOP_SKILL_1_VALUE}", skills[0].Item2.ToString());
            text = text.Replace("{PLAYER_TOP_SKILL_2_VALUE}", skills[1].Item2.ToString());
        }
        
        // Game state replacements (scores, hammer, ends, etc.)
        if (gm != null)
        {
            text = ReplaceGameState(text, gm, cm);
        }
        
        return text;
    }
    
    /// <summary>
    /// Replace game state tokens (scores, hammer, ends, rocks in house, etc.)
    /// </summary>
    private static string ReplaceGameState(string text, GameManager gm, CareerManager cm = null)
    {
        // Basic scores
        text = text.Replace("{RED_SCORE}", gm.redScore.ToString());
        text = text.Replace("{YELLOW_SCORE}", gm.yellowScore.ToString());
        
        // Perspective-based scores (player vs opponent)
        if (cm != null)
        {
            bool playerIsRed = !gm.aiTeamRed && gm.aiTeamYellow;
            text = text.Replace("{PLAYER_SCORE}", playerIsRed ? gm.redScore.ToString() : gm.yellowScore.ToString());
            text = text.Replace("{OPPONENT_SCORE}", playerIsRed ? gm.yellowScore.ToString() : gm.redScore.ToString());
        }
        
        // Hammer info
        text = text.Replace("{HAMMER}", gm.redHammer ? gm.redTeamName : gm.yellowTeamName);
        text = text.Replace("{HAMMER_TEAM}", gm.redHammer ? "Red" : "Yellow");
        if (cm != null)
        {
            bool playerIsRed = !gm.aiTeamRed && gm.aiTeamYellow;
            bool playerHasHammer = (playerIsRed && gm.redHammer) || (!playerIsRed && !gm.redHammer);
            text = text.Replace("{PLAYER_HAMMER}", playerHasHammer ? "Yes" : "No");
        }
        
        // End info
        text = text.Replace("{END_CURRENT}", (gm.endCurrent + 1).ToString()); // Display 1-indexed
        text = text.Replace("{END_TOTAL}", gm.endTotal.ToString());
        text = text.Replace("{ENDS_REMAINING}", (gm.endTotal - gm.endCurrent).ToString());
        
        // Rocks in house (all rocks in house for each team)
        int redInHouse = 0;
        int yellowInHouse = 0;
        if (gm.houseList != null)
        {
            foreach (var rock in gm.houseList)
            {
                if (rock.rockInfo.teamName == gm.redTeamName)
                    redInHouse++;
                else if (rock.rockInfo.teamName == gm.yellowTeamName)
                    yellowInHouse++;
            }
        }
        
        text = text.Replace("{RED_IN_HOUSE}", redInHouse.ToString());
        text = text.Replace("{YELLOW_IN_HOUSE}", yellowInHouse.ToString());
        
        // Rocks in scoring position (only the leading team's rocks until opponent's first rock)
        int redInScoring = 0;
        int yellowInScoring = 0;
        if (gm.houseList != null && gm.houseList.Count > 0)
        {
            string leadingTeam = gm.houseList[0].rockInfo.teamName;
            foreach (var rock in gm.houseList)
            {
                if (rock.rockInfo.teamName == leadingTeam)
                {
                    if (leadingTeam == gm.redTeamName)
                        redInScoring++;
                    else
                        yellowInScoring++;
                }
                else
                {
                    break; // Stop when we hit opponent's first rock
                }
            }
        }
        
        text = text.Replace("{RED_IN_SCORING}", redInScoring.ToString());
        text = text.Replace("{YELLOW_IN_SCORING}", yellowInScoring.ToString());
        
        // Perspective-based house/scoring info
        if (cm != null)
        {
            bool playerIsRed = !gm.aiTeamRed && gm.aiTeamYellow;
            text = text.Replace("{PLAYER_IN_HOUSE}", playerIsRed ? redInHouse.ToString() : yellowInHouse.ToString());
            text = text.Replace("{OPPONENT_IN_HOUSE}", playerIsRed ? yellowInHouse.ToString() : redInHouse.ToString());
            text = text.Replace("{PLAYER_IN_SCORING}", playerIsRed ? redInScoring.ToString() : yellowInScoring.ToString());
            text = text.Replace("{OPPONENT_IN_SCORING}", playerIsRed ? yellowInScoring.ToString() : redInScoring.ToString());
            
            // Opponent team skills (top 2)
            GameSettingsPersist gsp = Object.FindFirstObjectByType<GameSettingsPersist>();
            if (gsp != null)
            {
                Team opponentTeam = playerIsRed ? gsp.yellowTeam : gsp.redTeam;
                if (opponentTeam != null && opponentTeam.players != null && opponentTeam.players.Count > 0)
                {
                    var oppSkills = new[]
                    {
                        ("Weight", opponentTeam.players.Sum(p => p.weight)),
                        ("Finesse", opponentTeam.players.Sum(p => p.finesse)),
                        ("Aim", opponentTeam.players.Sum(p => p.aim)),
                        ("Sweep Strength", opponentTeam.players.Sum(p => p.sweepStrength)),
                        ("Sweep Endurance", opponentTeam.players.Sum(p => p.sweepEnduro)),
                        ("Sweep Cohesion", opponentTeam.players.Sum(p => p.sweepCohesion))
                    }.OrderByDescending(s => s.Item2).ToArray();
                    
                    text = text.Replace("{OPPONENT_TOP_SKILL_1}", oppSkills[0].Item1);
                    text = text.Replace("{OPPONENT_TOP_SKILL_2}", oppSkills[1].Item1);
                    text = text.Replace("{OPPONENT_TOP_SKILL_1_VALUE}", oppSkills[0].Item2.ToString());
                    text = text.Replace("{OPPONENT_TOP_SKILL_2_VALUE}", oppSkills[1].Item2.ToString());
                }
            }
        }
        
        return text;
    }
    
    private static string FormatCurrency(float amount)
    {
        return "$" + amount.ToString("n0");
    }
    
    private static string FormatRank(int rank)
    {
        if (rank <= 0) return rank.ToString();
        
        int lastDigit = rank % 10;
        int lastTwoDigits = rank % 100;
        
        // Handle special cases (11th, 12th, 13th)
        if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
            return rank + "th";
        
        // Regular cases
        switch (lastDigit)
        {
            case 1: return rank + "st";
            case 2: return rank + "nd";
            case 3: return rank + "rd";
            default: return rank + "th";
        }
    }
}
