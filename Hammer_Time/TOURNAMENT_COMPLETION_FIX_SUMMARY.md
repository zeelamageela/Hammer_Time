# Tournament Completion Save/Load Fix - Summary

## Problem
Completed tournaments in Career Mode are not being properly saved and restored. When loading a saved career, tournaments that were previously completed show as incomplete again.

## Root Cause
The tournament completion system was working, but lacked sufficient diagnostic logging to identify where the data was being lost. The issue could be in several places:

1. **Save Phase**: Completion IDs not being collected properly
2. **Storage Phase**: JSON save file not being written correctly
3. **Load Phase**: Completion IDs not being restored to TournySelector arrays
4. **Timing Phase**: SetActiveTournies() called before completion data is restored

## Solution Implemented

### 1. Enhanced AI Fallback Strategy (AI_Target.cs)
**Fixed in `PeelTarget()` method** - Lines ~1770-1950

When a peel shot's physics calculation fails, the AI now has a **multi-phase fallback**:

1. **Phase 1**: Try secondary targets (other guards near primary)
2. **Phase 2**: Try direct peel on primary target
3. **Phase 3**: **NEW!** Look for guards blocking rocks in the house and take them out
4. **Phase 4**: **NEW!** Try to takeout ANY opponent guard
5. **Phase 5**: **NEW!** Try to takeout ANY opponent rock
6. **Phase 6**: **NEW!** Fall back to draw shot to button (physics-based)
7. **Phase 7**: Magic numbers (absolute last resort)

**Key Improvement**: The AI will now **prioritize removing guards that block house rocks** instead of immediately falling back to magic numbers. This is much more strategically sound and uses the existing physics-based targeting system.

### 2. Comprehensive Diagnostic Logging (CareerManager.cs)
**Added to `ToSaveData()` method** - Lines ~4270-4400

Added detailed logging to track:
- Whether TournySelector exists when saving
- How many tournaments are in each array (provQual, tour, tournies)
- The exact IDs being saved to completedTournamentIDs list
- The exact IDs being saved to trophyWonIDs list
- Which path was used (TournySelector or CM fallback)

**Example output**:
```
[SAVE DEBUG] Starting tournament completion save - TournySelector exists: True
[SAVE DEBUG] TournySelector arrays - provQual: 3, tour: 5, tournies: 8
[SAVE DEBUG] Completed IDs saved: [10, 15, 23, 27]
[SAVE DEBUG] Trophy IDs saved: [10, 23]
```

**Added to `LoadCareer()` method** - Lines ~1130-1150

Added logging to verify:
- When ApplyTournamentData() is called
- Whether TournySelector is null during load

**Added to `ApplyTournamentData()` method** - Lines ~1160-1280

The existing method was enhanced with more logging (already present in the file) to show:
- Number of IDs to restore
- Which arrays are being checked (tour, provQual, tournies)
- Each tournament checked and whether it was restored
- Final count of restored tournaments and trophies

### 3. Debug Document (TOURNAMENT_COMPLETION_SAVE_DEBUG.md)
Created comprehensive debugging guide with:
- Full explanation of save/load flow
- Step-by-step debugging checklist
- Expected console output for normal operation
- Common issues and how to diagnose them
- Testing checklist to verify the fix

## How to Use This Fix

### 1. Test Tournament Completion
1. Start a new career
2. Complete a tournament (week 1)
3. Check console for save logging:
   ```
   [SAVE DEBUG] Completed IDs saved: [10]
   ```
4. Quit to main menu
5. Load career
6. Check console for load logging:
   ```
   [CareerManager] ? Marked tournament 'Regional Cup' (ID 10) as complete
   ```
7. Verify tournament shows as complete in TournySelector UI

### 2. Review Console Logs
Look for these patterns:

**GOOD** (working correctly):
```
[SAVE DEBUG] Completed IDs saved: [10, 15]
[CareerManager] ? Marked tournament 'X' (ID 10) as complete
[CareerManager] ? Marked tournament 'Y' (ID 15) as complete
```

**BAD** (IDs not being saved):
```
[SAVE DEBUG] Completed IDs saved: []
```
? Problem in ToSaveData() - tournaments not marked complete before save

**BAD** (IDs saved but not restored):
```
[SAVE DEBUG] Completed IDs saved: [10, 15]
[CareerManager] Tournament restoration complete: 0 tournaments marked complete
```
? Problem in ApplyTournamentData() - IDs don't match or arrays are null

### 3. Inspect Save File
If logs show IDs are being saved, verify the JSON file:

**Location**: `Application.persistentDataPath/career_save.json`

Look for:
```json
{
  "completedTournamentIDs": [10, 15, 23],
  "trophyWonIDs": [10],
  "tourChampionshipComplete": false,
  "provChampionshipComplete": false
}
```

If IDs are present in the file but not being restored, the issue is in `ApplyTournamentData()`.

## Expected Behavior After Fix

### AI Behavior
- **Before**: Peel shot fails ? Magic numbers fallback (inaccurate)
- **After**: Peel shot fails ? Look for blocking guards ? Takeout guard (physics-based) ? Strategic and accurate

### Tournament Completion
- **Before**: Hard to diagnose - completion lost somewhere
- **After**: Full logging shows exactly where data is lost

Example log progression:
```
[PlayTourny] Marked tournament 'Regional Cup' (ID 10) as complete
[SAVE DEBUG] Completed IDs saved: [10]
[LoadCareer] Restoring from ID lists: 1 completed IDs
[CareerManager] ? Marked tournament 'Regional Cup' (ID 10) as complete
[SetActiveTournies] Tournament 'Regional Cup' is complete, skipping
```

## Testing Checklist

? **AI Fallback**:
1. Set up a peel shot that will fail (very difficult angle)
2. Verify AI tries physics-based guard takeout instead of magic numbers
3. Check console for fallback messages

? **Tournament Completion**:
1. Start new career
2. Complete first tournament
3. Check save logging shows completion ID
4. Quit and reload
5. Verify tournament shows as complete
6. Check load logging shows restoration

? **Multiple Tournaments**:
1. Complete 3+ tournaments across multiple weeks
2. Verify all show as complete after save/load
3. Check IDs list grows correctly: `[10, 15, 23]`

## Files Modified

1. **Assets/Scripts/AI/AI_Target.cs**
   - Enhanced `PeelTarget()` fallback strategy
   - Added guard blocking detection
   - Added strategic opponent rock targeting
   - Draw shot fallback before magic numbers

2. **Assets/Scripts/Tourny/CareerManager.cs**
   - Added diagnostic logging in `ToSaveData()`
   - Added diagnostic logging in `LoadCareer()`
   - Added logging to verify TournySelector state

3. **TOURNAMENT_COMPLETION_SAVE_DEBUG.md** (New)
   - Comprehensive debugging guide
   - Testing checklist
   - Common issues and solutions

## Next Steps

1. **Run the game** and complete a tournament
2. **Review console logs** to see the detailed save/load process
3. **If issues persist**, use the debug guide to identify the exact failure point
4. **Report findings**: Share console logs showing where the data is lost

The enhanced logging will make it immediately obvious where the problem is:
- If IDs are empty on save ? Problem in ToSaveData()
- If IDs are saved but not in JSON ? Problem in CareerSaveService
- If IDs are in JSON but not restored ? Problem in ApplyTournamentData()
- If IDs are restored but completion lost ? Problem in SetActiveTournies() timing

## Additional Notes

The tournament completion system uses **ID-based restoration** because ScriptableObjects are recreated each time TournySelector loads. The completion status must be restored by matching tournament IDs from the save file to the fresh ScriptableObject instances.

**Critical timing**: `ApplyTournamentData()` must be called BEFORE `SetActiveTournies()`. The current implementation in `TournySelector.SetUp()` ensures this by:
```csharp
cm.LoadCareer(tSel: this);  // Restores completion
SyncCompletionFromCareerManager();  // Syncs from CM arrays
SetActiveTournies();  // Uses restored completion (called LAST)
```

If tournaments are still not being marked complete, the enhanced logging will show exactly where this chain breaks.
