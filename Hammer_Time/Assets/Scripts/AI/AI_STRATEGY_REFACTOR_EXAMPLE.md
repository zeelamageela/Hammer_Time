# AI Strategy Refactor Example

## What We've Built

You now have a **2-layer architecture** for AI decision-making:

### Layer 1: AI_Strategy (WHAT to do)
- **High-level strategic decisions**
- **Game situation awareness** (score, hammer, phase)
- **Simple, readable code**
- Creates `ShotIntent` with context

### Layer 2: AI_Target (HOW to do it)
- **Tactical execution**
- **Evaluates ALL shot options** (takeout, peel, raise, tick)
- **Physics-based simulation**
- Picks the BEST execution method

---

## Example: Simplified Strategy Method

Here's how the **NEW way** compares to the **OLD way**:

### OLD WAY (100+ lines of nested ifs):
```csharp
public void ConservativeSteal(int rockCurrent, string phase)
{
    if (gm.houseList.Count != 0)
    {
        if (closestRockInfo.teamName != rockInfo.teamName)
        {
            if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
            {
                if (cenGuard || tCenGuard)
                {
                    if (IsGuardBlocking(cenGuard, closestRock))
                    {
                        aiTarg.OnTarget("Peel", rockCurrent, cenGuard.rockIndex);
                    }
                    else
                    {
                        aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                    }
                }
                else
                {
                    aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                }
            }
            // ... 80 more lines of nested conditions ...
        }
    }
}
```

### NEW WAY (10 lines total):
```csharp
public void ConservativeStealSimplified(int rockCurrent, string phase)
{
    // Phase 1: Identify the threat
    int threatRock = FindBiggestThreat(activeTeamName);
    
    if (threatRock >= 0)
    {
        // Phase 2: Let AI_Target figure out HOW to remove it
        ShotContext context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
        context.acceptRisk = (phase == "late"); // More aggressive late in end
        aiTarg.ExecuteIntent(context, rockCurrent);
    }
    else
    {
        // No threat - create opportunity
        ShotContext context = new ShotContext(ShotIntent.CreateOpportunity);
        aiTarg.ExecuteIntent(context, rockCurrent);
    }
}
```

---

## What Happens Behind the Scenes

When you call `aiTarg.ExecuteIntent(ShotIntent.RemoveThreat, targetRock)`:

1. **AI_Target.ExecuteIntent()** switches on the intent
2. Calls **EvaluateRemovalOptions()** which:
   - Simulates **direct takeout** ? Score: 60
   - Simulates **peel guard** ? Score: 50
   - Simulates **raise friendly rock** ? Score: 40
   - Simulates **tick shot** ? Score: 0 (not a good situation)
3. **Picks the winner** (takeout with score 60)
4. Calls `OnTarget("Take Out", rockCurrent, targetRock)`
5. Physics-based shot executes!

---

## Benefits

### ? **Strategy is Simple**
- Easy to read and understand
- Easy to tweak (change when to be aggressive)
- Easy to add new strategies

### ? **Tactics are Smart**
- AI considers ALL options automatically
- Uses physics simulation for each option
- Picks the BEST one every time

### ? **Easy to Expand**
Just add new intents:
```csharp
case ShotIntent.FreezeOnShot:
    // AI_Target figures out how to freeze
    SimulateFreezeShot(context, rockCurrent);
    break;
```

---

## How to Use It

### Option 1: Replace Existing Strategies (Gradual)
Keep your existing strategies, but add this at the start of each one:
```csharp
public void ConservativeSteal(int rockCurrent, string phase)
{
    // NEW: Try intent-based approach first
    if (TryIntentBasedShot(rockCurrent, phase))
        return;
    
    // FALLBACK: Old logic (as backup)
    // ... existing 500 lines ...
}

private bool TryIntentBasedShot(int rockCurrent, string phase)
{
    int threat = FindBiggestThreat(activeTeamName);
    if (threat >= 0)
    {
        ShotContext context = new ShotContext(ShotIntent.RemoveThreat, threat);
        aiTarg.ExecuteIntent(context, rockCurrent);
        return true;
    }
    return false; // Couldn't decide, use fallback
}
```

### Option 2: Create New Simplified Strategies
Add new methods alongside old ones:
```csharp
public void AggressiveNotHammerV2(int rockCurrent, string phase)
{
    // NEW ARCHITECTURE
    int threat = FindBiggestThreat(activeTeamName);
    
    if (threat >= 0 && phase == "late")
    {
        // Aggressive - remove threats
        ShotContext context = new ShotContext(ShotIntent.RemoveThreat, threat);
        context.acceptRisk = true;
        aiTarg.ExecuteIntent(context, rockCurrent);
    }
    else if (HasStrongLead(activeTeamName))
    {
        // Protect lead
        ShotContext context = new ShotContext(ShotIntent.ProtectLead);
        aiTarg.ExecuteIntent(context, rockCurrent);
    }
    else
    {
        // Score points
        ShotContext context = new ShotContext(ShotIntent.ScorePoints);
        aiTarg.ExecuteIntent(context, rockCurrent);
    }
}
```

---

## Next Steps

1. **Test the new system** by calling `ExecuteIntent()` from one strategy
2. **Compare results** with the old system
3. **Gradually replace** old strategies with intent-based ones
4. **Add new intents** as needed (FreezeOnShot, RunBack, etc.)

---

## Summary

**Before:**
- Strategy makes tactical decisions (which shot type)
- 500+ lines per strategy method
- Hard to maintain
- Hard to add new shot types

**After:**
- Strategy makes strategic decisions (goals/intents)
- 10-20 lines per strategy method
- Easy to read and maintain
- Adding new shots = just add to AI_Target simulation list!

?? **You've separated strategy from tactics, making both simpler and smarter!**
