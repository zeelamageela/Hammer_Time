/// <summary>
/// What committing to a tournament costs the career: the event's entry fee plus the weekly running cost.
/// Charged once when the event is picked (TournySelector.PlayTourny) and refunded in full by Back.
/// </summary>
public static class TournyFees
{
    public static float Total(Tourny t, float costPerWeek)
    {
        return (t != null ? t.entryFee : 0f) + costPerWeek;
    }

    /// <summary>
    /// False only when real events are on offer and the career can pay for none of them: the run is over.
    /// An empty board (placeholders only) is not bankruptcy.
    /// </summary>
    public static bool AnyAffordable(Tourny[] offered, Tourny empty, float costPerWeek, float cash)
    {
        bool anyReal = false;
        if (offered != null)
        {
            foreach (var t in offered)
            {
                if (t == null || ReferenceEquals(t, empty)) continue;
                anyReal = true;
                if (CanAfford(t, costPerWeek, cash)) return true;
            }
        }
        return !anyReal;
    }

    /// <summary>True if the career can pay <see cref="Total"/> without going below zero.</summary>
    public static bool CanAfford(Tourny t, float costPerWeek, float cash)
    {
        return cash >= Total(t, costPerWeek);
    }
}
