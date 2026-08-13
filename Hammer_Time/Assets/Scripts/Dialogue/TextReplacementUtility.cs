using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Utility for replacing text tokens in dialogue with dynamic game values.
///
/// Career tokens:    {PLAYER_NAME} {TEAM_NAME} {EARNINGS} {PROV_RANK} {TOUR_RANK} {WEEK}
///                   {CAREER_RECORD}  (e.g. "14-6")
///                   {PLAYER_TOP_SKILL_1/2} {PLAYER_TOP_SKILL_1/2_VALUE}
///                   {OPPONENT_TOP_SKILL_1/2} {OPPONENT_TOP_SKILL_1/2_VALUE}
///
/// Game-state tokens:{RED_SCORE} {YELLOW_SCORE} {PLAYER_SCORE} {OPPONENT_SCORE}
///                   {SCORE_DIFF}               (absolute point gap)
///                   {PLAYER_LEADING}           ("ahead" / "behind" / "tied")
///                   {OPPONENT_NAME}            (opposing team name)
///                   {HAMMER} {HAMMER_TEAM} {PLAYER_HAMMER} {PLAYER_HAS_HAMMER}
///                   {END_CURRENT} {END_PREVIOUS} {END_TOTAL} {ENDS_REMAINING}
///                   {LAST_END_SCORE}           (points player scored last end)
///                   {ROCKS_REMAINING}          (rocks left this end)
///                   {RED_IN_HOUSE} {YELLOW_IN_HOUSE} {PLAYER_IN_HOUSE} {OPPONENT_IN_HOUSE}
///                   {RED_IN_SCORING} {YELLOW_IN_SCORING} {PLAYER_IN_SCORING} {OPPONENT_IN_SCORING}
///
/// Variety token:    {PICK:option1|option2|option3}  (picks one at random each display)
/// </summary>
public static class TextReplacementUtility
{
    public static string Replace(string text, CareerManager cm, GameManager gm = null)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        if (cm == null && gm != null)
        {
            text = ReplaceGameState(text, gm);
            return ReplacePicks(text);
        }

        if (cm == null)
            return text;

        // Player info
        text = text.Replace("{PLAYER_NAME}", cm.playerName);
        text = text.Replace("{TEAM_NAME}", cm.teamName);
        text = text.Replace("{EARNINGS}", FormatCurrency(cm.earnings));
        text = text.Replace("{CAREER_RECORD}", $"{(int)cm.record.x}-{(int)cm.record.y}");

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

        // Tournament rank
        if (cm.tourRankList != null && cm.tourRankList.Count > 0)
        {
            cm.tourRankList.Sort();

            for (int i = 0; i < cm.tourRankList.Count; i++)
            {
                if (cm.playerTeamIndex == cm.tourRankList[i].team.id)
                {
                    int rank = cm.tourRankList[i].team.rank;
                    text = text.Replace("{TOUR_RANK}", FormatRank(rank));
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
                ("Weight",          cm.cStats.weightAccuracy  + cm.activePlayers.Sum(p => p.weight)        + cm.modStats.weightAccuracy),
                ("Finesse",         cm.cStats.finesseAccuracy + cm.activePlayers.Sum(p => p.finesse)       + cm.modStats.finesseAccuracy),
                ("Aim",             cm.cStats.aimAccuracy     + cm.activePlayers.Sum(p => p.aim)           + cm.modStats.aimAccuracy),
                ("Sweep Strength",  cm.cStats.sweepStrength   + cm.activePlayers.Sum(p => p.sweepStrength) + cm.modStats.sweepStrength),
                ("Sweep Endurance", cm.cStats.sweepEndurance  + cm.activePlayers.Sum(p => p.sweepEnduro)   + cm.modStats.sweepEndurance),
                ("Sweep Cohesion",  cm.cStats.sweepCohesion   + cm.activePlayers.Sum(p => p.sweepCohesion) + cm.modStats.sweepCohesion)
            }.OrderByDescending(s => s.Item2).ToArray();

            text = text.Replace("{PLAYER_TOP_SKILL_1}",       skills[0].Item1);
            text = text.Replace("{PLAYER_TOP_SKILL_2}",       skills[1].Item1);
            text = text.Replace("{PLAYER_TOP_SKILL_1_VALUE}", skills[0].Item2.ToString());
            text = text.Replace("{PLAYER_TOP_SKILL_2_VALUE}", skills[1].Item2.ToString());
        }

        // Game state replacements (scores, hammer, ends, rocks in house, etc.)
        if (gm != null)
            text = ReplaceGameState(text, gm, cm);

        return ReplacePicks(text);
    }

    /// <summary>
    /// Replace game-state tokens. Safe to call with cm == null for game-only contexts.
    /// </summary>
    private static string ReplaceGameState(string text, GameManager gm, CareerManager cm = null)
    {
        // Basic scores
        text = text.Replace("{RED_SCORE}",    gm.redScore.ToString());
        text = text.Replace("{YELLOW_SCORE}", gm.yellowScore.ToString());

        // Score differential (absolute gap)
        int diff = Mathf.Abs(gm.redScore - gm.yellowScore);
        text = text.Replace("{SCORE_DIFF}", diff.ToString());

        // Hammer info
        text = text.Replace("{HAMMER}",      gm.redHammer ? gm.redTeamName : gm.yellowTeamName);
        text = text.Replace("{HAMMER_TEAM}", gm.redHammer ? "Red" : "Yellow");

        // End info
        text = text.Replace("{END_CURRENT}",   (gm.endCurrent + 1).ToString()); // 1-indexed
        text = text.Replace("{END_PREVIOUS}",  gm.endCurrent.ToString());        // just-completed end, 1-indexed
        text = text.Replace("{END_TOTAL}",     gm.endTotal.ToString());
        text = text.Replace("{ENDS_REMAINING}", (gm.endTotal - gm.endCurrent).ToString());

        // Rocks remaining this end
        int totalRocksPerEnd = gm.rocksPerTeam * 2;
        if (totalRocksPerEnd > 0)
        {
            int rocksIntoEnd  = gm.rockCurrent % totalRocksPerEnd;
            int rocksRemaining = Mathf.Max(0, totalRocksPerEnd - rocksIntoEnd);
            text = text.Replace("{ROCKS_REMAINING}", rocksRemaining.ToString());
        }

        // Rocks in house (all rocks)
        int redInHouse    = 0;
        int yellowInHouse = 0;
        if (gm.houseList != null)
        {
            foreach (var rock in gm.houseList)
            {
                if (rock.rockInfo.teamName == gm.redTeamName)       redInHouse++;
                else if (rock.rockInfo.teamName == gm.yellowTeamName) yellowInHouse++;
            }
        }

        text = text.Replace("{RED_IN_HOUSE}",    redInHouse.ToString());
        text = text.Replace("{YELLOW_IN_HOUSE}", yellowInHouse.ToString());

        // Rocks in scoring position (leading team's rocks until opponent's first)
        int redInScoring    = 0;
        int yellowInScoring = 0;
        if (gm.houseList != null && gm.houseList.Count > 0)
        {
            string leadingTeam = gm.houseList[0].rockInfo.teamName;
            foreach (var rock in gm.houseList)
            {
                if (rock.rockInfo.teamName == leadingTeam)
                {
                    if (leadingTeam == gm.redTeamName) redInScoring++;
                    else                               yellowInScoring++;
                }
                else break;
            }
        }

        text = text.Replace("{RED_IN_SCORING}",    redInScoring.ToString());
        text = text.Replace("{YELLOW_IN_SCORING}", yellowInScoring.ToString());

        // Perspective-based tokens (require knowing which team is the player)
        if (cm != null)
        {
            bool playerIsRed = !gm.aiTeamRed && gm.aiTeamYellow;

            int playerScore   = playerIsRed ? gm.redScore    : gm.yellowScore;
            int opponentScore = playerIsRed ? gm.yellowScore : gm.redScore;

            text = text.Replace("{PLAYER_SCORE}",   playerScore.ToString());
            text = text.Replace("{OPPONENT_SCORE}", opponentScore.ToString());

            // Who's winning
            string leadState = playerScore > opponentScore ? "ahead"
                             : playerScore < opponentScore ? "behind"
                             : "tied";
            text = text.Replace("{PLAYER_LEADING}", leadState);

            // Opponent team name
            text = text.Replace("{OPPONENT_NAME}", playerIsRed ? gm.yellowTeamName : gm.redTeamName);

            // Hammer possession
            bool playerHasHammer = (playerIsRed && gm.redHammer) || (!playerIsRed && !gm.redHammer);
            text = text.Replace("{PLAYER_HAMMER}",     playerHasHammer ? "Yes" : "No");
            text = text.Replace("{PLAYER_HAS_HAMMER}", playerHasHammer ? "have" : "don't have");

            // Points player scored in the most recently completed end
            string lastEndScore = "0";
            if (gm.endCurrent > 0)
            {
                GameSettingsPersist gsp = Object.FindFirstObjectByType<GameSettingsPersist>();
                if (gsp?.score != null && gsp.score.Length >= gm.endCurrent)
                {
                    Vector2Int lastEnd = gsp.score[gm.endCurrent - 1];
                    lastEndScore = (playerIsRed ? lastEnd.x : lastEnd.y).ToString();
                }
            }
            text = text.Replace("{LAST_END_SCORE}", lastEndScore);

            // House / scoring counts from player perspective
            text = text.Replace("{PLAYER_IN_HOUSE}",    playerIsRed ? redInHouse.ToString()    : yellowInHouse.ToString());
            text = text.Replace("{OPPONENT_IN_HOUSE}",  playerIsRed ? yellowInHouse.ToString() : redInHouse.ToString());
            text = text.Replace("{PLAYER_IN_SCORING}",  playerIsRed ? redInScoring.ToString()  : yellowInScoring.ToString());
            text = text.Replace("{OPPONENT_IN_SCORING}", playerIsRed ? yellowInScoring.ToString() : redInScoring.ToString());

            // Opponent team skills (top 2)
            GameSettingsPersist gspSkills = Object.FindFirstObjectByType<GameSettingsPersist>();
            if (gspSkills != null)
            {
                Team opponentTeam = playerIsRed ? gspSkills.yellowTeam : gspSkills.redTeam;
                if (opponentTeam?.players != null && opponentTeam.players.Count > 0)
                {
                    var oppSkills = new[]
                    {
                        ("Weight",          opponentTeam.players.Sum(p => p.weight)),
                        ("Finesse",         opponentTeam.players.Sum(p => p.finesse)),
                        ("Aim",             opponentTeam.players.Sum(p => p.aim)),
                        ("Sweep Strength",  opponentTeam.players.Sum(p => p.sweepStrength)),
                        ("Sweep Endurance", opponentTeam.players.Sum(p => p.sweepEnduro)),
                        ("Sweep Cohesion",  opponentTeam.players.Sum(p => p.sweepCohesion))
                    }.OrderByDescending(s => s.Item2).ToArray();

                    text = text.Replace("{OPPONENT_TOP_SKILL_1}",       oppSkills[0].Item1);
                    text = text.Replace("{OPPONENT_TOP_SKILL_2}",       oppSkills[1].Item1);
                    text = text.Replace("{OPPONENT_TOP_SKILL_1_VALUE}", oppSkills[0].Item2.ToString());
                    text = text.Replace("{OPPONENT_TOP_SKILL_2_VALUE}", oppSkills[1].Item2.ToString());
                }
            }
        }

        return text;
    }

    /// <summary>
    /// Replaces {PICK:A|B|C} tokens with a random choice from the pipe-separated options.
    /// Example: "A {PICK:great|solid|fantastic} shot!" → "A solid shot!"
    /// </summary>
    private static string ReplacePicks(string text)
    {
        if (!text.Contains("{PICK:"))
            return text;

        return Regex.Replace(text, @"\{PICK:([^}]+)\}", match =>
        {
            string[] options = match.Groups[1].Value.Split('|');
            return options[Random.Range(0, options.Length)];
        });
    }

    private static string FormatCurrency(float amount)
    {
        return "$" + amount.ToString("n0");
    }

    private static string FormatRank(int rank)
    {
        if (rank <= 0) return rank.ToString();

        int lastDigit    = rank % 10;
        int lastTwoDigits = rank % 100;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
            return rank + "th";

        switch (lastDigit)
        {
            case 1:  return rank + "st";
            case 2:  return rank + "nd";
            case 3:  return rank + "rd";
            default: return rank + "th";
        }
    }
}
