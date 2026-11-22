using System;

/// <summary>
/// Chooses which game-over ending (index into GameOverDialogue.endings) a finished season gets.
/// Numbers 0-5 are the original endings and 6-9 belong to the debug-only circuit modes, so the newer tiers start at 10.
/// </summary>
public static class EndingPicker
{
    // Original qualification-only endings (how far you got).
    public const int NoQualifiers = 0;      // missed everything, never reached the provincial qualifier
    public const int MissedChampionships = 1; // played the provincial qualifier, made neither championship
    public const int TourChampOnly = 2;     // qualified for the Tour championship only
    public const int ProvChampOnly = 3;     // qualified for the Provincial championship only
    public const int BothChamps = 4;        // qualified for both
    public const int Bankrupt = 5;

    // Result tiers (how well you did).
    public const int WonBothTitles = 10;
    public const int WonTourTitle = 11;
    public const int WonProvTitle = 12;
    public const int MoneyRecord = 13;      // career earnings beat the best career on the leaderboard
    public const int MoneyLeader = 14;      // career earnings top every other team in the league
    public const int BigMoney = 15;         // career earnings reached BigMoneyEarnings

    public const int EndingCount = 16;

    // Tuning knob: "made a lot of money" for the BigMoney ending.
    public const float BigMoneyEarnings = 250000f;

    public struct Facts
    {
        public int qualification;       // result of Qualification(...)
        public bool wonTourTitle;
        public bool wonProvTitle;
        public float earnings;          // the player's career earnings
        public float bestLeagueRival;   // highest earnings among the other teams in the league
        public float bestRecord;        // highest career earnings on the leaderboard before this career (0 if none)
    }

    public static int Qualification(bool provQual, bool tourQual, bool champQual)
    {
        if (!tourQual && !champQual && !provQual) return NoQualifiers;
        if (!tourQual && !champQual) return MissedChampionships;
        if (tourQual && !champQual) return TourChampOnly;
        if (!tourQual && champQual) return ProvChampOnly;
        return BothChamps;
    }

    // Order matters: the rarest achievement wins, and anything unlucky enough to win nothing still gets the
    // qualification ending, so the original endings stay as the floor.
    public static int Pick(Facts f)
    {
        if (f.wonTourTitle && f.wonProvTitle) return WonBothTitles;
        if (f.bestRecord > 0f && f.earnings > f.bestRecord) return MoneyRecord;
        if (f.wonTourTitle) return WonTourTitle;
        if (f.wonProvTitle) return WonProvTitle;
        if (f.earnings > f.bestLeagueRival && f.earnings > 0f) return MoneyLeader;
        if (f.earnings >= BigMoneyEarnings) return BigMoney;
        return f.qualification;
    }
}
