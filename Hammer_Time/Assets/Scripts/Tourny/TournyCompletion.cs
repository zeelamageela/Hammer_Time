using System.Collections.Generic;

/// <summary>
/// Weekly "consume the offered events" rule for the tournament selector: once the player
/// commits to one event, the other real events offered that week are marked complete too.
/// Works on object identity, never on Tourny.id, because ids are shared across arrays
/// (emptyTourny and tournies[0] are both 0; locals and qualifiers both use 50-52).
/// </summary>
public static class TournyCompletion
{
    /// <summary>
    /// Marks every offered, not-yet-complete event as complete, skipping the empty placeholder
    /// and the filler local events. Returns exactly the events it changed so Undo can restore them.
    /// </summary>
    public static List<Tourny> MarkOffered(Tourny[] active, Tourny empty, Tourny[] locals)
    {
        var marked = new List<Tourny>();
        if (active == null) return marked;

        foreach (var t in active)
        {
            if (t == null || t.complete || IsPlaceholderOrLocal(t, empty, locals)) continue;
            t.complete = true;
            marked.Add(t);
        }
        return marked;
    }

    public static void Undo(List<Tourny> marked)
    {
        if (marked == null) return;
        foreach (var t in marked)
        {
            if (t != null) t.complete = false;
        }
        marked.Clear();
    }

    public static bool IsPlaceholderOrLocal(Tourny t, Tourny empty, Tourny[] locals)
    {
        return ReferenceEquals(t, empty) || (locals != null && System.Array.IndexOf(locals, t) >= 0);
    }
}
