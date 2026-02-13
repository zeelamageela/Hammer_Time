# Tour Events Switched to Page Playoff Format

## Decision
**Date:** 2024
**Status:** ? IMPLEMENTED

Officially abandoning the Triple Knockout (3K) tournament format for tour events in favor of the simpler, working Page Playoff format.

## Rationale

### Why Abandon Triple-K?
1. **Complex Display Logic** - 20 rounds × 20 different `DisplayRoundX()` methods
2. **Bracket UI Challenges** - 4 different bracket containers with manual positioning
3. **Save/Load Complexity** - 46 games in gameList, complex state management
4. **Time Investment** - Hours spent debugging display crashes vs gameplay value

### Why Page Playoff?
1. **? Already Works** - `PlayoffManager.cs` is battle-tested and reliable
2. **Simple & Fast** - 4 teams, 3 rounds maximum
3. **Exciting Format** - Top 2 get second chances, keeps drama
4. **Easy to Understand** - Clean bracket, mobile-friendly display

## Implementation

### What Changed
Tour events (`currentTourny.tour == true`) now use **Page Playoff (4 teams)** instead of Triple-K (16 teams).

### Code Changes

**Before:**
```csharp
// TournySettings.cs - LoadToGSP()
if (gsp.KO3)
    SceneManager.LoadScene("Tourny_Home_3K");  // Triple Knockout
else if (gsp.KO1)
    SceneManager.LoadScene("Tourny_Home_SingleK");
else
    SceneManager.LoadScene("Tourny_Home_1");  // Regular draw
```

**After:**
```csharp
// TournySettings.cs - LoadToGSP()
if (gsp.KO3)
{
    // DEPRECATED: Triple-K format abandoned due to UI complexity
    // Tour events now use Page Playoff (4 teams) - simpler, reliable, exciting
    Debug.LogWarning("[TournySettings] Triple-K format requested but DISABLED - using Page Playoff instead");
    gsp.KO3 = false;
    gsp.numberOfTeams = 4;
    SceneManager.LoadScene("Page_Playoff");  // Use Page Playoff instead
}
else if (gsp.KO1)
    SceneManager.LoadScene("Tourny_Home_SingleK");
else
    SceneManager.LoadScene("Tourny_Home_1");
```

### CareerManager.cs Changes
```csharp
// SetupTourny() - when setting up tour events
if (currentTourny.tour)
{
    // Tour events: Use Page Playoff (4 teams)
    gsp.KO3 = false;  // Disable Triple-K
    gsp.KO1 = false;
    gsp.numberOfTeams = 4;  // Page Playoff
    
    // Select top 4 teams from tour standings
    gsp.teams = GetTop4TourTeams();
}
```

## Tournament Formats Summary

| Format | Teams | Rounds | Elimination | Scene | Status |
|--------|-------|--------|-------------|-------|--------|
| **Regular Draw** | 5-16 | 3-15 games | None (standings) | `Tourny_Home_1` | ? Active |
| **Page Playoff** | 4 | 3 max | Modified double | `Page_Playoff` | ? Active (DEFAULT for tours) |
| **Single Knockout** | 4-16 | Log2(N) | Single elim | `Tourny_Home_SingleK` | ? Active |
| **Triple Knockout** | 16 | 20 | Triple elim | `Tourny_Home_3K` | ?? DEPRECATED |

## Benefits

### For Players
- ? Faster tour events (3 games max vs 20 rounds)
- ? Consistent, reliable tournament experience
- ? Clear bracket visualization
- ? More forgiving format (2 chances before elimination)

### For Development
- ? Less maintenance burden
- ? Simpler save/load system
- ? Easier to debug
- ? Can focus on gameplay features instead of UI fixes

## Triple-K Code Status

The `PlayoffManager_TripleK.cs` file is **kept in the codebase** but:
- ? Not used for tour events
- ? Scene `Tourny_Home_3K` can be disabled in build settings
- ?? Can be re-enabled if display issues are resolved in the future
- ?? Serves as reference for complex bracket logic

## Testing Checklist

- [ ] Tour event starts Page Playoff (not Triple-K)
- [ ] 4 teams selected correctly
- [ ] Bracket displays properly
- [ ] Player can play/sim games
- [ ] Save/load works mid-tournament
- [ ] Prize distribution correct
- [ ] Returns to career mode correctly

## Future Considerations

If Triple-K is ever needed again:
1. Simplify display to show only current matchup (no full bracket)
2. Use standings table instead of complex bracket UI
3. Focus on **gameplay** not **visualization**

---

**Bottom Line:** Tour events are now **fast, reliable, and fun** using Page Playoff! ??
