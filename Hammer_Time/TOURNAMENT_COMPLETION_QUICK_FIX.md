# Tournament Completion - Quick Diagnostic Guide

## Quick Check: Is it working?

Run this test in 5 minutes:

1. **Start new career** (Week 1)
2. **Play and win first tournament**
3. **Check console** - Look for:
   ```
   [SAVE DEBUG] Completed IDs saved: [<some_number>]
   ```
4. **Quit to main menu**
5. **Load career**
6. **Check console** - Look for:
   ```
   [CareerManager] ? Marked tournament '<name>' (ID <number>) as complete
   ```
7. **Check UI** - Tournament should show as complete (grayed out or marked)

## If It's NOT Working

Look at console logs and find which category matches:

### Category 1: Nothing is being saved
**Symptom**: 
```
[SAVE DEBUG] Completed IDs saved: []
```

**Cause**: Tournaments not being marked complete in TournySelector arrays

**Fix Location**: `TournySelector.PlayTourny()` - Verify this code runs:
```csharp
for (int i = 0; i < tournies.Length; i++)
{
    if (activeTournies[j].id == tournies[i].id)
    {
        tournies[i].complete = true;  // ? This must execute!
    }
}
```

---

### Category 2: IDs saved but not in JSON file
**Symptom**:
```
[SAVE DEBUG] Completed IDs saved: [10]
```
But when you open `career_save.json`, you see:
```json
"completedTournamentIDs": []
```

**Cause**: CareerSaveService not writing correctly

**Fix**: Check `CareerSaveService.SaveCareer()` - Verify file write succeeds

---

### Category 3: IDs in JSON but not being loaded
**Symptom**:
JSON file shows:
```json
"completedTournamentIDs": [10]
```
But console shows:
```
[LOAD DEBUG] IDs to restore: 0
```

**Cause**: JSON deserialization failed

**Fix**: Check Unity Console for JSON errors. Verify save file isn't corrupted.

---

### Category 4: IDs loaded but not being applied
**Symptom**:
```
[LOAD DEBUG] Completed IDs to restore: 1
[LOAD DEBUG] Completed IDs: [10]
[CareerManager] Tournament restoration complete: 0 tournaments marked complete
```

**Cause**: Tournament IDs don't match OR arrays are null

**Fix**: Add logging in `ApplyTournamentData()` to check:
```csharp
foreach (var tourny in tSel.tour)
{
    Debug.Log($"Checking tourny '{tourny.name}' ID={tourny.id} vs saved ID 10");
}
```

If IDs don't match, the ScriptableObjects are being recreated with new IDs!

---

### Category 5: Everything loads but UI shows incomplete
**Symptom**:
```
[CareerManager] ? Marked tournament 'Regional Cup' (ID 10) as complete
```
But UI shows tournament as available to play

**Cause**: Timing issue - `SetActiveTournies()` called before restoration

**Fix**: Verify call order in `TournySelector.SetUp()`:
```csharp
cm.LoadCareer(tSel: this);  // Must be FIRST
SyncCompletionFromCareerManager();  // Must be SECOND  
SetActiveTournies();  // Must be LAST
```

---

## Quick Fix Checklist

If you see the issue immediately, try these fixes in order:

1. ? **Verify tournament is marked complete**:
   - Add breakpoint in `TournySelector.PlayTourny()` after marking complete
   - Check `tournies[i].complete` is `true`

2. ? **Verify CM arrays are synced**:
   - Add breakpoint after `cm.tournies = tournies;`
   - Check `cm.tournies[i].complete` is `true`

3. ? **Verify save is called**:
   - Add breakpoint in `cm.SaveCareer()`
   - Step through to `ToSaveData()`
   - Check `data.completedTournamentIDs.Count > 0`

4. ? **Verify JSON file is written**:
   - Open `Application.persistentDataPath/career_save.json`
   - Search for `"completedTournamentIDs"`
   - Verify array has IDs: `[10, 15]`

5. ? **Verify load reads JSON**:
   - Add breakpoint in `CareerManager.LoadCareer()`
   - Check `saveData.completedTournamentIDs.Count > 0`

6. ? **Verify restoration applies to arrays**:
   - Add breakpoint in `ApplyTournamentData()`
   - Step through each tournament check
   - Verify `tourny.complete = true` executes

## Most Common Issue

**90% of the time**, the issue is:

**Tournament IDs change between save and load**

This happens if:
- ScriptableObjects are recreated with auto-increment IDs
- Tournament pool is shuffled differently
- Tournament objects are not persistent assets

**How to verify**: 
```csharp
// On save
Debug.Log($"Saving tournament '{tournies[0].name}' with ID {tournies[0].id}");

// On load  
Debug.Log($"Loaded tournament '{tournies[0].name}' with ID {tournies[0].id}");
```

If IDs are different (e.g., 10 vs 47), that's the problem!

**Fix**: Ensure tournament ScriptableObjects have **fixed, persistent IDs** set in the Unity Inspector, not auto-generated at runtime.

---

## Emergency Fallback

If you can't fix it quickly, add a temporary workaround in `TournySelector.SetActiveTournies()`:

```csharp
// TEMPORARY: Skip all completed tournaments by name
bool IsCompleted(string tournamentName)
{
    if (PlayerPrefs.HasKey($"Completed_{tournamentName}"))
        return PlayerPrefs.GetInt($"Completed_{tournamentName}") == 1;
    return false;
}

// Mark complete using name instead of ID
void MarkCompleteByName(string tournamentName)
{
    PlayerPrefs.SetInt($"Completed_{tournamentName}", 1);
    PlayerPrefs.Save();
}
```

This is not ideal (won't work for multiple careers), but will unblock you while debugging the ID system.

---

## Contact Points

If stuck, share these 3 things:

1. **Console logs** from tournament completion through next load
2. **Contents of career_save.json** (the completedTournamentIDs section)
3. **Tournament IDs** from before and after save/load

Example:
```
SAVE: Tournament 'Regional Cup' ID=10, complete=true
JSON: "completedTournamentIDs": [10]
LOAD: Tournament 'Regional Cup' ID=47, complete=false
```

? Shows the ID is changing! That's the root cause.
