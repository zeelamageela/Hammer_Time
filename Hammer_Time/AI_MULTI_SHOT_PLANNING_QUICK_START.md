# Multi-Shot Planning - Quick Start ?

## ? **What's Been Built**

3 new files creating a **strategic planning system**:
1. `EndPlan.cs` - Plan data structure  
2. `StrategyPatternLibrary.cs` - 11 proven strategies
3. `MultiShotPlanner.cs` - Planning engine

1 modified file:
- `AI_Strategy.cs` - Integrated into all 5 strategy methods

---

## ?? **Quick Setup (5 min)**

### Step 1: Import in Unity
```
1. Open Unity Editor
2. Assets ? Reimport All (or Refresh with Ctrl+R)
3. Wait for "Compilation finished"
4. Check Console for errors
```

### Step 2: Enable in Inspector
```
1. Open scene with AI
2. Find "AI_Strategy" component
3. Enable ? "Use Multi Shot Planning"
4. Optional: ? "Planning Verbose Logging" (for debug)
```

### Step 3: Test!
```
1. Start AI vs AI game
2. Watch Console logs
3. See plans like "Guard-Draw-Draw" execute across 3 shots!
```

---

## ?? **What It Does**

### Before:
```
Rock 10: Remove threat (reactive)
Rock 12: Draw to button (reactive)
Rock 14: Draw again (reactive)
```

### After:
```
Rock 10: Guard (step 1 of "Guard-Draw-Draw" plan)
Rock 12: Draw behind guard (step 2)  
Rock 14: Add second counter (step 3)
```

**Result**: Coherent 3-shot strategies instead of random individual shots!

---

## ?? **Controls**

### Inspector Toggles:
- `Use Multi Shot Planning` - ON/OFF master switch
- `Planning Verbose Logging` - Debug logs

### What Planning Does:
? Analyzes game state  
? Selects from 11 proven strategies  
? Creates 2-3 shot plan  
? Executes plan step-by-step  
? Adapts if opponent disrupts plan

---

## ?? **11 Strategy Patterns**

### Without Hammer:
1. Guard-Draw-Protect
2. Corner-Guard-Corner  
3. Remove-Draw-Remove
4. Blank-Force

### With Hammer:
5. Guard-Draw-Draw
6. Clear-Clear-Score
7. Draw-Draw-Draw
8. Blank-To-Keep-Hammer

### Late Game:
9. Desperation-All-Out
10. Protect-Lead

---

## ?? **Troubleshooting**

| Problem | Solution |
|---------|----------|
| Build errors about missing types | Reimport All in Unity |
| "MultiShotPlanner not found" | Refresh Unity project (Ctrl+R) |
| AI behavior unchanged | Check toggle is ON in Inspector |
| Plans not visible in logs | Enable "Planning Verbose Logging" |

---

## ?? **Sample Debug Log**

```
[MultiShotPlanner] Planning for rock 10: Rocks left=3, Hammer=true
[MultiShotPlanner] Selected plan: Guard-Draw-Draw (Confidence: 0.80)
[MultiShot] Executing plan 'Guard-Draw-Draw' step 1/3: CreateOpportunity
[AggressiveHammer] ? Following multi-shot strategic plan!
```

---

## ?? **Next Steps**

1. Import into Unity ?
2. Enable in Inspector ?
3. Test AI vs AI game ?
4. Compare with planning OFF vs ON ?
5. Watch AI become strategically smarter! ??

---

**Full Documentation**: See `AI_MULTI_SHOT_PLANNING_COMPLETE.md` for details!
