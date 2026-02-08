# Page Playoff Button Flow - FINAL IMPLEMENTATION

## ? **Correct Button Logic**

### **The Rules:**
1. **Player HAS a game** ? **Play button ONLY**
2. **NO human players** (all AI) ? **Sim button ONLY**
3. **Player has BYE** ? **Sim button ONLY**
4. **Player knocked out** ? **Sim button ONLY**
5. **After simulation** ? **Continue button ONLY** (to advance to next round)
6. **Tournament complete** ? **Next button ONLY** (to exit tournament)

---

## **Implementation Details**

### **1. ConfigurePagePlayoffButtons() Method**
```csharp
void ConfigurePagePlayoffButtons(bool playerHasGame)
{
    if (playerHasGame)
    {
        // Player has a game - show ONLY Play button
        playButton.gameObject.SetActive(true);
        simButton.gameObject.SetActive(false);
        contButton.gameObject.SetActive(false);
    }
    else
    {
        // Player knocked out, has BYE, or all AI games - show ONLY Sim button
        playButton.gameObject.SetActive(false);
        simButton.gameObject.SetActive(true);
        contButton.gameObject.SetActive(false);
    }
}
```

**Parameters:**
- `playerHasGame`: `true` if player has an actual game to play (not knocked out, not BYE)

---

## **Button States by Scenario**

### **Scenario 1: Player in Round 1**
- **Setup:** Player is seeded 1-4, has an opponent
- **Buttons:** **Play ONLY**
- **User Action:** Click Play ? Goes to game
- **Code:** `ConfigurePagePlayoffButtons(true)`

---

### **Scenario 2: Player Has BYE (Round 2)**
- **Setup:** Player won 1v2 match, gets BYE to finals
- **VS Display:** Shows "BYE TO FINALS"
- **Buttons:** **Sim ONLY**
- **User Action:** Click Sim ? Simulates 5v6 match ? Shows Continue button
- **Code:** `ConfigurePagePlayoffButtons(false)` (hasActualGame = false)

---

### **Scenario 3: Player Knocked Out**
- **Setup:** Player lost and is eliminated
- **VS Display:** Shows "Knocked Out!" (or similar)
- **Buttons:** **Sim ONLY**
- **User Action:** Click Sim ? Simulates remaining games ? Shows Continue button
- **Code:** `ConfigurePagePlayoffButtons(false)`

---

### **Scenario 4: All AI Games**
- **Setup:** Player hasn't qualified or is watching other games
- **Buttons:** **Sim ONLY**
- **User Action:** Click Sim ? Simulates all games ? Shows Continue button
- **Code:** `ConfigurePagePlayoffButtons(false)`

---

### **Scenario 5: After Simulation**
- **Setup:** User clicked Sim, round is complete
- **Buttons:** **Continue ONLY**
- **User Action:** Click Continue ? Advances to next round
- **Code:** Set manually in `SimPlayoff()` after `SetPlayoffs()`:
  ```csharp
  playButton.gameObject.SetActive(false);
  simButton.gameObject.SetActive(false);
  contButton.gameObject.SetActive(true);
  ```

---

### **Scenario 6: Tournament Complete (Round 4)**
- **Setup:** Finals are over, champion determined
- **Buttons:** **Next ONLY**
- **User Action:** Click Next ? Returns to career menu
- **Code:**
  ```csharp
  playButton.gameObject.SetActive(false);
  contButton.gameObject.SetActive(false);
  simButton.gameObject.SetActive(false);
  nextButton.gameObject.SetActive(true);
  ```

---

## **SetPlayoffs() Cases**

### **Case 1: Page Playoff - Round 1**
```csharp
heading.text = "Page Playoff - Round 1";
DisplayPagePlayoffTeams(4, highlightPlayer: true);
bool playerActive1 = SetupPagePlayoffVsDisplay();
ConfigurePagePlayoffButtons(playerActive1);  // Play if player has game, Sim otherwise
```

---

### **Case 2: Semifinals**
```csharp
heading.text = "Semifinals";
DisplayPagePlayoffTeams(7, highlightPlayer: true);
bool playerActive2 = SetupPagePlayoffVsDisplay();

// Special BYE handling
bool hasActualGame = tm.vsDisplay[1].name.text != "BYE TO FINALS";
ConfigurePagePlayoffButtons(hasActualGame);  // Sim if BYE, Play if actual game
```

---

### **Case 3: Finals**
```csharp
heading.text = "Finals";
DisplayPagePlayoffTeams(8, highlightPlayer: true);
bool playerActive3 = SetupPagePlayoffVsDisplay();
ConfigurePagePlayoffButtons(playerActive3);  // Play if in finals, Sim if knocked out
```

---

### **Case 4: Tournament Complete**
```csharp
heading.text = "Tournament Complete";
// ... prize distribution ...
playButton.gameObject.SetActive(false);
contButton.gameObject.SetActive(false);
simButton.gameObject.SetActive(false);
nextButton.gameObject.SetActive(true);  // ONLY Next button
```

---

## **SimPlayoff() Coroutine**

After each simulation round, show **Continue button** to allow user to review results before advancing:

```csharp
StartCoroutine(RefreshPlayoffPanel());
playoffRound++;
SetPlayoffs();

// After advancing round, show Continue button to let user review and advance
playButton.gameObject.SetActive(false);
simButton.gameObject.SetActive(false);
contButton.gameObject.SetActive(true);
```

**Why After `SetPlayoffs()`?**
- `SetPlayoffs()` might set button states based on the NEW round
- We override those states to show Continue button
- This ensures user can review simulation results before next action

---

## **User Flow Examples**

### **Example 1: Player Wins 1v2 (Gets BYE)**
1. **Round 1:** Player sees **Play button** ? Plays game ? Wins
2. **Round 2:** Player sees **Sim button** (BYE to finals) ? Clicks Sim
3. **After Sim:** Player sees **Continue button** ? Clicks Continue
4. **Round 3:** Player sees **Play button** (Finals) ? Plays finals

---

### **Example 2: Player Loses 3v4 (Knocked Out)**
1. **Round 1:** Player sees **Play button** ? Plays game ? Loses
2. **Round 2:** Player sees **Sim button** (knocked out) ? Clicks Sim
3. **After Sim:** Player sees **Continue button** ? Clicks Continue
4. **Round 3:** Player sees **Sim button** (knocked out) ? Clicks Sim
5. **After Sim:** Player sees **Continue button** ? Clicks Continue
6. **Round 4:** Tournament complete, sees **Next button**

---

### **Example 3: All AI Tournament**
1. **Round 1:** Player sees **Sim button** (not qualified) ? Clicks Sim
2. **After Sim:** Player sees **Continue button** ? Clicks Continue
3. **Round 2:** Player sees **Sim button** ? Clicks Sim
4. **After Sim:** Player sees **Continue button** ? Clicks Continue
5. (Continues until tournament complete)

---

## **Summary**

? **One button at a time** - Never show multiple action buttons simultaneously  
? **Clear user intent** - Play = I'm playing, Sim = Simulate others, Continue = Next round  
? **Consistent logic** - Same rules apply across all playoff systems  
? **No confusion** - User always knows what will happen when they click a button  

**Build Status:** ? Successful!
