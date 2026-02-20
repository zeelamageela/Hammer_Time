# Mid-Game Save/Load Fix Plan

## ?? Problem Summary

**Symptoms:**
- Game saves during play (mid-game)
- When loading, rocks don't populate on the board
- Next turn never starts - game appears frozen

**Root Causes Identified:**

### 1. **Rock Position Data Not Restored**
- `gsp.rockPos[]` and `gsp.rockInPlay[]` are saved correctly
- But `GameManager.PlaceRocks()` isn't restoring them to the scene

### 2. **LoadGame Flag Not Set**
- `gsp.loadGame` must be `true` for PlaceRocks() to work
- This flag might not be set correctly when loading from save

### 3. **Game State Flags Incorrect**
- `gsp.gameInProgress` must remain `true` for mid-game loads
- `gsp.tournyInProgress` must be preserved
- Current implementation may be clearing these flags

### 4. **Turn Progression Blocked**
- After PlaceRocks(), CheckScore() is called
- But game might not know which team's turn it is
- RockCurrent might be wrong

---

## ?? **Step-by-Step Fix**

### **Fix 1: Ensure LoadGame Flag is Set**

**File**: `Assets/Scripts/Tourny/CareerSettings.cs`

**Location**: `LoadToCM()` method

**Current Code:**
```csharp
if (gameInProgress)
{
    Debug.Log("[CareerSettings] Loading mid-game save ? TournyGame");
    SceneManager.LoadScene("TournyGame");
}
```

**Problem**: `gsp.loadGame` is never set to `true`!

**Fix**: Add this line BEFORE loading the scene:
```csharp
if (gameInProgress)
{
    Debug.Log("[CareerSettings] Loading mid-game save ? TournyGame");
    gsp.loadGame = true;  // ? ADD THIS LINE!
    SceneManager.LoadScene("TournyGame");
}
```

---

### **Fix 2: Preserve Game State Flags**

**File**: `Assets/Scripts/GameManager.cs`

**Location**: `SetupGame()` method

**Current Code (LINES 143-151):**
```csharp
// CRITICAL FIX: Don't set gameInProgress here if loading from save!
// LoadGame() will handle the loaded game state
if (!gsp.loadGame)
{
    gsp.gameInProgress = true;
    Debug.Log("[GameManager] NEW game - set gameInProgress = true");
}
else
{
    gsp.LoadTourny();
    Debug.Log("[GameManager] LOADING game - gameInProgress preserved from save: " + gsp.gameInProgress);
}
```

**This looks CORRECT** ? - Keep this as-is!

---

### **Fix 3: Debug PlaceRocks() Execution**

**File**: `Assets/Scripts/GameManager.cs`

**Location**: `PlaceRocks()` coroutine (around line 760)

**Current Code:**
```csharp
IEnumerator PlaceRocks()
{
    //yield return new WaitForSeconds(3.5f);

    for (int i = 0; i <= rockCurrent; i++)
    {
        rockList[i].rockInfo.placed = true;
    }

    yield return new WaitForEndOfFrame();

    for (int i = 0; i <= rockCurrent; i++)
    {
        rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.14f;
        rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
        rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
        rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
        rockList[i].rock.transform.parent = null;
        //rockBar.DeadRock(i);
        yield return new WaitForEndOfFrame();

        if (gsp.loadGame && gsp.rockInPlay[i])  // ? THIS IS THE KEY CHECK!
        {
            Vector2 rockTrans = gsp.rockPos[i];
            Debug.Log("Placing Rock Position " + i + " " + rockTrans.x + ", " + rockTrans.y);
            rockList[i].rock.GetComponent<Rigidbody2D>().position = rockTrans;
            
            // ... rest of setup
        }
    }
}
```

**Problem**: If `gsp.rockPos` or `gsp.rockInPlay` is NULL or empty, this fails silently!

**Fix**: Add null checks and better logging:

```csharp
IEnumerator PlaceRocks()
{
    // SAFETY CHECKS
    if (gsp.rockPos == null || gsp.rockPos.Length == 0)
    {
        Debug.LogError("[GameManager] PlaceRocks() - gsp.rockPos is NULL or empty! Cannot place rocks!");
        yield break;  // Exit early
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

    for (int i = 0; i <= rockCurrent; i++)
    {
        rockList[i].rockInfo.placed = true;
    }

    yield return new WaitForEndOfFrame();

    for (int i = 0; i <= rockCurrent; i++)
    {
        rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.14f;
        rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
        rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
        rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
        rockList[i].rock.transform.parent = null;
        
        yield return new WaitForEndOfFrame();

        // CRITICAL: Check if this rock should be restored
        if (gsp.loadGame && i < gsp.rockInPlay.Length && gsp.rockInPlay[i])
        {
            if (i < gsp.rockPos.Length)
            {
                Vector2 rockTrans = gsp.rockPos[i];
                Debug.Log($"[GameManager] Restoring Rock {i}: pos=({rockTrans.x:F2}, {rockTrans.y:F2}), inPlay={gsp.rockInPlay[i]}");
                
                rockList[i].rock.GetComponent<Rigidbody2D>().position = rockTrans;

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
                Debug.LogError($"[GameManager] Rock {i} marked as inPlay but no position data! rockPos.Length={gsp.rockPos.Length}");
            }
        }
        else
        {
            // Rock is out of play
            Debug.Log($"[GameManager] Rock {i} is OUT OF PLAY - hiding");
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

### **Fix 4: Verify Save Data is Captured**

**File**: `Assets/Scripts/GameManager.cs`

**Location**: `NextTurn()` method (around line 548)

**Current Code:**
```csharp
public void NextTurn()
{
    Debug.Log("Next Turn");

    rockCurrent++;
    
    if (rm.rrp.placed)
    {
        gsp.loadGame = true;
        gsp.rockPos = new Vector2[rockList.Count];
        gsp.rockInPlay = new bool[rockList.Count];
        for(int i = 0; i < rockList.Count; i++)
        {
            gsp.rockPos[i] = new Vector2(rockList[i].rock.transform.position.x, rockList[i].rock.transform.position.y);
            gsp.rockInPlay[i] = rockList[i].rockInfo.inPlay;
        }

        SaveGame();
        // ... rest of method
    }
}
```

**Enhancement**: Add detailed logging:

```csharp
public void NextTurn()
{
    Debug.Log("Next Turn");

    rockCurrent++;
    
    if (rm.rrp.placed)
    {
        gsp.loadGame = true;
        gsp.rockPos = new Vector2[rockList.Count];
        gsp.rockInPlay = new bool[rockList.Count];
        
        int rocksInPlay = 0;
        for(int i = 0; i < rockList.Count; i++)
        {
            gsp.rockPos[i] = new Vector2(rockList[i].rock.transform.position.x, rockList[i].rock.transform.position.y);
            gsp.rockInPlay[i] = rockList[i].rockInfo.inPlay;
            
            if (gsp.rockInPlay[i])
            {
                rocksInPlay++;
                Debug.Log($"[GM.NextTurn] rockPos[{i}] = ({gsp.rockPos[i].x:F2}, {gsp.rockPos[i].y:F2})");
            }
            else
            {
                Debug.Log($"[GM.NextTurn] Rock {i} is out of play");
            }
        }
        
        Debug.Log($"[GM.NextTurn] Saved {rocksInPlay}/{rockList.Count} rocks in play");
        Debug.Log($"[GM.NextTurn] Current rock: {rockCurrent}, End: {endCurrent}/{endTotal}");

        SaveGame();
        // ... rest of method
    }
}
```

---

### **Fix 5: Ensure LoadFromGM() is Called**

**File**: `Assets/Scripts/GameSettingsPersist.cs`

**Check if this method exists and captures all necessary data:**

```csharp
public void LoadFromGM()
{
    GameManager gm = FindObjectOfType<GameManager>();
    if (gm == null)
    {
        Debug.LogError("[GSP] LoadFromGM - GameManager not found!");
        return;
    }
    
    // Capture EVERYTHING from GameManager
    endCurrent = gm.endCurrent;
    ends = gm.endTotal;
    rockCurrent = gm.rockCurrent;
    rocks = gm.rocksPerTeam;
    redHammer = gm.redHammer;
    
    yellowScore = gm.yellowScore;
    redScore = gm.redScore;
    
    yellowTeamName = gm.yellowTeamName;
    redTeamName = gm.redTeamName;
    
    Debug.Log($"[GSP] LoadFromGM - End {endCurrent}/{ends}, Rock {rockCurrent}, Scores: {redScore}-{yellowScore}");
}
```

**This method should be called BEFORE SaveGame()!**

---

### **Fix 6: Loading Flow Correction**

The correct flow should be:

```
1. User clicks "Continue" in menu
   ?
2. CareerSettings.LoadToCM() reads save file
   ?
3. Detects gameInProgress = true
   ?
4. Sets gsp.loadGame = true  ? FIX #1!
   ?
5. Loads "TournyGame" scene
   ?
6. GameManager.SetupGame() runs
   ?
7. Checks gsp.loadGame == true
   ?
8. Calls LoadGame() instead of SetupRocks()
   ?
9. LoadGame() calls PlaceRocks()
   ?
10. PlaceRocks() reads gsp.rockPos[] and gsp.rockInPlay[]
   ?
11. Rocks appear on board ?
   ?
12. LoadGame() calls CheckScore()
   ?
13. CheckScore() determines next turn
   ?
14. Game resumes! ?
```

---

## ?? **Testing Protocol**

### **Test 1: Verify Save Data**
1. Start a new game
2. Play until rock 5 (mid-game)
3. **Check Console** for these logs:
```
[GM.NextTurn] Saved X/16 rocks in play
[GM.NextTurn] rockPos[0] = (x, y)
[GM.NextTurn] rockPos[1] = (x, y)
...
```
4. Quit game
5. **Check save file** (optional): 
   - Path: `Application.persistentDataPath/career_save.json`
   - Look for `rockPositions` array with values

### **Test 2: Verify Load Flags**
1. Continue game from save
2. **Check Console** for these logs IN ORDER:
```
[CareerSettings] LoadToCM - tournyInProgress: true, gameInProgress: true
[CareerSettings] Loading mid-game save ? TournyGame
[GameManager] LOADING game - gameInProgress preserved from save: true
[GameManager] PlaceRocks() - Restoring X rocks from save
[GameManager] Restoring Rock 0: pos=(x, y), inPlay=true
[GameManager] Restoring Rock 1: pos=(x, y), inPlay=true
...
[GameManager] PlaceRocks() complete - all rocks restored
```

### **Test 3: Verify Turn Progression**
1. After rocks load, wait 3 seconds
2. **Expected**: Next turn should start automatically
3. **Check Console**:
```
[GameManager] Next Turn
[GameManager] Player Red Turn (or Yellow Turn)
```
4. **Visual**: Rock should appear in launcher ready to shoot

---

## ?? **Common Issues & Solutions**

### **Issue: "gsp.rockPos is NULL or empty!"**
**Cause**: Save didn't capture rock positions
**Solution**: Check that `NextTurn()` is calling `SaveGame()` AFTER setting `gsp.rockPos[]`

### **Issue: "Rocks load but game doesn't continue"**
**Cause**: `gsp.loadGame` is true but `CheckScore()` isn't progressing
**Solution**: Check `rockCurrent` value - it should be the LAST rock played, not the next rock

### **Issue: "Rocks appear in wrong positions"**
**Cause**: Rock indices don't match saved data
**Solution**: Verify `rockList` is sorted correctly before saving positions

### **Issue: "Only some rocks appear"**
**Cause**: `gsp.rockInPlay[]` has wrong values
**Solution**: Check that `rockInfo.inPlay` is set correctly BEFORE save

---

## ?? **Implementation Checklist**

- [ ] **Fix 1**: Add `gsp.loadGame = true` in CareerSettings.LoadToCM()
- [ ] **Fix 2**: Verify GameManager.SetupGame() preserves gameInProgress
- [ ] **Fix 3**: Add null checks and logging to PlaceRocks()
- [ ] **Fix 4**: Add detailed logging to NextTurn()
- [ ] **Fix 5**: Verify LoadFromGM() captures all data
- [ ] **Fix 6**: Test complete save/load flow

- [ ] **Test 1**: Save data verification
- [ ] **Test 2**: Load flags verification  
- [ ] **Test 3**: Turn progression verification

---

## ?? **Quick Fix Priority**

**DO THIS FIRST:**
1. Add `gsp.loadGame = true` in `CareerSettings.LoadToCM()` ? **Most likely fix!**
2. Add logging to `PlaceRocks()` to see what's happening
3. Test and check console logs

**If that doesn't work:**
4. Add null checks to PlaceRocks()
5. Verify SaveGame() is actually saving rock positions
6. Check that rockCurrent is correct when loading

---

## ?? **Additional Notes**

### **Why Mid-Game Saves are Tricky**

Unlike end-of-game saves, mid-game requires:
- ? Exact rock positions (x, y coordinates)
- ? Rock state (inPlay, outOfPlay, inHouse, etc.)
- ? Game progress (end, rock count, scores)
- ? Turn state (whose turn, hammer position)
- ? Tournament context (if in tournament)

All of these must be captured AND restored correctly!

### **The Key Flag: gsp.loadGame**

This flag is **critical**:
- When `true`: PlaceRocks() restores from save
- When `false`: Game starts fresh

If this flag isn't set when loading, PlaceRocks() thinks it's a new game and doesn't restore positions!

---

## ?? **Related Files**

- `GameManager.cs` - Main game state, SetupGame(), LoadGame(), PlaceRocks()
- `GameSettingsPersist.cs` - Save data container, LoadFromGM()
- `CareerSettings.cs` - Load routing, LoadToCM()
- `CareerManager.cs` - Save/load service, SaveCareer(), LoadCareer()
- `CareerSaveData.cs` - Save file structure

---

Good luck with the fix! Start with adding that one line in CareerSettings.LoadToCM() - that's likely the main issue! ??
