# Mid-Game Save/Load - Implementation Guide

## ?? **Critical Issue Identified**

After analyzing your codebase, I found the **root cause**:

### **The Problem**
1. `GameManager.NextTurn()` saves rock positions to `gsp.rockPos[]` ?
2. `CareerSettings.LoadToCM()` sets `gsp.loadGame = true` ?  
3. **BUT** `GameManager.PlaceRocks()` is **NOT reading from `gsp.rockPos[]`** to restore positions! ?

Looking at `LoadGame.cs`, the `PlaceRocks()` method doesn't restore positions at all - it just sets up rock states without actual position data!

---

## ?? **The Fix**

You need to modify `GameManager.PlaceRocks()` to **actually restore rock positions from the save data**.

### **Step 1: Locate PlaceRocks() in GameManager.cs**

Search for this method in your `GameManager.cs` file (it's probably around line 750-850).

### **Step 2: Replace PlaceRocks() Implementation**

The method should look like this:

```csharp
IEnumerator PlaceRocks()
{
    // SAFETY CHECKS
    if (gsp.rockPos == null || gsp.rockPos.Length == 0)
    {
        Debug.LogError("[GameManager] PlaceRocks() - gsp.rockPos is NULL or empty! Cannot place rocks!");
        yield break;
    }
    
    if (gsp.rockInPlay == null || gsp.rockInPlay.Length == 0)
    {
        Debug.LogError("[GameManager] PlaceRocks() - gsp.rockInPlay is NULL or empty! Cannot place rocks!");
        yield break;
    }
    
    Debug.Log($"[GameManager] PlaceRocks() - Restoring {rockCurrent + 1} rocks from save");
    Debug.Log($"[GameManager] gsp.loadGame = {gsp.loadGame}");
    Debug.Log($"[GameManager] gsp.rockPos.Length = {gsp.rockPos.Length}");
    Debug.Log($"[GameManager] gsp.rockInPlay.Length = {gsp.rockInPlay.Length}");
    Debug.Log($"[GameManager] rockList.Count = {rockList.Count}");

    // Mark rocks as placed
    for (int i = 0; i <= rockCurrent; i++)
    {
        if (i < rockList.Count)
        {
            rockList[i].rockInfo.placed = true;
        }
    }

    yield return new WaitForEndOfFrame();

    // Configure rocks and restore positions
    for (int i = 0; i <= rockCurrent; i++)
    {
        if (i >= rockList.Count)
        {
            Debug.LogError($"[GameManager] Rock index {i} out of bounds (rockList.Count = {rockList.Count})");
            continue;
        }

        // Basic rock setup
        rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.14f;
        rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
        rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
        rockList[i].rock.transform.parent = null;
        
        yield return new WaitForEndOfFrame();

        // CRITICAL: Restore position from save data
        if (gsp.loadGame && i < gsp.rockInPlay.Length && i < gsp.rockPos.Length)
        {
            if (gsp.rockInPlay[i])
            {
                // Rock is IN PLAY - restore position
                Vector2 rockTrans = gsp.rockPos[i];
                Debug.Log($"[GameManager] Restoring Rock {i}: pos=({rockTrans.x:F2}, {rockTrans.y:F2}), inPlay=true");
                
                rockList[i].rock.GetComponent<Rigidbody2D>().position = rockTrans;
                rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
                rockList[i].rock.GetComponent<CircleCollider2D>().enabled = true;
                rockList[i].rock.GetComponent<Rock_Release>().enabled = true;
                rockList[i].rock.GetComponent<Rock_Force>().enabled = true;
                rockList[i].rock.GetComponent<Rock_Colliders>().enabled = true;
                
                rockList[i].rockInfo.inPlay = true;
                rockList[i].rockInfo.outOfPlay = false;
                rockList[i].rockInfo.moving = false;
                rockList[i].rockInfo.shotTaken = true;
                rockList[i].rockInfo.released = true;
                rockList[i].rockInfo.stopped = true;
                rockList[i].rockInfo.rest = true;
            }
            else
            {
                // Rock is OUT OF PLAY - hide it
                Debug.Log($"[GameManager] Rock {i} is OUT OF PLAY - hiding");
                rockList[i].rock.SetActive(false);
                rockList[i].rockInfo.inPlay = false;
                rockList[i].rockInfo.outOfPlay = true;
            }
        }
        else
        {
            // No save data or index out of range - mark as out of play
            Debug.LogWarning($"[GameManager] No save data for rock {i} - marking as out of play");
            rockList[i].rock.SetActive(false);
            rockList[i].rockInfo.inPlay = false;
            rockList[i].rockInfo.outOfPlay = true;
        }

        yield return new WaitForEndOfFrame();
    }

    yield return new WaitForEndOfFrame();
    rm.rrp.placed = true;
    
    Debug.Log("[GameManager] PlaceRocks() complete - all rocks restored");
}
```

---

## ?? **Testing Protocol**

### **Test 1: Verify Save Captures Data**
1. Start a new game
2. Play until rock 5
3. **Open Console** - you should see:
```
[GM.NextTurn] Saved 5/16 rocks in play
[GM.NextTurn] rockPos[0] = (x, y)
[GM.NextTurn] rockPos[1] = (x, y)
...
```
4. If you see these logs, save is working ?
5. If NOT, the problem is in `NextTurn()` or `SaveGame()`

### **Test 2: Verify Load Restores Data**
1. Continue from save
2. **Watch Console** for these logs IN ORDER:
```
[CareerSettings] LoadToCM - gameInProgress: true
[CareerSettings] Loading mid-game save ? TournyGame
[GameManager] LOADING game - gameInProgress preserved
[GameManager] PlaceRocks() - Restoring 5 rocks from save
[GameManager] Restoring Rock 0: pos=(x, y), inPlay=true
[GameManager] Restoring Rock 1: pos=(x, y), inPlay=true
...
[GameManager] PlaceRocks() complete - all rocks restored
```
3. **Visual Check**: Do rocks appear on the board in correct positions?
4. **Turn Check**: Does next turn start after 2-3 seconds?

### **Test 3: Verify Turn Progression**
1. After rocks load, wait
2. **Expected**: Next rock should appear in launcher
3. **Check Console**:
```
[GameManager] Next Turn
[GameManager] Player Red Turn (or Yellow Turn)
```

---

## ?? **Common Issues & Fixes**

### **Issue: "gsp.rockPos is NULL or empty!"**

**Diagnosis**:
- Save didn't capture rock positions
- OR save file is corrupted

**Fix Steps**:
1. Check `NextTurn()` - is it calling `SaveGame()` AFTER setting `gsp.rockPos[]`?
2. Check `SaveGame()` - is it calling `gsp.LoadFromGM()` first?
3. Check save file exists: `Application.persistentDataPath/career_save.json`

### **Issue: "Rocks appear but in wrong positions"**

**Diagnosis**:
- Rock indices don't match between save and load
- OR `rockList` isn't sorted correctly

**Fix Steps**:
1. Check `rockList` is sorted before saving: `rockList.Sort()`
2. Check rock indices match: `rockList[i].rockInfo.rockIndex` should equal `i`
3. Verify positions in save file match actual rock positions

### **Issue: "Some rocks missing after load"**

**Diagnosis**:
- `rockInPlay[]` has wrong values
- OR rocks marked as out of play incorrectly

**Fix Steps**:
1. Check `NextTurn()` sets `gsp.rockInPlay[i] = rockList[i].rockInfo.inPlay`
2. Verify `rockInfo.inPlay` is correct BEFORE save
3. Check array lengths match: `rockPos.Length == rockInPlay.Length`

### **Issue: "Rocks load but turn doesn't start"**

**Diagnosis**:
- `CheckScore()` not progressing to next turn
- OR `rockCurrent` is wrong value

**Fix Steps**:
1. Check `rockCurrent` value after load - should be LAST rock played
2. Check `endCurrent` - should be current end number
3. Verify `CheckScore()` logic handles mid-end state
4. Check `NextTurn()` is eventually called

---

## ?? **Files You Need to Modify**

### **Primary File:**
- `Assets/Scripts/GameManager.cs` - Implement new `PlaceRocks()` method

### **Files to Check (probably already correct):**
- `Assets/Scripts/Tourny/CareerSettings.cs` - Verify `gsp.loadGame = true` is set
- `Assets/Scripts/GameManager.cs` - Verify `SetupGame()` preserves `gameInProgress`
- `Assets/Scripts/GameManager.cs` - Verify `NextTurn()` saves rock positions
- `Assets/Scripts/GameSettingsPersist.cs` - Verify `LoadFromGM()` captures all data

---

## ?? **Complete Save/Load Flow**

### **SAVE Flow (Mid-Game):**
```
1. Player plays several rocks
   ?
2. GameManager.NextTurn() called after each rock
   ?
3. NextTurn() sets:
   - gsp.rockPos[] = rock positions
   - gsp.rockInPlay[] = rock states
   ?
4. NextTurn() calls SaveGame()
   ?
5. SaveGame() calls gsp.LoadFromGM()
   - Captures: endCurrent, rockCurrent, scores, etc.
   ?
6. SaveGame() calls gsp.AutoSave()
   ?
7. gsp.AutoSave() calls cm.SaveCareer()
   ?
8. cm.SaveCareer() writes to JSON file
   ?
9. Data is safely saved ?
```

### **LOAD Flow (Mid-Game):**
```
1. User clicks "Continue" in menu
   ?
2. CareerSettings.Start() runs
   ?
3. cm.LoadCareer() reads JSON file
   - Restores ALL save data including rockPos[], rockInPlay[], etc.
   ?
4. CareerSettings.LoadToCM() checks flags
   ?
5. Detects gsp.gameInProgress == TRUE
   ?
6. Sets gsp.loadGame = TRUE
   ?
7. Loads "TournyGame" scene
   ?
8. GameManager.SetupGame() runs
   ?
9. Checks gsp.loadGame == TRUE
   ?
10. Calls LoadGame() instead of SetupRocks()
   ?
11. LoadGame() calls PlaceRocks()
   ?
12. PlaceRocks() reads gsp.rockPos[] and gsp.rockInPlay[]
   ?
13. PlaceRocks() restores each rock:
    - Sets position from gsp.rockPos[i]
    - Sets state from gsp.rockInPlay[i]
    - Enables/disables based on state
   ?
14. Rocks appear on board ?
   ?
15. LoadGame() calls CheckScore()
   ?
16. CheckScore() updates house list, guards
   ?
17. CheckScore() calls NextTurn() or Scoring()
   ?
18. Game resumes! ?
```

---

## ?? **Why This Fix Works**

### **Before (Broken):**
- `PlaceRocks()` didn't use `gsp.rockPos[]` at all
- It only set up rock states without actual positions
- Result: Rocks invisible or in wrong places

### **After (Fixed):**
- `PlaceRocks()` reads `gsp.rockPos[]` for each rock
- It sets `Rigidbody2D.position` to saved coordinates
- It uses `gsp.rockInPlay[]` to show/hide rocks correctly
- Result: Rocks appear exactly where they were saved ?

---

## ?? **Quick Fix Summary**

**If you only do ONE thing, do this:**

1. Find `IEnumerator PlaceRocks()` in `GameManager.cs`
2. Replace it with the implementation shown above
3. Build and test

That should fix 90% of the issue!

---

## ?? **Support Checklist**

If it still doesn't work after implementing the fix:

**Provide these logs:**
1. Console output when saving (look for `[GM.NextTurn]` logs)
2. Console output when loading (look for `[GameManager] PlaceRocks()` logs)
3. Values of these variables when loading:
   - `gsp.loadGame` (should be true)
   - `gsp.gameInProgress` (should be true)
   - `gsp.rockPos.Length` (should equal number of rocks)
   - `gsp.rockInPlay.Length` (should equal number of rocks)
   - `rockCurrent` (should be last rock played)

**Also helpful:**
- Does the save file exist? (Check `Application.persistentDataPath/career_save.json`)
- What scene does it load to? (Should be "TournyGame")
- Do ANY rocks appear? (Even in wrong positions)

---

Good luck! This fix should resolve your mid-game save/load issue. The core problem was that `PlaceRocks()` wasn't actually using the saved position data. ??
