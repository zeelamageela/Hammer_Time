/// <summary>
/// Sponsors are money only: a weekly income while signed, and a bonus for a good tournament result. They give no stats.
/// One rule per tier says when a sponsor can be signed.
/// </summary>
public static class SponsorRules
{
    public enum Tier { Local, National, International }

    /// <summary>Local sponsors come knocking from this career week.</summary>
    public const int LocalFromWeek = 2;
    /// <summary>International sponsors want this many career game wins. National ones want provincial qualification.</summary>
    public const int InternationalWins = 20;
    /// <summary>A card with no length of its own runs this many weeks.</summary>
    public const int DefaultTerm = 4;

    /// <summary>Bonus as a multiple of the weekly income: for winning a tournament, and for finishing 2nd to 4th.</summary>
    public const float WinBonusWeeks = 3f;
    public const float TopFourBonusWeeks = 1f;

    public static bool Available(Tier tier, int week, bool provQualified, int careerWins)
    {
        switch (tier)
        {
            case Tier.Local: return week >= LocalFromWeek;
            case Tier.National: return provQualified;
            default: return careerWins >= InternationalWins;
        }
    }

    public static int Term(int cardDuration) { return cardDuration > 0 ? cardDuration : DefaultTerm; }

    /// <summary>What the signed sponsors pay on top for finishing a tournament at this rank (1 = won).</summary>
    public static float ResultBonus(float weeklyIncome, int rank)
    {
        if (weeklyIncome <= 0f || rank < 1 || rank > 4) return 0f;
        return weeklyIncome * (rank == 1 ? WinBonusWeeks : TopFourBonusWeeks);
    }
}
