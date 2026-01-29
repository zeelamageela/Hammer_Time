# Player Metadata Save/Load Fix

## Problem

After loading a saved career, team members were missing their **image (photo)**, **cost**, and **description** in the Team Menu. The stats were loading correctly, but the UI couldn't display player portraits or salary info.

### Root Cause

The `PlayerData` save structure and conversion methods were only saving/restoring **stat values**, not **metadata fields** like:
- `cost` (player salary)
- `description` (player bio text)
- `image` (player portrait Sprite)

When `TeamMenu.SetUpTeam()` loaded `cm.activePlayers` from the save file, it got Player objects with stats but **no metadata**, causing blank portraits and missing cost info.

---

## Solution

### Phase 1: Extend Save Data Structure

**File**: `Assets/Scripts/Tourny/SaveData/CareerSaveData.cs`

Added `cost` and `description` to `PlayerData`:

```csharp
[Serializable]
public class PlayerData
{
    public int id;
    public string name;
    public int draw;
    public int guard;
    public int takeOut;
    public int sweepStrength;
    public int sweepEnduro;
    public int sweepCohesion;
    
    // Opponent stats
    public int oppDraw;
    public int oppGuard;
    public int oppTakeOut;
    public int oppStrength;
    public int oppEnduro;
    public int oppCohesion;
    
    // Metadata (for display purposes) - NEW!
    public float cost;
    public string description;
    // Note: image (Sprite) cannot be serialized, will be restored from playerPool by ID
}
```

**Why not save `image`?**
- `image` is a `Sprite` (Unity asset reference) which **cannot be serialized to JSON**
- Solution: Store player `id`, then **match against `playerPool`** on load to restore the Sprite reference

---

### Phase 2: Update Save Conversion

**File**: `Assets/Scripts/Tourny/CareerManager.cs`

#### `PlayerToData()` - Now saves cost and description:

```csharp
private PlayerData PlayerToData(Player player)
{
    return new PlayerData
    {
        id = player.id,
        name = player.name,
        draw = player.draw,
        guard = player.guard,
        takeOut = player.takeOut,
        sweepStrength = player.sweepStrength,
        sweepEnduro = player.sweepEnduro,
        sweepCohesion = player.sweepCohesion,
        oppDraw = player.oppDraw,
        oppGuard = player.oppGuard,
        oppTakeOut = player.oppTakeOut,
        oppStrength = player.oppStrength,
        oppEnduro = player.oppEnduro,
        oppCohesion = player.oppCohesion,
        cost = player.cost,              // NEW!
        description = player.description // NEW!
    };
}
```

---

### Phase 3: Update Load Conversion

**File**: `Assets/Scripts/Tourny/CareerManager.cs`

#### `DataToPlayer()` - Now restores cost, description, and image:

```csharp
private Player DataToPlayer(PlayerData data)
{
    Player player = new Player
    {
        id = data.id,
        name = data.name,
        draw = data.draw,
        guard = data.guard,
        takeOut = data.takeOut,
        sweepStrength = data.sweepStrength,
        sweepEnduro = data.sweepEnduro,
        sweepCohesion = data.sweepCohesion,
        oppDraw = data.oppDraw,
        oppGuard = data.oppGuard,
        oppTakeOut = data.oppTakeOut,
        oppStrength = data.oppStrength,
        oppEnduro = data.oppEnduro,
        oppCohesion = data.oppCohesion,
        cost = data.cost,                // NEW!
        description = data.description   // NEW!
    };
    
    // Restore image (Sprite) from playerPool by matching ID
    if (playerPool != null)
    {
        foreach (var poolPlayer in playerPool)
        {
            if (poolPlayer.id == data.id)
            {
                player.image = poolPlayer.image;
                Debug.Log($"[CareerManager] Restored image for player {data.name} (ID: {data.id})");
                break;
            }
        }
    }
    
    if (player.image == null)
    {
        Debug.LogWarning($"[CareerManager] Could not restore image for player {data.name} (ID: {data.id}) - not found in playerPool");
    }
    
    return player;
}
```

**Key Logic:**
1. Restore `cost` and `description` directly from save data
2. **Match player by `id` in `playerPool`** to get the Sprite reference
3. Log warnings if image restoration fails (e.g., playerPool not initialized)

---

## How It Works (Data Flow)

### Saving
```
Player (in cm.activePlayers)
  ? PlayerToData()
  ? Saves: id, name, stats, cost, description
PlayerData (in JSON)
  ? JSON serialization
career_save.json
```

### Loading
```
career_save.json
  ? JSON deserialization
PlayerData
  ? DataToPlayer()
  ? Restores: id, name, stats, cost, description
  ? Matches id ? playerPool[id].image (Sprite reference)
Player (in cm.activePlayers)
  ?
TeamMenu.SetUpTeam() displays player correctly!
```

---

## Testing Checklist

### Scenario 1: Fresh Save
- [ ] Start new career
- [ ] Hire team members (Lead, 2nd, 3rd)
- [ ] Save and quit
- [ ] Load career
- [ ] **Verify**: Team Menu shows player portraits, costs, and descriptions

### Scenario 2: Mid-Season Save
- [ ] Play several weeks
- [ ] Change team members
- [ ] Save and quit
- [ ] Load career
- [ ] **Verify**: Current team members display correctly with all metadata

### Scenario 3: playerPool Edge Cases
- [ ] Load save with player IDs not in playerPool (should log warning, but not crash)
- [ ] **Verify**: Game handles missing images gracefully

---

## Files Modified

1. **`Assets/Scripts/Tourny/SaveData/CareerSaveData.cs`**
   - Added `cost` and `description` fields to `PlayerData`

2. **`Assets/Scripts/Tourny/CareerManager.cs`**
   - Updated `PlayerToData()` to save cost and description
   - Updated `DataToPlayer()` to restore cost, description, and image (via playerPool lookup)

---

## Build Status

? **Build Successful**

---

## Summary

The save system now properly preserves **all player metadata**:
- ? Stats (draw, guard, takeout, etc.) - always worked
- ? Cost (salary) - now saved/restored
- ? Description (bio) - now saved/restored
- ? Image (portrait) - now restored via playerPool ID lookup

Team members will now display correctly in the Team Menu after loading a save! ??
