# Team Stats Structure - Clarified

## Overview
This document explains the correct structure for team stats in the game, clarifying the confusing previous implementation.

---

## Stat Storage Locations

### 1. **Player Character (Skip) Stats**
- **Location**: `CareerManager.cStats`
- **What**: The player's individual stats (4th team member - the Skip)
- **Example**: `cStats.drawAccuracy = 45`
- **Modified by**: XPManager when player spends skill points

### 2. **Team Member Stats**
- **Location**: `CareerManager.activePlayers[0-2]`
- **What**: Individual stats for the 3 hired team members
  - `activePlayers[0]` = Lead
  - `activePlayers[1]` = Second
  - `activePlayers[2]` = Third
- **Example**: `activePlayers[0].draw = 40`
- **Modified by**: XPManager when player spends skill points on team members

### 3. **Equipment & Sponsor Bonuses**
- **Location**: `CareerManager.modStats`
- **What**: ONLY bonuses from equipment and active sponsor cards
- **Example**: `modStats.drawAccuracy = +10` (from equipment)
- **Modified by**: EquipmentManager and SponsorManager
- **Important**: Should NEVER contain team member stats!

### 4. **Total Team Stats**
- **Location**: `CareerManager.teams[i].draw` (for player team where `teams[i].player == true`)
- **What**: Sum of all 4 players' stats (calculated via `UpdateTeamSkillsFromPlayers()`)
- **Formula**: `teams[i].draw = activePlayers[0].draw + activePlayers[1].draw + activePlayers[2].draw + cStats.drawAccuracy`
- **Modified by**: Calling `teams[i].UpdateTeamSkillsFromPlayers()` after roster changes

---

## Team Menu Sliders

### Base Sliders (No Mods)
Show the **total team stats** from all 4 players:
```csharp
// Calculate team base (sum of all 4 players)
int teamBaseDraw = activePlayers[0].draw + activePlayers[1].draw + activePlayers[2].draw + cStats.drawAccuracy;

drawSlider.value = teamBaseDraw;
```

### Mod Sliders (With Equipment/Sponsors)
Show the **total team stats + bonuses**:
```csharp
drawModSlider.value = teamBaseDraw + modStats.drawAccuracy;
```

---

## Workflow

### When Player Upgrades Stats (XPManager)
1. Player spends skill point on a stat
2. `XPManager.ApplySlidersToPlayer()` updates either:
   - `cm.cStats.drawAccuracy` (if upgrading Skip)
   - `cm.activePlayers[i].draw` (if upgrading team member)
3. **No automatic team update** - stats are saved to player/team member

### When Player Changes Team Roster (TeamMenu)
1. Player selects new team members
2. `TeamMenu.SetTeam()` updates `cm.teams[i].players` with new roster
3. **Calls `UpdateTeamSkillsFromPlayers()`** to recalculate team totals

### When Equipment/Sponsors Change
1. `EquipmentManager` or `SponsorManager` modifies `cm.modStats`
2. Team menu sliders automatically show updated totals in `Update()`

### When Loading Save File
1. `CareerManager.LoadFromSaveData()` restores:
   - `cStats` (Skip's stats)
   - `activePlayers` (team members with their stats)
   - `modStats` (equipment/sponsor bonuses)
2. `TeamMenu.SetUpTeam()` **does NOT overwrite** `activePlayers` from `playerPool`
3. `SetTeam()` calls `UpdateTeamSkillsFromPlayers()` to sync team totals

---

## Fixed Bugs

### Bug 1: Sliders Showed Only Skip's Stats
**Before:**
```csharp
drawSlider.value = cm.cStats.drawAccuracy;  // Only Skip!
```

**After:**
```csharp
int teamBaseDraw = activePlayers[0].draw + activePlayers[1].draw + activePlayers[2].draw + cStats.drawAccuracy;
drawSlider.value = teamBaseDraw;  // All 4 players!
```

### Bug 2: Stats Reset Between Weeks
**Before:**
```csharp
// TeamMenu.SetUpTeam() was overwriting activePlayers from playerPool
activePlayers[i].draw = cm.playerPool[j].draw;  // Overwrites upgrades!
```

**After:**
```csharp
// Only mark as active, DON'T copy stats
cm.playerPool[j].active = true;
activePlayers[i].active = true;
// Stats already correct from save file!
```

### Bug 3: `modStats` Contained Team Member Stats
**Before:**
```csharp
// PreviewPoints() was polluting modStats
cm.modStats.drawAccuracy = activePlayers[0].draw + activePlayers[1].draw + activePlayers[2].draw;
```

**After:**
```csharp
// Removed PreviewPoints() and UnPreviewPoints() entirely
// modStats only contains equipment/sponsor bonuses
```

---

## Summary

**The Golden Rule:**
- `cStats` = Skip's individual stats
- `activePlayers[i]` = Team member individual stats  
- `modStats` = Equipment/sponsor bonuses ONLY
- `teams[i].draw` = Sum of all 4 players (calculated via `UpdateTeamSkillsFromPlayers()`)

**Team Menu Sliders:**
- Base slider = Sum of all 4 players
- Mod slider = Sum of all 4 players + equipment/sponsor bonuses

This structure is now clear, consistent, and bug-free!
