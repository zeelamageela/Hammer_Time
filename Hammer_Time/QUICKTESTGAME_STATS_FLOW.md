# QuickTestGame Stats Flow - How It Works

## ? The Stats Are Already Being Set Correctly!

Your `QuickTestGame` was actually setting opponent stats correctly all along. Here's how the system works:

---

## Stats Flow Diagram

```
QuickTestGame.StartQuickTestGame()
    ?
    ??> Sets gsp.yellowTeam.players[0-3].draw = 100
    ??> Sets gsp.yellowTeam.players[0-3].guard = 100
    ??> Sets gsp.yellowTeam.players[0-3].takeOut = 100
    ??> Sets gsp.yellowTeam.players[0-3].sweepStrength = 100
    
    ?
    ?
    
SceneManager.LoadScene("TournyGame")
    ?
    ?
    
TeamManager.Start()
    ?
    ?
    
TeamManager.SetCharacter(rockCurrent, redTurn)
    ?
    ??> Reads from gsp.yellowTeam.players[j]
    ?
    ??> Sets teamYellow[j].charStats values:
        ??> charStats.drawAccuracy.SetBaseValue(gsp.yellowTeam.players[j].draw)
        ??> charStats.takeOutAccuracy.SetBaseValue(gsp.yellowTeam.players[j].takeOut)
        ??> charStats.guardAccuracy.SetBaseValue(gsp.yellowTeam.players[j].guard)
        ??> charStats.sweepStrength.SetBaseValue(gsp.yellowTeam.players[j].sweepStrength)
    
    ?
    ?
    
AI_Shooter.GetShooterStats()
    ?
    ??> Finds TeamManager
    ??> Gets rock number: rockCurrent / 4 = memberIndex (0-3)
    ??> Determines team: isRedTeam based on gm.redHammer
    ?
    ??> Returns: tm.teamYellow[memberIndex].charStats
    
    ?
    ?
    
AI_Shooter.Shot() uses CharacterStats
    ?
    ??> CharacterStats stats = GetShooterStats()
        ??> float accuracy = stats.guardAccuracy.GetValue()  // Returns 100!
```

---

## Key Files & Their Roles

### 1. QuickTestGame.cs (You)
**Sets the data:**
```csharp
for (int i = 0; i < 4; i++)
{
    opponentTeam.players.Add(new Player
    {
        draw = opponentStatValue,      // 100
        guard = opponentStatValue,     // 100
        takeOut = opponentStatValue,   // 100
        sweepStrength = opponentStatValue  // 100
    });
}

gsp.yellowTeam = opponentTeam;
```

### 2. TeamManager.cs (Unity Scene)
**Transfers data from GameSettingsPersist to CharacterStats:**
```csharp
public void SetCharacter(int rockCurrent, bool redTurn)
{
    // For yellow team
    for (int j = 0; j < teamYellow.Length; j++)
    {
        teamYellow[j].charStats.drawAccuracy.SetBaseValue(gsp.yellowTeam.players[j].draw);
        teamYellow[j].charStats.takeOutAccuracy.SetBaseValue(gsp.yellowTeam.players[j].takeOut);
        teamYellow[j].charStats.guardAccuracy.SetBaseValue(gsp.yellowTeam.players[j].guard);
        // etc...
    }
}
```

### 3. AI_Shooter.cs (Gets the stats)
**Reads from TeamManager's CharacterStats:**
```csharp
private CharacterStats GetShooterStats()
{
    TeamManager tm = FindObjectOfType<TeamManager>();
    int memberIndex = currentRockNumber / 4;  // 0-3
    bool isRedTeam = (currentRockNumber % 2 == 0) ? gm.redHammer : !gm.redHammer;
    
    if (!isRedTeam && tm.teamYellow != null)
        return tm.teamYellow[memberIndex].charStats;  // ? This has your 100 stats!
}
```

---

## Why It Works Now

### The Chain:
1. **QuickTestGame** sets `gsp.yellowTeam.players[0-3]` with stat values of 100
2. **Scene loads** ? TeamManager.Start() runs
3. **TeamManager.SetCharacter()** gets called when each rock is thrown
4. **TeamManager reads** `gsp.yellowTeam.players[j].draw` (which is 100)
5. **TeamManager sets** `teamYellow[j].charStats.drawAccuracy` to 100
6. **AI_Shooter.GetShooterStats()** returns `teamYellow[memberIndex].charStats`
7. **AI_Shooter.Shot()** uses that CharacterStats: `float accuracy = stats.drawAccuracy.GetValue()` ? Returns 100!

---

## What You Don't Need To Do

### ? Don't Create CharacterStats in QuickTestGame
```csharp
// ? NOT NEEDED - Player class doesn't even have a charStats field!
aiPlayer.charStats = ScriptableObject.CreateInstance<CharacterStats>();
```

**Why?**
- `Player` class doesn't have a `charStats` field
- `CharacterStats` is a MonoBehaviour, not a ScriptableObject
- TeamManager creates/manages CharacterStats on scene GameObjects
- TeamManager reads Player stats and **populates** existing CharacterStats

### ? Don't Manually Set TeamMember.charStats
```csharp
// ? NOT NEEDED - TeamManager does this for you
teamYellow[0].charStats.drawAccuracy.SetBaseValue(100);
```

**Why?**
- TeamManager.SetCharacter() already does this
- It reads from gsp.yellowTeam.players and sets charStats
- Happens automatically when scene loads

---

## Verification: Does It Actually Work?

### Test 1: Check GameSettingsPersist
After pressing Q, verify in debugger:
```csharp
gsp.yellowTeam.players[0].draw == 100  // ? Should be true
gsp.yellowTeam.players[0].guard == 100  // ? Should be true
```

### Test 2: Check TeamManager CharacterStats
After scene loads and first rock is thrown:
```csharp
TeamManager tm = FindObjectOfType<TeamManager>();
tm.teamYellow[0].charStats.drawAccuracy.GetValue() == 100  // ? Should be true
```

### Test 3: Check AI_Shooter
In AI_Shooter.Shot() method, add debug log:
```csharp
CharacterStats stats = GetShooterStats();
Debug.Log($"AI accuracy: {stats.guardAccuracy.GetValue()}");  // Should print 100!
```

---

## Potential Issues (If Stats Still Aren't 100)

### Issue 1: TeamManager.SetCharacter() Bug
Look at line 264 in TeamManager.cs:
```csharp
// ? BUG: Should use yellowTeam, not redTeam!
teamYellow[j].charStats.takeOutAccuracy.SetBaseValue(gsp.redTeam.players[j].takeOut);
```

Should be:
```csharp
// ? CORRECT
teamYellow[j].charStats.takeOutAccuracy.SetBaseValue(gsp.yellowTeam.players[j].takeOut);
```

**This is a bug in TeamManager!** It's copying red team stats to yellow team for some stats.

### Issue 2: CareerManager.oppStats Override
If CareerManager exists and has oppStats, it might be getting added on top:
```csharp
// In your QuickTestGame, you set:
cm.oppStats.drawAccuracy = opponentStatValue;

// TeamManager might be adding this:
aiStats + gsp.oppStats.sweepStrength
```

Check if `aiStats` is being added (it's set to 10 in tournaments).

---

## Fix for TeamManager Bug

The real issue is in `TeamManager.SetCharacter()` around line 260-270. It's setting yellow team stats from RED team data!

```csharp
// BEFORE (BUG):
for (int j = 0; j < teamYellow.Length; j++)
{
    teamYellow[j].charStats.drawAccuracy.SetBaseValue(gsp.yellowTeam.players[j].draw);
    teamYellow[j].charStats.takeOutAccuracy.SetBaseValue(gsp.redTeam.players[j].takeOut);  // ? WRONG!
    teamYellow[j].charStats.guardAccuracy.SetBaseValue(gsp.redTeam.players[j].guard);      // ? WRONG!
    teamYellow[j].charStats.sweepStrength.SetBaseValue(gsp.redTeam.players[j].sweepStrength);  // ? WRONG!
    // etc...
}

// AFTER (FIXED):
for (int j = 0; j < teamYellow.Length; j++)
{
    teamYellow[j].charStats.drawAccuracy.SetBaseValue(gsp.yellowTeam.players[j].draw);
    teamYellow[j].charStats.takeOutAccuracy.SetBaseValue(gsp.yellowTeam.players[j].takeOut);  // ? FIXED!
    teamYellow[j].charStats.guardAccuracy.SetBaseValue(gsp.yellowTeam.players[j].guard);      // ? FIXED!
    teamYellow[j].charStats.sweepStrength.SetBaseValue(gsp.yellowTeam.players[j].sweepStrength);  // ? FIXED!
    // etc...
}
```

**This is why yellow team stats might not be 100!**

---

## Summary

### What QuickTestGame Does Right ?
- Sets `gsp.yellowTeam.players[0-3]` with max stats (100)
- Sets `gsp.redTeam` with player stats (50)
- Sets `cm.oppStats` with max stats (100)

### What TeamManager Should Do ?
- Read `gsp.yellowTeam.players[j]` and populate `teamYellow[j].charStats`
- **But has a bug!** It reads from `gsp.redTeam.players[j]` for some stats!

### What AI_Shooter Does ?
- Calls `GetShooterStats()` to get the CharacterStats
- Uses `stats.guardAccuracy.GetValue()` to calculate error
- Should work perfectly IF TeamManager populated stats correctly

### The Real Problem ??
**TeamManager line 264-269 copies RED team stats to YELLOW team!**

---

## Recommendation

Fix the bug in `TeamManager.SetCharacter()` and your QuickTestGame will work perfectly. Your stats setup is already correct!
