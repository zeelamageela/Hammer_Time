# Legacy Save System Removal Plan

## ? **Status: READY TO EXECUTE**

Build Status: **SUCCESS** ?  
High Score System: **IMPLEMENTED** ?  
JSON Save System: **ACTIVE** ?

---

## **Summary**

This document outlines the complete removal of the legacy EasyFileSave-based save system from `CareerManager.cs`. All functionality has been migrated to the new JSON-based save system (`CareerSaveService.cs`) and a new persistent high score system (`HighScoreService.cs`).

---

## **What Was Completed**

### ? **Phase 1: New High Score System** 
**Files Created:**
1. `Assets/Scripts/Tourny/SaveData/HighScoreData.cs`
   - Data classes for persistent leaderboard
   - Independent of career saves (survives deletion)
   - Stores top 100 all-time careers
   - Tracks all-time trophy cabinet

2. `Assets/Scripts/Tourny/SaveData/HighScoreService.cs`
   - JSON-based save/load for high scores
   - Save path: `Application.persistentDataPath/high_scores.json`
   - Methods: `LoadHighScores()`, `SaveHighScores()`, `AddCareerEntry()`, `GetTopEntries()`, `GetAllTimeTrophies()`

### ? **Phase 2: Code Removal Preparation**
**Modified:**
1. **CareerManager.cs** - Removed legacy save toggle:
   - ? Removed `USE_NEW_SAVE_SYSTEM` constant
   - ? Removed `EasyFileSave myFile` field
   - ? Updated `LoadCareer()` to only use JSON
   - ? Updated `SaveCareer()` to only use JSON
   - ? Updated `SaveHighScore()` to use `HighScoreService`
   - ? Updated `SaveTournyState()` to redirect to JSON system
   - ? Updated `DeleteCareerSave()` to only use JSON
   - ? Updated `SaveFileExists()` to only use JSON
   - ? Updated `GetSaveFileInfo()` to only use JSON

---

## **Legacy Methods to Remove**

The following methods in `CareerManager.cs` are marked `[System.Obsolete]` and can be safely removed:

### **Load Methods (Lines ~379-900)**
```csharp
? LoadCareerLegacy()
? InitializeFileSave()
? LoadActivePlayers()
? LoadGameProgress()
? LoadDialogueStatus()
? LoadCardData()
? LoadTournamentData()
? LoadSponsorManager()
? LoadTeamsFromSave()
? LoadEquipment()
? LoadTourTeamData()
? UpdateCurrentTourny()
? LoadGameSettings()
? LoadCurrentGameState()
? LoadTournyState()
```

### **Save Methods (Lines ~900-1700)**
```csharp
? SaveCareerLegacy()
? SaveGameProgress()
? SaveActivePlayers()
? SaveTeamsToSave()
? SaveDialogueStatus()
? SaveCardData()
? SaveTournamentData()
? SaveTourTeamData()
? SaveTeamDetails()
? SaveEquipment()
? SaveCurrentGameState()
? SavePlayoffState()
? LoadPlayoffState()
```

**Total Lines to Remove:** ~1,300+ lines

---

## **Files That Will Remain**

### **Keep - Still Used:**
- ? `EasyFileSave.cs` - Third-party asset, may be used elsewhere
- ? `MMSaveLoadManager.cs` - MoreMountains save system (separate from career)
- ? `Assets/Scripts/Tourny/SaveData/CareerSaveData.cs` - JSON save data classes
- ? `Assets/Scripts/Tourny/SaveData/CareerSaveService.cs` - JSON save service
- ? `Assets/Scripts/Tourny/SaveData/HighScoreData.cs` - High score data classes
- ? `Assets/Scripts/Tourny/SaveData/HighScoreService.cs` - High score service

---

## **Migration Notes for Users**

### **For Existing Players:**
?? **Old save files will NOT be compatible**

**Before deploying this update:**
1. Players should complete their current careers
2. Consider adding a one-time migration script (optional)
3. Display warning: "Save file format has changed. Old saves will be deleted."

### **For Developers:**
? **All new saves use JSON format**
- Career saves: `career_save.json`
- High scores: `high_scores.json`
- Both in `Application.persistentDataPath`

---

## **Testing Checklist**

Before removing legacy code, verify:

- [x] New career starts correctly
- [x] Career saves after tournament
- [x] Career loads from save
- [x] High scores save on career end
- [x] High scores persist after career deletion
- [x] Trophy cabinet tracks across careers
- [x] No references to `myFile` in active code
- [x] Build compiles successfully

---

## **How to Execute Removal**

Due to the large number of methods to remove (~1,300 lines), here's the recommended approach:

### **Option A: Manual Removal (Safest)**
1. Open `CareerManager.cs` in your IDE
2. Search for `[System.Obsolete]`
3. Delete each marked method
4. Search for `myFile` and verify no remaining references
5. Remove the `EasyFileSave myFile;` field declaration
6. Build and test

### **Option B: Automated Script (Faster, Riskier)**
Create a script to:
1. Find all methods marked `[System.Obsolete]`
2. Delete them from the file
3. Verify compilation

---

## **Post-Removal Verification**

After removing legacy code:

```bash
# Search for any remaining legacy references
grep -r "myFile" Assets/Scripts/Tourny/CareerManager.cs
grep -r "EasyFileSave myFile" Assets/Scripts/Tourny/
grep -r "LoadCareerLegacy" Assets/Scripts/
grep -r "SaveCareerLegacy" Assets/Scripts/

# Should return: NO RESULTS
```

Then build and test:
1. ? Start new career
2. ? Play tournament
3. ? Save career
4. ? Quit to menu
5. ? Load career
6. ? Complete second tournament
7. ? End career
8. ? Verify high score saved
9. ? Start new career
10. ? Verify trophy cabinet persists

---

## **Estimated Impact**

| Metric | Before | After | Change |
|--------|--------|-------|--------|
| **CareerManager.cs Lines** | ~3,200 | ~1,900 | **-40%** |
| **Legacy Save Methods** | 27 | 0 | **-100%** |
| **Save System Dependencies** | 2 | 1 | **-50%** |
| **Code Complexity** | High | Medium | **Simplified** |

---

## **Final Notes**

? **This is a clean, safe removal**
- All functionality preserved
- High score system improved
- No loss of features
- Better maintainability

?? **Breaking Change**
- Old saves will NOT load
- Players must start new careers
- Consider migration period

?? **Recommended Deployment:**
1. Mark as **major version update** (e.g., v2.0.0)
2. Add update notes explaining save format change
3. Consider backup/export feature for old saves (optional)
4. Test extensively before release

---

**Created:** 2024
**Status:** Ready for execution
**Risk Level:** Medium (breaking changes for old saves)
**Estimated Time:** 1-2 hours for removal + testing
