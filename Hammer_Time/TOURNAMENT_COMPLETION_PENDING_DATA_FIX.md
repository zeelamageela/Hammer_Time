# Tournament Completion - Corrected Pending Data Fix

## Your Insight Was Correct!

You identified a **critical flaw** in the previous fix: trying to restore completion data to CM arrays that are **stale/reset/empty**. This would accomplish nothing.

## The Problem with the Previous Approach

**Previous Fix (WRONG)**:
```csharp
// Try to restore to CM arrays when TournySelector doesn't exist
RestoreCompletionToCMArrays(saveData);  // ? CM arrays are stale!
```

**Why it failed**:
- CM arrays (`cm.tournies`, `cm.tour`, `cm.prov`) are either NULL or freshly reset
- They contain ScriptableObject instances with `complete = false`
- The tournament IDs might not even match
- We're essentially trying to restore to empty/wrong data

## The Corrected Fix

**New Approach (CORRECT)**:
```csharp
// Store completion IDs in CareerManager fields until TournySelector exists
private List<int> pendingCompletedTournamentIDs;
private List<int> pendingTrophyWonIDs;
private bool pendingTourChampionshipComplete;
private bool pendingProvChampionshipComplete;
```

### How It Works

1. **On Load (No TournySelector)**:
   ```csharp
   LoadCareerJSON() {
       if (tSel == null) {
           // Store IDs in CareerManager fields
           StorePendingCompletionData(saveData);
       }
   }
   ```

2. **When TournySelector Loads**:
   ```csharp
   TournySelector.SetUp() {
       cm.LoadCareer(tSel: this);
       cm.ApplyPendingCompletionData(this);  // ? Apply stored IDs here!
       SetActiveTournies();
   }
   ```

3. **On Auto-Save (Before TournySelector)**:
   ```csharp
   ToSaveData() {
       if (tSel == null) {
           if (pendingCompletedTournamentIDs.Count > 0) {
               // Use pending data instead of CM arrays!
               data.completedTournamentIDs = pendingCompletedTournamentIDs;
           }
       }
   }
   ```

## Key Differences

### Old Approach (WRONG)
- ? Tried to restore to stale CM arrays
- ? CM arrays get overwritten on auto-save (empty)
- ? Completion data lost

### New Approach (CORRECT)
- ? Stores IDs in CareerManager fields (persistent)
- ? Auto-save uses pending data (not empty arrays)
- ? TournySelector gets IDs when it loads
- ? Completion data preserved through entire lifecycle

## The Flow

### Scenario 1: Load from Main Menu (No TournySelector)

```
1. CareerSettings.Start() calls cm.LoadCareer()
   ?
2. LoadCareerJSON() ? StorePendingCompletionData()
   - pendingCompletedTournamentIDs = [0]  ? Stored in CareerManager!
   ?
3. Auto-save triggers (30 seconds later)
   ?
4. ToSaveData() ? Uses pendingCompletedTournamentIDs
   - Saves: completedTournamentIDs: [0]  ? NOT empty!
   ?
5. TournySelector scene loads
   ?
6. TournySelector.SetUp() calls cm.ApplyPendingCompletionData()
   - tournies[0].complete = true  ? Applied from pending!
   - pendingCompletedTournamentIDs.Clear()  ? Consumed
```

### Scenario 2: Load in TournySelector (TournySelector Exists)

```
1. TournySelector.SetUp() calls cm.LoadCareer(tSel: this)
   ?
2. LoadCareerJSON() ? ApplyTournamentData(tSel, saveData)
   - tournies[0].complete = true  ? Applied directly!
   ?
3. No pending data needed (immediate application)
```

## Code Changes

### 1. Added Pending Data Fields to CareerManager

```csharp
// CRITICAL: Preserve completion IDs between load and TournySelector creation
private List<int> pendingCompletedTournamentIDs = new List<int>();
private List<int> pendingTrophyWonIDs = new List<int>();
private bool pendingTourChampionshipComplete = false;
private bool pendingProvChampionshipComplete = false;
```

### 2. Replaced RestoreCompletionToCMArrays() with StorePendingCompletionData()

**Before** (WRONG):
```csharp
private void RestoreCompletionToCMArrays(CareerSaveData saveData)
{
    // Try to restore to stale CM arrays - WRONG!
    foreach (var tourny in tournies) {
        if (saveData.completedTournamentIDs.Contains(tourny.id)) {
            tourny.complete = true;  // ? But 'tournies' is stale/empty!
        }
    }
}
```

**After** (CORRECT):
```csharp
private void StorePendingCompletionData(CareerSaveData saveData)
{
    // Store IDs in CareerManager fields - CORRECT!
    pendingCompletedTournamentIDs = new List<int>(saveData.completedTournamentIDs);
    pendingTrophyWonIDs = new List<int>(saveData.trophyWonIDs);
    // These IDs will be used by TournySelector.SetUp() later
}
```

### 3. Added ApplyPendingCompletionData() Method

```csharp
public void ApplyPendingCompletionData(TournySelector tSel)
{
    if (pendingCompletedTournamentIDs.Count == 0) return;
    
    // NOW we can restore - TournySelector arrays are fresh and correct!
    foreach (var tourny in tSel.tournies) {
        if (pendingCompletedTournamentIDs.Contains(tourny.id)) {
            tourny.complete = true;  // ? 'tSel.tournies' is fresh!
        }
    }
    
    // Clear pending data after applying
    pendingCompletedTournamentIDs.Clear();
}
```

### 4. TournySelector Calls ApplyPendingCompletionData()

```csharp
// In TournySelector.SetUp()
cm.LoadCareer(tSel: this);
cm.ApplyPendingCompletionData(this);  // ? NEW: Apply pending IDs
SyncCompletionFromCareerManager();
SetActiveTournies();
```

## Why This Works

### The Pending Data is Always Valid

1. **On Load**: IDs are stored in `pendingCompletedTournamentIDs` (CareerManager fields)
2. **On Auto-Save**: IDs are saved from `pendingCompletedTournamentIDs` (not empty!)
3. **On TournySelector Load**: IDs are applied from `pendingCompletedTournamentIDs` to fresh arrays
4. **After Application**: Pending data is cleared (consumed)

### The Chain of Custody

```
JSON File
   ? (Load)
CareerManager.pendingCompletedTournamentIDs  ? Temporary storage
   ? (Auto-Save)
JSON File  ? Preserved!
   ? (TournySelector.SetUp)
TournySelector.tournies[].complete = true  ? Applied to fresh arrays
```

The completion data is **never lost** because it's stored in CareerManager fields, not in stale arrays.

## Testing

Run the same test:

1. **Complete tournament** ? Save shows: `[0]`
2. **Quit to main menu**
3. **Check logs** - Should show:
   ```
   [CareerManager] StorePendingCompletionData - Preserving completion IDs
     Stored 1 completed IDs: [0]
   ```
4. **Wait 30 seconds** (auto-save)
5. **Check logs** - Should show:
   ```
   [CareerManager] Using pending completion data (1 IDs)
   [SAVE DEBUG] Completed IDs saved: [0]  ? NOT empty!
   ```
6. **Load career**
7. **Check logs** - Should show:
   ```
   [CareerManager] ApplyPendingCompletionData
     ? Marked tournament 'The Fall Rookie Invitational' (ID 0) as complete
   ```

## Summary

**Your insight was spot-on**: We can't restore to CM arrays because they're stale/reset.

**The correct solution**: Store completion IDs in **CareerManager fields** as a temporary buffer:
- ? Independent of CM arrays
- ? Survives auto-save
- ? Applied to fresh TournySelector arrays when they exist
- ? Completion data never lost

This is a **proper buffer pattern** - the pending data acts as a **holding area** between the JSON file and TournySelector arrays, preventing data loss during the gap when TournySelector doesn't exist yet.
